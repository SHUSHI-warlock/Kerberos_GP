package encryptUtils;

import java.util.Arrays;
import java.util.Random;

public class DesKey {
    BitSequence key;
    public DesKey(byte[] key)
    {
        this.key = new BitSequence(key);
    }
    public DesKey(){
        key=new BitSequence();
    }

    protected BitSequence getKey(){
        return key;
    }

    public byte[] getKeyBytes(){
        return key.toBytes();
    }

    public void GenKey()
    {
        Random rnd = new Random(System.currentTimeMillis());
        byte[] a = new byte[8];
        rnd.nextBytes(a);
        this.key = new BitSequence(a);
    }

    /**
     * 根据指定的数据生成的秘钥
     * @param str
     */
    public void GenKey(byte[] str)
    {
        //System.out.println(Arrays.toString(str));
        int seed = Arrays.hashCode(str);
        Random rnd = new Random(seed);
        byte[] a = new byte[8];
        rnd.nextBytes(a);
        this.key = new BitSequence(a);
    }



}
