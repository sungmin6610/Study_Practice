#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
	int num1;
	int num2;

	printf("정수를 입력하세요. ");
	scanf("%d", &num1);

	printf("정수를 입력하세요. ");
	scanf("%d", &num2);

		if (num1 > num2)
			printf("%d", num1);
		else
			printf("%d", num2);

		return 0;
}