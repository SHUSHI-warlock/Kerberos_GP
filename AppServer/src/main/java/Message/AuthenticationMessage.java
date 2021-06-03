package Message;


import myutil.DESUtil.DESUtils;
import myutil.DESUtil.DesKey;

import static Message.byteManage.subBytes;

public class AuthenticationMessage {
    public String IDc; //客户端ID
    public String ADc; //客户端网络地址
    public String IDtgs; //TGS服务器ID
    public String IDv; //服务器ID
    public String TS; //时间戳
    //public String Key; //加密秘钥（客户端和TGS之间、客户端和服务器V之间)
    public DesKey Key;
    public int Lifetime; //票据有效期
    public Authenticator authenticator; //认证字段(经过加密后的)
    public Ticket ticket; //票据
    //public String ticket1; //加密票据
    public byte[] ticket1=new byte[64];


    public AuthenticationMessage(){
        this.IDc="";
        this.ADc="";
        this.IDtgs="";
        this.IDv="";
        this.TS="";
        this.Key=new DesKey();
        this.ticket=new Ticket();
        this.authenticator=new Authenticator();
    }

    public AuthenticationMessage(String id,String tgs,String ts){
        this.IDc=id;
        this.IDtgs=tgs;
        this.TS=ts;
    }
    public AuthenticationMessage(DesKey key,String tgs,String ts,int lifetime,Ticket ticket){
        this.Key=key;
        this.IDtgs=tgs;
        this.TS=ts;
        this.Lifetime=lifetime;
        this.ticket=ticket;
    }
    public AuthenticationMessage(String id,Ticket ticket,Authenticator au){
        this.IDv=id;
        this.ticket=ticket;
        this.authenticator=au;
    }
    public AuthenticationMessage(DesKey key,String id,String ts,Ticket ticket){
        this.Key=key;
        this.IDv=id;
        this.TS=ts;
        this.ticket=ticket;
    }
    public AuthenticationMessage(Ticket ticket,Authenticator au){
        this.ticket=ticket;
        this.authenticator=au;
    }
    public AuthenticationMessage(String ts){
        this.TS=ts;
    }



    public void CASMessage(byte[] message){ //AS接收到客户端消息

        byte[] MESSAGE=message;
        byte[] idc=subBytes(MESSAGE,0,20);
        String id=new String(idc);
        byte[] tg=subBytes(MESSAGE,20,1);
        String tgs=new String(tg);
        byte[] ts=subBytes(MESSAGE,21,12);
        String Ts=new String(ts);
        this.IDc=id.trim();
        this.IDtgs=tgs;
        this.TS=Ts;
    }



    public void ASCMessage(byte[] message,DesKey Key){  //客户端接收到AS消息
        //对信息进行解密
        DESUtils des = new DESUtils(Key);
        byte[] me=des.Decryption(message);
        //String as=new String(me);
        //System.out.println(as);
        //System.out.println(message.length);

        //byte[] me=mess.getBytes();
        byte[] key=subBytes(me,0,8);
        //String K=new String(key);
        DesKey b=new DesKey(key);
        this.Key=new DesKey();
        this.Key=b;
        byte[] tgs=subBytes(me,8,1);
        String id=new String(tgs);
        this.IDtgs=id;
        //System.out.println(IDtgs);
        byte[] ts=subBytes(me,9,12);
        String T=new String(ts);
        this.TS=T;
        byte[] li=subBytes(me,21,1);
        String L=new String(li);
        this.Lifetime=Integer.parseInt(L);
        byte[] tic=subBytes(me,22,64);
        //String tick=new String(tic);
        ticket1=tic;

    }

    public int CTGSMessage(byte[] message,DesKey TGSKeyAS){ //TGS接收到客户端信息
        byte[] MESSAGE=message;
        byte[] idv=subBytes(MESSAGE,0,1);
        String v=new String(idv);
        this.IDv=v;
        byte[] tic=subBytes(MESSAGE,1,64);
        this.ticket1=tic;
        //String tick=new String(tic);
        this.ticket=new Ticket();
        ticket.ticketDecrypt(tic,TGSKeyAS);
        //this.ticket=new Ticket();
        //this.ticket=ticket;
        //this.Key=new DesKey();
        this.Key=ticket.Key;
        this.IDc=ticket.IDc.trim();
        this.ADc=ticket.ADc;
        this.IDtgs=ticket.ID;
        this.TS=ticket.TS;
        this.Lifetime=ticket.Lifetime;
        byte[] au=subBytes(MESSAGE,65,48);
        String Au=new String(au);
        Authenticator AU=new Authenticator();
        AU.Decrypt(au,Key);
        //AU.Decrypt(Au,this.Key);
        this.authenticator=AU;
        return 0;
    }



    public int TGSCMessage(byte[] message,DesKey sKey){ //客户端接收到TGS消息

        DESUtils des=new DESUtils(sKey);
        byte[] MESSAGE=des.Decryption(message);

        byte[] key = subBytes(MESSAGE, 0, 8);
        //String K = new String(key);
        DesKey a=new DesKey(key);
        this.Key=new DesKey();
        this.Key = a;
        byte[] id = subBytes(MESSAGE, 8, 1);
        String v = new String(id);
        this.IDv = v;
        byte[] ts = subBytes(MESSAGE, 9, 12);
        String t = new String(ts);
        this.TS = t.trim();
        byte[] tic = subBytes(MESSAGE, 21, 64);
        //String tick = new String(tic);
        this.ticket1 = tic;
        return 0;

    }


    public void CVMEssage(byte[] message,DesKey TGSKeyV){ //服务器接收到客户端消息
        byte[] tic=subBytes(message,0,64);
        this.ticket1=tic;
        Ticket ticket=new Ticket();
        ticket.ticketDecrypt(tic,TGSKeyV);
        this.ticket=ticket;
        this.Key=ticket.Key;
        this.IDc=ticket.IDc.trim();
        this.ADc=ticket.ADc;
        this.IDv=ticket.ID;
        this.TS=ticket.TS;
        this.Lifetime=ticket.Lifetime;
        byte[] au=subBytes(message,64,48);
        String Au=new String(au);
        Authenticator AU=new Authenticator();
        AU.Decrypt(au,this.Key);
        this.authenticator=AU;
    }
    public void VCMessage(byte[] message,DesKey Kcv){
        DESUtils des=new DESUtils(Kcv);
        byte[] MESSAGE=des.Decryption(message);
        String t=new String(MESSAGE);
        this.TS=t;
    }

}
