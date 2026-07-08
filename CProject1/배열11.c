#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>
//5명의 학생의 이름과 점수를 배열에 입력받아서, 학생들의 이름과 석차를 출력하시오.
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

	for (int i = 0; i < 5; i++)
	{
		printf("이름: %s, 석차: %d\n", name[i], rank[i]);
	}
}