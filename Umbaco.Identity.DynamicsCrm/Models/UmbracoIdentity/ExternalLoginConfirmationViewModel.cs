using System.ComponentModel.DataAnnotations;

namespace Umbaco.Identity.DynamicsCrm.Models.UmbracoIdentity
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
