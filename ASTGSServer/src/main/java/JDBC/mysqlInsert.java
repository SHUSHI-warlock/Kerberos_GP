package JDBC;

import encryptUtils.DesKey;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;

public class mysqlInsert {

    public static void insert(String ID,byte[] Key) throws SQLException {
        //注册驱动    使用驱动连接数据库
        Connection con = null;
        PreparedStatement stmt = null;
        ResultSet rs = null;
        try {
            con = JDBCUtils.getConnection();
            String sql = "insert into message(`ID`,`Key`) values(?,?)";
            stmt = con.prepareStatement(sql);
            stmt.setString(1, ID);
            stmt.setBytes(2,Key);
            int result =stmt.executeUpdate();// 返回值代表收到影响的行数
            System.out.println("插入成功"+ID);
        } catch (Exception e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
        }finally {
            JDBCUtils.close(rs, stmt, con);
        }
    }

    public static void main(String[] args) {
        DesKey key=new DesKey();
        String a="123";
        key.GenKey(a.getBytes());
        try {
            mysqlInsert.insert("123",key.getKeyBytes());
        } catch (SQLException throwables) {
            throwables.printStackTrace();
        }
    }
}