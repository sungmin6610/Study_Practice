#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
	for (int i = 50; i < 100; i++)
		if(!(i%5))printf("i=%d\n", i);
}