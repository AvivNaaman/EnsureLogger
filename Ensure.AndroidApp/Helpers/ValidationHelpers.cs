using System;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Android.App;
using Android.Content;

namespace Ensure.AndroidApp.Helpers
{
    /// <summary>
    /// Contains useful input validation helper methods
    /// </summary>
    public static class ValidationHelpers
    {
        // Regular expression for checking password
        static Regex pwdRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#$^+=!*()@%&]).{8,}$");
        /// <summary>
        /// Returns whether the given password is valid,
        /// And shows an error dialog if not.
        /// </summary>
        public static bool ValidatePassword(string password, Context context)
        {
            if (pwdRegex.IsMatch(password))
            {
                return true;
            }
            else
            {
                ShowErrorDialog("Password must include a lower case, an upper case," +
                    " a number and a special charachter, as well as at least 8 digits.", context);
                return false;
            }
        }

        /// <summary>
        /// Returns whether the given email address is valid,
        /// and shows an error dialog if not.
        /// </summary>
        public static bool ValidateEmail(string email, Context context)
        {
            MailAddress e;
            try
            {
                e = new MailAddress(email);
                return true;
            } catch
            {
                ShowErrorDialog("Invalid email address.", context);
                return false;
            }
        }

        /// <summary>
        /// Returns whether ALL the given values are not empty strings,
        /// and shows an error dialog otherwise
        /// </summary>
        /// <param name="values">The string values to check</param>
        public static bool ValidateFilled(Context context, params string[] values)
        {
            if (values.Any(v => string.IsNullOrEmpty(v)))
            {
                ShowErrorDialog("All fields must be filled.", context);
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Shows a validation error dialog
        /// </summary>
        /// <param name="message">The dialog's message</param>
        /// <param name="title">The dialog's title</param>
        public static void ShowErrorDialog(string message, Context context, string title = "Error")
        {
            if (context != null)
            {
                new AlertDialog.Builder(context)
                           .SetTitle("Error")
                           .SetMessage(message)
                           .SetCancelable(true)
                           .Create().Show();
            }
        }
    }
}
