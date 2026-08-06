using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace IAX.IXApi.Modules.Identity.Roles
{
    public class AspNetRole : IdentityRole<string>
    {
        [MaxLength(255)]
        public string? Description { get; set; }
    }
}

