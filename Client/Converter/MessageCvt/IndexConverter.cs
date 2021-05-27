using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;


namespace Client.Converter.MessageCvt
{
    //[ValueConversion(typeof(Int32), typeof(ListViewItem))]
    class IndexConverter : IValueConverter
    {
        /// <summary>
        /// 自动列表序号
        /// </summary>
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            ListBoxItem item = (ListBoxItem)value;
            ListBox listView = ItemsControl.ItemsControlFromItemContainer(item) as ListBox;
            return listView.ItemContainerGenerator.IndexFromContainer(item) + 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}




