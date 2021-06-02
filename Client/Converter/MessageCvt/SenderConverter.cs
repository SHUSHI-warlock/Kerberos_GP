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
    class SenderConverter : IValueConverter
    {
        /// <summary>
        /// 自动列表序号
        /// </summary>
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            return MessageShow.ParseSender((int)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
}
