#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
    int a;

    printf("정수를 입력하세요: ");
    scanf("%d", &a);

    if (a > 0)
        printf("입력한 숫자는 양수입니다.\n");

    if (a == 0)
        printf("입력한 숫자는 ZERO입니다.\n");

    if (a < 0)
        printf("입력한 숫자는 음수입니다.\n");

}