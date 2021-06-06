package JDBC;

import myutil.PropUtil;

import java.sql.*;

public class JDBCFacade {
    private Connection conn=null;
    private Statement statement=null;

    private static final PropUtil pp = PropUtil.getInstance();
    private static final String SqlDriver = "com.mysql.cj.jdbc.Driver";
    private static final String SqlUrl = "jdbc:mysql://localhost:3306/as?allowPublicKeyRetrieval=true";
    private static final String SqlName = pp.getValueByKey("SQLUsername");
    private static final String SqlPsw = pp.getValueByKey("SQLPsw");

    public void open(String driver,String jdbcUrl,String userName,String userPwd) {
        try {
            Class.forName(driver).newInstance();
            conn = DriverManager.getConnection(jdbcUrl,userName,userPwd);
            statement = conn.createStatement();
        }
        catch (Exception e) {
            e.printStackTrace();
        }
    }

    public void open() {
        open(SqlDriver,SqlUrl,SqlName,SqlPsw);
    }


    public int executeUpdate(String sql) {
        try {
            return statement.executeUpdate(sql);
        }
        catch (SQLException e) {
            e.printStackTrace();
            return -1;
        }
    }

    public ResultSet executeQuery(String sql) {
        try {
            return statement.executeQuery(sql);
        } catch (SQLException e) {
            e.printStackTrace();
            return null;
        }
    }

    public void close() {
        try {
            conn.close();
            statement.close();
        } catch (SQLException e) {
            e.printStackTrace();
        }
    }

}
