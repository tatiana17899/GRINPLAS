using Microsoft.AspNetCore.Identity;
using System;


namespace GRINPLAS.Models
{
    public class ApplicationUser : IdentityUser
    {
        public Cliente Cliente { get; set; }
    }
}