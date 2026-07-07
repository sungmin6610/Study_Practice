import java.util.Arrays;
import java.util.HashSet;
import java.util.Iterator;

public class Sample {

	public static void main(String[] args) {
		// TODO Auto-generated method stub
		HashSet<String> set1 = new HashSet<>();
		set1.add("apple");
		set1.add("banana");
		
		HashSet<String> set2 = new HashSet<> (Arrays.asList("go", "to", "home"));
		
		System.out.println(set1);
		System.out.println(set2);
		
		if(set1.contains("banana"))
			System.out.println("있다");
		else
			System.out.println("없다");
		
		set2.remove("to");
		
		System.out.println(set2);
		
		for(String val: set2)
			System.out.println(val);
		
		Iterator<String> it = set2.iterator();
		while(it.hasNext())
			System.out.println(it.next() + "\t");
		
		System.out.println(set1.size());
	}

}
