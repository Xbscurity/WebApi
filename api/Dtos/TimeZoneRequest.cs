using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using api.Converters;

namespace api.Dtos
{
     public class TimeZoneRequest
    {
        [TypeConverter(typeof(TimeZoneInfoConverter))]
        public TimeZoneInfo TimeZone { get; set; }
    }
}