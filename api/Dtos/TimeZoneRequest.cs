using api.Converters;
using System.ComponentModel;

namespace api.Dtos
{
    public class TimeZoneRequest
    {
        [TypeConverter(typeof(TimeZoneInfoConverter))]
        public TimeZoneInfo TimeZone { get; set; }
    }
}