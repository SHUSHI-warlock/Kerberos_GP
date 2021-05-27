package Service;

import java.util.Date;

public class ChatMsg {
    private String UserId;
    private Date Time;
    private String Msg;
    private int Type;

    public void setUserId(String UserId) {
        this.UserId = UserId;
    }
    public String getUserId() {
        return UserId;
    }
    public void setTime(Date Time) {
        this.Time = Time;
    }
    public Date getTime() {
        return Time;
    }
    public void setMsg(String Msg) {
        this.Msg = Msg;
    }
    public String getMsg() {
        return Msg;
    }
    public void setType(int Type) {
        this.Type = Type;
    }
    public int getType() {
        return Type;
    }

}
