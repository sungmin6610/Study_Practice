#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
	int select= menu();

	while (select!=5)
	{
		switch (select)
		{
		case 1:
			add();
			printf("\nµ¡¼À·çÆ¾\n");
			break;
		case 2:
			sub();
			printf("\n»¬¼À·çÆ¾\n");
			break;
		case 3:
			mul();
			printf("\n°ö¼À·çÆ¾\n");
			break;
		case 4:
			div();
			printf("\n³ª´°¼À·çÆ¾\n");
			break;
	    }
		select = menu();
	}
}
menu()
{
	int sel;
	printf("\n=====¸Þ´º ¹øÈ£¸¦ ¼±ÅÃÇÏ¼¼¿ä.=====\n");
	printf("===¸Þ´º===\n");
	printf("\n1. µ¡¼À\n");
	printf("\n2. »¬¼À\n");
	printf("\n3. °ö¼À\n");
	printf("\n4. ³ª´°¼À\n");
	printf("\n5. Á¾·á\n");
	scanf("%d", &sel);
	return sel;
}