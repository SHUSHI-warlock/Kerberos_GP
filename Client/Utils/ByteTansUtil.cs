using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Utils
{
	/**
	 * 字节跟Java基本类型的转换工具
	 * 
	 * @author myumen
	 * 
	 */
	public class ByteTransUtil
	{
		/**
		 * 将整数转换为字节数组，BIG-ENDIAN格式，即高位数字在高地址、低位数字在低地址。
		 * 0xff二进制[00000000][00000000][00000000][11111111]，数字与0xff进行与(&)
		 * 就相当于抹去数字的高位3个字节，只留下低位1个字节
		 * 
		 * @param i
		 * @return
		 */
		public static byte[] intToByteArray(int i)
		{
			byte[] result = new byte[4];
			result[0] = (byte)((i >> 24) & 0xFF);
			result[1] = (byte)((i >> 16) & 0xFF);
			result[2] = (byte)((i >> 8) & 0xFF);
			result[3] = (byte)(i & 0xFF);
			return result;
		}

		/**
		 * 将整数转换为字节数组，可由参数决定是否用小端还是大端
		 * 
		 * @param i
		 * @param littleEndian
		 *            true表示用小端格式,false表示大端
		 * @return
		 */
		public static byte[] intToByteArray(int i, bool littleEndian)
		{
			byte[] result = new byte[4];
			if (littleEndian)
			{
				result[0] = (byte)(i & 0xFF);
				result[1] = (byte)((i >> 8) & 0xFF);
				result[2] = (byte)((i >> 16) & 0xFF);
				result[3] = (byte)((i >> 24) & 0xFF);
			}
			else
			{
				result[0] = (byte)((i >> 24) & 0xFF);
				result[1] = (byte)((i >> 16) & 0xFF);
				result[2] = (byte)((i >> 8) & 0xFF);
				result[3] = (byte)(i & 0xFF);
			}
			return result;
		}

		

		/**
		 * byte[]转换为整数，byte[]为大端格式
		 * 
		 * @param b
		 * @return
		 */
		public static int byteArrayToInt(byte[] b)
		{
			int i = 0;
			i += ((b[0] & 0xff) << 24);
			i += ((b[1] & 0xff) << 16);
			i += ((b[2] & 0xff) << 8);
			i += ((b[3] & 0xff));
			return i;
		}

		/**
		 * byte[]转换为整数，通过参数确定byte[]为大端格式还是小端格式
		 * 
		 * @param b
		 * @param littleEndian
		 *            true为小端,false为大端
		 * @return
		 */
		public static int byteArrayToInt(byte[] b, bool littleEndian)
		{
			int i = 0;
			if (littleEndian)
			{
				i += ((b[0] & 0xff));
				i += ((b[1] & 0xff) << 8);
				i += ((b[2] & 0xff) << 16);
				i += ((b[3] & 0xff) << 24);
			}
			else
			{
				i += ((b[0] & 0xff) << 24);
				i += ((b[1] & 0xff) << 16);
				i += ((b[2] & 0xff) << 8);
				i += ((b[3] & 0xff));
			}
			return i;
		}

		

		/**
		 * 整形转换成网络传输的字节流（字节数组）型数据，小端格式
		 * 
		 * @param num
		 *            一个整型数据
		 * @return 4个字节的自己数组
		 */
		public static byte[] intToBytes(int num)
		{
			byte[] bytes = new byte[4];
			bytes[0] = (byte)(0xff & (num >> 0));
			bytes[1] = (byte)(0xff & (num >> 8));
			bytes[2] = (byte)(0xff & (num >> 16));
			bytes[3] = (byte)(0xff & (num >> 24));
			return bytes;
		}

		/**
		 * 四个字节的字节数据(小端)转换成一个整形数据
		 * 
		 * @param bytes
		 *            4个字节的字节数组
		 * @return 一个整型数据
		 */
		public static int byteToInt(byte[] bytes)
		{
			int num = 0;
			int temp;
			temp = (0x000000ff & (bytes[0])) << 0;
			num = num | temp;
			temp = (0x000000ff & (bytes[1])) << 8;
			num = num | temp;
			temp = (0x000000ff & (bytes[2])) << 16;
			num = num | temp;
			temp = (0x000000ff & (bytes[3])) << 24;
			num = num | temp;
			return num;
		}
	}

}
