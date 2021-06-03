package Message;

import encryptUtils.DESUtils;
import encryptUtils.DesKey;

import java.nio.charset.StandardCharsets;

public class SERVER {
    public DesKey TGSKeyV; //TGS和V之间的加密秘钥
    public AuthenticationMessage fromClient; //从客户端接收到的消息
    //public String Kcv; //和客户端之间的加密秘钥
    public DesKey Kcv;
    public byte[] Ticket;
    public Ticket ticket;
    public Authenticator authenticator;
    public String cTS; //来自于c的时间戳
    public int status=0; //状态码

    public void verify(AuthenticationMessage fromClient, DesKey TGSKeyV){
        this.Ticket=fromClient.ticket1;
        ticket=new Ticket();
        ticket.ticketDecrypt(Ticket,TGSKeyV);
        Kcv=new DesKey();
        this.Kcv=fromClient.Key;
        this.authenticator=fromClient.authenticator;
        this.cTS=ticket.TS;

    }


    public byte[] generateBack(String TS,DesKey Kcv){
        DESUtils des=new DESUtils(Kcv);
        byte[] ts=TS.getBytes(StandardCharsets.UTF_8);
        byte[] result=des.Encryption(ts);
        return result;

    }



}
