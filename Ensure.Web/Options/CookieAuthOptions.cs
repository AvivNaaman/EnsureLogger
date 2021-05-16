using System;
using System.Text;

namespace Ensure.Web.Options
{
    public class CookieAuthOptions
    {
        public string Key
        {
            get => key;
            set
            {
                key = value;
                keyBytes = Encoding.UTF8.GetBytes(value);
            }
        }

        public byte[] KeyBytes
        {
            get
            {
                return keyBytes;
            }
        }

        private byte[] keyBytes;
        private string key;
    }
}
