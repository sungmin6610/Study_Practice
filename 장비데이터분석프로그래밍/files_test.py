# glob_practice.py
import glob

# 1) data 폴더의 모든 csv 파일 경로 찾기
files = glob.glob("data/*.csv")

#print("=== data 폴더의 모든 CSV 파일 ===")
#print(files)
#print("파일 개수:", len(files))

# 2) 패턴으로 특정 파일만 찾기: 2026년 3월 로그
files = glob.glob("data/log_2026-03-*.csv")
#print("\n=== 2026년 3월 로그 파일 ===")
#print(files)
#print("3월 로그 파일 개수:", len(files))
# 3) 하위 폴더까지 모두 찾기
files = glob.glob("data/**/*.csv", recursive=True)

#print("\n=== data 폴더와 하위 폴더의 모든 CSV 파일 ===")
#print(files)
#print("전체 CSV 파일 개수:", len(files))


import glob, pandas as pd

files = glob.glob('data/log_2026-03-*.csv')

# 각 파일을 읽어 리스트에 담고 한 번에 합치기
df_list = []
for f in files:
    temp = pd.read_csv(f)
    temp['source'] = f       # 어느 파일에서 왔는지 기록
    df_list.append(temp)

merged = pd.concat(df_list, ignore_index=True)
print(merged.shape)
