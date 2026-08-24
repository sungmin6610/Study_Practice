#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>

int nums[6] = [6, 8, 2, 9, 4, 7];
int length = sizeof(nums) / sizeof(int);
void SelectionSort();

int main(void)
{
	printf("Original data: ");
	for (int i = 0; i < length; i++) printf("%d", nums[i]);
	printf("\n============================================\n");
    SelectionSort(nums);
	printf("\n\n");
	return 0;
}

//void SelectionSort()
//{
//	int i, j, temp;
//	for (i = 0; i < length - 1; i++)
//	{
//		for (j = i + 1; j < length; j++)
//		{
//			if (nums[i] > nums[j])
//			{
//				temp = nums[i];
//				nums[i] = nums[j];
//				nums[j] = temp;
//			}
//		}
//	}
//	for (i = 0; i < length; i++)
//	{
//		printf("%2d", nums[i]);
//	}
//	print("\n");
//}

void SelectionSort()
{
	int i, j, temp, min_index;
	for (i = 0; i < length - 1; i++)
	{
      min_index = i;
		for (j = i + 1; j < length; j++)
		{
			if (nums[min_index] > nums[j])
			{
				min_index = j;
			}
		}
      temp = nums[i];
      nums[i] = nums[min_index];
      nums[min_index] = temp;
	}
	for (i = 0; i < length; i++)
	{
		printf("%2d", nums[i]);
	}
	print("\n");
}

