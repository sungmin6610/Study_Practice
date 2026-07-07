movie_rank = ["닥터 스트레인지", "스플릿", "럭키"]
print(movie_rank)
movie_rank.append("배트맨")
print(movie_rank)
movie_rank.append("슈퍼맨")
print(movie_rank)
del movie_rank[3]
print(movie_rank)

num_list = [1, 2, 3, 4, 5]
print(num_list)
num_list.reverse()
print(num_list)
print(max(num_list), min(num_list), sum(num_list)/len(num_list))

lang1 = ["C", "C++", "JAVA"]
lang2 = ["Python", "Go", "C#"]
langs = lang1 + lang2
print(langs)

price = ['20180728', 100, 130, 140, 150, 160, 170]
print(price[1:])

nums = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]
print(num[::2])
print(num)
print(" ".join(interest))

nums = [11, 2, 3, 4, 5, 6, 7, 8, 9, 10]
nums.sort(reverse = True)
print(nums)