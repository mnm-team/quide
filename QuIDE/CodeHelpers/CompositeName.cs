#region

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

#endregion

namespace QuIDE.CodeHelpers;

public class CompositeName : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        const string pattern = @"^\s*[a-zA-Z_][a-zA-Z0-9_]*\s*$";
        Regex regex = new Regex(pattern);

        value ??= string.Empty;

        return regex.Match(value.ToString()).Success
            ? ValidationResult.Success
            : new ValidationResult($"Entered name must match pattern: {regex}");
    }
}