package ASserver;

public class serverMain {
    public static void main(String[] args) throws Exception {
        //����server����
        new server().bind(4567);
    }


}
