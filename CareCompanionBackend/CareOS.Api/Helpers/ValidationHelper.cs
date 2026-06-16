using System.Text.RegularExpressions;

namespace CareOS.Api.Helpers
{
    public static class ValidationHelper
    {
        // EMAIL VALIDATION
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        // PHONE NUMBER VALIDATION (Pakistan format)
        public static bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Remove spaces, dashes, parentheses
            phone = Regex.Replace(phone, @"[\s\-\(\)]", "");

            // Pakistan: +92-3XX-XXXXXXX or 03XX-XXXXXXX
            var regex = new Regex(@"^(\+92|0)?3\d{9}$");
            return regex.IsMatch(phone);
        }

        // PIN VALIDATION (4 digits)
        public static bool IsValidPin(string pin)
        {
            if (string.IsNullOrWhiteSpace(pin))
                return false;

            return Regex.IsMatch(pin, @"^\d{4}$");
        }

        // PASSWORD STRENGTH
        public static (bool isValid, string message) ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password is required");

            if (password.Length < 8)
                return (false, "Password must be at least 8 characters");

            if (!Regex.IsMatch(password, @"[A-Z]"))
                return (false, "Password must contain at least one uppercase letter");

            if (!Regex.IsMatch(password, @"[a-z]"))
                return (false, "Password must contain at least one lowercase letter");

            if (!Regex.IsMatch(password, @"\d"))
                return (false, "Password must contain at least one number");

            if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]"))
                return (false, "Password must contain at least one special character");

            return (true, "Password is strong");
        }

        // FILE SIZE VALIDATION (in bytes)
        public static bool IsValidFileSize(long fileSize, long maxSizeMB = 5)
        {
            long maxSizeBytes = maxSizeMB * 1024 * 1024;
            return fileSize <= maxSizeBytes;
        }

        // IMAGE FILE EXTENSION
        public static bool IsValidImageExtension(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(fileName).ToLower();
            return allowedExtensions.Contains(extension);
        }
    }
}