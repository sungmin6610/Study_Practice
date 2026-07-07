import java.util.Random;
import java.util.Scanner;

public class UPNDOWN {
	public static void main(String[] args) {
		Random r = new Random();
		Scanner s = new Scanner(System.in);
		int com = r.nextInt(100)+1;
//		System.out.println(com);
		int my;
		int count = 0;
		
		while(true)
		{
			System.out.println("정수를 입력하세요.");
			my = s.nextInt();
			count++;
			if(my==com) {
				System.out.println(count+"번 만에 정답입니다.");
				break;
				}
			else if(my<com)			
				System.out.println("숫자가 더 큽니다.");
			else
				System.out.println("숫자가 더 작습니다.");
		}

	}
}
