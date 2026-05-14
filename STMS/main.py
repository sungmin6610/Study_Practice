import tkinter as tk
from tkinter import filedialog, messagebox, ttk
import customtkinter as ctk
import pandas as pd
from datetime import datetime
import os
import matplotlib.pyplot as plt

# 폰트 및 기본 설정
plt.rcParams['font.family'] = 'Malgun Gothic'
plt.rcParams['axes.unicode_minus'] = False

# 테마 설정
ctk.set_appearance_mode("light")  # 모드: "System" (standard), "Dark", "Light"
ctk.set_default_color_theme("blue")  # 테마: "blue" (standard), "green", "dark-blue"

class ModernInventoryManager(ctk.CTk):
    def __init__(self):
        super().__init__()

        self.title('STMS 재고 관리 시스템')
        self.geometry('1400x900')
        self.configure(fg_color="#F8F9FA")  # 전체 배경색 (연한 회색)

        self.manual_mode = False
        self.inventory_file = '부품_재고현황.xlsx'
        self.history_file = 'inventory_history.xlsx'
        
        # 데이터 로드
        try:
            self.load_inventory()
        except Exception as e:
            messagebox.showerror("오류", f"엑셀 파일을 불러올 수 없습니다: {e}")
            self.inventory = pd.DataFrame(columns=['순번','신품번','구품번','품명','재고수량','공정진행','입고수량'])

        self.build_ui()

    def load_inventory(self):
        if not os.path.exists(self.inventory_file):
            # 파일이 없을 경우 빈 데이터프레임 생성 (테스트용)
            self.raw = pd.DataFrame()
            self.inventory = pd.DataFrame(columns=['순번','신품번','구품번','품명','재고수량','공정진행','입고수량'])
            return

        self.raw = pd.read_excel(self.inventory_file)
        # 기존 로직: 3번째 행부터 데이터, 특정 컬럼 선택
        self.inventory = self.raw.iloc[2:, [0,1,2,3,4,5,6]].copy()
        self.inventory.columns = ['순번','신품번','구품번','품명','재고수량','공정진행','입고수량']
        self.inventory['재고수량'] = pd.to_numeric(self.inventory['재고수량'], errors='coerce').fillna(0)

    def build_ui(self):
        # 1. 헤더 영역
        self.header = ctk.CTkFrame(self, fg_color="transparent")
        self.header.pack(fill="x", padx=40, pady=(30, 10))

        self.title_label = ctk.CTkLabel(
            self.header, 
            text="📦 STMS 재고 관리 시스템", 
            font=ctk.CTkFont(family="Malgun Gothic", size=32, weight="bold"),
            text_color="#1A1A1A"
        )
        self.title_label.pack(side="left")

        # 상단 우측 버튼들
        self.btn_frame = ctk.CTkFrame(self.header, fg_color="transparent")
        self.btn_frame.pack(side="right")

        self.manual_btn = ctk.CTkButton(
            self.btn_frame, text="수정모드 OFF", 
            fg_color="#E2E8F0", text_color="#4A5568", hover_color="#CBD5E0",
            width=120, height=40, font=("Malgun Gothic", 13, "bold"),
            command=self.toggle_manual
        )
        self.manual_btn.pack(side="left", padx=5)

        self.refresh_btn = ctk.CTkButton(
            self.btn_frame, text="새로고침", 
            width=100, height=40, font=("Malgun Gothic", 13, "bold"),
            command=self.refresh_data
        )
        self.refresh_btn.pack(side="left", padx=5)

        self.dash_btn = ctk.CTkButton(
            self.btn_frame, text="대시보드", 
            fg_color="#4F46E5", hover_color="#4338CA",
            width=100, height=40, font=("Malgun Gothic", 13, "bold"),
            command=self.dashboard
        )
        self.dash_btn.pack(side="left", padx=5)

        # 2. 업로드 영역 (Drop Zone 스타일)
        self.upload_frame = ctk.CTkFrame(self, fg_color="#FFFFFF", corner_radius=15, border_width=2, border_color="#E2E8F0")
        self.upload_frame.pack(fill="x", padx=40, pady=20)

        self.upload_btn = ctk.CTkButton(
            self.upload_frame, 
            text="\n\n📄 납품명세서 엑셀 파일을 클릭하여 업로드하세요.\n\n",
            fg_color="transparent", text_color="#718096",
            hover_color="#F8FAFC",
            font=("Malgun Gothic", 16),
            command=self.upload_delivery
        )
        self.upload_btn.pack(fill="both", expand=True, padx=10, pady=10)

        # 3. 데이터 테이블 영역 (표)
        self.table_container = ctk.CTkFrame(self, fg_color="#FFFFFF", corner_radius=15)
        self.table_container.pack(fill="both", expand=True, padx=40, pady=(10, 40))

        table_header = ctk.CTkLabel(
            self.table_container, text="📊 현재 재고 현황", 
            font=("Malgun Gothic", 18, "bold"), text_color="#2D3748"
        )
        table_header.pack(anchor="w", padx=25, pady=(20, 10))

        # Treeview 스타일 설정
        style = ttk.Style()
        style.theme_use("default")
        style.configure("Treeview",
                        background="#FFFFFF",
                        foreground="#2D3748",
                        rowheight=40,
                        fieldbackground="#FFFFFF",
                        borderid=0,
                        font=("Malgun Gothic", 11))
        style.configure("Treeview.Heading",
                        background="#F1F5F9",
                        foreground="#4A5568",
                        relief="flat",
                        font=("Malgun Gothic", 11, "bold"))
        style.map("Treeview", background=[('selected', '#E0E7FF')], foreground=[('selected', '#1E40AF')])

        cols = ('순번','신품번','구품번','품명','재고수량','공정진행','입고수량')
        self.tree = ttk.Treeview(self.table_container, columns=cols, show='headings', selectmode="browse")
        
        # 컬럼 너비 및 정렬 설정
        column_widths = [60, 150, 150, 400, 100, 100, 100]
        for i, col in enumerate(cols):
            self.tree.heading(col, text=col)
            self.tree.column(col, width=column_widths[i], anchor="center" if i != 3 else "w")

        # 스크롤바
        scrollbar = ttk.Scrollbar(self.table_container, orient="vertical", command=self.tree.yview)
        self.tree.configure(yscrollcommand=scrollbar.set)
        
        self.tree.pack(side="left", fill="both", expand=True, padx=(25, 0), pady=(0, 25))
        scrollbar.pack(side="right", fill="y", padx=(0, 25), pady=(0, 25))

        self.tree.bind('<Double-1>', self.edit_inventory)
        
        # 하단 수정 버튼 (플로팅 스타일 느낌으로 표 안에 배치)
        self.edit_btn = ctk.CTkButton(
            self.table_container, text="재고 직접 수정", 
            fg_color="#3B82F6", hover_color="#2563EB",
            width=150, height=35, font=("Malgun Gothic", 12, "bold"),
            command=self.edit_inventory
        )
        self.edit_btn.place(relx=0.97, rely=0.03, anchor="ne")

        self.refresh_tree()

    def refresh_tree(self):
        for i in self.tree.get_children():
            self.tree.delete(i)
        for _, row in self.inventory.iterrows():
            self.tree.insert('', 'end', values=list(row))

    def refresh_data(self):
        self.load_inventory()
        self.refresh_tree()
        messagebox.showinfo("알림", "데이터가 새로고침되었습니다.")

    def upload_delivery(self):
        file = filedialog.askopenfilename(filetypes=[('Excel files','*.xlsx *.xls')])
        if not file:
            return
        try:
            delivery = pd.read_excel(file)
            count = 0
            
            # 납품명세서 컬럼 확인 (신품번 또는 품번)
            part_col = '신품번'
            if '신품번' not in delivery.columns:
                if '품번' in delivery.columns:
                    part_col = '품번'
                else:
                    raise Exception("엑셀 파일에 '신품번' 또는 '품번' 컬럼이 존재하지 않습니다.")
            
            if '납품수량' not in delivery.columns:
                raise Exception("엑셀 파일에 '납품수량' 컬럼이 존재하지 않습니다.")

            for _, row in delivery.iterrows():
                part = row[part_col]
                qty = row['납품수량']
                idx = self.inventory[self.inventory['신품번']==part].index
                if len(idx):
                    self.inventory.loc[idx,'재고수량'] -= qty
                    count += 1
            
            self.save_inventory()
            self.refresh_tree()
            messagebox.showinfo('완료',f'{count}건의 품목 재고 업데이트가 완료되었습니다.')
        except Exception as e:
            messagebox.showerror("오류", f"파일 처리 중 오류가 발생했습니다: {e}")

    def save_inventory(self):
        date = datetime.today().strftime('%Y-%m-%d')
        # 원본 데이터 업데이트
        if not self.raw.empty:
            self.raw.columns.values[6] = date
            self.raw.iloc[2:,4] = self.inventory['재고수량'].values
            self.raw.to_excel(self.inventory_file, index=False)
        
        # 로그 기록
        log = self.inventory[['신품번','재고수량']].copy() # 기존 코드 '품번'을 '신품번'으로 수정 (컬럼명 일치)
        log['날짜']=date
        if os.path.exists(self.history_file):
            try:
                old = pd.read_excel(self.history_file)
                log = pd.concat([old,log])
            except: pass
        log.to_excel(self.history_file,index=False)

    def toggle_manual(self):
        if not self.manual_mode:
            # 커스텀 비밀번호 창
            pw_window = ctk.CTkToplevel(self)
            pw_window.title('관리자 인증')
            pw_window.geometry('350x200')
            pw_window.attributes("-topmost", True)
            
            center_x = self.winfo_x() + (self.winfo_width() // 2) - 175
            center_y = self.winfo_y() + (self.winfo_height() // 2) - 100
            pw_window.geometry(f"+{center_x}+{center_y}")

            label = ctk.CTkLabel(pw_window, text='관리자 비밀번호를 입력하세요', font=("Malgun Gothic", 14))
            label.pack(pady=(30, 10))
            
            pw_entry = ctk.CTkEntry(pw_window, show='*', width=200, height=35)
            pw_entry.pack(pady=10)
            pw_entry.focus_set()

            def check_pw(event=None):
                if pw_entry.get() == 'admin1234':
                    self.manual_mode = True
                    self.manual_btn.configure(text="수정모드 ON", fg_color="#FEE2E2", text_color="#B91C1C", hover_color="#FECACA")
                    pw_window.destroy()
                    messagebox.showinfo('성공', '관리자 모드가 활성화되었습니다.')
                else:
                    messagebox.showerror('오류', '비밀번호가 일치하지 않습니다.')
            
            ok_btn = ctk.CTkButton(pw_window, text='인증하기', command=check_pw, width=100)
            ok_btn.pack(pady=15)
            pw_window.bind('<Return>', check_pw)
            
        else:
            self.manual_mode = False
            self.manual_btn.configure(text="수정모드 OFF", fg_color="#E2E8F0", text_color="#4A5568", hover_color="#CBD5E0")
            messagebox.showinfo('알림', '관리자 모드가 종료되었습니다.')

    def edit_inventory(self, event=None):
        if not self.manual_mode:
            messagebox.showwarning('경고','수동 모드(관리자 인증)를 먼저 활성화하세요.')
            return
        
        selected = self.tree.selection()
        if not selected:
            messagebox.showwarning('경고','수정할 품목을 리스트에서 선택해 주세요.')
            return
            
        values = self.tree.item(selected[0])['values']
        
        edit_win = ctk.CTkToplevel(self)
        edit_win.title('재고 수동 수정')
        edit_win.geometry('400x450')
        edit_win.attributes("-topmost", True)

        # 정보 표시
        info_frame = ctk.CTkFrame(edit_win, fg_color="transparent")
        info_frame.pack(fill="x", padx=30, pady=30)
        
        ctk.CTkLabel(info_frame, text="재고 수정", font=("Malgun Gothic", 20, "bold")).pack(pady=(0,20))
        ctk.CTkLabel(info_frame, text=f"품명: {values[3]}", wraplength=300).pack(anchor="w")
        ctk.CTkLabel(info_frame, text=f"신품번: {values[1]}").pack(anchor="w")
        ctk.CTkLabel(info_frame, text=f"현재 재고: {values[4]}개", font=("Malgun Gothic", 12, "bold")).pack(anchor="w", pady=(10,0))

        # 입력 필드
        ctk.CTkLabel(edit_win, text="변경할 재고 수량").pack(anchor="w", padx=30)
        qty_entry = ctk.CTkEntry(edit_win, width=340)
        qty_entry.insert(0, values[4])
        qty_entry.pack(padx=30, pady=(5, 15))

        ctk.CTkLabel(edit_win, text="수정 사유").pack(anchor="w", padx=30)
        reason_entry = ctk.CTkEntry(edit_win, width=340, placeholder_text="예: 실사 후 재고 조정")
        reason_entry.pack(padx=30, pady=(5, 20))

        def save_edit():
            try:
                new_qty = int(qty_entry.get())
                reason = reason_entry.get()
                if not reason:
                    messagebox.showwarning("알림", "수정 사유를 입력해주세요.")
                    return
                
                idx = self.inventory[self.inventory['신품번']==values[1]].index
                old_qty = self.inventory.loc[idx,'재고수량'].iloc[0]
                self.inventory.loc[idx,'재고수량'] = new_qty
                
                # 수동 로그 저장
                log_data = {
                    '날짜': datetime.today().strftime('%Y-%m-%d %H:%M:%S'),
                    '신품번': values[1],
                    '품명': values[3],
                    '변경전': old_qty,
                    '변경후': new_qty,
                    '사유': reason
                }
                log_df = pd.DataFrame([log_data])
                
                if os.path.exists('manual_log.xlsx'):
                    try:
                        old_log = pd.read_excel('manual_log.xlsx')
                        log_df = pd.concat([old_log, log_df])
                    except: pass
                log_df.to_excel('manual_log.xlsx', index=False)
                
                self.save_inventory()
                self.refresh_tree()
                edit_win.destroy()
                messagebox.showinfo('완료','재고가 성공적으로 수정되었습니다.')
            except ValueError:
                messagebox.showerror("오류", "수량은 숫자만 입력 가능합니다.")

        save_btn = ctk.CTkButton(edit_win, text='저장하기', command=save_edit, height=40, font=("Malgun Gothic", 14, "bold"))
        save_btn.pack(pady=20, padx=30, fill="x")

    def dashboard(self):
        if not os.path.exists(self.history_file):
            messagebox.showerror('오류','재고 이력 데이터가 없습니다.')
            return
        try:
            hist = pd.read_excel(self.history_file)
            if '날짜' not in hist.columns or '재고수량' not in hist.columns:
                messagebox.showerror("오류", "이력 데이터 형식이 올바르지 않습니다.")
                return
                
            summary = hist.groupby('날짜')['재고수량'].sum()
            
            plt.figure(figsize=(10, 6))
            summary.plot(kind='line', marker='o', color='#3B82F6', linewidth=2)
            plt.title('전체 재고량 추이', fontsize=15, pad=20)
            plt.xlabel('날짜')
            plt.ylabel('총 재고 합계')
            plt.grid(True, linestyle='--', alpha=0.7)
            plt.tight_layout()
            plt.show()
        except Exception as e:
            messagebox.showerror("오류", f"대시보드를 생성하는 중 오류가 발생했습니다: {e}")

if __name__ == "__main__":
    app = ModernInventoryManager()
    app.mainloop()
