import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import seaborn as sns


df22 = pd.read_csv('D:/bigData/실습/금요일 오후/data/SeoulBikeData.csv')

df24 = df22[df22['Rainfall'] == 'Yes' ].copy()

df22['체감온도'] = (df22['Temperature'] - (0.55 - 0.0055 * df22['Humidity']) * (df22['Temperature'] - 14.5))
df22['체감온도']

bins = [-0.1, 0, 5, df22['Rainfall'].max()]
labels = ['비 없음', '약한 비', '폭우']

df22['강수등급'] = pd.cut(df22['Rainfall'],bins=bins,labels=labels)


correlation = df22[
    ['Rented Bike Count',
     'Rainfall',
     '체감온도',
     'Temperature']
].corr()

print(correlation['Rented Bike Count'])

rain_summary = (df22.groupby('강수등급', observed=True)['Rented Bike Count'].mean())
print(rain_summary)

mean_rent = df22['Rented Bike Count'].mean()
outliers = df22[(df22['Rainfall'] > 5) &(df22['Rented Bike Count'] > mean_rent)]
outliers[ ['Date', 'Hour', 'Rainfall', 'Rented Bike Count']].head()

fig, axes = plt.subplots(1, 3, figsize=(20, 6))

#그래프1

sns.barplot(
    data=df22,
    
    x='강수등급',
    y='Rented Bike Count',
    estimator=np.mean,
    palette='Blues',
    ax=axes[0]
)

axes[0].set_title('강수 등급별 평균 자전거 대여량', fontsize=14)
axes[0].set_xlabel('강수 등급')
axes[0].set_ylabel('평균 대여량')


for p in axes[0].patches:
    axes[0].annotate(
        f'{p.get_height():.0f}',
        (p.get_x() + p.get_width()/2., p.get_height()),
        ha='center',
        va='bottom'
    )
    
    
#그래프2

sns.regplot(
    data=df22,
    x='체감온도',
    y='Rented Bike Count',
    scatter_kws={'alpha':0.15, 'color':'green'},
    line_kws={'color':'red'},
    ax=axes[1]
)

axes[1].set_title('체감온도와 자전거 대여량 관계', fontsize=14)
axes[1].set_xlabel('체감온도')
axes[1].set_ylabel('자전거 대여량')

#그래프3

sns.scatterplot(
    data=df22,
    x='Rainfall',
    y='Rented Bike Count',
    alpha=0.25,
    color='purple',
    ax=axes[2]
)

# 폭우 기준선
axes[2].axvline(
    x=5,
    color='red',
    linestyle='--',
    linewidth=2,
    label='폭우 기준선 (5mm)'
)

axes[2].set_title('강수량에 따른 자전거 대여량 변화', fontsize=14)
axes[2].set_xlabel('강수량(mm)')
axes[2].set_ylabel('자전거 대여량')

axes[2].legend()


# 전체 제목
fig.suptitle(
    '서울 공공자전거 날씨 영향 분석',
    fontsize=18,
    fontweight='bold'
)

plt.tight_layout()
plt.show()