package Message;


import myutil.DESUtil.DESUtils;
import myutil.DESUtil.DesKey;

import java.nio.charset.StandardCharsets;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.Date;

import static Message.byteManage.generateTime;

public class SERVER {
    public String IDv="1";
    public DesKey TGSKeyV; //TGS和V之间的加密秘钥
    public AuthenticationMessage fromClient; //从客户端接收到的消息
    //public String Kcv; //和客户端之间的加密秘钥
    public DesKey Kcv;
    public byte[] Ticket;
    public Ticket ticket;
    public Authenticator authenticator;
    public String cTS; //来自于c的时间戳
    public String ADc;
    public int status=0; //状态码

    public void verify(AuthenticationMessage fromClient){
        this.Ticket=fromClient.ticket1;
        //ticket=new Ticket();
        //ticket.ticketDecrypt(Ticket,TGSKeyV);
        //Kcv=new DesKey();
        this.Kcv= ticket.Key;;
        this.ticket= fromClient.ticket;
        this.authenticator=fromClient.authenticator;
        this.cTS=ticket.TS;

        if(!ticket.ID.equals(this.IDv)){
            status=1;
            return;
        }

        if(!ticket.IDc.equals(authenticator.IDc)){
            status=3;
            return;
        }
        if(!ticket.ADc.equals(authenticator.ADc)){
            status=3;
            return;
        }
        String ts=ticket.TS;
        String hour=ts.substring(0,2);
        int ho=Integer.parseInt(hour);
        int limit=ho+ticket.Lifetime;
        String Ts=generateTime();
        String ts2=Ts.trim();
        String hour2=ts2.substring(0,2);
        int ho2=Integer.parseInt(hour2);
        if(ho2+ticket.Lifetime>limit){ //检查票据有效期
            status=2;
            return;
        }

        status=0;
    }



    public byte[] generateBack(String TS,DesKey Kcv){
        DESUtils des=new DESUtils(Kcv);
        SimpleDateFormat sd = new SimpleDateFormat("HH:mm:ss");
        Calendar cal = Calendar.getInstance();
        Date date= null;
        try {
            date = sd.parse(TS.trim());
        } catch (ParseException e) {
            e.printStackTrace();
        }

        cal.setTime(date);
        cal.add(Calendar.MINUTE,1);
        String result=sd.format(cal.getTime());
        if(result.length()<12){
            for(int i=result.length();i<12;i++){
                result+=" ";
            }
        }
        byte[] ts=result.getBytes(StandardCharsets.UTF_8);
        byte[] fin=des.Encryption(ts);
        return fin;

    }




}
