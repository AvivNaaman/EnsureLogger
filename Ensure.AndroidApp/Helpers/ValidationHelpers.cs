using System;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Android.App;
using Android.Content;

namespace Ensure.AndroidApp.Helpers
{
    public static class ValidationHelpers
    {
        public static bool ValidatePassword(string password, Context context)
        {
            if (Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#$^+=!*()@%&]).{8,}$"))
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
        public static void ShowErrorDialog(string message, Context context, string title = "Error")
        {
            if (context != null)
            {
                new AlertDialog.Builder(context)
                           .SetTitle("Error")
                           .SetMessage(message)
                           .Create().Show();
            }
        }
    }
}
