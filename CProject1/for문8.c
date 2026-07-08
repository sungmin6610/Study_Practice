#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
	int N = 0;
	int a = 0;
	printf("정수 N을 입력하세요. ");
	scanf("%d", &N);

	for (int i = 1; i <= 9; i++)
	{
		a = N * i;
		printf("%d * %d = %d\n", N, i, N * i);
	}
}