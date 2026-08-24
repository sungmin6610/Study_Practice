import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import seaborn as sns

# 1. 데이터 로드 및 전처리

df22 = pd.read_csv('D:/bigData/실습/금요일 오후/data/SeoulBikeData.csv')


# 운영일만 사용
df24 = df22[df22['Snowfall'] > 0 ].copy()

bins = [-0.1,3,5,8, df22['Snowfall'].max()]
labels = ['없음','도깨비눈','진눈깨비','폭설']
df24['폭설등급'] = pd.cut(df24['Snowfall'],bins=bins,labels=labels)


corre = df24[
    ['Rented Bike Count',
     'Snowfall',
     'Wind speed',
     'Temperature']
].corr()

corre

"\n상관계수 분석--"
snow_sum = (df24.groupby('폭설등급',observed=True)['Rented Bike Count'].mean())


df22['Date'] = pd.to_datetime(df22['Date'], dayfirst=True)

mean_snow = df22['Rented Bike Count'].mean()

outliers = df22[(df22['Snowfall']>8)&(df22['Rented Bike Count']>mean_snow)]

outliers [['Date','Hour','Snowfall','Rented Bike Count']].head()

fig,axes = plt.subplots(1,1,figsize = (10,8))

sns.barplot(
    data=df24,
    
    x='폭설등급',
    y='Rented Bike Count',
    estimator=np.mean,
    palette='pink',
    ax=axes
)

axes.set_title('폭설 평균 자전거 대여량', fontsize=14)
axes.set_xlabel('폭설 등급')
axes.set_ylabel('평균 대여량')

# 값 표시
for p in axes.patches:
    axes.annotate(
        f'{p.get_height():.0f}',
        (p.get_x() + p.get_width()/2., p.get_height()),
        ha='center',
        va='bottom'
    )
    
plt.show()