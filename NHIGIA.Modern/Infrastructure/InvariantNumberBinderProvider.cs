using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace NHIGIA.Modern.Infrastructure;

/// <summary>
/// HTML number inputs always send a decimal point. Reading them with the server locale turned 1.5 into 15 on vi-VN machines.
/// </summary>
public sealed class InvariantNumberBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context.BindingInfo?.BinderType != null) return null;
        var type = context.Metadata.UnderlyingOrModelType;
        return type == typeof(decimal) || type == typeof(double) || type == typeof(float) ? new Binder() : null;
    }

    private sealed class Binder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext context)
        {
            var value = context.ValueProvider.GetValue(context.ModelName);
            if (value == ValueProviderResult.None) return Task.CompletedTask;
            context.ModelState.SetModelValue(context.ModelName, value);
            var type = context.ModelMetadata.UnderlyingOrModelType;
            if (string.IsNullOrWhiteSpace(value.FirstValue))
            {
                if (context.ModelMetadata.IsReferenceOrNullableType) context.Result = ModelBindingResult.Success(null);
                else context.ModelState.TryAddModelError(context.ModelName, "Vui lòng nhập giá trị số.");
                return Task.CompletedTask;
            }
            if (decimal.TryParse(value.FirstValue, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var number))
                context.Result = ModelBindingResult.Success(type == typeof(decimal) ? number : Convert.ChangeType(number, type, CultureInfo.InvariantCulture));
            else
                context.ModelState.TryAddModelError(context.ModelName, "Giá trị số không hợp lệ.");
            return Task.CompletedTask;
        }
    }
}
