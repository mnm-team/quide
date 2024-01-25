#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace QuIDE.CodeHelpers;

public class IntegerNumber : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        // Is a whole number? 
        return !int.TryParse((string)value, out _)
            ? new ValidationResult("Not a number.")
            :
            // Number is valid 
            ValidationResult.Success;
    }
}