using System;
using System.Linq;

namespace Ensure.Web.Helpers
{
    public static class DateTimeHelper
    {
        /// <summary>
        /// Parses a date of format dd-MM-yyyy to a DateTime object.
        /// If failed, returns null
        /// </summary>
        /// <param name="input">The input string</param>
        public static DateTime? FastParseFormattedDate(this string input)
        {
            try
            {
                var splitted = input.Split("-"); // split to 3 params
                if (splitted.Length < 3) return null; // if no 3, failed
                // init new datetime
                return new DateTime(int.Parse(splitted[2]), int.Parse(splitted[1]), int.Parse(splitted[0]));
            }
            catch { }
            return null;
        }
    }
}
