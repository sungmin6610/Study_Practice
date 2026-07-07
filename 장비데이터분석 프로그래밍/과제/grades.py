import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
from sklearn.preprocessing import StandardScaler

# ==========================================
# 학생 성적 데이터 1000개 자동 생성
# ==========================================
np.random.seed(42)
n = 1000

df_raw = pd.DataFrame({
    'student_id': ['STU_{:04d}'.format(i) for i in range(1, n + 1)],
    'midterm_score': np.random.normal(70, 10, n).round(1),
    'final_score': np.random.normal(75, 8, n).round(1),
    'attendance_days': np.random.randint(15, 21, n)
})

# 의도적 오류 주입 (결측치, 이상치, 중복 데이터, 자료형 오류)
nan_idx = np.random.choice(df_raw.index, 45, replace=False)
df_raw.loc[nan_idx, 'final_score'] = np.nan
df_raw.loc[df_raw.index[:8], 'midterm_score'] = 999.0
df_raw = pd.concat([df_raw, df_raw.iloc[500:520]], ignore_index=True)
df_raw['attendance_days'] = df_raw['attendance_days'].astype(str)

# 생성된 원본 저장
df_raw.to_csv('messy_grades.csv', index=False)
print("1. messy_grades.csv 생성 완료!")

# 더러운 데이터 다시 읽기
df = pd.read_csv('messy_grades.csv')
print(df.info(),"\n")
print(df.isnull().sum(),"\n")
print(df.duplicated().sum(),"\n")
print(df.describe(),"\n")

# ==========================================
# 전처리 시작
# ==========================================
df = pd.read_csv('messy_grades.csv')

# 1) 자료형 정리
df['attendance_days'] = pd.to_numeric(df['attendance_days'], errors='coerce')

# 2) 중복 데이터 제거
df = df.drop_duplicates()

# 3) 이상치 제거
mid_data = df['midterm_score'].dropna()
Q1 = mid_data.quantile(0.25)
Q3 = mid_data.quantile(0.75)
iqr = Q3 - Q1
df = df[(df['midterm_score'] >= Q1 - 1.5 * iqr) & (df['midterm_score'] <= Q3 + 1.5 * iqr)]

# 4) 결측치 대체 (중앙값 적용)
df['final_score'] = df['final_score'].fillna(df['final_score'].median())
df['attendance_days'] = df['attendance_days'].fillna(df['attendance_days'].median())

# 백업용 데이터 (시각화를 위해 표준화 전 상태 저장)
df_pre_scale = df.copy()

# 5) 표준화 
scaler = StandardScaler()
scale_cols = ['midterm_score', 'final_score', 'attendance_days']
df[scale_cols] = scaler.fit_transform(df[scale_cols])

# 최종 결과 저장
df.to_csv('clean_grades.csv', index=False)
print("2. clean_grades.csv 전처리 및 저장 완료!\n")


# ==========================================
# 이상치 및 전처리 결과 시각화
# ==========================================

# 시각화 1: boxplot (999.0점 이상치 확인)
df_raw.boxplot(column='midterm_score')
plt.title('Midterm Score Boxplot (Before)')
plt.ylabel('Scores')
plt.show()

# 시각화 2: 전처리 전 선 그래프 (999.0점이 위로 솟구침)
df_raw['midterm_score'].plot(title='Midterm Score (Before)', color='orange')
plt.xlabel('Index')
plt.ylabel('Scores')
plt.show()

# 시각화 3: 전처리 후 선 그래프
df_pre_scale['midterm_score'].plot(title='Midterm Score (After IQR)', color='green')
plt.xlabel('Index')
plt.ylabel('Scores')
plt.show()