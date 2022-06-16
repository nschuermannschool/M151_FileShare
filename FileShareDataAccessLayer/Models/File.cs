using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileShareDataAccessLayer.Models
{
    public class File
    {
        public Guid Id { get; set; }
        public string FileHash { get; set; }
        public string FilePath { get; set; }
        public ICollection<ApplicationUser> Users { get; set; }
    }
}
