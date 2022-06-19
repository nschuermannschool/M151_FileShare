using FileShareDataAccessLayer.Data;
using Microsoft.AspNetCore.Http;
using File = FileShareDataAccessLayer.Models.File;
using ApplicationUserFile = FileShareDataAccessLayer.Models.ApplicationUserFile;
using Microsoft.AspNetCore.Hosting;

namespace FileShareBusinessLayer.Helper
{
    public class FileHelper
    {
        private readonly ApplicationDbContext _context;
        private readonly IHostingEnvironment _hostingEnvironment;
        public FileHelper(ApplicationDbContext context, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        public void Save(IFormFile file, string userEmail)
        {
            var user = _context.Users.First(x => x.Email == userEmail);

            var st = file.OpenReadStream();
            var mst = new MemoryStream();
            st.CopyTo(mst);

            foreach (var fileFromDb in _context.Files)
            {
                if (Pdkd2Helper.Verify(mst.ToArray(), fileFromDb.FileHash))
                {
                    fileFromDb.Users.Add(user);
                    _context.SaveChanges();

                    var userFileFromDb = _context.ApplicationUserFile.First(x => x.UserId == user.Id && x.FileId == fileFromDb.Id);
                    userFileFromDb.FileName = file.FileName;
                    _context.SaveChanges();

                    return;
                }
            }

            var dbFile = new File
            {
                FileHash = Pdkd2Helper.CreateHash(mst.ToArray())
            };
            var userList = new List<FileShareDataAccessLayer.Models.ApplicationUser>();
            userList.Add(user);
            dbFile.Users = userList;

            _context.Files.Add(dbFile);

            string directory = Path.Combine(_hostingEnvironment.ContentRootPath, "Files") ;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            dbFile.FilePath = Path.Combine("Files",  dbFile.Id + Path.GetExtension(file.FileName));
            _context.SaveChanges();

            var fst = System.IO.File.Create(dbFile.FilePath, (int)file.Length);
            fst.Write(mst.ToArray(), 0, (int)file.Length);
            fst.Close();

            var userFile = _context.ApplicationUserFile.First(u => u.UserId == user.Id && u.FileId == dbFile.Id);
            userFile.FileName = file.FileName;
            _context.SaveChanges();
        }

        public List<ApplicationUserFile> GetUserFiles(string userEmail)
        {
            var user = _context.Users.First(x => x.Email == userEmail);
            return _context.ApplicationUserFile.Where(f => f.User == user).ToList();
        }

        public byte[] DownloadFile(string id)
        {
            var mst = new MemoryStream();

            var fileFromDb = _context.Files.First(f => f.Id.ToString() == id);
            using (var st = System.IO.File.Open(fileFromDb.FilePath, FileMode.Open))
            {
                st.CopyTo(mst);
            }

            return mst.ToArray();
        }
    }
}