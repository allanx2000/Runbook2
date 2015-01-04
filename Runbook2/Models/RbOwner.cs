using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Runbook2.Models
{
    public class RbOwner
    {
        public string Name { get; set; }
        public int? ID { get; private set; }

        public RbOwner(int? ID, string Name)
        {
            this.ID = ID;
            this.Name = Name;
        }

        public override bool Equals(object obj)
        {
            return obj is RbOwner ? ((RbOwner)obj).Name == this.Name : false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }


        internal void SetID(int id)
        {
            this.ID = id;
        }
    }
}
