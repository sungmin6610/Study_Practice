#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
//스위치 문은 선택이 많을때 씀.
//if문은 참이냐 거짓이냐를 따질때 씀.
	switch (1 + 1)
	{
	case 1: printf("1\n");
		    break;
	case 2: printf("2\n");
		    printf("2A\n");
			
	case 3: printf("3\n");
	    {
		   printf("3A\n");
		   break;
		   printf("3B\n");
	    }
	default: printf("D");
	}
}