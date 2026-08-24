#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
	int arr[6];
	int a = 0;
	int sum = 0;

	for (int i = 0; i < 5; i++)
	{
		printf("5개의 정수를 입력하세요: ");
		scanf("%d", &arr[i]);
		sum += arr[i];
	}
	arr[5] = sum;

	for (int i = 0; i < 6; i++)
	{
		printf("%d ", arr[i]);
	}
}