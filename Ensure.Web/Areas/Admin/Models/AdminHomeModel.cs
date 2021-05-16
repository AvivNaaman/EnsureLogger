using System;
using System.Collections.Generic;

namespace Ensure.Web.Areas.Admin.Models
{
    public class AdminHomeModel
    {
        // for all last 30 days stuff
        public DateTime StartDayFor30 { get; set; }


        /* Last 30 days ensures stats */
        public int[] EnsuresData { get; set; }

        /* Users joined in the last 30 days */
        public int[] UsersData { get; set; }

        public int[] TasteData { get; set; }


        public int TotalRegisteredUsers { get; set; }
        public int TotalLoggedEnsures { get; set; }
    }

}
