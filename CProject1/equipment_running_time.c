#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
	int sec;
 
	printf("생산장비의 사용시간을 입력하세요.\n");

	scanf("%d", &sec);

	int hr = sec / 3600;
	int min = (sec % 3600) / 60;

	printf("생산장비의 가동시간: %d 시간 %d 분", hr, min);
}