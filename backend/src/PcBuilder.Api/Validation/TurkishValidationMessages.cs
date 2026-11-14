using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace PcBuilder.Api.Validation;

/// <summary>
/// Replaces ASP.NET Core's built-in English validation texts with Turkish ones, so every
/// 400 response the mobile app receives can be shown to the user as-is.
/// Attribute messages ([Range], [Required]...) are set on the DTOs themselves.
/// </summary>
public static class TurkishValidationMessages
{
    public const string ValidationTitle = "Bir veya daha fazla doğrulama hatası oluştu.";

    /// <summary>Messages produced while binding query strings, route values and JSON bodies.</summary>
    public static void Apply(DefaultModelBindingMessageProvider messages)
    {
        messages.SetAttemptedValueIsInvalidAccessor((value, field) => $"'{value}' değeri {field} alanı için geçerli değil.");
        messages.SetNonPropertyAttemptedValueIsInvalidAccessor(value => $"'{value}' değeri geçerli değil.");
        messages.SetUnknownValueIsInvalidAccessor(field => $"{field} alanına gönderilen değer geçerli değil.");
        messages.SetNonPropertyUnknownValueIsInvalidAccessor(() => "Gönderilen değer geçerli değil.");
        messages.SetValueIsInvalidAccessor(value => $"'{value}' değeri geçerli değil.");
        messages.SetValueMustBeANumberAccessor(field => $"{field} alanı bir sayı olmalı.");
        messages.SetNonPropertyValueMustBeANumberAccessor(() => "Değer bir sayı olmalı.");
        messages.SetValueMustNotBeNullAccessor(value => $"'{value}' değeri boş olamaz.");
        messages.SetMissingBindRequiredValueAccessor(field => $"{field} alanı için bir değer gönderilmeli.");
        messages.SetMissingKeyOrValueAccessor(() => "Bir değer gönderilmeli.");
        messages.SetMissingRequestBodyRequiredValueAccessor(() => "İstek gövdesi boş olamaz.");
    }

    /// <summary>
    /// When a JSON body can't be parsed (broken JSON, text where a number is expected),
    /// the model error is added without a message and the framework would fall back to
    /// the English "The input was not valid.". Fill those in before the 400 is built.
    /// </summary>
    public static void FillMissingMessages(ModelStateDictionary modelState)
    {
        foreach (var entry in modelState.Values)
        {
            for (var i = 0; i < entry.Errors.Count; i++)
            {
                if (string.IsNullOrEmpty(entry.Errors[i].ErrorMessage))
                {
                    entry.Errors[i] = new ModelError(
                        "Gönderilen değer okunamadı. JSON biçimini ve alan tiplerini kontrol edin.");
                }
            }
        }
    }

    /// <summary>Overrides the English title of ValidationProblemDetails responses.</summary>
    public static void TranslateTitle(ProblemDetailsContext context)
    {
        if (context.ProblemDetails is HttpValidationProblemDetails)
        {
            context.ProblemDetails.Title = ValidationTitle;
        }
    }
}
