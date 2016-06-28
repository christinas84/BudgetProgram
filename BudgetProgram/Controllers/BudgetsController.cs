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

namespace BudgetProgram.Controllers
{
    [RequireHttps]
    [AuthorizeHouseHoldRequired]
    public class BudgetsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Budgets
        public ActionResult Index()
        {
            var budget = db.Budget.Include(b => b.HouseHold);
            return View(budget.ToList());
        }

        // GET: Budgets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Budget budget = db.Budget.Find(id);
            if (budget == null)
            {
                return HttpNotFound();
            }
            return View(budget);
        }

        // GET: Budgets/Create
        public ActionResult Create()
        {
            ViewBag.HouseHoldId = new SelectList(db.HouseHolds, "Id", "Name");
            return View();
        }

        // POST: Budgets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Created,Updated,HouseHoldId")] Budget budget)
        {
            if (ModelState.IsValid)
            {
                db.Budget.Add(budget);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.HouseHoldId = new SelectList(db.HouseHolds, "Id", "Name", budget.HouseHoldId);
            return View(budget);
        }

        // GET: Budgets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Budget budget = db.Budget.Find(id);
            if (budget == null)
            {
                return HttpNotFound();
            }
            ViewBag.HouseHoldId = new SelectList(db.HouseHolds, "Id", "Name", budget.HouseHoldId);
            return View(budget);
        }

        // POST: Budgets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Created,Updated,HouseHoldId")] Budget budget)
        {
            if (ModelState.IsValid)
            {
                db.Entry(budget).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.HouseHoldId = new SelectList(db.HouseHolds, "Id", "Name", budget.HouseHoldId);
            return View(budget);
        }

        // GET: Budgets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Budget budget = db.Budget.Find(id);
            if (budget == null)
            {
                return HttpNotFound();
            }
            return View(budget);
        }

        // POST: Budgets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Budget budget = db.Budget.Find(id);
            db.Budget.Remove(budget);
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
