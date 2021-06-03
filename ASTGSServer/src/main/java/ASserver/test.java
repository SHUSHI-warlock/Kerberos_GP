package ASserver;

import encryptUtils.DesKey;
import encryptUtils.KeyPair;
import encryptUtils.RSAUtils;

import java.math.BigInteger;
import java.nio.charset.StandardCharsets;
import java.util.Arrays;

public class test {
    public static void main(String[] args) {

        BigInteger temp = BigInteger.valueOf(-10534354);
        byte[] temp2 = temp.toByteArray();
        System.out.println(Arrays.toString(temp2));



    }
}
