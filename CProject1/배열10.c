#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>
int main()
{
	char name[3][5];
	for (int i = 0; i < 3; i++)
	{
		printf("이름이 뭐에요?\n");
		scanf("%s", &name[i]);
	}
	for (int i = 0; i < 3; i++)
	{
		printf("%s", name[i]);
	}
}