using FileShareDataAccessLayer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileShareDataAccessLayer.Data
{
    public enum Roles 
    {
        Administrator
    }
    public class DbInitializer
    {
        public static async Task Seed(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) 
        {
            context.Database.EnsureCreated();

            #region Add Roles to DB
            if (!context.Roles.Any(r => r.Name == Roles.Administrator.ToString()))
            {
                await roleManager.CreateAsync(new IdentityRole { Name = Roles.Administrator.ToString() });
            }
            #endregion

            context.SaveChanges();

            #region Add local users to DB
            if (!context.Users.Any(u => u.UserName == "test@test.com"))
            {
                ApplicationUser user = new ApplicationUser { Email = "test@test.com", UserName = "test@test.com" };
                await userManager.CreateAsync(user, "123456");
            }

            if (!context.Users.Any(u => u.UserName == "admin@test.com"))
            {
                ApplicationUser user = new ApplicationUser { Email = "admin@test.com", UserName = "admin@test.com" };
                await userManager.CreateAsync(user, "123456");
            }
            #endregion

            context.SaveChanges();

            #region Add roles to users
            ApplicationUser adminUser = context.Users.SingleOrDefault(u => u.Email == "admin@test.com");
            ApplicationUser testUser = context.Users.SingleOrDefault(u => u.Email == "test@test.com");

            if (!(await userManager.IsInRoleAsync(adminUser, Roles.Administrator.ToString())))
            {
                await userManager.AddToRoleAsync(adminUser, Roles.Administrator.ToString());
            }
            #endregion

            context.SaveChanges();

            Models.File file = new Models.File() { FileHash = "TESTHASH", FilePath="PATH", Users = new List<ApplicationUser>() { adminUser } };
            context.Add(file);
            
            context.SaveChanges();

            ApplicationUserFile applicationUserFile = context.ApplicationUserFile.First(x => x.File == file);
            applicationUserFile.FileName = "Test";

            context.SaveChanges();

        }
    }
}
