#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
	int num1;
	int num2;
	int result;

	printf("정수를 입력하세요: ");
	scanf("%d", &num1);

	printf("정수를 입력하세요: ");
	scanf("%d", &num2);

	if (num1 > num2)
		result = num1 - num2;

	if (num2 < num1)
		result = num2 - num1;

	printf("두 수의 차이는 %d입니다.\n", result);

	return 0;
}