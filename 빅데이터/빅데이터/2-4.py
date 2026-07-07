import pandas as pd

age = pd.Series([25, 34, 19, 45])
print(age)
age.index = ['Jhon', 'Jane', 'Tom', 'Luka']
print(age)

print(age.iloc[2])
print(age.loc['Tom'])

score = pd.DataFrame([[85, 96, 40, 95],
                      [73, 69, 45, 80],
                      [78, 50, 60, 90]])
print(score)
score.index = ['John', 'Jane', 'Tom']
score.columns = ['KOR', 'ENG', 'MATH', 'SCI']
print(score)



print(score.iloc[2, 1])
print(score.loc['Tom', 'ENG'])

age = pd.Series([25, 34, 19, 45, 60])
print(age)
age.index = ['Jhon', 'Jane', 'Tom', 'Micle', 'Tom']
print(age)

print(age.iloc[3])
print(age.loc['Tom'])



population = pd.Series([523, 675, 690, 720, 800])
print(population)
population.index = [2021, 2022, 2023, 2024, 2025]
print(population)
population.iloc[2]
population.loc[2023]
population.iloc[2023]