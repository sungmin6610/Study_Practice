#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>
//5명의 학생의 이름과 점수를 배열에 입력받아서, 학생들의 이름과 점수를 석차 순서대로 출력하시오.
int main()
{
	char name[5][15];
	int score[5];
	int rank[5];

	for (int i = 0; i < 5; i++)
	{
		printf("%d번째 학생의 이름을 입력해주세요: ", i + 1);
		scanf("%s", name[i]);

		printf("%d번째 학생의 점수를 입력해주세요: ", i + 1);
		scanf("%d", &score[i]);

		rank[i] = 1;
	}

	for (int i = 0; i < 5; i++)
	{
		for (int j = 0; j < 5; j++)
		{
			if (score[i] < score[j])
			{
				rank[i]++;
			}
		}
	}

	for (int i = 1; i < 6; i++)
	{
		for (int j = 0; j < 5; j++)
		{
			if (rank[j] == i)
			{
				printf("이름: %s, 점수: %d, 석차: %d\n", name[j], score[j], rank[j]);
			}
		}
	}
}