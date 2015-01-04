using Runbook2.Models;
using Runbook2.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runbook2
{
    /// <summary>
    /// Storing State, Need to serialize the objects to strings or something
    /// </summary>
    [Serializable]
    public class TasksServiceState
    {
        [Serializable]
        public struct Owner
        {
            public string Name { get; set; }
            public int ID { get; set; }
        }

        [Serializable]
        public struct Tag
        {
            public string Name { get; set; }
            public int ID { get; set; }
        }

        [Serializable]
        public struct Task
        {
            public string Name { get; set; }
            public int? ID { get; set; }
            public int Duration { get; set; }
            public string Owners { get; set; }
            public string Tags { get; set; }
            public string PreReqs { get; set; }
            public string Notes { get; set; }
            public DateTime? ManualStartTime { get; set; }
        }

        public DateTime MinDate { get; set; }
        
        
        public List<Task> Tasks { get; set; }

        public List<Owner> Owners { get; set; }

        public List<Tag> Tags { get; set; }

        public TasksServiceState()
        {
            MinDate = DateTime.Now;

            Tasks = new List<Task>();
            Owners = new List<Owner>();
            Tags = new List<Tag>();
        }


        public static TasksServiceState FromTaskService(IEnumerable<RbTask> tasks, IEnumerable<RbOwner> owners, IEnumerable<RbTag> tags)
        
        {
            var svc = TasksService.Service;

            var state = new TasksServiceState();
            state.MinDate = svc.MinDate;

            foreach (RbOwner owner in owners)
            {
                var o = new Owner()
                {
                    Name = owner.Name,
                    ID = owner.ID.Value
                };

                state.Owners.Add(o);
            }

            foreach (RbTag tag in tags)
            {
                var t = new Tag()
                {
                    Name = tag.Name,
                    ID = tag.ID.Value
                };
            
                state.Tags.Add(t);
            }

            foreach (RbTask task in tasks)
            {
                var t = new Task()
                {
                    ID = task.ID,
                    Name = task.Description,
                    Duration = task.Duration,
                    Owners = MakeString(task.Owners),
                    PreReqs = MakeString(task.PreReqs),
                    Tags = MakeString(task.Tags),
                    ManualStartTime = task.GetManualStartTime(),
                    Notes = task.Notes
                };

                state.Tasks.Add(t);
            }

            return state;

        }

        private static string MakeString(List<RbOwner> owners)
        {
            return String.Join(",", from o in owners select o.ID);
        }

        private static string MakeString(List<RbTag> tags)
        {
            return String.Join(",", from o in tags select o.ID);
        }

        private static string MakeString(List<RbTask> tasks)
        {
            return String.Join(",", from o in tasks select o.ID);
        }


    }

}
