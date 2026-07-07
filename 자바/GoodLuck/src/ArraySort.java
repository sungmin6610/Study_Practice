
public class ArraySort {
	
	public static void main(String[] args) {
		int[] num= {40, 50, 12, 43, 11, 9, 10, 90, 33, 76};
		
		for(int i = 0; i<num.length-1; i++) {
			int minIndex = i;
			for(int j=i+1; j<num.length; j++)
			{
				if(num[minIndex] > num[j])
					minIndex = j;
			}
			if(i != minIndex) {
				int temp = num[i];
				num[i] = num[minIndex];
				num[minIndex] = temp;	
			}
		}
		for(int n:num)
			System.out.print(n +" ");
		System.out.println();
		
		for(int i = 0; i<num.length/2; i++) {
			int temp = num[i];
			num[i] = num[num.length-1-i];
			num[num.length-1-i] = temp;
		}
		for(int n:num)
			System.out.print(n + " ");
		
	}

}
