import pandas as pd
age = pd.Series([25, 34, 19, 45, 60])
print(age)
print(type(age))

data = ['spring', 'summer', 'fall', 'winter']
season = pd.Series(data)
print(data, type(data))
print(season, type(season))
print(season.iloc[2])