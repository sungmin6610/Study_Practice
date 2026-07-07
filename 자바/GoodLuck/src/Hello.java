import java.util.Scanner;

public class Hello {

	public static void main(String[] args) {
		
		int a, b;
  		Scanner s = new Scanner(System.in);
		System.out.println("정수를 입력해주세요.");
		a = s.nextInt();
//		b = s.nextInt();
//		System.out.print(a+b);
//		
//		String str = "Hello";
//		String str1 = new String("Java");
//		System.out.println(str.charAt(2));
//		System.out.println(str.substring(1));
//		System.out.println(str.substring(1, 4));
//		System.out.println(str.toUpperCase());
//		System.out.println(str.concat(str1));
//		
//		Integer a1;
//		Float a2;
//		Double a3;
//		Character a4;
//		
//		if(str.equals(str1))
//			System.out.println("같다");
//		else
//			System.out.println("다르다");
		
		int sum = 0;
		for(int i = 1; i <= a; i++)
			sum = sum + i;
		
		System.out.println("1부터 a까지의 합은 " + sum + "입니다.");

	}

}
