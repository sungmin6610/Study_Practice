#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

struct student
{
	char name[20];
	int A;
	int B;
	int total;
};

int main()
{
	struct student s[5];
	struct student temp;
	int i, j;

	for (i = 0; i < 5; i++)
	{
		printf("%d번째 학생 이름: ", i + 1);
		scanf("%s", s[i].name);

		printf("과목 A 성적: ");
		scanf("%d", &s[i].A);

		printf("과목 B 성적: ");
		scanf("%d", &s[i].B);

		s[i].total = s[i].A + s[i].B;
	}

	for (i = 0; i < 4; i++)
	{
		for (j = i + 1; j < 5; j++)
		{
			if (s[i].total < s[j].total)
			{
				temp = s[i];
				s[i] = s[j];
				s[j] = temp;
			}
		}
	}
	printf("\n이름\tA\tB\t총점\n");
	for (i = 0; i < 5; i++)
	{
		printf("%s\t%d\t%d\t%3d\n", s[i].name, s[i].A, s[i].B, s[i].total);
	}
}