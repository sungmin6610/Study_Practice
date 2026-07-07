import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.TreeMap;

public class Mapplay {
	public static void main(String[] args) {
		HashMap<String, String> map = new HashMap<>();
		map.put("사과", "apple");
		map.put("야구", "baseball");
		map.put("당근", "carrot");
		map.put("문", "door");
		map.put("지구", "earth");
		map.put("얼굴", "face");
		map.put("녹색", "green");
		map.put("집", "home");
		System.out.println(map);
		
		TreeMap<String, String> sortedMap = new TreeMap<>(map);
		System.out.println(sortedMap);
		
		System.out.println(sortedMap);
//		map.remove("당근");
//		System.out.println(map.keySet());
//		System.out.println(map.values());
//		
//		System.out.println(map.get("당근"));
//		System.out.println(map.get("당"));
//		
//		System.out.println(map.containsKey("당근"));
//		System.out.println(map.containsValue("door"));
		map.clear();
	}

}
