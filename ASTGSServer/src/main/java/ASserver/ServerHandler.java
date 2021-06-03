package ASserver;

import Message.AS;
import Message.AuthenticationMessage;
import Message.MessageInfo;
import com.google.gson.Gson;
import encryptUtils.DESUtils;
import encryptUtils.DesKey;
import encryptUtils.RSAUtils;
import io.netty.channel.ChannelHandlerContext;
import io.netty.channel.ChannelInboundHandlerAdapter;
import messages.Message;
import messages.NettyMessage;
import myutil.PropUtil;
import org.apache.log4j.Logger;
import JDBC.JDBCFacade;

import java.sql.ResultSet;
import java.util.Arrays;

import static Message.byteManage.generateTime;
import static Message.byteManage.subBytes;
import static JDBC.mysqlInsert.insert;


public class ServerHandler extends ChannelInboundHandlerAdapter{

    protected static Logger logger = Logger.getLogger(ServerHandler.class);
    private AS as=new AS();
    private static Gson gson = new Gson();
    //接受client发送的消息

    private static PropUtil pp = PropUtil.getInstance();
    private static String SqlName = pp.getValueByKey("SQLUsername");
    private static String SqlPsw = pp.getValueByKey("SQLPsw");

    private DESUtils des;

    @Override
    public void channelRead(ChannelHandlerContext ctx, Object msg) throws Exception {
        System.out.println("接收到客户端消息");
        //Message message = new Message((String)msg);
        NettyMessage message=(NettyMessage)msg;
        //NettyMessage message=new NettyMessage((String)msg);
        logger.info(String.format("接收到报文Head:[P2P:%d Type:%d State:%d UnCode:%d Length:%d]",
                message.getMessageP2P(),message.getMessageType(),message.getStateCode(),message.getUnEncode(),message.getLength()));



        if(message.getMessageP2P()!=0){
            System.out.println("错误消息，抛弃");
            return;
        }

        switch (message.getMessageType())
        {
            case 0://用户请求验证
                enterAS(ctx, message.bodyToString());
                break;
            case 1://用户请求注册
            {
                NettyMessage mes=new NettyMessage(1,1,0);
                mes.setMessageBody(gson.toJson(as.keyPair.getPk()));
                //String me= gson.toJson(as.keyPair.getPk());
                //System.out.println(me);
                //Message mes=new Message(1,1,0,-1);
                //mes.setMsg(me);
                //mes.setMessageBody(me);
                ctx.writeAndFlush(mes);
                System.out.println("发回公钥");
                break;
            }
            case 2://用户建立连接
            {
                //MessageInfo mes=gson.fromJson(message.getMsg(),MessageInfo.class);
                //byte[] ke=mes.getMessage();
                //byte[] k= RSAUtils.Decryption(as.keyPair.getSk(),ke);
                byte[] k=message.getMessageBody();

                ///测试代码：
                //byte[] k1 = RSAUtils.Encryption(as.keyPair.getPk(), new byte[]{1,-44,1,-99,1,-82,1,14});

                byte[] temp = RSAUtils.Decryption(as.keyPair.getSk(), k);

                //byte[] temp2 = RSAUtils.Decryption(as.keyPair.getSk(), k1);

                as.signKey=new DesKey(temp);

                //as.signKey=key;
                //回复这条消息代表准备就绪
                String ba="connection formed";
                des=new DESUtils(as.signKey);
                byte[] b=des.Encryption(ba.getBytes());

                NettyMessage back=new NettyMessage(1,2,0);
                back.setMessageBody(b);
                ctx.writeAndFlush(back);
                System.out.println("发回建立连接信息");
                break;
            }
            case 3://用户注册信息
            {
                //MessageInfo mes=gson.fromJson(message.getMsg(),MessageInfo.class);
                //byte[] me=mes.getMessage();
                //String s=message.bodyToString();
                byte[] me=message.getMessageBody();

                byte[] MESSAGE=des.Decryption(me);
                byte[] id=subBytes(MESSAGE,0,20);
                String i=new String(id);
                i=i.trim();
                byte[] pass=subBytes(MESSAGE,20,20);
                String pas=new String(pass);
                pas=pas.trim();
                DesKey ke=new DesKey();
                ke.GenKey(pas.getBytes());

                JDBCFacade jdbc=new JDBCFacade();
                jdbc.open("com.mysql.cj.jdbc.Driver","jdbc:mysql://localhost:3306/as",SqlName,SqlPsw);
                ResultSet rs =  jdbc.executeQuery("select * from message");
                int state=0;
                while (rs.next()) {
                    String ID = rs.getString("ID");
                    if(ID.equals(i)){
                        state=1;
                        break;
                    }
                }
                if(state==0){
                    insert(i,ke.getKeyBytes());
                    NettyMessage mes=new NettyMessage(1,3,0);
                    //ctx.writeAndFlush(new Message(1,3,0,-1).toString());
                    ctx.writeAndFlush(mes);
                    logger.info(String.format("注册成功"));
                }
                else {
                    NettyMessage mes=new NettyMessage(1,3,state);
                    //ctx.writeAndFlush(new Message(1,3,state,-1).toString());
                    ctx.writeAndFlush(mes);
                    logger.info(String.format("注册失败"));
                }
                System.out.println("发回注册信息");
                break;
            }
            case 4://用户取消注册
                //cancel(user,message.getMsg());
                logger.info("客户端取消注册");
                break;
            default:
                //
                break;
        }

    }

    //通知处理器最后的channelRead()是当前批处理中的最后一条消息时调用
    @Override
    public void channelReadComplete(ChannelHandlerContext ctx) throws Exception {
        System.out.println("服务端接收数据完毕..");
        ctx.flush();
    }

    //读操作时捕获到异常时调用
    @Override
    public void exceptionCaught(ChannelHandlerContext ctx, Throwable cause) {
        ctx.close();
    }

    //客户端去和服务端连接成功时触发
    @Override
    public void channelActive(ChannelHandlerContext ctx) throws Exception {
        //ctx.writeAndFlush("hello client");
    }


    public void enterAS(ChannelHandlerContext channel, String message){
        //MessageInfo msg=gson.fromJson(message, MessageInfo.class);
        //byte[] mes=msg.getMessage();
        byte[] mes=message.getBytes();
        AuthenticationMessage fromClient=new AuthenticationMessage();
        fromClient.CASMessage(mes);

        logger.info(String.format("用户%s开始进行验证",fromClient.IDc));

        as.verify(fromClient);

        as.ADc="127.0.0.1";
        if(as.ADc.length()<16){
            for(int i=as.ADc.length();i<16;i++){
                as.ADc+=" ";
            }
        }
        if(as.status==0){
            logger.info(String.format("用户%s验证成功",fromClient.IDc));
            as.generateTGSkey();
            String ts=generateTime();
            as.TS=ts;
            if(as.IDc.length()<20){
                for(int i=as.IDc.length();i<20;i++){
                    as.IDc+=" ";
                }
            }
            /*
            System.out.println(as.sKey.getKeyBytes().length);
            System.out.println(as.IDc.length());
            System.out.println(as.ADc.length());
            System.out.println(as.TGSid.length());
            System.out.println(as.TS.length());
            System.out.println(String.valueOf(as.Lifetime).length());

             */
            byte[] ticket=as.generateTicket();
            //System.out.println(ticket.length);
            System.out.println(Arrays.toString(ticket));
            //System.out.println(as.TGSid);
            //System.out.println(Arrays.toString(as.sKey.getKeyBytes()));
            //System.out.println(Arrays.toString(as.cKey.getKeyBytes()));
            DesKey k=new DesKey();
            String pa="123";
            k.GenKey(pa.getBytes());
            //System.out.println(Arrays.toString(as.TGSkeyAS.getKeyBytes()));
            //System.out.println("key:"+Arrays.toString(pa.getBytes()));
            //as.cKey=k;
            //System.out.println(Arrays.toString(k.getKeyBytes()));
            //System.out.println(Arrays.toString(as.cKey.getKeyBytes()));
            byte[] mess=as.generateBack(as.TGSid,ticket,as.sKey,ts,as.cKey);
            //System.out.println(mess.length);
            //System.out.println(Arrays.toString(mess));
            //MessageInfo back=new MessageInfo(1,0,0,-1);
            //back.setMessage(mess);
            NettyMessage back=new NettyMessage(1,0,0);
            back.setMessageBody(mess);
            System.out.println("正在发送消息");
            channel.writeAndFlush(back);

        }
        else{
            NettyMessage back=new NettyMessage(1,0,as.status);
            //channel.writeAndFlush(new Message(1,0,as.status,-1).toString());
            channel.writeAndFlush(back);
            logger.info(String.format("用户%s验证失败",fromClient.IDc));
        }

    }
}
