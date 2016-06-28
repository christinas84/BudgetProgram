using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BudgetProgram.Models
{
    public class BudgetItems
    {
        public BudgetItems()
        {
            this.Transaction = new HashSet<Transactions>();
        }

        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Amount Limit")]
        public decimal AmountLimit { get; set; }

        public decimal Balance { get; set; }
        [Required]
        public int CategoryId { get; set; }

        public int HouseHoldId { get; set; }

        public string CreatorId { get; set; }

        public bool Income { get; set; }

        public int? WarningId { get; set; }

        public bool IsSoftDeleted { get; set; }




        public virtual Categories Category { get; set; }

        public virtual HouseHolds HouseHold { get; set; }

        public virtual ICollection<Transactions> Transaction { get; set; }

        public virtual ApplicationUser Creator { get; set; }

        public virtual Warnings Warning { get; set; }

    }
}