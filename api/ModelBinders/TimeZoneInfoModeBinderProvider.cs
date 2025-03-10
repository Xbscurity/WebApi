using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace api.ModelBinders
{
    public class TimeZoneInfoModeBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(TimeZoneInfo))
            {
                return TimeZoneInfoModelBinder.Instance;
            }
            return null;
        }
    }
}