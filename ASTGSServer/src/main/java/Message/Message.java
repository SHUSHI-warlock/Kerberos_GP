package Message;
import com.google.gson.Gson;
import net.sf.json.JSONObject;

public class Message {
    int messageP2P;
    int messageType;
    int stateCode;
    int UnDefineCode;
    int length;

    String msg;

    public Message(String msg) {
        this.msg = msg;
        PraseHead();
    }

    public Message(int messageP2P,
                   int messageType,
                   int stateCode,
                   int UnDefineCode) {
        this.messageP2P = messageP2P;
        this.messageType = messageType;
        this.stateCode = stateCode;
        this.UnDefineCode = UnDefineCode;
        this.length = 0;
    }

    @Override
    public String toString() {
        Gson gson = new Gson();
        return gson.toJson(this);
    }

    public void PraseHead() {
        try {
            JSONObject jsonObject = JSONObject.fromObject(msg);
            messageP2P = jsonObject.getInt("messageP2P");
            messageType = jsonObject.getInt("messageType");
            stateCode = jsonObject.getInt("stateCode");
            UnDefineCode = jsonObject.getInt("UnDefineCode");
            length = jsonObject.getInt("length");
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public void setStateCode(int stateCode) {
        this.stateCode = stateCode;
    }

    public int getMessageP2P() {
        return messageP2P;
    }

    public int getLength() {
        return length;
    }

    public int getMessageType() {
        return messageType;
    }

    public int getStateCode() {
        return stateCode;
    }

    public int getUnDefineCode() {
        return UnDefineCode;
    }

    public String getMsg() {
        return msg;
    }

    public Message praseBody() {
        throw new UnsupportedOperationException();
    }

    public void setMsg(String a){msg=a;}
}
