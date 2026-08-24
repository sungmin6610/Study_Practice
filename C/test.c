#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
	int a;
	int b;
	printf("두 정수를 공백으로 구분하여 입력하세요.\n");
	scanf("%d %d", &a, &b);

	int c1 = a * b;
	int c2 = a + b;
	int c3 = a - b;
	printf("곱: %d\n합: %d\n차: %d", c1, c2, c3);
}