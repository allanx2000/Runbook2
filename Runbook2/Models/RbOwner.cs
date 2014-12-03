using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Runbook2.Models
{
    public class RbOwner
    {
        public string Name { get; set; }
        public int ID { get; private set; }

        public RbOwner(int ID, string Name)
        {
            this.ID = ID;
            this.Name = Name;
        }
    }
}
