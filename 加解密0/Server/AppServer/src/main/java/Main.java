import encryptUtils.DesKey;

import java.util.Arrays;

public class Main {
    public static void main(String[] args) {



        //输入秘钥
        byte[] m = {98,-77,94,2,-39,73,-45,90};
        DesKey a = new DesKey();
        a.GenKey(m);
        System.out.println(Arrays.toString(a.getKeyBytes()));

    }
}
