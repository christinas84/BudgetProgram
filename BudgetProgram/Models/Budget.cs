using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BudgetProgram.Models
{
    public class Budget
    {
       

        public int Id { get; set; }

        public string Name { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset? Updated { get; set; }

        public int HouseHoldId { get; set; }


        public virtual HouseHolds HouseHold { get; set; }              

        

    }
}