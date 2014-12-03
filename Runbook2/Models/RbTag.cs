using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Runbook2.Models
{
    public class RbTag
    {
        public string Name { get; set; }
        public int ID { get; private set; }
        public int? TagOrder { get; private set; }

        public RbTag(int ID, string Name)
        {
            this.ID = ID;
            this.Name = Name;
        }

        public void ClearTagOrder()
        {
            TagOrder = null;
        }

        public void SetTagOrder(int order)
        {
            TagOrder = order;
        }
    }
}
