using FileShareDataAccessLayer.Data;
using Microsoft.AspNetCore.Http;
using File = FileShareDataAccessLayer.Models.File;
using ApplicationUserFile = FileShareDataAccessLayer.Models.ApplicationUserFile;

namespace FileShareBusinessLayer.Helper
{
    public class FileHelper
    {
        private readonly ApplicationDbContext _context;
        public FileHelper(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Save(IFormFile file, string userEmail)
        {
            var user = _context.Users.First(x => x.Email == userEmail);

            var st = file.OpenReadStream();
            var mst = new MemoryStream();
            st.CopyTo(mst);

            foreach (var fileFromDb in _context.Files)
            {
                if(Pdkd2Helper.Verify(mst.ToArray(), fileFromDb.FileHash))
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

            dbFile.FilePath = "Files/" + dbFile.Id + Path.GetExtension(file.FileName);
            _context.SaveChanges();

            var fst = System.IO.File.Create(dbFile.FilePath, (int)file.Length);
            fst.Write(mst.ToArray(), 0, (int)file.Length);
            fst.Close();

            var userFile = _context.ApplicationUserFile.First(u => u.UserId == user.Id && u.FileId == dbFile.Id);
            userFile.FileName = file.FileName;
            _context.SaveChanges();
        }
    }
}