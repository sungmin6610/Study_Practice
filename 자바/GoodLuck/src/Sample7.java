import java.util.Scanner;

public class Sample7 {
	
	public static void main(String[] args) {
		//어떠한 문자열을 입력받았을때 공백을 제외한 단어수와 글자수를 출력하는 코드를 작성해봐
		
		Scanner sc = new Scanner(System.in);
		String str = sc.nextLine();
		int charCnt = 0;
		int wordCnt = 1;
		for(int i = 0; i<str.length(); i++) {
			if(str.charAt(i)!=' ')
				charCnt++;
			else
				wordCnt++;
		}
		System.out.println("글자수: "+charCnt);
		System.out.println("단어수: "+charCnt);
	}

}
