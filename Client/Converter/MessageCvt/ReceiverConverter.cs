using Client.MsgTrans;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Client.Converter.MessageCvt
{
    class ReceiverConverter : IValueConverter
    {
        /// <summary>
        /// 自动列表序号
        /// </summary>
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            switch((P2PType)value)
            {
                case P2PType.CtoAS:
                    return "AS服务器";
                case P2PType.CtoTGS:
                    return "TGS服务器";
                case P2PType.CtoS:
                    return "应用服务器";
                case P2PType.AStoC:
                case P2PType.TGStoC:
                case P2PType.StoC:
                    return "客户端";
                default:
                    return "未知";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
