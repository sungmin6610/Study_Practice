#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
	int n;
label:
	printf("정수를 입력하세요.\n");
	scanf("%d", &n);
	if (n == 0) goto end;
	if (n % 2)
		printf("홀수입니다.");
	else
		printf("짝수입니다.");

	goto label;
end:;
}