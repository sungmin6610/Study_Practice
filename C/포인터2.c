#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

void swap(int* a, int* b, int* c) {
	int temp = 0;

	if (*a < *b)
	{
		temp = *a;
		*a = *b;
		*b = temp;
	}

	if (*b < *c)
	{
		temp = *b;
		*b = *c;
		*c = temp;
	}

	if (*a < *b)
	{
		temp = *b;
		*b = *a;
		*a = temp;
	}
}

int main() {
	int num1, num2, num3;

	printf("첫번째 숫자를 입력하세요.");
	scanf("%d", &num1);
	printf("두번째 숫자를 입력하세요.");
	scanf("%d", &num2);
	printf("세번째 숫자를 입력하세요.");
	scanf("%d", &num3);

	swap(&num1, &num2, &num3);
	printf("크기 순: %d  %d  %d", num1, num2, num3);

}
