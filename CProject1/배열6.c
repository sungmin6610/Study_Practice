#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
	int arr[] = { 3, 6, 5, 2, 8, 4, 9, 1, 7 };
	int a = 0;
	int b = 0;

	printf("임의의 수를 입력하세요: ");
	scanf("%d", &a);

	for (int i = 0; i < 9; i++)
	{
		if (arr[i] == a)
		{
			b = -1;
			printf("임의의 수는 %d 번째 배열에 저장되어 있습니다.", i + 1);
		}
	}
	if (b == 0)
		printf("값이 없습니다.");


}