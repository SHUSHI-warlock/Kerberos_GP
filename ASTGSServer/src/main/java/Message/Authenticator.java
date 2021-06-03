package Message;
import encryptUtils.DESUtils;
import encryptUtils.DesKey;

import static Message.byteManage.subBytes;

public class Authenticator {
    public String IDc; //客户端ID
    public String ADc; //客户端网络地址
    public String TS; //时间戳

    Authenticator(){
        this.IDc="";
        this.ADc="";
        this.TS="";
    }

    Authenticator(String id,String ad,String ts) {
        this.IDc = id;
        this.ADc = ad;
        this.TS = ts;
    }

    public byte[] Encrypt(DesKey Key){
        String mid=IDc+ADc+TS;
        byte[] message=mid.getBytes();
        DESUtils des=new DESUtils(Key);
        byte[] result=des.Encryption(message);
        //调用DES加密函数对mid进行加密
        return result;
    }

    public void Decrypt(byte[] message, DesKey Key){

        DESUtils des=new DESUtils(Key);
        byte[] Message=des.Decryption(message);//首先调用DES解密函数对message进行解密

        byte[] id=subBytes(Message,0,20);
        String ID=new String(id);
        this.IDc=ID;
        byte[] ad=subBytes(Message,20,16);
        String AD=new String(ad);
        this.ADc=AD;
        byte[] ts=subBytes(Message,36,12);
        String T=new String(ts);
        this.TS=T;

    }

}
