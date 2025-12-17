namespace AutoNexus.Application.Common.Validation
{
    public static class CpfValidator
    {
        public static bool IsValid(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf)) return false;

            string cleanCpf = GetOnlyNumbers(cpf);

            if (!IsFormatValid(cleanCpf)) return false;

            return ValidateDigits(cleanCpf);
        }

        public static string GetOnlyNumbers(string cpf)
        {
            return cpf.Trim().Replace(".", "").Replace("-", "");
        }

        #region Private Helpers

        private static bool IsFormatValid(string cpf)
        {
            if (cpf.Length != 11) return false;

            bool allDigitsEqual = new string(cpf[0], cpf.Length) == cpf;
            if (allDigitsEqual) return false;

            return true;
        }

        private static bool ValidateDigits(string cpf)
        {
            string tempCpf = cpf.Substring(0, 9);

            // 1º Dígito Verificador
            int[] multiplier1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string firstDigit = CalculateDigit(tempCpf, multiplier1);

            // 2º Dígito Verificador
            int[] multiplier2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string secondDigit = CalculateDigit(tempCpf + firstDigit, multiplier2);

            return cpf.EndsWith(firstDigit + secondDigit);
        }

        private static string CalculateDigit(string source, int[] multipliers)
        {
            int sum = 0;
            for (int i = 0; i < multipliers.Length; i++)
            {
                sum += int.Parse(source[i].ToString()) * multipliers[i];
            }

            int remainder = sum % 11;
            int digit = remainder < 2 ? 0 : 11 - remainder;

            return digit.ToString();
        }

        #endregion
    }
}