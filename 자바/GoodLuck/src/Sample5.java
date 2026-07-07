import java.util.ArrayList;
import java.util.Scanner;

public class Sample5 {
	public static void main(String[] args) {
		// 10미만의 자연수에서 3과 5의 배수를 구하면 3, 5, 6, 9이다. 이들의 총합은 23이다.
		// 그렇다면 1000미만의 자연수에서 3과 5의 배수의 총합을 구하라.
		//입력받는 값은 1부터 999까지(1000)미만의 자연수 이다.
		// 출력받는 값은 3의 배수와 5의 배수의 총합이다.
		
		Scanner sc = new Scanner(System.in);
		int n = sc.nextInt();
		int sum = 0;
		ArrayList<Integer> list = new ArrayList();
		
		for(int i = 1; i < n; i++) {
			if (i % 3 == 0 || i % 5 == 0) 
			{
				list.add(i);
                sum += i;
            }
		}
		for(int i = 0; i<list.size(); i++) {
			if(i!=list.size()-1)
			    System.out.print(list.get(i) + "+");
			else
				System.out.print(list.get(i) + "=");
		}
		System.out.println(sum);
		
	}

}
