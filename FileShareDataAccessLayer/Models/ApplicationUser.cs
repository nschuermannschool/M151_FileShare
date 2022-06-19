using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileShareDataAccessLayer.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Models.File> Files { get; set; }
    }
}
