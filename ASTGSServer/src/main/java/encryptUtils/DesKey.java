package encryptUtils;

import java.io.UncheckedIOException;
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
        String seed = "";
        for (int i=0;i<str.length ;i++ )
        {
            seed += str[i];
        }
        byte[] a = new byte[8];
        for(int i = 0;i < 8 ;i++)
        {
            a[i] = (byte)Hash0(seed + 256*i);
        }
        this.key = new BitSequence(a);
    }

    public static void main(String[] args) {

        DesKey desKey = new DesKey();
        byte[] b = {0,1,2,3};
        desKey.GenKey(b);


    }
    public int Hash0(String str)
    {
        int BitsInUnsignedInt = (int)(4 * 8);
        int ThreeQuarters     = (int)((BitsInUnsignedInt  * 3) / 4);
        int OneEighth         = (int)(BitsInUnsignedInt / 8);
        int HighBits          = (int)(0xFFFFFFFF) << (BitsInUnsignedInt - OneEighth);
        int hash              = 0;
        int test              = 0;

        for(int i = 0; i < str.length(); i++)
        {
            hash = (hash << OneEighth) + str.charAt(i);

            if((test = hash & HighBits)  != 0)
            {
                hash = (( hash ^ (test >> ThreeQuarters)) & (~HighBits));
            }
        }

        return hash;
    }
    public static int Hash1(String str)
    {
        int seed = 131;
        int hash = 0;
        int count;
        char[] bitarray = str.toCharArray();
        count = bitarray.length;
        while (count > 0)
        {
            hash = hash * seed + (bitarray[bitarray.length - count]);
            count--;
        }

        return (hash & 0x7FFFFFFF);
    }

}
