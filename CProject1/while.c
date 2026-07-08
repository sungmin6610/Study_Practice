#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

//사과 재배 농가에서 중량이 적절한 상품을 선별하고자 한다. 표준 중량은 200g이며 허용 오차는 +-5g이다.
//선별과정을 통과하지 못한 사과는 폐기한다.
//폐기할 사과의 비율을 알려주는 코드를 작성하시오.
//선별기를 통과하는 사과의 개수는 모릅니다.

int main()
{
	int apple = 0; //사과의 중량
	int sum = 0; //입력한 사과의 총량
	int ter = 0; //폐기되는 사과의 수
	
	while(1)
	{
		printf("사과의 중량을 입력하세요: ");
		scanf("%d", &apple);

		if (apple == 0)
		{
			break;
			printf("프로그램을 종료합니다.");
		}

		if (apple < 195 || apple > 205)
		{
			ter ++;
		}

		sum ++;
	}
	
	printf("폐기할 사과의 비율: %d%%", 100*ter/sum);
}
	
