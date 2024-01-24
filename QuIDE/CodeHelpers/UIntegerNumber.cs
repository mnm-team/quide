#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace QuIDE.CodeHelpers;

public class UIntegerNumber : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        // Is a whole number? 
        return !uint.TryParse((string)value, out _)
            ? new ValidationResult("Not an unsigned number.")
            :
            // Number is valid 
            ValidationResult.Success;
    }
}