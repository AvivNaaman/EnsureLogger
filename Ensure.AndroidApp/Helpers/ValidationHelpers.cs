using System;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Android.App;
using Android.Content;
using Ensure.Domain.Helpers;

namespace Ensure.AndroidApp.Helpers
{
    /// <summary>
    /// Contains useful input validation helper methods
    /// </summary>
    public static class ValidationHelpers
    {
        /// <summary>
        /// Returns whether the given password is valid,
        /// And shows an error dialog if not.
        /// </summary>
        public static bool ValidatePassword(string password, Context context)
        {
            if (UserDataValidator.ValidatePassword(password))
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
            if (UserDataValidator.ValidateEmail(email))
            {
                return true;
            }
            else
            {
                ShowErrorDialog("Invalid email address.", context);
                return false;
            }
        }


        /// <summary>
        /// Returns whether the given email address is valid,
        /// and shows an error dialog if not.
        /// </summary>
        public static bool ValidateUserName(string email, Context context)
        {
            if (UserDataValidator.ValidateUserName(email))
            {
                return true;
            }
            else
            {
                ShowErrorDialog("Invalid user name", context);
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
            if (UserDataValidator.ValidateNotNullOrEmpty(values))
            {
                return true;
            }
            else
            {
                ShowErrorDialog("All fields must be filled.", context);
                return false;
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

        public static bool ValidateTarget(string target, Context context)
        {
            if (int.TryParse(target, out int t) && t >= 0) return true;
            else
            {
                ShowErrorDialog("Target should be an integer with a value of at least 0.", context);
                return false;
            }
        }
    }
}
