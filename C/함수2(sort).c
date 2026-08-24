#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main(void)
{
	int select = menu();

	while (select != 4)
	{
		switch (select)
		{
		case 1:
			printf("\n선택정렬\n");
			break;
		case 2:
			printf("\n버블정렬\n");
			break;
		case 3:
			printf("\n삽입정렬\n");
			break;
		
		}
		select = menu();
	}
}
int menu()
{
	int sel;
	printf("\n=====메뉴 번호를 선택하세요.=====\n");
	printf("===메뉴===\n");
	printf("\n1. 선택정렬\n");
	printf("\n2. 버블정렬\n");
	printf("\n3. 삽입정렬\n");
	printf("\n5. 종료\n");
	scanf("%d", &sel);
	return sel;
}