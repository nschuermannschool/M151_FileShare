using System.ComponentModel.DataAnnotations;

namespace FileShare.ViewModels
{
    public class FileViewModel
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        [Required]
        public IFormCollection File { get; set; }
    }
}
