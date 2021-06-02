package myutil.DESUtil;

import java.util.ArrayList;
import java.util.List;

class BitSequence {
    int length;     //序列长度
    int num;        //字（int 容器）的个数
    int headWidth;  //首字长度
    private static final int WordWidth = 16; //每个int存储几个bit
    int[] content; //用int存序列

    /**
     * 输出字符串格式
     * @return a
     */
    @Override
    public String toString() {
        StringBuilder str = new StringBuilder();
        for (int i = 0; i < length; i++) {
            if(getBit(i)) str.append("1");
            else  str.append("0");
        }
        return str.toString();
    }

    public byte[] toBytes() {
        byte[] res = new byte[length / 8];
        int con=0;
        int temp;
        if(WordWidth%8==0)
        {
            int k = length / 8 - 1;
            for(int i=num-1;i>=0;i--){
                temp = 0;
                con = content[i];
                for(int j=0;j<WordWidth/8;j++,k--)
                {

                    temp = con%256;
                    con >>= 8;
                    if (temp > 128) {
                        res[k] = (byte) (temp - 256);
                    } else
                        res[k] = (byte) temp;
                }
            }
        }
        else {
            for (int i = 0; i < length / 8; i++) {
                temp = 0;
                for (int j = 0; j < 8; j++) {
                    temp <<= 1;
                    temp += (getBit(i * 8 + j) ? 1 : 0);
                }
                if (temp > 128)
                    res[i] = (byte) (temp - 256);
                else
                    res[i] = (byte) temp;
            }
        }

        return res;
    }

    @Override
    protected BitSequence clone() {
        BitSequence temp = new BitSequence(length);
        temp.content = content.clone();
        return temp;
    }

    public BitSequence(int length){
        num = length / WordWidth  +  (length%WordWidth==0?0:1);
        content = new int[num];
        this.length = length;
        if(length%WordWidth!=0)
            headWidth=length%WordWidth;
        else
            headWidth = WordWidth;
    }

    /**
     *
     * @param bytes int数组
     * @param width 每个int所携带的大小
     */
    public BitSequence(int[] bytes,int width) {
            this.length = width*bytes.length;
            if(this.length%WordWidth!=0) {
                headWidth = this.length % WordWidth;
                num = this.length / WordWidth + 1;
            }
            else {
                headWidth = WordWidth;
                num = this.length / WordWidth;
            }

            content = new int[num];




            if(WordWidth%width==0){
                int i=0,j=0;
                for (i = 0; i < bytes.length; i++) {
                    content[j]<<=width;
                    content[j]+=bytes[i];
                    if((i+1)%(WordWidth/width)==0)
                        j++;
                }
            }
            else {
                int i = 0;
                for(int temp:bytes){
                    for(int j=0;j<width;j++)
                    {
                        boolean t = (temp % 2 == 1);
                        setBit(i+width-j-1, t);
                        temp/=2;
                    }
                    i+=width;
                }
            }
    }
    public BitSequence(byte[] bytes) {
        this(bytes.length*8);
        int i = 0;
        int temp;
        if(WordWidth%8==0)
        {
            int k=0;
            for(i=0;i<num;i++) {
                temp = 0;
                for (int j = 0; j < WordWidth / 8; j++,k++) {
                    if (bytes[k] < 0)
                        temp = bytes[k] + 256;
                    else
                        temp = bytes[k];
                    content[i] <<= 8;
                    content[i] += temp;
                }
            }
        }
        else {
            for (byte b : bytes) {
                if (b >= 0)
                    temp = b;
                else
                    temp = (1 << 8) + b;
                for (int j = 0; j < 8; j++) {
                    boolean t = (temp % 2 == 1);
                    setBit(i + 8 - j - 1, t);
                    temp /= 2;
                }
                i += 8;
            }
        }
    }
    public BitSequence(String bits) {
        //调用前面的构造函数
        this(bits.length());

        int i=0;
        for (char c : bits.toCharArray()){
            setBit(i,(c != '0'));
            i++;
        }
    }

    public static void main(String[] args) {
        int [] K = {0x13,0x34,0x57,0x79,0x9b,0xbc,0xdf,0xf1};

        BitSequence b = new BitSequence(K,8);
        //BitSequence b = new BitSequence("0001001100110100010101110101011110011011101111001101111111110001");
        System.out.println(b.length);
        System.out.println("原文:"+"0001001100110100010101110101011110011011101111001101111111110001");
        System.out.println("存储:"+b.toString());
        System.out.print("判别:");
        for(int i=0;i<b.length;i++)
            System.out.print(b.getBit(i)?1:0);
        System.out.println(" ");

        for(int i=13;i>=0;i--)
            b.setBit(i,true);
        System.out.println("设置:"+b.toString());

        BitSequence a = new BitSequence("110000000");
        System.out.println(a.length);
        //a.XOR(b);
        //System.out.println(a.toString());
/*
        a.rightShift(1);
        System.out.println(a.toString());
*/
        System.out.println(a.toString());

        a.leftShift(1);
        System.out.println(a.toString());


    }

    /**
     * 返回某一位上的值
     * @param index 位置
     * @return true代表1 false代表false
     */
    public boolean getBit(int index){

        //获取对应的word
        int temp = content[index/WordWidth];
        //获取在word中对应的位置
        int loc = WordWidth - index%WordWidth - 1;
        //与1操作
        return (temp & (1 << loc)) != 0;

        //return (content[index/WordWidth] & (1<<(index%WordWidth))) != 0;
    }
    /**
     * 设置某一位上的值
     * @param index 下标
     * @param flag 值
     *
     */
    public void setBit(int index,boolean flag) {
        content[index/WordWidth] |= 1<<(WordWidth - index%WordWidth -1);
        if(!flag)
            content[index/WordWidth] -= 1<<(WordWidth - index%WordWidth -1);
    }

    /**
     * 异或操作（不考虑长度不等的情况）
     * @param a a
     * @param b b
     * @return 异或结果
     */
    public static BitSequence XOR(BitSequence a, BitSequence b) {
        BitSequence c = a.clone();
        c.XOR(b);
        return c;
    }
    public BitSequence XOR(BitSequence b){
        for(int i=0;i<num;i++)
            content[i] ^= b.content[i];
        return this;
    }

    /**
     * 自身循环左移
     *
     * @param k 移位数
     */
    public void leftShift(int k) {
        for(int i=0;i<k;i++) {
         //循环k次，每次移动一位
            boolean temp = getBit(0);
            for (int j = 0; j < length - 1; j++)
                setBit(j,getBit(j+1));
            setBit(length-1,temp);
        }
    }

    /**
     * 自身循环右移
     *
     * @param k 位移数
     */
    public void rightShift(int k) {
        for (int i = 0; i < k; i++) {
            //循环k次，每次移动一位
            boolean temp = getBit(length - 1);
            for (int j = length - 2; j >= 0; j--)
                setBit(j + 1, getBit(j));
            setBit(0, temp);
        }
    }

    /**
     *
     * @param list 列表
     * @param length 每个的长度
     * @return
     */
    public static BitSequence merge(List<BitSequence> list,int length) {
        if (list.size() == 0)
            return null;
        BitSequence res = new BitSequence(list.size() * length);
        if (WordWidth%length==0){//字宽是长度的整数倍，多个字组成一个字
            int i=0,j,k;
            for (j = 0; j < res.num;j++) {
                for(k = j*WordWidth/length;k<j+WordWidth/length&&k<list.size();k++)
                {
                    res.content[j]<<=length;
                    res.content[j]+=list.get(k).content[0];
                }
            }
        }
        else if(length%WordWidth==0){//是字宽的整数倍
            for (int j = 0; j < list.size(); j++) {
                BitSequence temp = list.get(j);
                System.arraycopy(temp.content,0,res.content, j*length/WordWidth, length/WordWidth);
            }
        }
        else {//肯定稳妥，但是可能慢
            for (int j = 0; j < list.size(); j++) {
                BitSequence temp = list.get(j);
                for (int i = 0; i < length; i++)
                    res.setBit(j * length + i, temp.getBit(i));
            }
        }

        return res;
    }

    /**
     * 分两半
     * @param k 拆分成k份
     * @return list 0,1
     */
    public List<BitSequence> split(int k) {
        List<BitSequence> list = new ArrayList<>(k);
        boolean flag = false;
        if((length/k)%WordWidth==0)
            flag = true;
            for (int j = 0; j < k; j++) {
                BitSequence temp = new BitSequence(length / k);
                if(flag)
                    System.arraycopy(content, j * temp.num, temp.content, 0, temp.num);
                else
                    for (int i = 0; i < temp.length; i++)
                        temp.setBit(i, getBit(j * temp.length + i));
                list.add(temp);
            }

        return list;
    }

    /**
     * 置换
     *
     * @param table 置换表
     * @return 置换结果
     */
    public BitSequence permutation(final int[] table) {
        BitSequence res = new BitSequence(table.length);
        for(int i=0;i<table.length;i++)
            res.setBit(i,getBit(table[i]-1));
        return res;
    }
}
