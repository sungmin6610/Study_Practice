#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
	int a, b, c;

	printf("함량을 입력하세요. ");
	scanf("%d""%d""%d", &a, &b, &c);

	if (a == b)
		printf("c");
	else if (a == c)
		printf("b");
	else if (b == c)
		printf("a");

	return 0;
}