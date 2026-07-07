import java.util.Scanner;

//public class GUGUDAN {
//    public static void main(String[] args) {
//        Scanner scanner = new Scanner(System.in);
//        int N = scanner.nextInt();
//
//        for (int i = 1; i <= 9; i++) {
//            System.out.println(N + " * " + i + " = " + (N * i));
//        }
//
//        scanner.close();
//    }
//}  

import java.util.Scanner;

public class GUGUDAN {
    public static void main(String[] args) {
        // 바깥쪽 루프: 1부터 9까지 곱해지는 수 (i++)로 수정
        for (int i = 1; i < 10; i++) { 
            // 안쪽 루프: 2단부터 9단까지 단수 지정
            for (int j = 2; j < 10; j++) { 
                // 단별로 가로 출력을 위해 끝에 탭(\t) 추가
                System.out.print(j + " * " + i + " = " + (i * j) + "\t");
            }
            System.out.println(); // 한 행이 끝나면 줄바꿈
        }
    }
}
