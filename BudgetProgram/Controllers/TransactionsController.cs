using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BudgetProgram.Models;
using BudgetProgram.Helpers;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;

namespace BudgetProgram.Controllers
{
    [RequireHttps]
    public class TransactionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Transactions
        public ActionResult Index()
        {
            //var transactions = db.Transactions.Include(t => t.Account).Include(t => t.Category).Include(t => t.TransactionType);
            //return View(transactions.ToList());
            var hh = db.HouseHolds.Find(int.Parse(User.Identity.GetHouseHoldId()));
            return View(hh.Accounts.SelectMany(t=>t.Transactions).Where(a => a.IsSoftDeleted != true).OrderBy(a => a.Date).ToList());
        }

        //// GET: Transactions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transactions transactions = db.Transactions.Find(id);
            if (transactions == null)
            {
                return HttpNotFound();
            }
            return View(transactions);
        }

        // GET: Transactions/Create
        public ActionResult Create()
        {
            var userId = User.Identity.GetUserId();
            var hh = userId.GetHousehold();

            ViewBag.AccountId = new SelectList(db.Accounts.Where(a=>a.IsSoftDeleted!=true && a.HouseHoldId == hh.Id), "Id", "Name");
            ViewBag.CategoryId = new SelectList(db.Category.Where(c=>c.HouseHoldId == hh.Id), "Id", "Name");
            //ViewBag.TransactionTypeId = new SelectList(db.TransactionTypes, "Id", "Name");
            ViewBag.BudgetItemsId = new SelectList(db.BudgetItems.Where(b => b.IsSoftDeleted != true && b.HouseHoldId == hh.Id), "Id", "Name");
            return View();
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,ReconStatus,Description,IsNotBudgetItem,Transacted,Amount,Date,AccountId,CategoryId, BudgetItemId,Income,Reconciled,EnteredById")] Transactions transactions)
        {
            var id = User.Identity.GetUserId();
            var hh = id.GetHousehold();

            if (ModelState.IsValid)
            {
                var acc = db.Accounts.FirstOrDefault(a => a.Id == transactions.AccountId);
                var bud = db.BudgetItems.FirstOrDefault(b => b.Id == transactions.BudgetItemId);
                if (transactions.BudgetItemId != null)
                    transactions.CategoryId = bud.CategoryId;
                else if (transactions.BudgetItemId == null && transactions.CategoryId == null)
                    transactions.CategoryId = hh.Category.FirstOrDefault(c => c.Name == "Miscellaneous").Id;

                //balance calculations
                acc.Balance = acc.StartBal + acc.Transactions.Where(t => t.Income == true).Sum(s => s.Amount) - acc.Transactions.Where(t => t.Income == false).Sum(s => s.Amount);
                if (transactions.BudgetItemId != null) bud.Balance = transactions.GetBudgetBalance();

                //db.SaveChanges();//should this not be here?

                transactions.Date = DateTimeOffset.Now;
                transactions.EnteredById = id;
                db.Transactions.Add(transactions);

                db.SaveChanges();

                var budBal = bud.Category.Transactions.Where(t => t.Income != true).Sum(t => t.Amount);

                ////check budget warnings and send alert
                if (transactions.BudgetItemId != null && bud.Warning.WarningLimit != "None" && (bud.AmountLimit - budBal <= Convert.ToDecimal(bud.Warning.WarningLimit)))
                {

                    var users = db.Users.Where(u => u.HouseHoldId == bud.HouseHoldId);
                    foreach (var user in users)
                    {
                        var es = new EmailService();
                        var msg = user.CreateBudgetWarningMessage(bud, budBal);
                        await es.SendAsync(msg);
                    }
                    TempData["AlertMessage"] = "You have exceeded your budget limit!";
                    //return RedirectToAction("Create");

                }

                return RedirectToAction("Index");
            }

            ViewBag.AccountId = new SelectList(db.Accounts.Where(a=>a.IsSoftDeleted!=true && a.HouseHoldId == hh.Id), "Id", "Name", transactions.AccountId);
            ViewBag.CategoryId = new SelectList(db.Category.Where(c=>c.HouseHoldId == hh.Id), "Id", "Name", transactions.CategoryId);          
            ViewBag.BudgetItemsId = new SelectList(db.BudgetItems.Where(b => b.IsSoftDeleted != true && b.HouseHoldId == hh.Id),"Id", "Name");
            return View(transactions);
        }

        // GET: Transactions/Edit/5
        //public PartialViewResult _Edit(int? id)
          public ActionResult Edit(int? id)

        {
            var userId = User.Identity.GetUserId();
            var hh = userId.GetHousehold();

            Transactions transactions = db.Transactions.Find(id);
            
            ViewBag.AccountId = new SelectList(db.Accounts.Where(a=>a.IsSoftDeleted!=true && a.HouseHoldId == hh.Id), "Id", "Name", transactions.AccountId);
            ViewBag.CategoryId = new SelectList(db.Category.Where(c=>c.HouseHoldId == hh.Id), "Id", "Name", transactions.CategoryId);
            ViewBag.BudgetItemId = new SelectList(db.BudgetItems.Where(b=>b.IsSoftDeleted!=true && b.HouseHoldId == hh.Id), "Id", "Name", transactions.BudgetItemId);
            //return PartialView(transactions);
            return View(transactions);

        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ReconStatus,Description,ReconAmount,Amount,Date,AccountId,CategoryId,EnteredById,Reconciled")] Transactions transactions)
        {
            var id = User.Identity.GetUserId();
            var hh = id.GetHousehold();
            if (ModelState.IsValid)
            {
                var originalTrans = db.Transactions.AsNoTracking().FirstOrDefault(t => t.Id == transactions.Id);
                var acc = db.Accounts.FirstOrDefault(a => a.Id == originalTrans.AccountId);
                var bud = db.BudgetItems.FirstOrDefault(b => b.Id == originalTrans.BudgetItemId);

                //balance calculations
                acc.Balance = originalTrans.RevertAccountBalance();
                acc = db.Accounts.FirstOrDefault(a => a.Id == transactions.AccountId);
                acc.Balance = transactions.GetAccountBalance();

                if (bud != null)
                    bud.Balance = originalTrans.RevertBudgetBalance();

                bud = db.BudgetItems.FirstOrDefault(b => b.Id == transactions.BudgetItemId);
                if (bud != null)
                    bud.Balance = transactions.GetBudgetBalance();

                transactions.EnteredById = id;
                db.SaveChanges();

                ////check budget warnings and send alert
                //if (transactions.BudgetItem != null && (budget.AmountLimit - budget.Balance <= Convert.ToDecimal(budget.Warning.WarningLimit)))
                //{
                //    var users = db.Users.Where(u => u.HasAdminRights == true && u.HouseholdId == budget.HouseholdId);
                //    foreach (var user in users)
                //    {
                //        var es = new EmailService();
                //        var msg = user.CreateBudgetWarningMessage(budget);
                //        es.SendAsync(msg);
                //    }
                //}

                //set category
                transactions.BudgetItem = bud;
                if (transactions.BudgetItemId != null) transactions.CategoryId = transactions.BudgetItem.CategoryId;

                db.SaveChanges();
                db.Entry(transactions).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
        }
            ViewBag.AccountId = new SelectList(db.Accounts.Where(a => a.IsSoftDeleted != true && a.HouseHoldId == hh.Id), "Id", "Name", transactions.AccountId);
            ViewBag.CategoryId = new SelectList(db.Category.Where(c => c.HouseHoldId == hh.Id), "Id", "Name", transactions.CategoryId);
            ViewBag.BudgetItemsId = new SelectList(db.BudgetItems.Where(b => b.IsSoftDeleted != true && b.HouseHoldId == hh.Id), "Id", "Name");
            //ViewBag.TransactionTypeId = new SelectList(db.TransactionTypes, "Id", "Name", transactions.TransactionTypeId);
            return View(transactions);
        }

        // GET: Transactions/Delete/5
        public ActionResult Delete(int? id)
        {
            
            Transactions transactions = db.Transactions.Find(id);
           
            return View(transactions);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Transactions transaction = db.Transactions.Find(id);
            var userId = User.Identity.GetUserId();
            var account = db.Accounts.FirstOrDefault(a => a.Id == transaction.AccountId);
            var budget = db.BudgetItems.FirstOrDefault(b => b.Id == transaction.BudgetItemId);

            //balance calculations
            account.Balance = transaction.RevertAccountBalance();
            if (budget != null) budget.Balance = transaction.RevertBudgetBalance();     
            db.Transactions.Remove(transaction);
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
