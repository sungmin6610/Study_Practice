import java.util.Scanner;

public class Sample6 {
	public static void main(String[] args) {
		
	Scanner sc = new Scanner(System.in);
	
	int n = sc.nextInt();
	System.out.print(n);
	int count = 0;
	
	while(true) {
		if(n==0) break;
		n=n/10;
		count++;
	}
  }
}
