package myutil;

import java.io.*;
import java.util.Enumeration;
import java.util.Properties;

public class PropUtil {
    //关于Properties类常用的操作
    private static final String filePath = "src/main/resources/config.properties";
    private static PropUtil instance = new PropUtil();

    private Properties pps;

    private PropUtil() {
        pps = new Properties();
        try {
            InputStream in = new BufferedInputStream(new FileInputStream(filePath));
            pps.load(in);
        }
        catch (IOException ex){
            ex.printStackTrace();
        }
    }

    public static PropUtil getInstance() {
        return instance;
    }

    //根据Key读取Value
    public String getValueByKey( String key) {
        return pps.getProperty(key);
    }

    //读取Properties的全部信息
    private static void GetAllProperties(String filePath) throws IOException {
        Properties pps = new Properties();
        InputStream in = new BufferedInputStream(new FileInputStream(filePath));
        pps.load(in);
        Enumeration en = pps.propertyNames(); //得到配置文件的名字

        while (en.hasMoreElements()) {
            String strKey = (String) en.nextElement();
            String strValue = pps.getProperty(strKey);
            System.out.println(strKey + "=" + strValue);
        }

    }

    //写入Properties信息
    private static void WriteProperties(String filePath, String pKey, String pValue) throws IOException {
        Properties pps = new Properties();

        InputStream in = new FileInputStream(filePath);
        //从输入流中读取属性列表（键和元素对）
        pps.load(in);
        //调用 Hashtable 的方法 put。使用 getProperty 方法提供并行性。
        //强制要求为属性的键和值使用字符串。返回值是 Hashtable 调用 put 的结果。
        OutputStream out = new FileOutputStream(filePath);
        pps.setProperty(pKey, pValue);
        //以适合使用 load 方法加载到 Properties 表中的格式，
        //将此 Properties 表中的属性列表（键和元素对）写入输出流
        pps.store(out, "Update " + pKey + " name");
    }

    public static void main(String[] args) throws IOException {
        //String value = GetValueByKey("db.properties", "name");
        //System.out.println(value);
        //GetAllProperties("db.properties");
        //WriteProperties("db.properties", "long", "212");
        PropUtil pp = PropUtil.getInstance();
        String a = pp.getValueByKey("SQLUsername");
        System.out.println(a);
    }
}

