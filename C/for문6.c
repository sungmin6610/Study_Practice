#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
	int a = 0;
	int b = 0;
	int c = 0;

	for (int i = 0; i < 5; i++)
	{
		printf("정수를 입력하세요: ");
		scanf("%d", &a);
		if (a % 2 != 0)
			c = c + 1;
		else
			b = b + 1;
		    
	}
	printf("짝수 개수: %d\n", b);

	printf("홀수 개수: %d", c);
}