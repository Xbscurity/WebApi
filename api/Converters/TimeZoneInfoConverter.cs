
namespace api.Converters
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    public class TimeZoneInfoConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string timeZoneId && TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out TimeZoneInfo? timeZone))
            {
                return timeZone;
            }
            return null;
        }


    }
}