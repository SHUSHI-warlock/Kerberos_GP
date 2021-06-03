package Message;

import encryptUtils.DESUtils;
import encryptUtils.DesKey;
import encryptUtils.KeyPair;
import JDBC.JDBCFacade;
import myutil.PropUtil;

import java.nio.charset.StandardCharsets;
import java.sql.ResultSet;
import java.util.ArrayList;
import java.util.Arrays;

import static Message.byteManage.*;

public class AS {
    //public String sKey; //客户端和TGS服务器之间的秘钥
    public DesKey sKey;
    public DesKey signKey; //和客户端之间注册使用的对称秘钥
    public AuthenticationMessage fromClient; //从客户端接收的数据
    public String IDc; //客户端ID
    public String tgsTicket; //从数据库中提取的tgs服务器票据
    //public String cKey; //AS和客户端之间的加密秘钥
    public DesKey cKey;
    //public String TGSkeyAS; //和TGS之间的加密秘钥
    public DesKey TGSkeyAS;
    public String TGSid; //TGS服务器ID
    public String TS; //时间戳
    public int Lifetime=2; //TGS票据有效期
    public String ADc; //客户端的网络地址

    public int status=1; //状态码

    public KeyPair keyPair;

    ArrayList<String> tgs=new ArrayList<>();
    JDBCFacade jdbc=new JDBCFacade();

    private static PropUtil pp = PropUtil.getInstance();
    private static String SqlName = pp.getValueByKey("SQLUsername");
    private static String SqlPsw = pp.getValueByKey("SQLPsw");


    public AS(){
        this.IDc="";
        this.TGSid="";
        this.TS="";
        String a="duwhdawd";
        TGSkeyAS=new DesKey();
        TGSkeyAS.GenKey(a.getBytes());
        sKey=new DesKey();
        tgs.add("1");

        keyPair=new KeyPair();
        keyPair.GenKey(1024);
    }

    public void generateTGSkey(){ //用于进行客户端和TGS之间的秘钥生成
        sKey=new DesKey();
        DesKey a=new DesKey();
        a.GenKey();
        this.sKey=a;
    }

    public void generateCkey(String password){
        cKey=new DesKey();
        byte[] pass=password.getBytes(StandardCharsets.UTF_8);
        DesKey a=new DesKey();
        a.GenKey(pass);
        cKey=a;
    }

    public byte[] generateTicket(){
        Ticket ticket=new Ticket(sKey,IDc,ADc,TGSid,TS,Lifetime);
        byte[] result=ticket.ticketEncrypt(TGSkeyAS, ticket.Key);
        return result;
    }


    public void verify(AuthenticationMessage fromClient){
        status = 1;
        this.IDc=fromClient.IDc;
        this.TGSid=fromClient.IDtgs;
        //检查时间戳
        String ts=generateTime();
        String Ts=ts.trim();

        String hour=Ts.substring(0,2);
        //int time=Integer.parseInt(hour);
        //hour="";
        //Ts="";
        String TS=fromClient.TS.trim();
        String hour2=TS.substring(0,2);
        //Ts=fromClient.TS;
        //hour=Ts.substring(0,2);
        //int time2=Integer.parseInt(hour);
        //System.out.println(time);
        //System.out.println(time2);

        if(!(hour.equals(hour2))){
            status=3;
            return;
        }
        /*
        if(time>(time2+2)||time<(time2+2)){
            status=3;
            return;
        }

         */

        if(!searchList(tgs,TGSid)){
            status=2;
            return;
        }

        //进入数据库查找对应的信息
        try{
            jdbc.open("com.mysql.cj.jdbc.Driver","jdbc:mysql://localhost:3306/as",
                    SqlName,SqlPsw);
            ResultSet rs =  jdbc.executeQuery("select * from message");
            boolean flag = false;
            String ID="";
            while (rs.next()){
                ID=rs.getString("ID");
                if(ID.equals(IDc)){
                    byte[] passKey=rs.getBytes("Key");
                    String a=new String(passKey);
                    cKey=new DesKey(passKey);
                    flag = true;
                    break;
                }
            }
            if(!flag||!ID.equals(IDc)){
                status=1;
            }
            else {
                status=0;
            }
        }
        catch (Exception e){
            e.printStackTrace();
            status=1;
        }
    }

    public byte[] generateBack(String IDtgs,byte[] ticket,DesKey sKey ,String TS,DesKey cKey){
        byte[] result;
        String mid=IDtgs+TS+String.valueOf(Lifetime);
        byte[] miid=mid.getBytes();
        byte[] miiid=concat(miid,ticket);
        byte[] miiiid=concat(sKey.getKeyBytes(),miiid);

        for(int i=0;i<miiiid.length;i++){

        }
        DESUtils des=new DESUtils(cKey);
        result=des.Encryption(miiiid);
        //System.out.println(Arrays.toString(result));
        return result;
    }




}
