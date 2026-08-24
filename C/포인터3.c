#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

//Num_arr = [6, 8, 2, 9, 4, 7]를 선택정렬 알고리즘을 이용하여 오름차순으로 정렬하시오.

#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

void SelectionSort(int arr[], int length);

int main(void)
{
    int Num[6] = { 6, 8, 2, 9, 4, 7 };
    int length = sizeof(Num) / sizeof(int);
    for (int i = 0; i < length; i++)
    {
        printf("%d ", Num[i]);
    }

    SelectionSort(Num, length);

    printf("\n");
}

void SelectionSort(int arr[], int length)
{
    int i, j, temp;
    
    for (i = 0; i < length - 1; i++)
    {
        for (j = i + 1; j < length; j++)
        {
            
            if (arr[i] > arr[j])
            {
                temp = arr[i];
                arr[i] = arr[j];
                arr[j] = temp;
            }
        }
    }

    for (i = 0; i < length; i++)
    {
        printf("%2d", arr[i]);
    }
    printf("\n");
}
