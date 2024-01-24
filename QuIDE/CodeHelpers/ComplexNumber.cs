#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace QuIDE.CodeHelpers;

public class ComplexNumber : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        // Is a number? 
        return ComplexParser.TryParse((string) value, out _)
            ? ValidationResult.Success
            : new ValidationResult("Not a complex number.");
    }
}