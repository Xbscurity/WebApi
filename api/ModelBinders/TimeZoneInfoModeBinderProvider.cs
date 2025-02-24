using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace api.ModelBinders
{
    public class TimeZoneInfoModeBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(TimeZoneInfo))
            {
                return new TimeZoneInfoModelBinder();
            }
            return null;
        }
    }
}