#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
	int money = 0;
	int sum = 0;
	int avg = 0;
    
	for (int i = 1; i <= 10; i++)
	{
		printf("기부금을 입력하세요: ");
		scanf("%d", &money);
		sum = sum + money;
	}

	avg = sum /10 /100 *100;

	printf("기부금은 총 %d원입니다.", sum);
	printf("기부금 평균은 총 %d원입니다.", avg);
}