#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
	int a, b, c, d;
	printf("각각의 탁구공 무게를 입력하세요: ");
	scanf("%d""%d""%d""%d", &a, &b, &c, &d);

	if (a == b && a == c)
		printf("탁구공 d가 무게가 다름");

	else if (a == b && a == d)
		printf("탁구공 c가 무게가 다름");

	else if (a == c && a == d)
		printf("탁구공 b가 무게가 다름");
	
	else
		printf("탁구공 a가 무게가 다름");
}