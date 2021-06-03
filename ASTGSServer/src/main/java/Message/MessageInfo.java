package Message;

public class MessageInfo extends Message{
    public byte[] message;

    public MessageInfo(int messageP2P,int messageType,int stateCode,int UnDefineCode){
        super(messageP2P,messageType,stateCode,UnDefineCode);
        //message=new byte[100];
    }

    public void setMessage(byte[] messgae){
        this.message=messgae;
    }
    public byte[] getMessage(){return message;}
}
