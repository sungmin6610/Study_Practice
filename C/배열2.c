#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>
int main()
{
	char str[] = "C Programming for the first time";
	int slen = sizeof(str) / sizeof(str[0]) - 1;
	

	for (int i = 0; i < slen; i++)
		printf("%c\n", str[i]);
	
}