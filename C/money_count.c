#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
	int totalmoney = 127670;
	int p50000 = 0, p10000 = 0, p5000 = 0, p1000 = 0, c500 = 0, c100 = 0, c50 = 0, c10 = 0;

	p50000 = totalmoney / 50000;
	totalmoney = totalmoney % 50000;

	p10000 = totalmoney / 10000;
	totalmoney = totalmoney % 10000;

	p5000 = totalmoney / 5000;
	totalmoney = totalmoney % 5000;

	p1000 = totalmoney / 1000;
	totalmoney = totalmoney % 1000;

	c500 = totalmoney / 500;
	totalmoney = totalmoney % 500;

	c100 = totalmoney / 100;
	totalmoney = totalmoney % 100;

	c50 = totalmoney / 50;
	totalmoney = totalmoney % 50;


	c10 = totalmoney / 10;

	int sum = p50000 + p10000 + p5000 + p1000 + c500 + c100 + c50 + c10;
	printf("가장 작은 우리나라 화폐의 개수 : %d", sum);

	return 0;
}

