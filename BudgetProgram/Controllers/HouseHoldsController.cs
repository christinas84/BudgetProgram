using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BudgetProgram.Models;
using Microsoft.AspNet.Identity.Owin;
using BudgetProgram.Helpers;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using SendGrid;
using System.Configuration;
using System.Net.Mail;
using System.Web.Security;
using Newtonsoft.Json;
using System.Collections;

namespace BudgetProgram.Controllers
{
    [RequireHttps]
    public class HouseHoldsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationUserManager _userManager;
        private ApplicationSignInManager _signInManager;


        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        public HouseHoldsController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }
        public HouseHoldsController(){}
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        // GET: HouseHolds
        [AuthorizeHouseHoldRequired]
        public ActionResult Index()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var hh = user.HouseHoldId;

            //var hhId = db.HouseHolds.Where(h => h.Id == user.Id).ToList();

            if (User.IsInRole("Admin"))
            {
                return View(db.HouseHolds.ToList());
            }
            else
            {
                return View(hh);

            }
        }

        //Get: Home
        [AuthorizeHouseHoldRequired]
        public ActionResult Home()
        {
            // var user = db.Users.Find(User.Identity.GetUserId());
            //HouseHolds hh = db.HouseHolds.Find(user.HouseHoldId);

            // return View(hh);
            var userId = User.Identity.GetUserId();
            //var hh = userId.GetHousehold();
            var hh = db.HouseHolds.Find(int.Parse(User.Identity.GetHouseHoldId()));
            var accounts = hh.Accounts;
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
                                    ReconBalance = reconciledI - reconciledE,
                                    Balance = account.StartBal - reconciledE + reconciledI
                                }).ToList();
            var budgetsList = hh.BudgetItems.Where(b => b.IsSoftDeleted != true).OrderBy(b => b.Name);
            var HouseHolds = hh;
            //var Accounts = accounts;
            var model = new DashboardViewModel
            {
                ReconciledAccounts = accountsList,
                BudgetList = budgetsList,
                HouseHolds = HouseHolds,
                Accounts = accounts
            };
            InviteCleanUp(hh.Id);
            return View(model);
        }
       

        private void InviteCleanUp (int id)
        {
            //using (var context = new MyContext())
            //{
            //    var parent = context.Parents.Include(p => p.Children)
            //        .SingleOrDefault(p => p.Id == parentId);

            //    foreach (var child in parent.Children.ToList())
            //        context.Children.Remove(child);

            //    context.SaveChanges();
            //}
            var dt = DateTime.Now;
            var hh = db.HouseHolds.Find(id);
            var invList = hh.Invites.ToList();
            foreach (var i in invList)
            {

                if (DateTime.Compare(i.InviteDate.Date.AddDays(7).Date, dt) < 0)
                {
                    db.Entry(i).State = EntityState.Deleted;

                }
                //hh.Invites.Remove(i);
            }
            db.SaveChanges();

        }
        //public override int SaveChanges()
        //{
        //    Invites
        //       .Local
        //       .Where(r => r.HouseHold == null)
        //       .ToList()
        //       .ForEach(r => Invites.Remove(r));

        //    return base.SaveChanges();
        //}


        public ActionResult GetChart()
        {
            var hId = Convert.ToInt32(User.Identity.GetHouseHoldId());
            var hh = db.HouseHolds.Find(hId);
            //var house = hh.Accounts.Where(a => a.Transactions.Any(t => t.Income == true)).SelectMany(a =>)
            decimal income = hh.Accounts.SelectMany(t => t.Transactions).Where(t => t.Income == true).Sum(t => t.Amount);
            decimal expense = hh.Accounts.SelectMany(t => t.Transactions).Where(t => t.Income != true).Sum(t => t.Amount);
            var s = new[] { new { label = "Total Income", value= Convert.ToInt32(income) },
    new { label= "Total Expense", value= Convert.ToInt32(expense) } };
    //new { label= "Take-out", value= 7 },
    //new { label= "Groceries", value= 10 },
    //new { label= "Utilities", value= 10 },
    //new { label= "Travel", value= 20 }};
            return Content(JsonConvert.SerializeObject(s), "application/json");
        }

        public ActionResult GetBudgetChart()
        {
            //var budBal = budget.Category.Transactions.Where(t => t.Income != true).Sum(t => t.Amount);

            var hId = Convert.ToInt32(User.Identity.GetHouseHoldId());
            var hh = db.HouseHolds.Find(hId);
            //var budgetsList = hh.BudgetItems.Where(b => b.IsSoftDeleted != true).OrderBy(b => b.Name);
            ArrayList dnut = new ArrayList();
            foreach (var bd in hh.BudgetItems) {
                decimal itm = hh.BudgetItems.Where(b => b.Name == bd.Name).SelectMany(t => t.Transaction).Sum(t => t.Amount);
                dnut.Add(new { label = bd.Name, value = Convert.ToInt32(itm) });
            }


            ////decimal Food = budBal;
            //decimal food = hh.Accounts.SelectMany(t => t.Transactions).Where(t => t.BudgetItem.Name == "Food").Sum(t => t.Amount); /*error says not set to instance of an object use new keyword*/

            //decimal Travel = hh.BudgetItems.SelectMany(t => t.Transaction).Where(t => t.BudgetItem.Name == "Travel").Sum(t => t.Amount);
            //var s = new[] { new { label = "Travel", value= Convert.ToInt32(Travel) },
            //        new { label= "Food", value= Convert.ToInt32(food) } };

            return Content(JsonConvert.SerializeObject(dnut), "application/json");
        }


        // GET: HouseHolds/Details/5
        [AuthorizeHouseHoldRequired]
        public ActionResult Details()
        {

            var hId = Convert.ToInt32(User.Identity.GetHouseHoldId());
            var hh = db.HouseHolds.Find(hId);

            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            ViewBag.UserId = new SelectList(hh.Users, "Id", "FirstName");

            if (hh == null)
            {
                return RedirectToAction("Create");
            }
            return View(hh);
        }
        

        //get: HouseHolds/Invite
        [AuthorizeHouseHoldRequired]
        public ActionResult Invite()
        {
            return View();
        }


        //Post: HouseHolds/Invite
        [HttpPost]
        [AuthorizeHouseHoldRequired]
        public async Task<ActionResult> Invite([Bind(Include = "Id,Email, InviteCode, InvitedUser, HouseHoldId")] Invite invite, HouseHolds HouseHold)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                var user = db.Users.FirstOrDefault(u => u.Id == userId);
                var hHoldId = int.Parse(User.Identity.GetHouseHoldId());
                invite.HouseHoldId = hHoldId;
                invite.InviteCode = Membership.GeneratePassword(8, 2);
                invite.InviteSentBy = user.FullName;
                invite.InviteDate = DateTimeOffset.Now;
                db.Invites.Add(invite);
                db.SaveChanges();
                var es = new EmailService();
                var msg = invite.InviteMessage();
                await es.SendAsync(msg);

                return RedirectToAction("Details", "HouseHolds", (new { id = user.HouseHoldId }));                
            }
            

            return View(invite);
        }


        //leave household post:
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeHouseHoldRequired]
        public async Task<ActionResult> LeaveHouseHold(string id)
        {
                     
            if (ModelState.IsValid)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                user.HouseHoldId = null;
                db.SaveChanges();

                await ControllerContext.HttpContext.RefreshAuthentication(user);

                return RedirectToAction("Home", "Households");
            }
            return View();
        }
    

        //Join household Post
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Join(string InviteCode)
        {
            var email = User.Identity.GetUserName();
            var Invuser = db.Invites.FirstOrDefault(u => u.InviteCode == InviteCode && u.Email == email);
            var codeValidFrom = DateTime.Now.AddDays(-7);
            ViewBag.JoinErrorMessage = TempData["ErrorMessage"];
            var user = db.Users.Find(User.Identity.GetUserId());



            if (ModelState.IsValid)
            {
                if (Invuser == null)
                {
                    TempData["ErrorMessage"] = "The invite code you entered and email do not match.";
                    return RedirectToAction("JoinCreateHouseHold");
                }              
                else
                {
                    if (Invuser.InviteDate < codeValidFrom)
                    {
                        TempData["ErrorMessage"] = "This code is no longer valid.";
                        return RedirectToAction("JoinCreateHouseHold");
                    }                  

                    if (Invuser != null)
                    {
                        var usEr = db.Users.FirstOrDefault(u => u.Email == Invuser.Email);

                        usEr.HouseHoldId = Invuser.HouseHoldId;
                        db.SaveChanges();


                        db.Invites.Remove(Invuser);
                        db.SaveChanges();                  


                        await ControllerContext.HttpContext.RefreshAuthentication(user);

                        return RedirectToAction("Home", "HouseHolds");
                    }
                }
            }

            return RedirectToAction("JoinCreateHouseHold");
        }
        

        //GET: HouseHolds/Create
        public ActionResult JoinCreateHouseHold()
        {
            return View();
        }

        // POST: HouseHolds/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> JoinCreateHouseHold([Bind(Include = "Id,Name,Created")] HouseHolds houseHolds)
        {
            //var hhId = db.HouseHolds.Find(houseHolds.Id);
            var uId = User.Identity.GetUserId();
            var usr = db.Users.FirstOrDefault(u => u.Id == uId);
            //var usr = db.Users.Find(User.Identity.GetUserId());

            if (ModelState.IsValid)
            {
                if (usr.HouseHoldId != null)
                {
                    ViewBag.ErrorMessage = "You may not create more than one household";
                    return View();
                }

                houseHolds.Created = DateTime.Now;

                db.HouseHolds.Add(houseHolds);
                db.SaveChanges();            
                usr.HouseHoldId = houseHolds.Id;
                db.SaveChanges();
                await ControllerContext.HttpContext.RefreshAuthentication(usr);
                return RedirectToAction("Home","HouseHolds");
            }

            return View();
        }

        // GET: HouseHolds/Edit/5
        [AuthorizeHouseHoldRequired]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HouseHolds houseHolds = db.HouseHolds.Find(id);
            if (houseHolds == null)
            {
                return HttpNotFound();
            }
            return View(houseHolds);
        }

        // POST: HouseHolds/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeHouseHoldRequired]
        public ActionResult Edit([Bind(Include = "Id,Name,Created,Updated")] HouseHolds houseHolds)
        {
            if (ModelState.IsValid)
            {
                houseHolds.Updated = DateTime.Now;

                db.Entry(houseHolds).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Home", "HouseHolds");
            }
            return View(houseHolds);
        }

        // GET: HouseHolds/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HouseHolds houseHolds = db.HouseHolds.Find(id);
            if (houseHolds == null)
            {
                return HttpNotFound();
            }
            return View(houseHolds);
        }

        // POST: HouseHolds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            HouseHolds houseHolds = db.HouseHolds.Find(id);
            db.HouseHolds.Remove(houseHolds);
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
