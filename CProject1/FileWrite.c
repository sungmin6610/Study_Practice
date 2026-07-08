#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>
#include <string.h>

typedef struct Student
{
	char name[15];
	int score;
};

int main()
{
	struct Student Student[3];
	FILE* fp = fopen("d:\student_data.txt", "r");

	printf("3명의 이름과 점수를 입력하세요: \n");
	
	for (int i = 0; i < 3; i++)
	{
		fprintf(fp, "%s %d\n", Student[i].name, &Student[i].score);
		printf("%s %d\n", Student[i].name, Student[i].score);
	}
	fclose(fp);
}

