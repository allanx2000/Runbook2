using Innouvous.Utils.MVVM;
using Runbook2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runbook2.ViewModels
{
    public class RbTagViewModel : ObservableClass
    {
        private RbTag data;
        
        public RbTagViewModel(RbTag tag)
        {
            // TODO: Complete member initialization
            this.data = tag;
        }

        public RbTag Data
        {
            get
            {
                return data;
            }
        }

        public string Name
        {
            get
            {
                return data.Name;
            }
            set
            {
                data.Name = value;
                RaiseEvent("Name");
            }
        }

        public int ID
        {
            get
            {
                return data.ID;
            }
        }
    }
}
