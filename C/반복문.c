#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
	int n, i = 0;
	printf("\n정수를 입력하세요.");
	scanf("%d", &n);
label:
	if (n == i) goto end;
	printf("%d\n", ++i);
	goto label;
end:;
}