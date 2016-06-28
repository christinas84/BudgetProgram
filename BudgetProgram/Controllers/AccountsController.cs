using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BudgetProgram.Models;
using Microsoft.AspNet.Identity;
using BudgetProgram.Helpers;

namespace BudgetProgram.Controllers
{
    [RequireHttps]
    [Authorize]
    [AuthorizeHouseHoldRequired]
    public class AccountsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Accounts
        public ActionResult Index()
        {
            var id = User.Identity.GetUserId();
            HouseHolds hh = id.GetHousehold();
            var visAccounts = hh.Accounts.Where(a => a.IsSoftDeleted != true);

            ViewBag.recAcct = visAccounts.SelectMany(a => a.Transactions).Where(t => t.Reconciled == true).Sum(t => t.Amount);
            ViewBag.totAcct = visAccounts.SelectMany(a => a.Transactions).Sum(t => t.Amount);

            var accountsList = (from account in db.Accounts.Include("Transactions")
                                where account.IsSoftDeleted != true && account.HouseHoldId == hh.Id
                                let reconciledI = (from transaction in account.Transactions 
                                                   where transaction.Reconciled == true &&
                                                   transaction.Income == true
                                                   select transaction.Amount).DefaultIfEmpty().Sum()
                                let reconciledE = (from transaction in account.Transactions
                                                   where transaction.Reconciled == true &&
                                                   transaction.Income == false
                                                   select transaction.Amount)
                                                   .DefaultIfEmpty().Sum()
                                let I = (from transaction in account.Transactions
                                                   where
                                                   transaction.Income == true
                                                   select transaction.Amount).DefaultIfEmpty().Sum()
                                let E = (from transaction in account.Transactions
                                                   where 
                                                   transaction.Income == false
                                                   select transaction.Amount)
                                                   .DefaultIfEmpty().Sum()
                                select new ReconciledAccount
                                {
                                    Account = account,
                                    ReconBalance = reconciledE - reconciledI,
                                    Balance = I - E
                                }).ToList();
            var model = new ManageAccountsViewModel
            {
                ReconciledAccounts = accountsList,
            };
            //var accounts = db.Accounts.Include(a => a.HouseHold);
            return View(model);
        }

        // GET: Accounts/Details/5
        public PartialViewResult _Details(int? id)
        {
            //var Uid = User.Identity.GetUserId();
            //HouseHolds hh = Uid.GetHousehold();
         
            Account account = db.Accounts.Find(id);
            
            //if (id == null)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //}
            //if (account == null)
            //{
            //    return HttpNotFound();
            //}
            return PartialView(account);
        }

        // GET: Accounts/Create
        public PartialViewResult _Create()
        {
            var hHoldId = int.Parse(User.Identity.GetHouseHoldId());
            ViewBag.message = db.HouseHolds.Find(hHoldId).Name;
            //ViewBag.HouseHoldId = new SelectList(db.HouseHolds, "Id", "Name");
            return PartialView();
        }

        // POST: Accounts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Created,Balance,StartBal")] Account account)
        {
            if (ModelState.IsValid)
            {
                account.Created = DateTimeOffset.Now;
              
                account.HouseHoldId = Convert.ToInt32(User.Identity.GetHouseHoldId());
                db.Accounts.Add(account);
                db.SaveChanges();
                var hId = Convert.ToInt32(User.Identity.GetHouseHoldId());
                var hh = db.HouseHolds.Find(hId);
                

                //Transactions originalTransaction = new Transactions()
                //{
               
                //    AccountId = account.Id,
                //    EnteredById = User.Identity.GetUserId(),
                //    Category = hh.Category.FirstOrDefault((m => m.Name == "Miscellaneous")),
                //    Transacted = DateTimeOffset.Now,
                //    Date = DateTimeOffset.Now,
                //    Amount = account.Balance,
                //    Description = "Starting balance",
                //    Income = true,
                //    Reconciled = true,
                //};

                //db.Transactions.Add(originalTransaction);
                //db.SaveChanges();
            }
            TempData["formInput"] = account;
                return RedirectToAction("Index");
        }
       
            //ViewBag.HouseHoldId = new SelectList(db.HouseHolds, "Id", "Name", account.HouseHoldId);

    // GET: Accounts/Edit/5
    public PartialViewResult _Edit(int? id)
        {
            Account account = db.Accounts.Find(id);
            var input = TempData["formInput"];
            return PartialView(account);        
            }

        // POST: Accounts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind (Include ="Id, Name, Balance,StartBal, Created, HouseHoldId, ReconBalance")] Account account)
        {
      
            if (ModelState.IsValid)
            {
                account.Updated = DateTimeOffset.Now;
                //db.Accounts.Attach(account);
                //db.Entry(account).Property("Name").IsModified = true;
                db.Entry(account).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            TempData["formInput"] = account;

            return RedirectToAction("Index");
        }


       

        public PartialViewResult _Transactions(int? id)
        {
            Account bankAccount = db.Accounts.Find(id);
            return PartialView(bankAccount);
        }

        // GET: BankAccounts/_Delete/5
        public PartialViewResult _Delete(int? id)
        {
            Account account = db.Accounts.Find(id);
            return PartialView(account);
        }

        // POST: Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int? id)
        {
            Account account = db.Accounts.Find(id);
            account.IsSoftDeleted = true;

            //db.Accounts.Remove(account);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
