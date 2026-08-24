from flask import Flask, render_template, request, jsonify, send_file
import pandas as pd
import numpy as np
from datetime import datetime
import os
import sys
import io
import json

app = Flask(__name__)

if getattr(sys, 'frozen', False):
    template_folder = os.path.join(sys._MEIPASS, 'templates')
    app = Flask(__name__, template_folder=template_folder)
    application_path = os.path.dirname(sys.executable)
else:
    app = Flask(__name__)
    application_path = os.path.dirname(os.path.abspath(__file__))

inventory_file = os.path.join(application_path, '부품_재고현황.xlsx')
history_file = os.path.join(application_path, 'inventory_history.xlsx')
manual_log_file = os.path.join(application_path, 'manual_log.xlsx')
sales_file = os.path.join(application_path, '매출현황.xlsx')
price_file = os.path.join(application_path, '단가.xlsx')
safety_stock_file = os.path.join(application_path, 'safety_stock.json')

def get_safety_stock_data():
    if not os.path.exists(safety_stock_file):
        return {}
    try:
        with open(safety_stock_file, 'r', encoding='utf-8') as f:
            return json.load(f)
    except:
        return {}

def save_safety_stock_data(data):
    with open(safety_stock_file, 'w', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=4)

# --- 매출 데이터 자동 생성 로직 ---
def init_sales_data():
    if os.path.exists(sales_file):
        return
    
    # Generate mock sales data using prices from 단가.xlsx
    if not os.path.exists(price_file):
        # Create a fallback price file if it doesn't exist
        mock_prices = pd.DataFrame([
            {'품번': '803A030', '품명': 'Stud BOLT', '단가': 49565},
            {'품번': 'MA000082', '품명': 'COOLING BLOCK ASSY_ROTOR HOUSING SIDE_HD300H', '단가': 46000},
            {'품번': 'MA000083', '품명': 'COOLING BLOCK ASSY_ROTOR HOUSING RIGHT_HD300', '단가': 45000},
            {'품번': 'MA000093', '품명': 'COOLING BLOCK ASSY_NDEP LEFT_HD550', '단가': 31000},
            {'품번': 'MA000094', '품명': 'COOLING BLOCK ASSY_NDEP RIGHT_HD550', '단가': 31000}
        ])
        mock_prices.to_excel(price_file, index=False)
    
    try:
        prices_df = pd.read_excel(price_file)
        
        # Create 500 sales entries over the last 2 years
        np.random.seed(42)
        end_date = datetime.today()
        start_date = end_date - pd.Timedelta(days=730)
        
        dates = pd.date_range(start=start_date, end=end_date, freq='D')
        # Pick random dates as strings
        chosen_dates = np.random.choice(dates.strftime('%Y-%m-%d'), size=500, replace=True)
        
        sales_list = []
        for d in sorted(chosen_dates):
            # Pick a random item
            item = prices_df.sample(n=1).iloc[0]
            qty = int(np.random.randint(1, 15))
            rev = int(qty * item['단가'])
            sales_list.append({
                '날짜': d,
                '품번': str(item['품번']),
                '품명': str(item['품명']),
                '수량': qty,
                '단가': int(item['단가']),
                '매출액': rev
            })
            
        pd.DataFrame(sales_list).to_excel(sales_file, index=False)
    except Exception as e:
        print(f"Error creating sales mock data: {e}")

# Initialize mock sales data on startup
init_sales_data()


# --- 기존 재고 로드 및 저장 함수 ---
def get_inventory():
    if not os.path.exists(inventory_file):
        raise FileNotFoundError(f"{inventory_file} 파일이 없습니다.")
    raw = pd.read_excel(inventory_file)
    inventory = raw.iloc[2:, [0,1,2,3,4,5,6]].copy()
    inventory.columns = ['순번','신품번','구품번','품명','재고수량','공정진행','입고수량']
    inventory['재고수량'] = pd.to_numeric(inventory['재고수량'], errors='coerce').fillna(0)
    inventory = inventory.where(pd.notnull(inventory), None)
    return inventory, raw

def save_inventory(inventory, raw):
    date = datetime.today().strftime('%Y-%m-%d')
    raw.columns.values[6] = date
    raw.iloc[2:,4] = inventory['재고수량'].values
    raw.to_excel(inventory_file, index=False)
    
    log = inventory[['신품번','재고수량']].copy()
    log.columns = ['품번', '재고수량']
    log['날짜'] = date
    if os.path.exists(history_file):
        old = pd.read_excel(history_file)
        log = pd.concat([old, log])
    log.to_excel(history_file, index=False)


# --- 라우팅 ---
@app.route('/')
def index():
    return render_template('index.html')


# --- 재고 관리 API ---
@app.route('/api/inventory', methods=['GET'])
def api_get_inventory():
    try:
        inv, _ = get_inventory()
        ss_data = get_safety_stock_data()
        
        inv['안전재고기준'] = inv['신품번'].apply(lambda x: int(ss_data.get(str(x), 50)))
        inv['is_shortage'] = inv['재고수량'] < inv['안전재고기준']
        
        return jsonify({'status': 'success', 'data': inv.to_dict('records')})
    except Exception as e:
        return jsonify({'status': 'error', 'message': str(e)})



@app.route('/api/upload', methods=['POST'])
def api_upload():
    if 'file' not in request.files:
        return jsonify({'status': 'error', 'message': '파일이 없습니다.'})
    file = request.files['file']
    if file.filename == '':
        return jsonify({'status': 'error', 'message': '선택된 파일이 없습니다.'})
    
    try:
        delivery = pd.read_excel(file)
        inv, raw = get_inventory()
        
        # Load prices for sales recording
        prices_df = pd.read_excel(price_file) if os.path.exists(price_file) else pd.DataFrame(columns=['품번', '품명', '단가'])
        sales_entries = []
        today_str = datetime.today().strftime('%Y-%m-%d')
        
        for _, row in delivery.iterrows():
            if '신품번' in row:
                part = str(row['신품번'])
            elif '품번' in row:
                part = str(row['품번'])
            else:
                continue
                
            if '납품수량' in row:
                qty = int(row['납품수량'])
            else:
                continue
                
            idx = inv[inv['신품번'] == part].index
            if len(idx):
                inv.loc[idx, '재고수량'] -= qty
                
                # Record sales entry
                part_name = inv.loc[idx, '품명'].iloc[0] or "알 수 없음"
                price_match = prices_df[prices_df['품번'] == part]
                unit_price = int(price_match['단가'].iloc[0]) if len(price_match) else 0
                revenue = qty * unit_price
                
                sales_entries.append({
                    '날짜': today_str,
                    '품번': part,
                    '품명': part_name,
                    '수량': qty,
                    '단가': unit_price,
                    '매출액': revenue
                })
                
        save_inventory(inv, raw)
        
        # Save sales records if any
        if len(sales_entries) > 0:
            new_sales = pd.DataFrame(sales_entries)
            if os.path.exists(sales_file):
                old_sales = pd.read_excel(sales_file)
                updated_sales = pd.concat([old_sales, new_sales], ignore_index=True)
            else:
                updated_sales = new_sales
            updated_sales.to_excel(sales_file, index=False)
            
        return jsonify({'status': 'success', 'message': '재고 업데이트 및 매출 반영 완료'})
    except Exception as e:
        return jsonify({'status': 'error', 'message': str(e)})

@app.route('/api/edit', methods=['POST'])
def api_edit():
    data = request.json
    part = data.get('part')
    new_qty = data.get('qty')
    reason = data.get('reason')
    new_safety = data.get('safety_stock')
    
    try:
        inv, raw = get_inventory()
        idx = inv[inv['신품번'] == part].index
        if not len(idx):
            return jsonify({'status': 'error', 'message': '품번을 찾을 수 없습니다.'})
            
        old_qty = inv.loc[idx, '재고수량'].iloc[0]
        inv.loc[idx, '재고수량'] = int(new_qty)
        
        log_entry = pd.DataFrame([{
            '날짜': datetime.today().strftime('%Y-%m-%d'),
            '품번': part,
            '변경전': old_qty,
            '변경후': int(new_qty),
            '사유': reason
        }])
        
        if os.path.exists(manual_log_file):
            old = pd.read_excel(manual_log_file)
            log_entry = pd.concat([old, log_entry])
        log_entry.to_excel(manual_log_file, index=False)
        
        save_inventory(inv, raw)
        
        if new_safety is not None:
            ss_data = get_safety_stock_data()
            ss_data[str(part)] = int(new_safety)
            save_safety_stock_data(ss_data)
            
        return jsonify({'status': 'success', 'message': '수정 저장 완료'})
    except Exception as e:
        return jsonify({'status': 'error', 'message': str(e)})


# --- 매출 현황 대시보드 API ---
@app.route('/api/sales/kpi', methods=['GET'])
def get_sales_kpis():
    if not os.path.exists(sales_file):
        return jsonify({'status': 'error', 'message': '매출 파일 없음'})
    try:
        df = pd.read_excel(sales_file)
        df['날짜_dt'] = pd.to_datetime(df['날짜'])
        
        today = datetime.today()
        today_str = today.strftime('%Y-%m-%d')
        
        # 1. Today
        today_sales = int(df[df['날짜'] == today_str]['매출액'].sum())
        yesterday_str = (today - pd.Timedelta(days=1)).strftime('%Y-%m-%d')
        yesterday_sales = int(df[df['날짜'] == yesterday_str]['매출액'].sum())
        today_growth = round(((today_sales - yesterday_sales) / yesterday_sales * 100), 1) if yesterday_sales > 0 else 0
        
        # 2. Week
        current_year = today.year
        current_week = today.isocalendar()[1]
        
        df['year'] = df['날짜_dt'].dt.year
        df['week'] = df['날짜_dt'].dt.isocalendar().week
        
        week_sales = int(df[(df['year'] == current_year) & (df['week'] == current_week)]['매출액'].sum())
        prev_week = current_week - 1 if current_week > 1 else 52
        prev_year = current_year if current_week > 1 else current_year - 1
        prev_week_sales = int(df[(df['year'] == prev_year) & (df['week'] == prev_week)]['매출액'].sum())
        week_growth = round(((week_sales - prev_week_sales) / prev_week_sales * 100), 1) if prev_week_sales > 0 else 0
        
        # 3. Month
        current_month = today.month
        month_sales = int(df[(df['year'] == current_year) & (df['날짜_dt'].dt.month == current_month)]['매출액'].sum())
        prev_month = current_month - 1 if current_month > 1 else 12
        prev_month_year = current_year if current_month > 1 else current_year - 1
        prev_month_sales = int(df[(df['year'] == prev_month_year) & (df['날짜_dt'].dt.month == prev_month)]['매출액'].sum())
        month_growth = round(((month_sales - prev_month_sales) / prev_month_sales * 100), 1) if prev_month_sales > 0 else 0
        
        # 4. Year
        year_sales = int(df[df['year'] == current_year]['매출액'].sum())
        prev_year_sales = int(df[df['year'] == (current_year - 1)]['매출액'].sum())
        year_growth = round(((year_sales - prev_year_sales) / prev_year_sales * 100), 1) if prev_year_sales > 0 else 0
        
        return jsonify({
            'status': 'success',
            'kpis': {
                'today': {'value': today_sales, 'growth': today_growth},
                'week': {'value': week_sales, 'growth': week_growth},
                'month': {'value': month_sales, 'growth': month_growth},
                'year': {'value': year_sales, 'growth': year_growth}
            }
        })
    except Exception as e:
        return jsonify({'status': 'error', 'message': str(e)})

@app.route('/api/sales/trend', methods=['GET'])
def get_sales_trend():
    period = request.args.get('period', 'monthly')
    start_date = request.args.get('start_date')
    end_date = request.args.get('end_date')
    
    if not os.path.exists(sales_file):
        return jsonify({'status': 'error', 'message': '매출 파일 없음'})
        
    try:
        df = pd.read_excel(sales_file)
        df['날짜_dt'] = pd.to_datetime(df['날짜'])
        
        # Apply custom date range filter if provided
        if start_date:
            df = df[df['날짜_dt'] >= pd.to_datetime(start_date)]
        if end_date:
            df = df[df['날짜_dt'] <= pd.to_datetime(end_date)]
            
        if period == 'daily':
            # Last 30 days if no custom date filter is set
            if not start_date and not end_date:
                limit_date = datetime.today() - pd.Timedelta(days=30)
                df = df[df['날짜_dt'] >= limit_date]
            grouped = df.groupby(df['날짜_dt'].dt.strftime('%m-%d'))['매출액'].sum().reset_index()
            grouped.columns = ['label', 'value']
            
        elif period == 'weekly':
            # Last 12 weeks if no custom date filter is set
            if not start_date and not end_date:
                limit_date = datetime.today() - pd.Timedelta(weeks=12)
                df = df[df['날짜_dt'] >= limit_date]
            filtered = df.copy()
            
            def get_weekly_label(dt):
                first_day = dt.replace(day=1)
                week_of_month = (dt.day + first_day.weekday() - 1) // 7 + 1
                return f"{dt.year} {dt.month}월 {week_of_month}주차"
                
            filtered['year_week'] = filtered['날짜_dt'].dt.strftime('%Y-%V')
            grouped = filtered.groupby('year_week').agg({
                '매출액': 'sum',
                '날짜_dt': 'min'
            }).reset_index()
            
            grouped['label'] = grouped['날짜_dt'].apply(get_weekly_label)
            grouped = grouped.sort_values(by='year_week')
            grouped = grouped[['label', '매출액']]
            grouped.columns = ['label', 'value']
            
        elif period == 'monthly':
            # Last 12 months if no custom date filter is set
            if not start_date and not end_date:
                limit_date = datetime.today() - pd.Timedelta(days=365)
                df = df[df['날짜_dt'] >= limit_date]
            grouped = df.groupby(df['날짜_dt'].dt.strftime('%Y-%m'))['매출액'].sum().reset_index()
            grouped.columns = ['label', 'value']
            
        elif period == 'quarterly':
            # Group by Quarter
            df['quarter'] = df['날짜_dt'].dt.to_period('Q').astype(str)
            grouped = df.groupby('quarter')['매출액'].sum().reset_index()
            grouped.columns = ['label', 'value']
            grouped = grouped.tail(8)
            
        elif period == 'half-yearly':
            # Group by half-year (H1/H2)
            def get_half_year(dt):
                return f"{dt.year} H1" if dt.month <= 6 else f"{dt.year} H2"
            df['half'] = df['날짜_dt'].apply(get_half_year)
            grouped = df.groupby('half')['매출액'].sum().reset_index()
            grouped.columns = ['label', 'value']
            grouped = grouped.tail(6)
            
        elif period == 'yearly':
            grouped = df.groupby(df['날짜_dt'].dt.strftime('%Y'))['매출액'].sum().reset_index()
            grouped.columns = ['label', 'value']
            
        else:
            return jsonify({'status': 'error', 'message': '잘못된 집계 단위'})
            
        return jsonify({
            'status': 'success',
            'labels': grouped['label'].tolist(),
            'data': [int(v) for v in grouped['value'].tolist()]
        })
    except Exception as e:
        return jsonify({'status': 'error', 'message': str(e)})

@app.route('/api/sales/top-items', methods=['GET'])
def get_top_items():
    if not os.path.exists(sales_file):
        return jsonify({'status': 'error', 'message': '매출 파일 없음'})
    try:
        df = pd.read_excel(sales_file)
        grouped = df.groupby('품명')['매출액'].sum().reset_index()
        grouped = grouped.sort_values(by='매출액', ascending=False).head(5)
        
        return jsonify({
            'status': 'success',
            'labels': grouped['품명'].tolist(),
            'data': [int(v) for v in grouped['매출액'].tolist()]
        })
    except Exception as e:
        return jsonify({'status': 'error', 'message': str(e)})

@app.route('/api/sales/recent', methods=['GET'])
def get_recent_sales():
    if not os.path.exists(sales_file):
        return jsonify({'status': 'error', 'message': '매출 파일 없음'})
    try:
        df = pd.read_excel(sales_file)
        df = df.sort_values(by='날짜', ascending=False).head(10)
        df = df.where(pd.notnull(df), None)
        
        return jsonify({
            'status': 'success',
            'data': df.to_dict('records')
        })
    except Exception as e:
        return jsonify({'status': 'error', 'message': str(e)})


if __name__ == '__main__':
    try:
        import webview
        webview.create_window('부품 재고 및 매출 관리 시스템', app, width=1600, height=900)
        webview.start()
    except ImportError:
        app.run(debug=True, port=5000)
