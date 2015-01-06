using Runbook2.Models;
using Runbook2.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runbook2
{
    public class Utilities
    {

        private static char[] commaDelim;

        public static char[] CommaDelim
        {
            get
            {
                if (commaDelim == null)
                    commaDelim = new char[] { ',' };

                return commaDelim;
            }
        }


        public static bool HasCircular(List<RbTask> tasks)
        {
            return HasCircular(null, tasks);
        }

        /// <summary>
        /// Determines if the preRequisites will cause a circular dependency
        /// </summary>
        /// <param name="task"></param>
        /// <param name="preReqs"></param>
        /// <returns></returns>
        public static bool HasCircular(RbTask task, IList<RbTask> preReqs, List<int> previousTasks = null)
        {
            if (task.ID == null)
                return false;

            if (previousTasks == null)
            {
                previousTasks = new List<int>();
            }
            else if (previousTasks.Contains(task.ID.Value))
                return true;

            if (preReqs.Count == 0)
                return false;
            else
            {

                previousTasks.Add(task == null ? -1 : task.ID.Value);

                foreach (RbTask t in preReqs)
                {
                    var newList = new List<int>(previousTasks);

                    if (HasCircular(t, t.PreReqs, newList))
                        return true;
                }

                return false;
            }
        }

        public static IEnumerable<int> GetInts(string commaSeparatedInts)
        {
            return commaSeparatedInts.Split(CommaDelim, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt32(x));
        }

    }

}
