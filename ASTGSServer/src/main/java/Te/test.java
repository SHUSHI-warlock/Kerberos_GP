package Te;

import messages.NettyMessage;

import java.nio.charset.StandardCharsets;
import java.util.Arrays;

public class test {
    public static void main(String[] args) {
        String a="1234567";
        byte[] aa=a.getBytes(StandardCharsets.UTF_8);
        System.out.println(Arrays.toString(aa));

        NettyMessage message=new NettyMessage(0,0,0);
        message.setMessageBody(aa);

        NettyMessage mes=(NettyMessage) message;
        String aaa=mes.bodyToString();
        System.out.println(aaa);
        System.out.println(Arrays.toString(aaa.getBytes()));
    }
}
