#region

using System.ComponentModel.DataAnnotations;
using System.Numerics;
using AvaloniaGUI.CodeHelpers;

#endregion

namespace AvaloniaGUI.ViewModels.Dialog;

public class IsComplexNumber : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        Complex number;

        // Is a number? 
        if (!ComplexParser.TryParse((string)value, out number))
        {
            return new ValidationResult(false, "Not a complex number.");
        }

        // Number is valid 
        return new ValidationResult(true, null);
    }
}