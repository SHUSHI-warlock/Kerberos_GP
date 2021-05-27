package Service.User;

public class Player extends User {

    public Player(User user){
        super(user);
    }
    public Player(String userId)
    {
        super(userId);
    }
    public int pos;
    public int steps;
    public int roomId;


}
