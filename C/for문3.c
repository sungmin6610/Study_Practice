#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
	int count = 0;
	for (int i = 50; i < 100; i++)
	  if(i%3 == 0)
		  count++;
	printf("count=%d\n", count);
}