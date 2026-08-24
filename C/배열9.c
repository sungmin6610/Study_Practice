#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

//점수가 다음과 같이 저장되어있다.score = {30, 60, 40, 20, 80, 40, 90, 10, 70}
//각 점수 별로 석차를 출력하시오.

int main()
{
	int score[] = { 30, 60, 40, 20, 80, 40, 90, 10, 70 };
	int rank = 1;
	int a = 0;
	

	for (int i = 0; i < sizeof(score)/sizeof(int); i++)
	{
		for (int j = 0; j < sizeof(score) / sizeof(int); j++)
		{
			a = score[j];

			if (score[i] < a)
			{
				rank++;
			}
		}
		printf("score[%d] = %d점 석차는 %d\n", i, score[i], rank);
		rank = 1;
	}

	


}