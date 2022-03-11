using System.ComponentModel.DataAnnotations;
using Trakx.Utils.Extensions;

namespace Trakx.Utils.Attributes;

public class IsValidEthereumAddressAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not string strValue || string.IsNullOrWhiteSpace(strValue)) return false;
        if (strValue.IsValidEthereumAddress()) return true;
            
        ErrorMessage = "Must be a valid ethereum address.";
        return false;
    }
}