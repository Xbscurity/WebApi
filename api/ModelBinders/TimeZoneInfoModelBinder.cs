using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace api.ModelBinders
{
    public class TimeZoneInfoModelBinder : IModelBinder
    {
        public static readonly TimeZoneInfoModelBinder Instance = new TimeZoneInfoModelBinder();
        private TimeZoneInfoModelBinder()
        {
            
        } 
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (valueProviderResult != ValueProviderResult.None)
            {
                var timezoneString = valueProviderResult.FirstValue;
                if (TimeZoneInfo.TryFindSystemTimeZoneById(timezoneString!, out TimeZoneInfo? timezoneInfo))
                {
                    bindingContext.Result = ModelBindingResult.Success(timezoneInfo);
                }
                else
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Invalid timezone");
                }
            }
            return Task.CompletedTask;
        }

    }
}
