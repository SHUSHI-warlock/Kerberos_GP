package JDBC;

import myutil.PropUtil;

import java.sql.*;

public class JDBCUtils {

    private static final PropUtil pp = PropUtil.getInstance();
    private static final String SqlDriver = "com.mysql.cj.jdbc.Driver";
    private static final String SqlUrl = "jdbc:mysql://localhost:3306/as?allowPublicKeyRetrieval=true";
    private static final String SqlName = pp.getValueByKey("SQLUsername");
    private static final String SqlPsw = pp.getValueByKey("SQLPsw");

    //创建数据库的连接
    public static Connection getConnection() {
        try {
            Class.forName(SqlDriver);
            return   DriverManager.getConnection(SqlUrl,SqlName,SqlPsw);
        } catch (Exception e) {

            e.printStackTrace();
        }
        return null;
    }

    //关闭数据库的连接
    public static void close(ResultSet rs,Statement stmt,Connection con) throws SQLException {
        if(rs!=null)
            rs.close();
        if(stmt!=null)
            stmt.close();
        if(con!=null)
            con.close();
    }
}
