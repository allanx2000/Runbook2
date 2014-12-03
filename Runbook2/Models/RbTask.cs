using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runbook2.Models
{
    public class RbTask : ObservableClass
    {
        private string name;
        private List<RbOwner> owners = new List<RbOwner>();
        private List<RbTag> tags = new List<RbTag>();
        private List<RbTask> preReqs = new List<RbTask>();
        private DateTime startTime;
        private string notes;

        private DateTime? manualStartTime;

        public RbTask()
        {
            this.startTime = TasksService.Service.MinDate;
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        #region Owners
        public List<RbOwner> Owners
        {
            get
            {
                return owners;
            }
        }

        private void SetOwners(IEnumerable<RbOwner> owners)
        {
            foreach (var o in owners)
            {
                this.owners.Add(o);
            }
            
            RaiseEvent("Owners");
        }

        public void AddOwner(RbOwner owner)
        {
            if (!owners.Contains(owner))
            {
                RaiseEvent("Owners");
                owners.Add(owner);
            }
        }

        public void RemoveOwner(RbOwner owner)
        {
            if (owners.Remove(owner))
            {
                RaiseEvent("Owners");
            }
        }

        public void ClearOwners()
        {
            owners.Clear();
            RaiseEvent("Owners");
        }

        #endregion

        #region Tags
        public List<RbTag> Tags
        {
            get
            {
                return tags;
            }
        }

        private void SetTags(IEnumerable<RbTag> tags)
        {
            Tags.Clear();
            //Tags.AddRange(tags);
            
            foreach (var t in tags)
            {
                Tags.Add(t);
            }

            RaiseEvent("Tags");
        }

        public void AddTag(RbTag tag)
        {
            if (!tags.Contains(tag))
            {
                tags.Add(tag);

                RaiseEvent("Tags");
            }
        }

        public void RemoveTag(RbTag tag)
        {
            if (tags.Remove(tag))
            {
                RaiseEvent("Tags");
            }
        }

        #endregion

        #region Prerequisite Tasks

        public List<RbTask> PreReqs
        {
            get
            {
                return preReqs;
            }
        }

        public void AddPreReq(RbTask task)
        {
            if (!this.preReqs.Contains(task))
            {
                this.preReqs.Add(task);
                RaiseEvent("PreReqs");
            }
        }

        public void SetPreReqs(IList<RbTask> tasks)
        {
            if (!Utilities.HasCircular(this, tasks))
            {

                PreReqs.Clear();

                foreach (var t in tasks)
                {
                    PreReqs.Add(t);
                }

                RaiseEvent("PreReqs");

                Recalculate();
            }
        }

        public void RemovePreReq(RbTask task)
        {
            if (PreReqs.Remove(task))
            {
                RaiseEvent("PreReqs");
                Recalculate();
            }
        }

        #endregion

        public int ID { get; private set; }

        public void Recalculate()
        {
            //Calculate StartTime
            if (PreReqs.Count > 0)
            {
                var preReqEndTime = PreReqs.Max(x => x.EndTime);
                
                if (manualStartTime.HasValue)
                {
                    if (preReqEndTime < manualStartTime.Value)
                    {
                        startTime = manualStartTime.Value;
                    }
                    else startTime = preReqEndTime;
                }
                else startTime = preReqEndTime;

                if (StartTime < TasksService.Service.MinDate)
                    startTime = TasksService.Service.MinDate;

                RaiseEvent("StartTime");
                RaiseEvent("EndTime");
            }
        }

        public int Duration { get; private set; }


        public void SetID(int id)
        {
            ID = id;
        }

        public void SetDuration(int duration)
        {
            Duration = duration;
            Recalculate();
        }

        public DateTime StartTime {
            get
            {
                return startTime;
            }
        }

        public void SetManualStartTime(DateTime dateTime)
        {
            manualStartTime = dateTime;
            Recalculate();
        }

        public void ClearManualStartTime()
        {
            manualStartTime = null;
            Recalculate();
        }

        public DateTime EndTime
        {
            get
            {
                return StartTime.AddMinutes(Duration);
            }
        }

        public DateTime? GetManualStartTime()
        {
            return manualStartTime;
        }
    }
}
