using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kpmg.Assessment.MultipartMediaFormatter.Converters
{
    internal class DateTimeConverterISO8601 : DateTimeConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value != null && value is DateTime && destinationType == typeof(string))
            {
                return ((DateTime)value).ToString("O"); // ISO 8601
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
