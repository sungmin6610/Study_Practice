import numpy as np

arr1 = np.array([10, 100, 1000])
print(arr1)
print(type(arr1))
print(arr1.dtype)
print(arr1.shape)

arr2 = np.array([[1, 2, 3],[4, 5, 6]])
print(arr2)
print(type(arr2))
print(arr2.shape)

print(arr1)
print(arr2)
print(arr1 + arr2)
print(arr1 - arr2)
print(arr1 * arr2)
print(arr1 / arr2)

arr2.sum()
arr2.sum(axis=0)
arr2.sum(axis=1)