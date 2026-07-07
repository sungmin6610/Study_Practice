import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.Scanner;

public class DBtest {
	public static void main(String[] args) {
		String url = "jdbc:mysql://localhost:3306/library";
		String user = "root";
		String password = "root";
		Connection conn=null;
		try {
			conn = DriverManager.getConnection(url, user, password);
			System.out.println("데이터베이스 연결 성공");
			
			String sql = "select * from students";
			PreparedStatement ps = conn.prepareStatement(sql);
			ResultSet rs = ps.executeQuery();
			System.out.println("번호\t이름\t나이\t학과");
			System.out.println("--------------------------------------------");
			while(rs.next()) {
				int no = rs.getInt("id");
				String name = rs.getString("name");
				int age = rs.getInt("age");
				String major = rs.getString("major");
				System.out.printf("%d\t%s\t%d\t%s\t\n", no, name, age, major);
			}
			Scanner s = new Scanner(System.in);
			String name = s.next();
			int age = s.nextInt();
			String major = s.next();
			
			sql = "insert into students(name, age, major) values(?,?,?)";
			ps = conn.prepareStatement(sql);
			ps.setString(1, name);
			ps.setInt(2,  age);
			ps.setString(3, major);
			ps.executeUpdate();
			System.out.println("학생정보 입력완료");
			
//			Scanner s = new Scanner(System.in);
//			System.out.println("삭제할 id를 입력해주세요.");
//			int delId = s.nextInt();
//			sql = "delete from students where id=?";
//			ps = conn.prepareStatement(sql);
//			ps.setInt(1, delId);
//			ps.executeUpdate();
//			
		}
		catch(Exception e) {
			System.out.println("데이터베이스 연결 실패");
			e.printStackTrace();
		}
		finally{
			if(conn!=null)
				try {
					conn.close();
				}catch(SQLException e) {
					e.printStackTrace();
				}
		}
	}

}
