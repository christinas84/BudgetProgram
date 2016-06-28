using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BudgetProgram.Models
{
    public class ReconciledAccount
    {
        public Account Account { get; set; }

        public decimal ReconBalance { get; set; }

        public decimal Balance { get; set; }
    }
}