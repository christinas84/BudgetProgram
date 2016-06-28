using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BudgetProgram.Models
{
    public class HouseHolds
    {
        public HouseHolds()
        {
            Users = new HashSet<ApplicationUser>();
            Accounts = new HashSet<Account>();
            Invites = new HashSet<Invite>();
            BudgetItems = new HashSet<BudgetItems>();
            this.Category = new HashSet<Categories>();

        }
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset? Updated { get; set; }

                

        public virtual ICollection<ApplicationUser> Users { get; set; }

        public virtual ICollection<Account> Accounts { get; set; }

        public virtual ICollection<Invite> Invites { get; set; }

        public virtual ICollection<BudgetItems> BudgetItems { get; set; }

        public virtual ICollection<Categories> Category { get; set; }
    }
}