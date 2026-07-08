#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>
// 500마리의 돼지를 방목하는 양돈장이 있다. 오늘은 총 중량 5000kg을 출하하는 날이다. 돼지의 무게는
//한마리씩 통과하는 길목에 계근대를 설치하여 측정을 한다. 출하 대상의 돼지는 60kg에서 80kg까지이다. 
//오늘 출하하는 돼지의 마릿수와 총 중량을 출력하시오. 만일 출하 목표량에 미치지못할경우 출하가 가능한 
// 돼지의 마릿수와 총 중량을 출력하시오. 조건)비교문 사용금지, while문 사용, 출하중량을 만족하지 않을수도 있음.
int main()
{
	int pignum = 0;
	int pigweight = 1;
	int totweight = 0;

	while (totweight <= 5000 && pigweight != 0)
	{
		printf("돼지의 무게를 입력하세요: ");
		scanf("%d", &pigweight);

		while (pigweight >= 60 && pigweight <= 80)
		{
			totweight += pigweight;
			pignum += 1;
			break;
		}
		
		printf("현재 출하된 돼지 수: %d마리", pignum);
		printf("\n현재 출하된 돼지의 무게: %dkg\n\n", totweight);
	}
	printf("출하가 종료되었습니다.");
	printf("출하한 돼지의 총 마리수: %d마리\n", pignum);
	printf("출하한 돼지의 총 중량: %dkg", totweight);
}