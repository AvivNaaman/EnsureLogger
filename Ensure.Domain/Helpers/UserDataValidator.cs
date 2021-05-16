using System;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Ensure.Domain.Helpers
{
    public class UserDataValidator
    {
        // Regular expression for checking password
        private static Regex pwdRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#$^+=!*()@%&]).{8,}$");
        private static Regex userNameRegex = new Regex(@"^[-0-9A-Za-z_]{5,15}$");

        /// <summary>
        /// Returns whether the given password is valid
        /// </summary>
        public static bool ValidatePassword(string password)
        {
            return pwdRegex.IsMatch(password);
        }


        /// <summary>
        /// Returns whether the given password is valid,
        /// and it's and the vertification matches.
        /// </summary>
        public static bool ValidatePassword(string password, string passwordVertification)
        {
            return password == passwordVertification && ValidatePassword(password);
        }

        /// <summary>
        /// Returns whether the given email address is valid
        /// </summary>
        public static bool ValidateEmail(string email)
        {
            MailAddress e;
            try
            {
                e = new MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns whether ALL the given values are not empty strings
        /// </summary>
        /// <param name="values">The string values to check</param>
        public static bool ValidateNotNullOrEmpty(params string[] values)
        {
            return !values.Any(v => string.IsNullOrEmpty(v));
        }

        /// <summary>
        /// Returns whether the user name is valid.
        /// </summary>
        public static bool ValidateUserName(string userName)
        {
            return userNameRegex.IsMatch(userName);
        }

        public static bool ValidateTarget(int target) => target >= 0;
    }
}
