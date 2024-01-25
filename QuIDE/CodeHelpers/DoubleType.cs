#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace QuIDE.CodeHelpers;

public class DoubleType : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        // Is a number? 
        return !double.TryParse((string)value, out _)
            ? new ValidationResult("Not a number.")
            :
            // Number is valid 
            ValidationResult.Success;
    }
}