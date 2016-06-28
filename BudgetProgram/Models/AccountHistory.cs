using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BudgetProgram.Models
{
    public class AccountHistory
    {
        public int Id { get; set; }

        public decimal OldAmount { get; set; }

        public decimal NewAmount { get; set; }

        public int AccountId { get; set; }

        public string ModifiedByUserId { get; set; }

        public DateTimeOffset Modified { get; set; }

        //public decimal ReconBalance { get; set; }


        public virtual Account Account { get; set; }

        public virtual ApplicationUser ModifiedByUser { get; set; }


    }
}