using BudgetProgram.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;

namespace BudgetProgram.Helpers
{

    public static class Extensions
    {
        private static ApplicationDbContext db = new ApplicationDbContext();

        public static HouseHolds GetHousehold(this string userId)
        {
            var user = db.Users.Find(userId);
            if (user == null || user.HouseHoldId == null)
            {
                return null;
            }

            var hh = db.HouseHolds.Find(user.HouseHoldId);

            return hh; //returns entire household
        }
        public static string GetHouseHoldId(this IIdentity user)
        {
            var claimsIdentity = (ClaimsIdentity)user;
            var HouseHoldClaim = claimsIdentity.Claims.FirstOrDefault(c => c.Type == "HouseHoldId");

            if (HouseHoldClaim != null)
                return HouseHoldClaim.Value;
            else
                return null;
        }

        public static IdentityMessage InviteMessage(this Invite user)
        {
            var invite = db.Invites.FirstOrDefault(u => u.Id == user.Id);
            var msg = new IdentityMessage();
            var dt = DateTime.Now.AddDays(7).ToLongDateString();
            msg.Destination = invite.Email; //ConfigurationManager.AppSettings["ContactEmail"];
            msg.Body = "Hi," + invite.InviteSentBy + " " + "has invited you to join their household on Fruitful. Copy the following code and then go to the Fruitful website by clicking <a href=\"http://cesimmons-budgetprogram.azurewebsites.net\">here</a>. After registering, enter your code to join the household. This code will be active until" + dt + ", after that date you can request a new code from " + invite.InviteSentBy + " Here is your invite code: " + invite.InviteCode;
            msg.Subject = "Invitation to join Fruitful";

            return msg;
        }
       
        public static bool IsInHouseHold(this IIdentity user)
        {
            var cUser = (ClaimsIdentity)user;
            var hid = cUser.Claims.FirstOrDefault(c => c.Type == "HouseHoldId");
            return (hid != null && !string.IsNullOrWhiteSpace(hid.Value));
        }
        public static async Task RefreshAuthentication(this HttpContextBase context, ApplicationUser user)
        {
            context.GetOwinContext().Authentication.SignOut();
            await context.GetOwinContext().Get<ApplicationSignInManager>().SignInAsync(user, isPersistent: false, rememberBrowser: false);
        }
    }  


    public class AuthorizeHouseHoldRequired : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var isAuthorized = base.AuthorizeCore(httpContext);
            if (!isAuthorized)
            {
                return false;
            }
            return httpContext.User.Identity.IsInHouseHold();
        }
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "HouseHolds", action = "JoinCreateHouseHold" }));
            }
        }
    }
  
}
