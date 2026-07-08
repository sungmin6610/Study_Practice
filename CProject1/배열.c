#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int main()
{
	int arr[8];
	int size = sizeof(arr);
	int length = size / sizeof(int);
	printf("arr = %p\n", arr);
	printf("arr = %p\n", &arr[0]);
	printf("arr = %p\n", &arr[1]);
	printf("배열의 크기:%d\r\n배열의 길이:%d", size, length);

	int arr[] = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
	int alen = sizeof(arr) / sizeof(arr[0]);

	char str[] = "C Programming for the first time";
	int slen = sizeof(str) / sizeof(str[0]) - 1;
	printf("slen: %d", slen);
	printf("%s", str);
}