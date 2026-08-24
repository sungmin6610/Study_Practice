#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
	int age = 1;
	int totnum = 0;
	int a = 0;
	int b = 0;
	int c = 0;
	int d = 0;

	for (;age!=0;)
	{
		printf("도서관 이용자의 나이를 입력해주세요: ");
		scanf("%d", &age);
		switch (age/10)
		{
			case 0:
			case 1: 
			case 2: 
				a++;
				break;
			case 3: 
			case 4: 
				b++;
			case 5:
				c++;
				break;
			default:
				d++;
		}
	}
	totnum = --a + b + c + d;

	printf("0 ~ 29세: %d명\n",a);
	printf("30 ~ 49세: %d명\n",b);
	printf("50 ~ 59세: %d명\n",c);
	printf("60세 이상: %d명\n",d);
	printf("이용자의 총 인원은 %d명입니다.", totnum);
}