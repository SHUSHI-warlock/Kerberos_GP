using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Ker
{
    public class byteManage
    {
        public static int byteToInt(byte b)
        {
            //System.out.println("byte 是:"+b);
            int x = b & 0xff;
            //System.out.println("int 是:"+x);
            return x;
        }

        public static byte[] subBytes(byte[] src, int begin, int count)
        {
            byte[] bs = new byte[count];
            Array.Copy(src, begin, bs, 0, count);
            //System.arraycopy(src, begin, bs, 0, count);
            return bs;
        }

        public static byte[] concat(byte[] a, byte[] b)
        {
            byte[] c = new byte[a.Length + b.Length];
            Array.Copy(a, 0, c, 0, a.Length);
            Array.Copy(b, 0, c, a.Length, b.Length);
            //System.arraycopy(a, 0, c, 0, a.length);
            //System.arraycopy(b, 0, c, a.length, b.length);
            return c;
        }

        public static String generateTime()
        {
            //SimpleDateFormat df = new SimpleDateFormat("HH:mm:ss");//设置日期格式
            //String date = df.format(new Date());// new Date()为获取当前系统时间，也可使用当前时间戳
            //String time = DateTime.Now.ToLongTimeString().ToString();
            DateTime dt = new DateTime();
            dt = System.DateTime.Now;
            //此处的大小写必须完全按照如下才能输出长日期长时间，时间为24小时制式，hh:mm:ss格式输出12小时制式时间
            string time = dt.ToString("HH:mm:ss");

            for (int i = time.Length; i < 12; i++)
            {
                time += " ";
            }
            return time;
        }

        public static String generateTime(out string Vtime)
        {
            //SimpleDateFormat df = new SimpleDateFormat("HH:mm:ss");//设置日期格式
            //String date = df.format(new Date());// new Date()为获取当前系统时间，也可使用当前时间戳
            //String time = DateTime.Now.ToLongTimeString().ToString();
            DateTime dt = new DateTime();
            dt = System.DateTime.Now;
            //此处的大小写必须完全按照如下才能输出长日期长时间，时间为24小时制式，hh:mm:ss格式输出12小时制式时间
            string time = dt.ToString("HH:mm:ss");
            Vtime = dt.AddMinutes(1).ToString("HH:mm:ss");
            for (int i = time.Length; i < 12; i++)
            {
                time += " ";
            }
            return time;
        }

        /*
        public static bool searchList(List<String> list, String target)
        {
            bool result = false;
           
            for (int i = 0; i < list.Count(); i++)
            {
                
                if (list.get(i).equals(target))
                {
                    result = true;
                }
            }
            return result;
        }
        */
    }
}
