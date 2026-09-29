using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace NHIGIA.Modern.Infrastructure;

// FormData and HTML number inputs send decimal points irrespective of server locale.
public sealed class InvariantCoordinateBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext context)
    {
        var value = context.ValueProvider.GetValue(context.ModelName);
        if (value == ValueProviderResult.None) return Task.CompletedTask;
        context.ModelState.SetModelValue(context.ModelName, value);
        if (string.IsNullOrWhiteSpace(value.FirstValue))
            context.Result = ModelBindingResult.Success(null);
        else if (double.TryParse(value.FirstValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) && double.IsFinite(number))
            context.Result = ModelBindingResult.Success(number);
        else
            context.ModelState.TryAddModelError(context.ModelName, "Tọa độ/độ chính xác không hợp lệ.");
        return Task.CompletedTask;
    }
}
