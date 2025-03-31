using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfApp1.Converters
{
	public class WidthConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is double width && parameter is string parameterString && double.TryParse(parameterString, out double factor))
			{
				return width * factor;
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}