#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>
//int main()
//{
//	int a;
//	int* p = &a;
//	*p = 1;
//	a = 10;
//
//	printf("a: %d", a);
//}
void swap(int* a, int* b) {
	int temp = *a;
	*a = *b;
	*b = temp;
}

int main(){
	int num1 = 10;
	int num2 = 20;

	printf("swap Àü: num1 = %d, num2 = %d\n", num1, num2);
	swap(&num1, &num2);
	printf("swap ÈÄ: num1 = %d, num2 = %d\n", num1, num2);
}