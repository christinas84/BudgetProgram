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
    [Authorize]
    [AuthorizeHouseHoldRequired]
    public class CategoriesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        //// GET: Categories
        //public ActionResult Index()
        //{
        //    var category = db.Category.Include(c => c.HouseHold);
        //    return View(category.ToList());
        //}

        //// GET: Categories/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Categories categories = db.Category.Find(id);
        //    if (categories == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(categories);
        //}

        // GET: Categories/Create
        public PartialViewResult _Create()
        {
            ViewBag.HouseHoldId = new SelectList(db.HouseHolds, "Id", "Name");
            return PartialView();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,HouseHoldId,Name")] Categories categories)
        {
            if (ModelState.IsValid)
            {
                categories.HouseHoldId = int.Parse(User.Identity.GetHouseHoldId());
                db.Category.Add(categories);
                db.SaveChanges();
                return RedirectToAction("Details", "HouseHolds");
            }

            //ViewBag.HouseHoldId = new SelectList(db.HouseHolds, "Id", "Name", categories.HouseHoldId);
            return RedirectToAction("Details","HouseHolds");
        }

        // GET: Categories/Edit/5
        public PartialViewResult _Edit(int? id)
        {
           
            Categories categories = db.Category.Find(id);
           
            //ViewBag.HouseHoldId = new SelectList(db.HouseHolds, "Id", "Name", categories.HouseHoldId);
            return PartialView(categories);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,HouseHoldId,Name")] Categories categories)
        {
            if (ModelState.IsValid)
            {
                var originalCat = db.Category.AsNoTracking().FirstOrDefault(c => c.Id == categories.Id);
                if (categories.Name != originalCat.Name)
                {
                    var transactions = db.Transactions.Where(t => t.Category.Name == originalCat.Name);
                    foreach (var trans in transactions)
                        trans.Category.Name = categories.Name;

                    var budgetItems = db.BudgetItems.Where(b => b.Category.Name == originalCat.Name);
                    foreach (var bud in budgetItems)
                        bud.Category.Name = categories.Name;
                }
                db.Entry(categories).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details","HouseHolds");
            }
            //ViewBag.HouseHoldId = new SelectList(db.HouseHolds, "Id", "Name", categories.HouseHoldId);
            return RedirectToAction("Details", "HouseHolds");
        }

        // GET: Categories/Delete/5
        public PartialViewResult _Delete(int? id)
        {
            //if (id == null)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //}
            Categories categories = db.Category.Find(id);
            //if (categories == null)
            //{
            //    return HttpNotFound();
            //}
            return PartialView(categories);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Categories categories = db.Category.Find(id);
            var transactions = db.Transactions.Where(t => t.CategoryId == id);
            var budgetItems = db.BudgetItems.Where(b => b.CategoryId == id);
            var misc = db.Category.FirstOrDefault(c => c.Name == "Miscellaneous").Id;
            foreach (var trans in transactions) trans.CategoryId = misc;
            foreach (var bud in budgetItems) bud.CategoryId = misc;
            db.Category.Remove(categories);
            db.SaveChanges();
            return RedirectToAction("Details","HouseHolds");
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
