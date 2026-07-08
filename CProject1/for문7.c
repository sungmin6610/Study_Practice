#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
	int M = 0;
	int N = 0;
	int sum = 0;
	printf("정수 N을 입력하세요: ");
	scanf("%d", &N);
	printf("정수 M을 입력하세요: ");
	scanf("%d", &M);

	if (M > N)
	{
		for (int i = N; i <= M; i++)
		{
			sum = sum + i;
		}
	}
	else
	{
		for (int i = M; i <= N; i++)
		{
			sum = sum + i;
		}
	}
	printf("정수 M부터 N까지의 합: %d",sum);
}