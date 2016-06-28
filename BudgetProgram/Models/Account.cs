using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BudgetProgram.Models
{
    public class Account
    {
        public Account()
        {
            this.Transactions = new HashSet<Transactions>();
        }


       public int Id { get; set; }

       [Required]
       public string Name { get; set; }

       public DateTimeOffset Created { get; set; }

       public DateTimeOffset? Updated { get; set; }

       public decimal Balance { get; set; }

       public int HouseHoldId { get; set; }

       public bool IsSoftDeleted { get; set; }
        
       public decimal ReconBalance { get; set; }

       public decimal StartBal { get; set; }



        public virtual HouseHolds HouseHold { get; set; }

       public virtual ICollection<Transactions> Transactions { get; set; }


    }
}