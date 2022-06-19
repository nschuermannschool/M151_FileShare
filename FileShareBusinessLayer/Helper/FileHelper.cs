using FileShareDataAccessLayer.Data;
using Microsoft.AspNetCore.Http;
using File = FileShareDataAccessLayer.Models.File;
using ApplicationUserFile = FileShareDataAccessLayer.Models.ApplicationUserFile;
using Microsoft.AspNetCore.Hosting;
using FileShareDataAccessLayer.Models;

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
            ApplicationUser user;

            user = _context.Users.First(x => x.Email == userEmail);
            
            var st = file.OpenReadStream();
            var mst = new MemoryStream();
            st.CopyTo(mst);

            File existingFile = null;

            foreach (var fileFromDb in _context.Files.ToList())
            {
                if (Pdkd2Helper.Verify(mst.ToArray(), fileFromDb.FileHash))
                {
                    existingFile = fileFromDb;
                }
            }

            if (existingFile != null)
            {
                var users = new List<ApplicationUser>();
                foreach (var appUserFile in _context.ApplicationUserFile.ToList())
                {
                    if (appUserFile.FileId == existingFile.Id)
                    {
                        users.Add(_context.Users.First(x => x.Id == appUserFile.UserId));
                    }
                }
                existingFile.Users = users;

                var adwe = _context.Files.First(x => x.Id == existingFile.Id);
                adwe.Users.Add(user);

                _context.SaveChanges();

                var userFileFromDb = _context.ApplicationUserFile.First(x => x.UserId == user.Id && x.FileId == existingFile.Id);
                userFileFromDb.FileName = file.FileName;
                
                _context.SaveChanges();

                return;

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

        public void DeleteFile(string id, string userEmail)
        {
            var user = _context.Users.First(x => x.Email == userEmail);
            var userFile = _context.ApplicationUserFile.First(a => a.UserId == user.Id && a.FileId.ToString() == id);
            _context.ApplicationUserFile.Remove(userFile);
            _context.SaveChanges();

            if(!_context.ApplicationUserFile.Any(f => f.FileId.ToString() == id))
            {
                var file = _context.Files.First(f => f.Id.ToString() == id);
                System.IO.File.Delete(file.FilePath);
                _context.Files.Remove(file);
                _context.SaveChanges();
            }
        }
    }
}