#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
	int team = 0;
	int saleprice = 0;
	int sumprice = 0;
	int sumteam = 0;

	for (; team != 0 ;)
	{
		printf("팀당 인원: ");
		scanf("%d", &team);

		if (team == 0)
		 break;

		printf("팀당 결제비용 : ");
		scanf("%d", &saleprice);

		sumprice += saleprice;
		sumteam += team;
	}
	printf("하루 총 매출액: %d", sumprice);
	printf("\n하루 총 고객수: %d", sumteam);
}