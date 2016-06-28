using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BudgetProgram.Models;
using System.Security.Principal;
using System.Linq;
using System.Web.Routing;
using System.Web.Mvc;
using System.Web;
using BudgetProgram;
using Microsoft.AspNet.Identity.Owin;

namespace BudgetProgram.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Display(Name = "User Name")]
        public string FullName { get { return FirstName + " " + LastName; } }
        public string DisplayName { get; set; }
        //public string Name { get; set; }
        public int? HouseHoldId { get; set; }
        public virtual HouseHolds HouseHold { get; set; }


        //public virtual HouseHolds HouseHold { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            userIdentity.AddClaim(new Claim("HouseHoldId", HouseHoldId.ToString()));
            return userIdentity;
        }
        //public ApplicationUser()
        //{
        //    transactions = new HashSet<Transactions>();
        //}

        //public string FirstName { get; set; }
        //public string LastName { get; set; }
        //[Display(Name = "User Name")]
        //public string FullName { get { return FirstName + " " + LastName; } }
        //public string DisplayName { get; set; }        
    }
}


public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }


    public DbSet<Transactions> Transactions { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<AccountHistory> AccountHistory { get; set; }
    public DbSet<Budget> Budget { get; set; }
    public DbSet<HouseHolds> HouseHolds { get; set; }
    public DbSet<Categories> Category { get; set; }
    public DbSet<BudgetItems> BudgetItems { get; set; }
    //public DbSet<TransactionTypes> TransactionTypes { get; set; }
    public DbSet<Invite> Invites { get; set; }
    public DbSet<Warnings> Warning { get; set; }


    //public System.Data.Entity.DbSet<BudgetProgram.Models.Warnings> Warnings { get; set; }
}
