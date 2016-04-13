using System.ComponentModel.DataAnnotations;

namespace Umbaco.Identity.DynamicsCrm.Models.UmbracoIdentity
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
