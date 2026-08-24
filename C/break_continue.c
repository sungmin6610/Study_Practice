#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
	for (int i = 0; i <= 10; i++)
	{
		if (i % 3) continue;
		printf("%d", i);
	}
}