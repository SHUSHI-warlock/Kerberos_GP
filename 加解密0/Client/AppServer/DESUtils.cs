using System;
using System.Collections.Generic;
using System.Text;

namespace AppServer
{
    class DESUtils
    {
        //IP置换表
        private static readonly int[] IP_TABLE = {
        58 ,50 ,42 ,34 ,26 ,18 ,10 ,2,
        60 ,52 ,44 ,36 ,28 ,20 ,12 ,4,
        62 ,54 ,46 ,38 ,30 ,22 ,14 ,6,
        64 ,56 ,48 ,40 ,32 ,24 ,16 ,8,
        57 ,49 ,41 ,33 ,25 ,17 ,9  ,1,
        59 ,51 ,43 ,35 ,27 ,19 ,11 ,3,
        61 ,53 ,45 ,37 ,29 ,21 ,13 ,5,
        63 ,55 ,47 ,39 ,31 ,23 ,15 ,7,
        };
        //IP-1置换表
        private static readonly int[] IIP_TABLE = {
        40 ,8 ,48 ,16 ,56 ,24 ,64 ,32,
        39 ,7 ,47 ,15 ,55 ,23 ,63 ,31,
        38 ,6 ,46 ,14 ,54 ,22 ,62 ,30,
        37 ,5 ,45 ,13 ,53 ,21 ,61 ,29,
        36 ,4 ,44 ,12 ,52 ,20 ,60 ,28,
        35 ,3 ,43 ,11 ,51 ,19 ,59 ,27,
        34 ,2 ,42 ,10 ,50 ,18 ,58 ,26,
        33 ,1 ,41 ,9  ,49 ,17 ,57 ,25,
        };

        //扩展置换(E)表 4*8 = 32->48 = 6*8
        private static readonly int[] EP_TABLE ={
            32, 1, 2, 3, 4, 5,
            4 , 5, 6, 7, 8, 9,
            8 , 9,10,11,12,13,
            12,13,14,15,16,17,
            16,17,18,19,20,21,
            20,21,22,23,24,25,
            24,25,26,27,28,29,
            28,29,30,31,32, 1,
        };
        //S盒-1代换表
        private   int[,,] S_BOX_TABLE = {
            {
                    {14,4,13,1,2,15,11,8,3,10,6,12,5,9,0,7},
                    {0,15,7,4,14,2,13,1,10,6,12,11,9,5,3,8},
                    {4,1,14,8,13,6,2,11,15,12,9,7,3,10,5,0},
                    {15,12,8,2,4,9,1,7,5,11,3,14,10,0,6,13}
            },

            {
                    {15,1,8,14,6,11,3,4,9,7,2,13,12,0,5,10},
                    {3,13,4,7,15,2,8,14,12,0,1,10,6,9,11,5},
                    {0,14,7,11,10,4,13,1,5,8,12,6,9,3,2,15},
                    {13,8,10,1,3,15,4,2,11,6,7,12,0,5,14,9}
            },

            {
                    {10,0,9,14,6,3,15,5,1,13,12,7,11,4,2,8},
                    {13,7,0,9,3,4,6,10,2,8,5,14,12,11,15,1},
                    {13,6,4,9,8,15,3,0,11,1,2,12,5,10,14,7},
                    {1,10,13,0,6,9,8,7,4,15,14,3,11,5,2,12}
            },

            {
                    {7,13,14,3,0,6,9,10,1,2,8,5,11,12,4,15},
                    {13,8,11,5,6,15,0,3,4,7,2,12,1,10,14,9},
                    {10,6,9,0,12,11,7,13,15,1,3,14,5,2,8,4},
                    {3,15,0,6,10,1,13,8,9,4,5,11,12,7,2,14}
            },

            {
                    {2,12,4,1,7,10,11,6,8,5,3,15,13,0,14,9},
                    {14,11,2,12,4,7,13,1,5,0,15,10,3,9,8,6},
                    {4,2,1,11,10,13,7,8,15,9,12,5,6,3,0,14},
                    {11,8,12,7,1,14,2,13,6,15,0,9,10,4,5,3}
            },

            {
                    {12,1,10,15,9,2,6,8,0,13,3,4,14,7,5,11},
                    {10,15,4,2,7,12,9,5,6,1,13,14,0,11,3,8},
                    {9,14,15,5,2,8,12,3,7,0,4,10,1,13,11,6},
                    {4,3,2,12,9,5,15,10,11,14,1,7,6,0,8,13}
            },

            {
                    {4,11,2,14,15,0,8,13,3,12,9,7,5,10,6,1},
                    {13,0,11,7,4,9,1,10,14,3,5,12,2,15,8,6},
                    {1,4,11,13,12,3,7,14,10,15,6,8,0,5,9,2},
                    {6,11,13,8,1,4,10,7,9,5,0,15,14,2,3,12}
            },

            {
                    {13,2,8,4,6,15,11,1,10,9,3,14,5,0,12,7},
                    {1,15,13,8,10,3,7,4,12,5,6,11,0,14,9,2},
                    {7,11,4,1,9,12,14,2,0,6,10,13,15,3,5,8},
                    {2,1,14,7,4,10,8,13,15,12,9,0,3,5,6,11}, 
            }
            };
        //P盒置换表
        private static readonly int[] P_BOX_TABLE ={
            16,7 ,20,21,29,12,28,17,
            1 ,15,23,26,5 ,18,31,10,
            2 ,8 ,24,14,32,27,3 ,9 ,
            19,13,30,6 ,22,11,4 ,25,
    };

        //PC-1表 64->56 这里面不包括8倍
        private static readonly int[] PC1_TABLE ={
            57,49,41,33,25,17,9,1,
            58,50,42,34,26,18,10,2,
            59,51,43,35,27,19,11,3,
            60,52,44,36,63,55,47,39,
            31,23,15,7,62,54,46,38,
            30,22,14,6,61,53,45,37,
            29,21,13,5,28,20,12,4,
    };
        //PC-2表 56->48
        private static readonly int[] PC2_TABLE ={
            14,17,11,24, 1, 5,
            3,28,15, 6,21,10,
            23,19,12, 4,26, 8,
            16, 7,27,20,13, 2,
            41,52,31,37,47,55,
            30,40,51,45,33,48,
            44,49,39,56,34,53,
            46,42,50,36,29,32,
    };
        //左移位规定表
        private static readonly int[] LEFTSHIFT_TABLE ={
            1,1,2,2,2,2,2,2,1,2,2,2,2,2,2,1,
        };
        //传入秘钥
        public DESUtils(DesKey K)
        {
            //生成子秘钥
            Ki = CreateChildrenKey(K.getKey());
        }
        private BitSequence[] Ki;   //子秘钥


        /**
         * DES 内部加密操作
         * @param M 明文序列-64bit
         * @return C 密文序列-64bit
         */
        private BitSequence desEncryption(BitSequence M)
        {
            //切分明文
            List<BitSequence> lr = (M.permutation(IP_TABLE)).split(2);
            BitSequence temp;
            for (int i = 0; i < 16; i++)
            {
                //轮函数
                BitSequence pBox_Output = F(lr[1], Ki[i]);

                if (i != 15)
                { //最后一轮不交换
                    temp = lr[0].clone();          //先将左边的放入中间变量
                    lr[0] = lr[1];                          //Li=Ri-1
                    lr[1] = pBox_Output.XOR(temp); //Ri=Li-i xor pBox_Out
                }
                else
                    lr[0] = pBox_Output.XOR(lr[0]);
            }
            //结束后再做一个IIP
            return (BitSequence.merge(lr, 32)).permutation(IIP_TABLE);
        }

        /**
         * 解密操作
         * @param C 密文序列-64bit
         * @return M 明文序列-64bit
         */
        private BitSequence desDecryption(BitSequence C)
        {
            //切分明文
            List<BitSequence> lr = (C.permutation(IP_TABLE)).split(2);
            BitSequence temp;
            for (int i = 0; i < 16; i++)
            {
                //轮函数(反着用Ki)
                BitSequence pBox_Output = F(lr[1], Ki[15 - i]);

                if (i != 15)
                { //最后一轮不交换
                    temp = lr[0].clone();          //先将左边的放入中间变量
                    lr[0] = lr[1];                           //Li=Ri-1
                    lr[1] = pBox_Output.XOR(temp); //Ri=Li-i xor pBox_Out
                }
                else
                    lr[0] = pBox_Output.XOR(lr[0]);
            }
            //结束后再做一个IIP
            return (BitSequence.merge(lr, 32)).permutation(IIP_TABLE);
        }

        /**
         * 轮函数
         * @param R 右边 32
         * @param Ki 子秘钥 48
         * @return 32
         */
        private BitSequence F(BitSequence R, BitSequence Ki)
        {
            BitSequence temp = R.permutation(EP_TABLE);
            temp.XOR(Ki);


            int[] sBoxOut = new int[8];

            //S盒代换
            int row, col;
            for (int k = 0; k < 48; k += 6)
            {
                row = (temp.getBit(k) ? 2 : 0) + (temp.getBit(k + 5) ? 1 : 0);
                col = (temp.getBit(k + 1) ? 8 : 0) + (temp.getBit(k + 2) ? 4 : 0) +
                        (temp.getBit(k + 3) ? 2 : 0) + (temp.getBit(k + 4) ? 1 : 0);
                
                sBoxOut[k / 6] = S_BOX_TABLE[k / 6,row,col];  //找到对应的代换
                // Console.WriteLine(sBoxInput.get(k).toString());
            }

            //拼接，赋值
            BitSequence pBox_Input = new BitSequence(sBoxOut, 4);

            //p盒置换
            return pBox_Input.permutation(P_BOX_TABLE);
        }

        /**
         * 生成子钥
         * @param K 原始秘钥
         * @return 16把子钥数组
         */
        private static BitSequence[] CreateChildrenKey(BitSequence K)
        {
            BitSequence[] res = new BitSequence[16];
            //PC1置换 64->56bit
            BitSequence PC1_out = K.permutation(PC1_TABLE);

            //对半28bit，准备迭代
            List<BitSequence> LR;
            LR = PC1_out.split(2);
            BitSequence temp;

            //循环迭代
            for (int i = 0; i < 16; i++)
            {
                //左移
                LR[0].leftShift(LEFTSHIFT_TABLE[i]);
                LR[1].leftShift(LEFTSHIFT_TABLE[i]);
                //合并，PC-2置换
                temp = BitSequence.merge(LR, 28);
                res[i] = temp.permutation(PC2_TABLE);
            }
            return res;
        }

        /**
         * 加密
         * @param M
         */
        public byte[] Encryption(byte[] M)
        {

            byte[] temp = new byte[8];
            byte[] res;
            if (M.Length % 8 != 0)
                res = new byte[M.Length + 8 - M.Length % 8];
            else
                res = new byte[M.Length];
            int index = 0;
            for (int i = 0; i < M.Length; i++)
            {
                temp[index++] = M[i];
                if (index == 8)
                {
                    Array.Copy((desEncryption(new BitSequence(temp)).toBytes()), 0, res, i - 7, 8);
                    index = 0;
                }
            }
            if (index != 0)
            {   //不为64整除，补0
                for (int i = index; i < 8; i++)
                    temp[i] = 0;
                Array.Copy((desEncryption(new BitSequence(temp)).toBytes()), 0, res, M.Length - M.Length % 8, 8);
            }

            return res;
        }

        /**
         * 加密
         * @param C
         */
        public byte[] Decryption(byte[] C)
        {

            byte[] temp = new byte[8];
            byte[] res = new byte[C.Length];
            int index = 0;
            for (int i = 0; i < C.Length; i++)
            {
                temp[index++] = C[i];
                if (index == 8)
                {
                     Array.Copy((desDecryption(new BitSequence(temp)).toBytes()), 0, res, i - 7, 8);
                    index = 0;
                }
            }

            return res;
        }
        
        //测试
        public static void main(String[] args)
        {
            //输入秘钥
            DesKey a = new DesKey();
            a.GenKey();

            Console.WriteLine(string.Join(",", ByteConverter.UbyteToSbyte(a.getKey().toBytes())));

            DESUtils des = new DESUtils(a);

            //输入明文
            
            Random rnd = new Random(System.DateTime.Now.Millisecond);
            byte[] m = new byte[127];
            rnd.NextBytes(m);
            
            //sbyte[] sb = { -98, -77, -94, -2, -39, -73, -45, -91 };
            //byte[] m = ByteConverter.SbyteToUbyte(sb);
            Console.WriteLine("  明文  :" + string.Join(",", m));
            byte[] C = des.Encryption(m);
            Console.WriteLine("加密结果:" + string.Join(",", ByteConverter.UbyteToSbyte(C)));
            byte[] M2 = des.Decryption(C);
            Console.WriteLine("解密结果:" + string.Join(",", ByteConverter.UbyteToSbyte(M2)));
        } 
        
    }
}
