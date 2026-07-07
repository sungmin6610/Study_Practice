import java.util.Scanner;

public class Arrayplay {
    public static void main(String[] args) {

        Scanner sin = new Scanner(System.in);

        System.out.print("몇 개의 성적을 입력하시나요? ");
        int n = sin.nextInt();

        int[] score = new int[n];

        int sum = 0;
        int max = 0;
        int min = 100;

        // 성적 입력
        for (int i = 0; i < n; i++) {
            score[i] = sin.nextInt();

            sum += score[i];

            if (max < score[i])
                max = score[i];

            if (min > score[i])
                min = score[i];
        }

        double avg = (double) sum / n;

        System.out.println("========== 수학 과목 성적 통계 ==========");

        // for-each 문 사용
        for (int s : score) {
            System.out.print(s + " ");
        }
        System.out.println();

        System.out.printf("** 총점: %d\n", sum);
        System.out.printf("** 평균: %.2f\n", avg);
        System.out.printf("** 최고점: %d\n", max);
        System.out.printf("** 최저점: %d\n", min);
        System.out.printf("======================================");
    }
}