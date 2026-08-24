#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
	int arr[] = { 3, 2, 4, 2, 3, 2, 9, 5, 7 };
	int a = 0;
	int count = 0;

	printf("임의의 수를 입력하세요: ");
	scanf("%d", &a);

	for (int i = 0; i <= 9; i++)
	{
		if (arr[i] > a)
		{
			count++;
		}
	}
	printf("임의의 수보다 큰 수의 개수: %d", count);
}