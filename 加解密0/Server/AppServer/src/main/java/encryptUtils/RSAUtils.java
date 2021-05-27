package encryptUtils;

import java.math.BigInteger;
import java.nio.charset.StandardCharsets;
import java.util.Arrays;
import java.util.Base64;

public class RSAUtils {

    public static void main(String[] args) {
        KeyPair keyPair = new KeyPair();
        keyPair.GenKey(1024);
        byte[] m = {98,-77,94,2,-39,73,-45,91};
        System.out.println("明文："+ Arrays.toString(m));
        byte[] c = Encryption(keyPair.getPk(),m);
        System.out.println("密文："+Arrays.toString(c));
        byte[] mm = Decryption(keyPair.getSk(),c);
        System.out.println("解密后结果:"+Arrays.toString(mm));

    }

    /**
     * 最大加密128字节
     * @param pk
     * @param M
     * @return
     */
    public static byte[] Encryption(PublicKey pk, byte[] M)
    {
        BigInteger m = new BigInteger(M);
        BigInteger c = m.modPow(pk.getE(),pk.getN());
        return c.toByteArray();
    }

    public static byte[] Decryption(PrivateKey sk, byte[] C)
    {
        BigInteger c = new BigInteger(C);
        BigInteger m = c.modPow(sk.getD(),sk.getN());
        return m.toByteArray();
    }
}
