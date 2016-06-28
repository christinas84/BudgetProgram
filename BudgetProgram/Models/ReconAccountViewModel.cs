using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BudgetProgram.Models
{
    public class DashboardViewModel
    {
        public IEnumerable<ReconciledAccount> ReconciledAccounts { get; set; }
        public IEnumerable<BudgetItems> BudgetList { get; set; }
        public HouseHolds HouseHolds { get; set; }
        public IEnumerable<Account> Accounts { get; set; }
    }

    public class ManageAccountsViewModel
    {
        public IEnumerable<ReconciledAccount> ReconciledAccounts { get; set; }
        public Account Account { get; set; }
    }
}