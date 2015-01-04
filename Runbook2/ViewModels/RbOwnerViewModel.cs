using Runbook2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runbook2.ViewModels
{
    public class RbOwnerViewModel
    {
        private RbOwner owner;

        public RbOwnerViewModel(RbOwner owner)
        {
            // TODO: Complete member initialization
            this.owner = owner;            
        }
        public RbOwner Data { 
            get
            {
                return owner;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is RbOwnerViewModel ? ((RbOwnerViewModel)obj).Data.Name == this.Data.Name : false;
        }

        public override int GetHashCode()
        {
            return Data.Name.GetHashCode();
        }
    }
}
