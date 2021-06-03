package Message;

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;

public class byteManage {
    public static int byteToInt(byte b){
        //System.out.println("byte 是:"+b);
        int x = b & 0xff;
        //System.out.println("int 是:"+x);
        return x;
    }

    public static byte[] subBytes(byte[] src, int begin, int count) {
        byte[] bs = new byte[count];
        System.arraycopy(src, begin, bs, 0, count);
        return bs;
    }

    public static byte[] concat(byte[] a, byte[] b) {
        byte[] c= new byte[a.length+b.length];
        System.arraycopy(a, 0, c, 0, a.length);
        System.arraycopy(b, 0, c, a.length, b.length);
        return c;
    }

    public static String generateTime(){
        SimpleDateFormat df = new SimpleDateFormat("HH:mm:ss");//设置日期格式
        String date = df.format(new Date());// new Date()为获取当前系统时间，也可使用当前时间戳
        for(int i=0;i<4;i++){
            date+=" ";
        }
        return date;
    }

    public static boolean searchList(ArrayList<String> list,String target){
        boolean result=false;
        for(int i=0;i<list.size();i++){
            if(list.get(i).equals(target)){
                result=true;
            }
        }
        return result;
    }
}
