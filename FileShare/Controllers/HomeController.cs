using FileShare.ViewModels;
using FileShareBusinessLayer.Helper;
using FileShareDataAccessLayer.Data;
using FileShareDataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace FileShare.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private FileHelper _fileHelper { get; set; }

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IHostingEnvironment hostingEnvironment)
        {
            _logger = logger;
            _fileHelper = new FileHelper(context, hostingEnvironment);
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(FileViewModel file)
        {
            if (file.File.Files.Count == 0) 
            {
                return View();
            }

            _fileHelper.Save(file.File.Files[0], User.Identity.Name);

            return View();
        }

        [Authorize]
        public IActionResult FileList()
        {
            var model = new List<FileViewModel>();
            var files = _fileHelper.GetUserFiles(User.Identity.Name);
            foreach (var file in files)
            {
                model.Add(new FileViewModel
                {
                    Id = file.FileId.ToString(),
                    FileName = file.FileName
                });
            }
            return View(model);
        }

        public IActionResult DownloadFile(string id, string fileName)
        {
            return File(_fileHelper.DownloadFile(id), "application/" + Path.GetExtension(fileName)?.TrimStart('.'), fileName);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}