using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BudgetProgram.Models
{
    public class Categories
    {
        public Categories()
        {
            this.Transactions = new HashSet<Transactions>();
        }
        public int Id { get; set; }

        public string Name { get; set; }
        
        public int? HouseHoldId { get; set; }


        public virtual HouseHolds HouseHold { get; set; }
        public virtual ICollection<Transactions> Transactions { get; set; }
    }
}

    
   

