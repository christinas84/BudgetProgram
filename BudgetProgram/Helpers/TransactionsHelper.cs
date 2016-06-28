using BudgetProgram.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BudgetProgram.Helpers
{
    
        public static class TransactionHelper
    {
        private static ApplicationDbContext db = new ApplicationDbContext();

        //Account Balances
        public static decimal GetAccountBalance(this Transactions transaction)
        {
            return ModifyAccountBalance(transaction, false);
        }

        public static decimal RevertAccountBalance(this Transactions transaction)
        {
            return ModifyAccountBalance(transaction, true);
        }

        private static decimal ModifyAccountBalance(this Transactions transaction, bool Delete)
        {
            var account = db.Accounts.FirstOrDefault(a => a.Id == transaction.AccountId);
            bool AddMoney;

            if (Delete) AddMoney = !transaction.Income; 
            else AddMoney = transaction.Income;

            if (AddMoney == true)
                account.Balance += transaction.Amount;
            else
                account.Balance -= transaction.Amount;

            return account.Balance;
        }

        //Budget Balances
        public static decimal GetBudgetBalance(this Transactions transaction)
        {
            return ModifyBudgetBalance(transaction, false);
        }

        public static decimal RevertBudgetBalance(this Transactions transaction)
        {
            return ModifyBudgetBalance(transaction, true);
        }

        private static decimal ModifyBudgetBalance(this Transactions transaction, bool Delete)
        {
            var budget = db.BudgetItems.FirstOrDefault(b => b.Id == transaction.BudgetItemId);
            bool AddMoney;

            if (Delete) AddMoney = !transaction.Income;
            else AddMoney = transaction.Income;

            if (AddMoney == true)
            {
                if (budget.Income == true)
                    budget.Balance += transaction.Amount;
                else budget.Balance -= transaction.Amount;
            }
            else
            {
                if (budget.Income == true)
                    budget.Balance -= transaction.Amount;
                else budget.Balance += transaction.Amount;
            }            
            return budget.Balance;            
        }

        public static IdentityMessage CreateBudgetWarningMessage(this ApplicationUser user, BudgetItems budget, decimal budBal)
        {
            var admin = user;
            var msg = new IdentityMessage();
            msg.Destination = admin.Email; //ConfigurationManager.AppSettings["ContactEmail"];
            msg.Body = "This is a notification from Fruitful to let you know that a budget in your household -" + budget.Name + " - is getting close to its limit. You requested to be notified when the budget approached $" + budget.Warning.WarningLimit + " and the balance is currently at $" + budBal + ". <br/><br/> To view your budget and transactions, click <a href=\"https://csimmons-budgetprogram.azurewebsites.net/BudgetItems/Index\">here</a>. To see your household overview, click <a href=\"https://csimmons-budgetprogram.azurewebsites.net/Households/Index\">here</a>.";
            msg.Subject = "Warning! Budget Limit has been exceeded";
            //the " + budget.Name + "     " + budget.HouseHold + "
            return msg;
        }
    }
}
