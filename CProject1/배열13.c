#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>
//두 정수를 입력받아서 변수 Large에는 큰 수를 Small에는 작은 수를 저장하고 출력하는 코드를 작성하시오.
int main()
{
	int a = 0;
	int b = 0;
	int Large = 0;
	int Small = 0;
	printf("두 정수를 입력하세요: ");
	scanf("%d %d", &a, &b);

	if (a > b)
	{
		Large = a;
		Small = b;
	}
	else
	{
		Large = b;
		Small = a;
	}

	printf("큰 수: %d\n", Large);
	printf("작은 수: %d", Small);

}

//교수님 코드
//int main()
//{
//	int large, small, temp;
//	scanf("%d %d", &large, &small);
//	if (large < small)
//	{
//		temp = small;
//		small = large;
//		large = temp;
//	}
//	printf("large = %d \nsmall = %d", large, small);
//}
