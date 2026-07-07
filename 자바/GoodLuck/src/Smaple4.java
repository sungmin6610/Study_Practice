import java.util.Scanner;

public class Smaple4 {
	
	public static void main(String[] args) {
		Scanner s = new Scanner(System.in);
		int kor = s.nextInt();
		int math = s.nextInt();
		int eng = s.nextInt();
		double avg = (kor + math + eng)/3.0;
		System.out.println("평균 = " + avg);
		
		if(avg >= 90)
			System.out.println("A학점");
		else if(avg >= 80)
			System.out.println("B학점");
		else if(avg >= 70)
			System.out.println("C학점");
		else if(avg >= 60)
			System.out.println("D학점");
		else
			System.out.println("F학점");
		
		switch((int)avg/10) {
		    case 10 : System.out.println("A학점");
		    case 9 : System.out.println("A학점");
		    case 8 : System.out.println("B학점");
		    case 7 : System.out.println("C학점");
		    case 6 : System.out.println("D학점");
		    default : System.out.println("F학점");
		}
	}

}
