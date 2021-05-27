using Client.core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Client.Converter.LobbyCvt
{
    class UserStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((UserState)value)
            {
                case UserState.in_room:
                    return "游戏中";
                case UserState.online:
                    return "在线";
                case UserState.prepared:
                case UserState.unprepared:
                    return "房间中";
                default:
                    return "离线";
            
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
