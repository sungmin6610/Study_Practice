#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
	int num;

    scanf("%d", &num);
    printf("정수를 입력하세요. ");

    if (num > 0)
        printf("양수입니다.");
    if (num < 0)
        printf("음수입니다.");
    if (num == 0)
        printf("ZERO 입니다.");

    return 0;
}