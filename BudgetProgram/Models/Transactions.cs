using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BudgetProgram.Models
{
    public class Transactions
    {
        
        public int Id { get; set; }

        public string ReconStatus { get; set; }

        public string Description { get; set; }
        
        public decimal ReconAmount { get; set; }

        public decimal Amount { get; set; }

        public DateTimeOffset Date { get; set; }

        [Required]
        [Display(Name = "Account Name")]
        public int AccountId { get; set; }

        public bool IsSoftDeleted { get; set; }

        public bool Income { get; set; }

        public bool Reconciled { get; set; }

        [Required]
        [Display(Name = "Transaction Date")]
        public DateTimeOffset Transacted { get; set; }

        public int? CategoryId { get; set; }

        public string EnteredById { get; set; }

        public int? BudgetItemId { get; set; }

        public bool IsNotBudgetItem { get; set; }




        public virtual Categories Category { get; set; }

        public virtual Account Account { get; set; }
   
        public virtual ApplicationUser EnteredBy { get; set; }

        public virtual BudgetItems BudgetItem { get; set; }


    }
}