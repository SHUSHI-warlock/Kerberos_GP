package TGSserver;


public class serverMain {
    public static void main(String[] args) throws Exception {
        //����server����
        new serverTGS().bind(2333);
    }


}
