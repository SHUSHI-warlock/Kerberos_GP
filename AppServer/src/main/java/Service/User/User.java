package Service.User;

public class User {
    protected String userId;
    public int userState;
    public  User(User user){
        userId = user.userId;
        userState = user.userState;
    }
    public User(String user )
    {
        this.userId = user;
    }
    public void setUserId(String id){userId =id;}
    public String getUserId() {
        return userId;
    }


}
