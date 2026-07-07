import java.util.Arrays;
import java.util.HashSet;

public class Sample1 {
	
	public static void main(String[] args) {
		
		HashSet<Integer> s1 = new HashSet<>(Arrays.asList(1, 2, 3, 4, 5, 6));
		HashSet<Integer> s2 = new HashSet<>(Arrays.asList(4, 5, 6, 7, 8, 9));
		
		HashSet<Integer> s3 = new HashSet<>(s1);
		s3.retainAll(s2);
		
		HashSet<Integer> s4 = new HashSet<>(s1);
		s4.retainAll(s2);
		
		HashSet<Integer> s5 = new HashSet<>(s1);
		s5.retainAll(s2);
		
		System.out.println(s1);
		System.out.println(s2);
		System.out.println(s3);
		System.out.println(s4);
		System.out.println(s5);
		
	}

}
