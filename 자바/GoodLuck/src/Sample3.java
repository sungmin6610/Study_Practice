import java.util.ArrayList;
import java.util.Arrays;

public class Sample3 {
	
	public static void main(String[] args) {
		
		String num = "123";
		int n = Integer.parseInt(num);
		int s = n + 1;
		System.out.println(s);
		String str = n + "";
		System.out.println(str);
		
		int n1 = 12345;
		String num1 = String.valueOf(n1);
		String num2 = Integer.toString(n1);
		
		String str1 = "123.234";
		double d = Double.parseDouble(str1);
		double dd = d + 1;
		System.out.println(dd);
		
		double d1 = n1;
		System.out.println(d1);
		
		final int N = 123;
		final ArrayList<String> list = new ArrayList<>(Arrays.asList("aa", "bb"));
		list.add("cc");
		
		System.out.println(list);
	}

}
