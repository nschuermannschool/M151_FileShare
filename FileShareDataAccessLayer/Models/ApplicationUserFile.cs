using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileShareDataAccessLayer.Models
{
    public class ApplicationUserFile
    { 
        public Guid FileId { get; set; }
        public Models.File File { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string? FileName { get; set; }
    }
}
