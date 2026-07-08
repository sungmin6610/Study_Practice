#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>
#include <string.h>
//이름과 과목 A, B의 점수를 입력받아서 총점이 높은 순서대로 출력하는 코드를 작성하세요.

int main()
{
	char name[5][20];
	int A[5], B[5];
	int total[5];

	int i, j;
	int temp;
	char tempname[20];

	for (i = 0; i < 5; i++)
	{
		printf("%d번째 학생 이름: ", i + 1);
		scanf("%s", name[i]);

		printf("과목 A 성적: ");
		scanf("%d", &A[i]);

		printf("과목 B 성적: ");
		scanf("%d", &B[i]);

		total[i] = A[i] + B[i];
	}

	for (i = 0; i < 4; i++)
	{
		for (j = i + 1; j < 5; j++)
		{
			if (total[i] < total[j])
			{
				temp = total[i];
				total[i] = total[j];
				total[j] = temp;

				temp = A[i];
				A[i] = A[j];
				A[j] = temp;

				temp = B[i];
				B[i] = B[j];
				B[j] = temp;

				strcpy(tempname, name[i]);
				strcpy(name[i], name[j]);
				strcpy(name[j], tempname);
			}
		}
	}

	printf("\n이름\tA\tB\t총점\n");
	for (i = 0; i < 5; i++)
	{
		printf("%s\t%d\t%d\t%3d\n", name[i], A[i], B[i], total[i]);
	}
	return 0;
}