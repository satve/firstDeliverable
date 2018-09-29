namespace BlogApp.Migrations
{
    using BlogApp.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<BlogApp.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(Models.ApplicationDbContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            if (!context.Roles.Any(r => r.Name == "Admin"))
            {
                roleManager.Create(new IdentityRole { Name = "Admin" });
            }
            if (!context.Roles.Any(r => r.Name == "Moderator"))
            {
                roleManager.Create(new IdentityRole { Name = "Moderator" });
            }

            //Check if the admin user is already created.
            //If not, create it.
            ApplicationUser adminUser = null;
            ApplicationUser moderatorUser = null;


            if (!context.Users.Any(p => p.UserName == "admin01@gmail.com"))
            {
                adminUser = new ApplicationUser();
                adminUser.UserName = "admin01@gmail.com";
                adminUser.Email = "admin01@gmail.com";
                adminUser.FirstName = "Admin";
                adminUser.LastName = "Dhillon";
                adminUser.DisplayName = "Admin Dhillon";
                userManager.Create(adminUser, "Password-1");
            }
            else
            {
                adminUser = context.Users.Where(p => p.UserName == "admin01@gmail.com")
                    .FirstOrDefault();
            }

            if (!context.Users.Any(p => p.UserName == "moderator01@gmail.com"))
            {
                moderatorUser = new ApplicationUser();
                moderatorUser.UserName = "moderator01@gmail.com";
                moderatorUser.Email = "moderator01@gmail.com";
                moderatorUser.FirstName = "Moderator";
                moderatorUser.LastName = "Dhillon";
                moderatorUser.DisplayName = "Moderator Dhillon";

                userManager.Create(moderatorUser, "Password-2");
            }
            else
            {
                moderatorUser = context.Users.Where(p => p.UserName == "moderator01@gmail.com")
                    .FirstOrDefault();
            }

            //Check if the adminUser is already on the Admin role
            //If not, add it.
            if (!userManager.IsInRole(adminUser.Id, "Admin"))
            {
                userManager.AddToRole(adminUser.Id, "Admin");
            }

            //Check if the moderatorUser is already on the Admin role
            //If not, add it.
            if (!userManager.IsInRole(moderatorUser.Id, "Moderator"))
            {
                userManager.AddToRole(moderatorUser.Id, "Moderator");
            }
        }
    }
}
