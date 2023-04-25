#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace AvaloniaGUI.CodeHelpers;

public class IntegerType : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // Is a number? 
        if (!int.TryParse((string)value, out _))
        {
            return new ValidationResult("Not a number.");
        }

        // Number is valid 
        return ValidationResult.Success;
    }
}