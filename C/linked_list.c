#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>
#include <malloc.h>

//노드 구조체
typedef struct Node
{
	int data;
	struct Node* next;
}node;

typedef struct
{
	node* point;
}head_point;

head_point* create_head(void);
void node_Append(head_point* head);
void node_Search(head_point* head);
void node_insert(head_point* head);
void node_delete(head_point* head);
void node_print(head_point* head);

int num = 0;
int in_data = 1;

int main() 
{
	head_point* head;
	head = create_head();
	while (num != 6) 
	{
		printf("\n1.노드 추가 2.데이터 검색 3.노드 삽입 4.노드 삭제 5.출력 6.종료");
		scanf("%d", &num);
		printf("\n\n");
		switch (num) 
		{
		 case 1:
			 node_Append(head); break;
		 case 2:
			 node_Search(head); break;
		 case 3:
			 node_insert(head); break;
		 case 4:
			 node_delete(head); break;
		 case 5:
			 node_pirnt(head); break;
		 case 6:
			 printf("종료\n"); break;
		}
	}
}

head_point* create_head(void)
{
	head_point* head;
	head = (head_point*)malloc(sizeof(head_point));
	head->point = NULL;
	return head;
}

void node_Append(head_point* head)
{
	node* newNode;
	node* temp;

	printf("추가 데이터를 입력하세요(0:추가 종료): ");
	scanf_s("%d", &in_data);
	while (in_data != 0)
	{
		newNode = (node*)malloc(sizeof(node));
		newNode->data = in_data;
		newNode->next = NULL;

		if (head->point == NULL)
		{
			head->point = newNode;
		}
		else
		{
			temp = head->point;
			while (temp->next != NULL)
				temp = temp->next;
			temp->next = newNode;
		}
		printf("추가 데이터를 입력하세요(0:추가 종료): ");
		scanf_s("%d", &in_data);
	}
	return;
}

void node_Search(head_point* head)
{
	int searchData;
	node* temp = head->point;

	printf("검색할 데이터 입력 : ");
	scanf("%d", &searchData);

	while (temp != NULL)
	{
		if (temp->data == searchData)
		{
			printf("데이터 %d를 찾았습니다.\n", searchData);
			printf("주소 : %p\n", temp);
			return;
		}
		temp = temp->next;
	}

	printf("데이터가 존재하지 않습니다.\n");
}

void node_insert(head_point* head)
{
	int findData;
	int newData;

	node* temp = head->point;
	node* newNode;

	printf("어느 데이터 뒤에 삽입할까요? : ");
	scanf("%d", &findData);

	while (temp != NULL)
	{
		if (temp->data == findData)
		{
			printf("삽입할 데이터 입력 : ");
			scanf("%d", &newData);

			newNode = (node*)malloc(sizeof(node));

			newNode->data = newData;
			newNode->next = temp->next;

			temp->next = newNode;

			printf("%d 뒤에 %d 삽입 완료\n", findData, newData);
			return;
		}

		temp = temp->next;
	}

	printf("기준 데이터가 없습니다.\n");
}

void node_delete(head_point* head)
{
	int delData;

	node* temp = head->point;
	node* prev = NULL;

	printf("삭제할 데이터 입력 : ");
	scanf("%d", &delData);

	if (temp == NULL)
	{
		printf("리스트가 비어 있습니다.\n");
		return;
	}

	/* 첫 번째 노드 삭제 */
	if (temp->data == delData)
	{
		head->point = temp->next;
		free(temp);

		printf("%d 삭제 완료\n", delData);
		return;
	}

	while (temp != NULL)
	{
		if (temp->data == delData)
		{
			prev->next = temp->next;
			free(temp);

			printf("%d 삭제 완료\n", delData);
			return;
		}

		prev = temp;
		temp = temp->next;
	}

	printf("삭제할 데이터가 없습니다.\n");
}

void node_print(head_point* head)
{
	node* temp = head->point;

	if (temp == NULL)
	{
		printf("비어 있음.");
		return;
	}

	while(temp != NULL)
	{
		printf("%p	%d	%p\n", temp, temp->data, temp->next);
		temp = temp->next;
	}
}
