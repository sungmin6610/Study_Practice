import java.util.Scanner;

public class PlayString {
	public static void main(String[] args) {
//		StringBuilder sb = new StringBuilder();
//		sb.append("Hello");
//		sb.append(" ");
//		sb.append("Jump to JAVA!");
//		
//		System.out.println(sb);
//		
//		sb.insert(0,  "Good");
//		System.out.println(sb);
//		sb.insert(11, "Fighting");
//		System.out.println(sb);
//		sb.delete(5, 10);
//		System.out.println(sb);
//		String s = sb.toString();
		
		Scanner s = new Scanner(System.in);
		int[] arr1 = new int[10];
		for(int i : arr1) 
			System.out.print(i + "\t");
		
		System.out.println();
			
		for(int i = 0; i<arr1.length; i++)
			System.out.print(arr1[i] + "\t");
		System.out.println();
		
		for(int i = 0; i<arr1.length; i++)
			arr1[i] = s.nextInt();
		
		for(int i : arr1)
			System.out.print(i + "\t");
		System.out.println();
		
		int sum = 0;
		double avg;
		
		for(int i : arr1)
			sum+=i;
		
		avg = (double)sum/arr1.length;
		System.out.println("합: " + sum +", "+"평균: "+ avg);
		
		
	}

}
