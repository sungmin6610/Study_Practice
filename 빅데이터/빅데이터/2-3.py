import pandas as pd
import numpy as np

np1 = np.array([[1, 2, 3], [4, 5, 6], [7, 8, 9]])
print(np1)
df1 = pd.DataFrame(np1)
print(df1, type(df1))

np2 = df1. to_numpy()
print(np2)