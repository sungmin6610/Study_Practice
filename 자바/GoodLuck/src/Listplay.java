import java.util.ArrayList;
import java.util.Comparator;
import java.util.Scanner;

public class Listplay {
	public static void main(String[] args) {
		ArrayList<Integer> num = new ArrayList<>();
		ArrayList<String> str = new ArrayList<>();
		
		num.add(10);
		num.add(20);
		num.add(0, 100);
//		num.remove(0);
//		num.remove(20);
//		num.remove(Integer.valueOf(20));
		num.set(0, 50);
		num.clear();
		
		str.add("java");
		str.add("Program");
		str.add("!!!");
		str.add(2, "Fighting");
		
		for(String s: str)
			System.out.print(s +" ");
		
		System.out.println();
		
		for(int n : num)
			System.out.print(n + " ");
		
//		System.out.println();
//		System.out.println(num.size());
//		System.out.println(str.size());
		
		ArrayList<Integer> list = new ArrayList<>();
		list.add(70);
		list.add(85);
		list.add(90);
		list.add(60);
		System.out.println("초기 리스트: "+ list);
		
		Scanner sin = new Scanner(System.in);
		while(true) {
			String signal = sin.next();
			if(signal.equals("i"))
			{
				int pos = sin.nextInt();
				int val = sin.nextInt();
				list.add(pos, val);
				System.out.println("삽입 후: "+ list);
			}
			else if(signal.equals("d"))
			{
				int del = sin.nextInt();
				if(list.contains(del))
					list.remove(Integer.valueOf(del));
				System.out.println("삭제 후: "+ list);
			}
			else if(signal.equals("s"))
			{
				int val = sin.nextInt();
				int n = list.indexOf(val);
				if(n != -1)
					System.out.println(n+1+"번째에 있습니다");
				else
					System.out.println("없습니다");
			}
			else if(signal.equals("m")) {
				int pos = sin.nextInt();
				int val = sin.nextInt();
				if(pos < list.size()) {
					list.set(pos, val);
					System.out.println("수정 후: "+ list);
				}
				else
					System.out.println("해당 위치가 존재하지 않습니다");
			}
			else if(signal.equals("id")) {
				int val = sin.nextInt();
				int n = list.indexOf(val);
				if(n==1)
					list.add(val);
				else
					list.remove(n);
				System.out.println("삽입 or 삭제후: "+list);
			}
		
		list.sort(Comparator.naturalOrder());
		System.out.println("정렬 후 : " + list);
		System.out.println("제일 작은 값 : " + list.get(0));
		
		list.sort(Comparator.reverseOrder());
		System.out.println("내림차순 정렬 후: " + list);
		System.out.println("제일 큰 값: "+list.get(0));
	    }
    }
}
