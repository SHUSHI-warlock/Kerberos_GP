package JDBC;

import java.sql.*;

public class JDBCUtils {

    private static final String connectionURL = "jdbc:mysql://localhost:3306/as?useUnicode=true&characterEncoding=UTF8&useSSL=false";
    private static final String username = "root";
    private static final String password = "lijiahui123";

    //�������ݿ������
    public static Connection getConnection() {
        try {
            Class.forName("com.mysql.cj.jdbc.Driver");
            return   DriverManager.getConnection(connectionURL,username,password);
        } catch (Exception e) {

            e.printStackTrace();
        }
        return null;
    }

    //�ر����ݿ������
    public static void close(ResultSet rs,Statement stmt,Connection con) throws SQLException {
        if(rs!=null)
            rs.close();
        if(stmt!=null)
            stmt.close();
        if(con!=null)
            con.close();
    }
}
