import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import seaborn as sns

df22 = pd.read_csv('D:/bigData/실습/금요일 오후/data/SeoulBikeData.csv')

df22['Date'] = pd.to_datetime(df22['Date'], dayfirst=True)

df22['Month'] = df22['Date'].dt.month

df22['계절'] = pd.cut(
    df22['Month'],
    bins=[0, 2, 5, 8, 12],
    labels=['Spring', 'Summer','Autumn','winter'],
    include_lowest=True
)

corre = df22[
    ['Rented Bike Count',
     'Rainfall',
     'Temperature',
     'Humidity']
].corr()

print(corre)

season_rain = df22.groupby('계절', observed=True)[
    ['Rainfall', 'Rented Bike Count']
].mean()

print("\n계절별 평균 값")
print(season_rain)

fig, axes = plt.subplots(1, 1, figsize=(10, 6))

sns.barplot(
    data=season_rain.reset_index(),
    x='계절',
    y='Rented Bike Count',
    palette='Blues',
    ax=axes
)

axes.set_title('계절별 강수량과 자전거 대여량')
axes.set_xlabel('계절')
axes.set_ylabel('평균 자전거 대여량')

# 값 표시
for p in axes.patches:
    axes.annotate(
        f'{p.get_height():.0f}',
        (p.get_x() + p.get_width()/2., p.get_height()),
        ha='center',
        va='bottom'
    )

plt.show()