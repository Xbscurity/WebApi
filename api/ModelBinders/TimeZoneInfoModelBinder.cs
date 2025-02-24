using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace api.ModelBinders
{
    public class TimeZoneInfoModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (valueProviderResult != ValueProviderResult.None)
            {
                var timezoneString = valueProviderResult.FirstValue;
                try
                {
                    var timezoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezoneString);
                    bindingContext.Result = ModelBindingResult.Success(timezoneInfo);
                }
                catch (TimeZoneNotFoundException)
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Invalid timezone");
                }
            }

            return Task.CompletedTask;
        }
    }
}