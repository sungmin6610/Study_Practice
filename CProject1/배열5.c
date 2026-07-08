#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
	int arr[] = { 3, 6, 4, 2, 8, 4, 9, 1, 7 };
	int max = 0;
	
	for (int i = 0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (arr[i] > max)
			max = arr[i];
	}
	printf("가장 큰 값: %d", max);
}