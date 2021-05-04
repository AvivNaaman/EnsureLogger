using System;
namespace Ensure.Web.Models
{
    /// <summary>
    /// Contains all the data in the password reset model.
    /// </summary>
    public class PasswordResetTokenModel
    {
        public const string ResetPasswordPurpose = "ResetPassword";

        public string Purpose { get; set; } = ResetPasswordPurpose;
        public string Email { get; set; }
        public DateTime Produced { get; set; } = DateTime.UtcNow;
    }
}
