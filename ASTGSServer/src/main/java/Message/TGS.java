package Message;

import encryptUtils.DESUtils;
import encryptUtils.DesKey;

import java.nio.charset.StandardCharsets;
import java.util.ArrayList;

import static Message.byteManage.*;

public class TGS {
    public AuthenticationMessage fromClient; //从客户端接收到的消息
    public String IDc; //客户端ID
    public String ADc; //客户端网络地址
    public String IDtgs="1"; //自身id

    //public String Kcv; //客户端和服务器之间的加密秘钥
    public DesKey Kcv;
    public String IDv; //服务器ID
    //public String TGSKeyV; //TGS和服务器之间的加密秘钥
    public DesKey TGSKeyV;
    //public String TGSKeyAS; //TGS和AS之间的加密秘钥
    public DesKey TGSKeyAS;
    //public String sKey; //客户端和TGS之间的加密秘钥
    public DesKey sKey;
    public Ticket Vticket; //服务器票据
    public byte[] vTicket;
    public Ticket TGSticket; //TGS票据
    public byte[] tgsTicket;
    public String TS; //时间戳
    public int Lifetime=2; //票据有效期
    public Authenticator au; //客户端发送的认证

    public int status=0; //状态码

    ArrayList<String> V=new ArrayList<>();

    public TGS(){
        this.IDc="";
        this.ADc="";
        this.TS="";
        sKey=new DesKey();
        String a="duwhdawd";
        TGSKeyAS=new DesKey();
        TGSKeyAS.GenKey(a.getBytes());
        String b="goeghirg";
        TGSKeyV=new DesKey();
        TGSKeyV.GenKey(b.getBytes());
        V.add("1");

        ADc="127.0.0.1";
        if(ADc.length()<16){
            for(int i=ADc.length();i<16;i++){
                ADc+=" ";
            }
        }
    }

    public void generateVkey(){ //用于进行客户端和服务器之间的秘钥
        DesKey key=new DesKey();
        key.GenKey();
        Kcv=new DesKey();
        Kcv=key;
    }

    public void verify(AuthenticationMessage fromClient){
        this.IDv=fromClient.IDv;
        this.tgsTicket=fromClient.ticket1;
        this.TGSticket=new Ticket();
        TGSticket=fromClient.ticket;
        this.IDc=fromClient.IDc;

        //TGSticket.ticketDecrypt(tgsTicket,TGSKeyV);
        //TGSticket.ticketDecrypt(fromClient.ticket1,TGSKeyAS);
        this.au=new Authenticator();
        this.au=fromClient.authenticator;
        sKey=new DesKey();
        sKey=fromClient.Key;

        if(!(au.IDc.equals(TGSticket.IDc))){
            status=4;
            return;
        }
        if(!(au.ADc.equals(TGSticket.ADc))){
            status=4;
            return;
        }
        if(!searchList(V,IDv)){ //检查票据是否正确
            status=1;
            return;
        }
        if(!(TGSticket.ID.equals(this.IDtgs))){
            status=2;
            return;
        }
        String ts=TGSticket.TS;
        String hour=ts.substring(0,2);
        int ho=Integer.parseInt(hour);
        int limit=ho+TGSticket.Lifetime;
        String Ts=generateTime();
        String ts2=Ts.trim();
        String hour2=ts2.substring(0,2);
        int ho2=Integer.parseInt(hour2);
        if(ho2+TGSticket.Lifetime>limit){ //检查票据有效期
            status=3;
            return;
        }

        status=0;
    }

    public byte[] generateTicket(){
        Ticket ticket=new Ticket(sKey,IDc,ADc,IDv,TS,Lifetime);
        byte[] tic=ticket.ticketEncrypt(TGSKeyV, ticket.Key);
        return tic;
    }


    public byte[] generateBack(String IDv,byte[] ticket,DesKey sKey,String TS,DesKey Kcv){
        String mid=IDv+TS;
        byte[] miid=mid.getBytes(StandardCharsets.UTF_8);
        byte[] miiid=concat(miid,ticket);
        byte[] miiiid=concat(Kcv.getKeyBytes(),miiid);

        DESUtils des=new DESUtils(sKey);
        byte[] result=des.Encryption(miiiid);
        return result;

    }

}
