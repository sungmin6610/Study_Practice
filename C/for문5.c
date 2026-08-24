#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
	int a;
	
	
	for (int i = 0; i < 5; i++)
	{
		printf("정수를 입력하세요: ");
		scanf("%d", &a);
		if (a % 2)
			printf("%d 는 홀수입니다.\n",a);
	}
}