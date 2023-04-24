#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace AvaloniaGUI.CodeHelpers;

public class ComplexNumber : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // Is a number? 
        if (!ComplexParser.TryParse((string)value, out _))
        {
            return new ValidationResult("Not a complex number.");
        }

        // Number is valid 
        return ValidationResult.Success;
    }
}