using System.ComponentModel.DataAnnotations;

namespace AutoNexus.Application.Common.Validation
{
    public class CpfAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return true;

            return CpfValidator.IsValid(value.ToString()!);
        }

        public override string FormatErrorMessage(string name)
        {
            return "O CPF informado é inválido.";
        }
    }
}