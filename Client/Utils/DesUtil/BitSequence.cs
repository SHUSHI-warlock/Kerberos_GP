using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Client.Utils.DesUtil
{
    public class BitSequence
    {
        int Length;     //序列长度
        int num;        //字（int 容器）的个数
        int headWidth;  //首字长度
        private static int WordWidth = 16; //每个int存储几个bit
        int[] content; //用int存序列
        /**
         * 输出字符串格式
         * @return a
         */

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < Length; i++)
            {
                if (getBit(i)) str.Append("1");
                else str.Append("0");
            }
            return str.ToString();
        }

        public byte[] toBytes()
        {
            byte[] res = new byte[Length / 8];
            int con = 0;
            int temp;
            if (WordWidth % 8 == 0)
            {
                int k = Length / 8 - 1;
                for (int i = num - 1; i >= 0; i--)
                {
                    temp = 0;
                    con = content[i];
                    for (int j = 0; j < WordWidth / 8; j++, k--)
                    {

                        temp = con % 256;
                        con >>= 8;
                        res[k] = (byte)temp;
                       
                    }
                }
            }
            else
            {
                for (int i = 0; i < Length / 8; i++)
                {
                    temp = 0;
                    for (int j = 0; j < 8; j++)
                    {
                        temp <<= 1;
                        temp += (getBit(i * 8 + j) ? 1 : 0);
                    }
                    res[i] = (byte)temp;
                }
            }
            

            return res;
        }

        
        public BitSequence clone()
        {
            BitSequence temp = new BitSequence(Length);
            temp.content = (int[])content.Clone();
            return temp;
        }

        public BitSequence(int Length)
        {
            num = Length / WordWidth + (Length % WordWidth == 0 ? 0 : 1);
            content = new int[num];
            this.Length = Length;
            if (Length % WordWidth != 0)
                headWidth = Length % WordWidth;
            else
                headWidth = WordWidth;
        }

        /**
         *
         * @param bytes int数组
         * @param width 每个int所携带的大小
         */
        public BitSequence(int[] bytes, int width)
        {
            this.Length = width * bytes.Length;
            if (this.Length % WordWidth != 0)
            {
                headWidth = this.Length % WordWidth;
                num = this.Length / WordWidth + 1;
            }
            else
            {
                headWidth = WordWidth;
                num = this.Length / WordWidth;
            }

            content = new int[num];




            if (WordWidth % width == 0)
            {
                int i = 0, j = 0;
                for (i = 0; i < bytes.Length; i++)
                {
                    content[j] <<= width;
                    content[j] += bytes[i];
                    if ((i + 1) % (WordWidth / width) == 0)
                        j++;
                }
            }
            else
            {
                int i = 0;
                foreach (int temp1 in bytes)
                {
                    int temp = temp1;
                    for (int j = 0; j < width; j++)
                    {
                        bool t = (temp % 2 == 1);
                        setBit(i + width - j - 1, t);
                        temp /= 2;
                    }
                    i += width;
                }
            }
        }
        public BitSequence(byte[] bytes):this(bytes.Length * 8)
        {
            int i = 0;
            int temp;
            if (WordWidth % 8 == 0)
            {
                int k = 0;
                for (i = 0; i < num; i++)
                {
                    temp = 0;
                    for (int j = 0; j < WordWidth / 8; j++, k++)
                    {
                        if (bytes[k] < 0)
                            temp = bytes[k] + 256;
                        else
                            temp = bytes[k];
                        content[i] <<= 8;
                        content[i] += temp;
                    }
                }
            }
            else
            {
                foreach (byte b in bytes)
                {
                    if (b >= 0)
                        temp = b;
                    else
                        temp = (1 << 8) + b;
                    for (int j = 0; j < 8; j++)
                    {
                        bool t = (temp % 2 == 1);
                        setBit(i + 8 - j - 1, t);
                        temp /= 2;
                    }
                    i += 8;
                }
            }
        }
        public BitSequence(string bits):this(bits.Length)
        {
            //调用前面的构造函数
            ;

            int i = 0;
            foreach (char c in bits.ToCharArray())
            {
                setBit(i, (c != '0'));
                i++;
            }
        }

        public static void main(string[] args)
        {
            int[] K = { 0x13, 0x34, 0x57, 0x79, 0x9b, 0xbc, 0xdf, 0xf1 };

            BitSequence b = new BitSequence(K, 8);
            //BitSequence b = new BitSequence("0001001100110100010101110101011110011011101111001101111111110001");
            Console.WriteLine(b.Length);
            Console.WriteLine("原文:" + "0001001100110100010101110101011110011011101111001101111111110001");
            Console.WriteLine("存储:" + b.ToString());
            Console.WriteLine("判别:");
            for (int i = 0; i < b.Length; i++)
                Console.WriteLine(b.getBit(i) ? 1 : 0);
            Console.WriteLine(" ");

            for (int i = 13; i >= 0; i--)
                b.setBit(i, true);
            Console.WriteLine("设置:" + b.ToString());

            BitSequence a = new BitSequence("110000000");
            Console.WriteLine(a.Length);
            //a.XOR(b);
            //Console.WriteLine(a.ToString());
            /*
                    a.rightShift(1);
                    Console.WriteLine(a.ToString());
            */
            Console.WriteLine(a.ToString());

            a.leftShift(1);
            Console.WriteLine(a.ToString());
        }

        /**
         * 返回某一位上的值
         * @param index 位置
         * @return true代表1 false代表false
         */
        public bool getBit(int index)
        {

            //获取对应的word
            int temp = content[index / WordWidth];
            //获取在word中对应的位置
            int loc = WordWidth - index % WordWidth - 1;
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
        public void setBit(int index, bool flag)
        {
            content[index / WordWidth] |= 1 << (WordWidth - index % WordWidth - 1);
            if (!flag)
                content[index / WordWidth] -= 1 << (WordWidth - index % WordWidth - 1);
        }

        /**
         * 异或操作（不考虑长度不等的情况）
         * @param a a
         * @param b b
         * @return 异或结果
         */
        public static BitSequence XOR(BitSequence a, BitSequence b)
        {
            BitSequence c = a.clone();
            c.XOR(b);
            return c;
        }
        public BitSequence XOR(BitSequence b)
        {
            for (int i = 0; i < num; i++)
                content[i] ^= b.content[i];
            return this;
        }

        /**
         * 自身循环左移
         *
         * @param k 移位数
         */
        public void leftShift(int k)
        {
            for (int i = 0; i < k; i++)
            {
                //循环k次，每次移动一位
                bool temp = getBit(0);
                for (int j = 0; j < Length - 1; j++)
                    setBit(j, getBit(j + 1));
                setBit(Length - 1, temp);
            }
        }

        /**
         * 自身循环右移
         *
         * @param k 位移数
         */
        public void rightShift(int k)
        {
            for (int i = 0; i < k; i++)
            {
                //循环k次，每次移动一位
                bool temp = getBit(Length - 1);
                for (int j = Length - 2; j >= 0; j--)
                    setBit(j + 1, getBit(j));
                setBit(0, temp);
            }
        }

        /**
         *
         * @param list 列表
         * @param Length 每个的长度
         * @return
         */
        public static BitSequence merge(List<BitSequence> list, int Length)
        {
            if (list.Count == 0)
                return null;
            BitSequence res = new BitSequence(list.Count * Length);
            if (WordWidth % Length == 0)
            {//字宽是长度的整数倍，多个字组成一个字
                int i = 0, j, k;
                for (j = 0; j < res.num; j++)
                {
                    for (k = j * WordWidth / Length; k < j + WordWidth / Length && k < list.Count; k++)
                    {
                        res.content[j] <<= Length;
                        res.content[j] += list[k].content[0];
                    }
                }
            }
            else if (Length % WordWidth == 0)
            {//是字宽的整数倍
                for (int j = 0; j < list.Count; j++)
                {

                    BitSequence temp = list[j];
                    // Array.Copy(temp.content, 0, res.content, j * Length / WordWidth, Length / WordWidth);
                    Array.Copy(temp.content, 0, res.content, j * Length / WordWidth, Length / WordWidth);
                }
            }
            else
            {//肯定稳妥，但是可能慢
                for (int j = 0; j < list.Count; j++)
                {
                    BitSequence temp = list[j];
                    for (int i = 0; i < Length; i++)
                        res.setBit(j * Length + i, temp.getBit(i));
                }
            }

            return res;
        }

        /**
         * 分两半
         * @param k 拆分成k份
         * @return list 0,1
         */
        public List<BitSequence> split(int k)
        {
           
            List<BitSequence> list = new List<BitSequence>(k);
            bool flag = false;
            if ((Length / k) % WordWidth == 0)
                flag = true;
            for (int j = 0; j < k; j++)
            {
                BitSequence temp = new BitSequence(Length / k);
                if (flag)
                    Array.Copy(content, j * temp.num, temp.content, 0, temp.num);
                else
                    for (int i = 0; i < temp.Length; i++)
                        temp.setBit(i, getBit(j * temp.Length + i));
                list.Add(temp);
            }

            return list;
        }

        /**
         * 置换
         *
         * @param table 置换表
         * @return 置换结果
         */
        public BitSequence permutation( int[] table)
        {
            BitSequence res = new BitSequence(table.Length);
            for (int i = 0; i < table.Length; i++)
                res.setBit(i, getBit(table[i] - 1));
            return res;
        }

    }
}
