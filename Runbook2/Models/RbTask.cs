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
        private string description;
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

        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
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
                owners.Add(owner);
                RaiseEvent("Owners");
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

        public int? ID { get; private set; }

        public void Recalculate()
        {
            DateTime newStartTime;
            //Calculate StartTime
            if (PreReqs.Count > 0)
            {
                var preReqEndTime = PreReqs.Max(x => x.EndTime);

                if (manualStartTime.HasValue)
                {
                    if (preReqEndTime < manualStartTime.Value)
                    {
                        newStartTime = manualStartTime.Value;
                    }
                    else newStartTime = preReqEndTime;
                }
                else newStartTime = preReqEndTime;

                if (newStartTime < TasksService.Service.MinDate)
                    newStartTime = TasksService.Service.MinDate;

            }
            else if (manualStartTime.HasValue)
                newStartTime = manualStartTime.Value;
            else
                newStartTime = TasksService.Service.MinDate;

            if (newStartTime != startTime)
            {
                startTime = newStartTime;

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

        public string Notes { get; set; }

        public RbTask Clone(bool useDummyId)
        {
            RbTask clone = new RbTask();
            clone.ID = useDummyId? -1 : this.ID;
            clone.manualStartTime = manualStartTime.HasValue? manualStartTime.Value : (DateTime?)null;
            clone.notes = this.notes.ToString();
            clone.owners = new List<RbOwner>(this.owners);
            clone.preReqs = new List<RbTask>(this.preReqs);
            clone.startTime = this.startTime;
            clone.Duration = this.Duration;
            clone.tags = this.tags;

            return clone;
        }

        public void SetOwners(List<RbOwner> owners)
        {
            this.owners = owners;
        }

        public void SetTags(List<RbTag> tags)
        {
            this.tags = tags;
        }

        public override bool Equals(object obj)
        {
            return obj is RbTask ? ((RbTask)obj).ID == this.ID : false;
        }

        public override int GetHashCode()
        {
            return ID == null? -1 : ID.Value;
        }

        public void CopyFrom(RbTask newValues)
        {
            this.Duration = newValues.Duration;
            this.description = newValues.description;
            this.manualStartTime = newValues.manualStartTime;
            this.notes = newValues.notes;
            this.owners = newValues.owners;
            this.preReqs = newValues.preReqs;
            this.startTime = newValues.startTime;
            this.tags = newValues.tags;
            this.Recalculate();
        }
    }
}
