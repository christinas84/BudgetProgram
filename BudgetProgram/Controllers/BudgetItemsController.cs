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

namespace BudgetProgram.Controllers
{
    [RequireHttps]
    [Authorize]
    [AuthorizeHouseHoldRequired]
    public class BudgetItemsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: BudgetItems
        public ActionResult Index()
        {
            
            //var budgetItems = db.BudgetItems.Include(b => b.Category).Include(b => b.Creator).Include(b => b.HouseHold).Include(b => b.Warning);
            var houseId = int.Parse(User.Identity.GetHouseHoldId());
            var hh = db.HouseHolds.FirstOrDefault(h => h.Id == houseId);
            //var budIt = hh.BudgetItems.Where(b => b.IsSoftDeleted != true);
            //var visAccounts = hh.Accounts.Where(a => a.IsSoftDeleted != true);

            //var recAcct = visAccounts.SelectMany(a => a.Transactions).Where(t => t.Reconciled == true).Sum(t => t.Amount);
            //var totAcct = visAccounts.SelectMany(a => a.Transactions).Sum(t => t.Amount);
            // balance = (from budgetList in db.BudgetItems select new BudgetItems { Balance = recAcct - totAcct });
            //var model = new DashboardViewModel
            //{
            //    //Balance = balance,
            //    BudgetList = balance
            //};

            return View(hh);
          
        }

        // GET: BudgetItems/Details/5
        public PartialViewResult _Details(int? id)
        {
            //if (id == null)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //}
            BudgetItems budgetItems = db.BudgetItems.Find(id);
            //if (budgetItems == null)
            //{
            //    return HttpNotFound();
            //}
            return PartialView(budgetItems);
        }

        // GET: BudgetItems/Create
        public PartialViewResult _Create()
        {
            var houseId = int.Parse(User.Identity.GetHouseHoldId());
            var hh = db.HouseHolds.FirstOrDefault(h => h.Id == houseId);
            var HouseHold = hh;
            HouseHold = new HouseHolds();
            //ViewBag.CategoryId = new SelectList(db.Category, "Id", "Name");
            //ViewBag.CreatorId = new SelectList(db.ApplicationUsers, "Id", "FirstName");
            //ViewBag.HouseHoldId = new SelectList(db.HouseHolds, "Id", "Name");
            //ViewBag.WarningId = new SelectList(db.Warnings, "Id", "Warning");

            ViewBag.CategoryId = new SelectList(db.Category.Where(c => c.HouseHoldId == hh.Id), "Id", "Name");
            ViewBag.WarningId = new SelectList(db.Warning, "Id", "WarningLimit");
            ViewBag.HouseHold = hh.Name;

            return PartialView();
        }

        // POST: BudgetItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,AmountLimit,Balance,CategoryId,HouseHoldId,CreatorId,Income,WarningId")] BudgetItems budgetItems)
        {
            if (ModelState.IsValid)
            {
                budgetItems.HouseHoldId = int.Parse(User.Identity.GetHouseHoldId());
                //budgetItems.Balance = 0;
                budgetItems.CreatorId = User.Identity.GetUserId();

                db.BudgetItems.Add(budgetItems);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
           
            var userId = User.Identity.GetUserId();
            var hh = userId.GetHousehold();

            ViewBag.CategoryId = new SelectList(db.Category.Where(c=>c.HouseHoldId == hh.Id), "Id", "Name", budgetItems.CategoryId);
            //ViewBag.CreatorId = new SelectList(db.ApplicationUsers, "Id", "FirstName", budgetItems.CreatorId);
            //ViewBag.HouseHoldId = new SelectList(db.HouseHolds, "Id", "Name", budgetItems.HouseHoldId);
            ViewBag.WarningId = new SelectList(db.Warning, "Id", "WarningLimit", budgetItems.WarningId);
            return View(budgetItems);
        }

        // GET: BudgetItems/Edit/5
        public PartialViewResult _Edit(int? id)
        {
            var userId = User.Identity.GetUserId();
            var hh = userId.GetHousehold();
            BudgetItems budgetItems = db.BudgetItems.Find(id);

            ViewBag.CategoryId = new SelectList(db.Category.Where(c=>c.HouseHoldId == hh.Id), "Id", "Name",budgetItems.CategoryId);
            ViewBag.WarningId = new SelectList(db.Warning, "Id", "WarningLimit", budgetItems.WarningId);
            return PartialView(budgetItems);
        }

        // POST: BudgetItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,AmountLimit,Balance,CategoryId,HouseHoldId,CreatorId,Income,WarningId")] BudgetItems budgetItems)
        {
            var userId = User.Identity.GetUserId();
            var hh = userId.GetHousehold();
            if (ModelState.IsValid)
            {
                budgetItems.HouseHoldId = hh.Id;
                
                db.Entry(budgetItems).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index","BudgetItems");
            }
            ViewBag.CategoryId = new SelectList(db.Category.Where(c=>c.HouseHoldId==hh.Id), "Id", "Name", budgetItems.CategoryId);
            //ViewBag.CreatorId = new SelectList(db.ApplicationUsers, "Id", "FirstName", budgetItems.CreatorId);
            //ViewBag.HouseHoldId = new SelectList(db.HouseHolds, "Id", "Name", budgetItems.HouseHoldId);
            ViewBag.WarningId = new SelectList(db.Warning, "Id", "WarningLimit", budgetItems.WarningId);
            return View(budgetItems);
        }

        // GET: BudgetItems/Delete/5
        public PartialViewResult _Delete(int? id)
        {
            BudgetItems budgetItems = db.BudgetItems.Find(id);
            //if (budgetItems == null)
            //{
            //    return HttpNotFound();
            //}
            return PartialView(budgetItems);
        }

        // POST: BudgetItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            BudgetItems budgetItems = db.BudgetItems.Find(id);
            var userId = User.Identity.GetUserId();
            var hh = userId.GetHousehold();
            var transac = db.Transactions.Where(t => t.BudgetItemId == id);
            var misc = hh.Category.FirstOrDefault(c => c.Name == "Miscellaneous");
            foreach (var trans in transac)
                trans.Category.Id = misc.Id;
            budgetItems.IsSoftDeleted = true;

            db.BudgetItems.Remove(budgetItems);
            db.SaveChanges();
            return RedirectToAction("Index","BudgetItems");
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
