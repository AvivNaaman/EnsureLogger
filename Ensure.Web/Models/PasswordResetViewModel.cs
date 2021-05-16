using System;
using System.ComponentModel.DataAnnotations;
using Ensure.Web.Data;

namespace Ensure.Web.Models
{
    public class PasswordResetViewModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Token { get; set; }
        [Required]
        [Display(Name = "New Password")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
        [Required]
        [Display(Name = "New Password (Vertification)")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword))]
        public string NewPasswordVertification { get; set; }
    }
}
