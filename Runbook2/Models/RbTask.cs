﻿using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runbook2.Models
{

    /// <summary>
    /// Model for the task object
    /// 
    /// NOTE: I don't really like how the class is observable; maybe should just have one Event that can notify the task changed.
    /// Need to sort out the separation between model, view model, and TasksService.
    /// </summary>
    public class RbTask : ObservableClass
    {
        //PropertyName passed for refreshing all ViewModel fields; not good...
        public const string REFRESH_ALL = "ALL";
        
        //Property Names
        public const string PROP_STARTDATE = "StartDate";
        public const string PROP_ENDDATE = "EndDate";
        public const string PROP_DURATION = "Duration";
        public const string PROP_DESCRIPTION = "Description";
        public const string PROP_TAGS = "Tags";
        public const string PROP_OWNERS = "Owners";
        public const string PROP_PREREQ = "PreReqs";
        public const string PROP_ID = "ID";
        public const string PROP_MANUAL_START_TIME = "ManualStartTime";
        public const string PROP_NOTES = "Notes";


        private string description;
        private List<RbOwner> owners = new List<RbOwner>();
        private List<RbTag> tags = new List<RbTag>();
        private List<RbTask> preReqs = new List<RbTask>();
        private DateTime startTime;

        private DateTime? manualStartTime;

        public int? ID { get; private set; }

        public int Duration { get; private set; }

        public RbTask()
        {
            this.startTime = TasksService.Service.MinDate;
        }

        /// <summary>
        /// Creates a new RbTask with the exact same values
        /// </summary>
        /// <param name="nullId">Set the cloned Id to null</param>
        /// <returns></returns>
        public RbTask Clone()
        {
            RbTask clone = new RbTask();
            clone.ID = this.ID;
            clone.manualStartTime = manualStartTime.HasValue ? manualStartTime.Value : (DateTime?)null;
            clone.Notes = this.Notes;
            clone.owners = new List<RbOwner>(this.owners);
            clone.preReqs = new List<RbTask>(this.preReqs);
            clone.startTime = this.startTime;
            clone.Duration = this.Duration;
            clone.tags = this.tags;

            return clone;
        }

        /// <summary>
        /// Copies the values from an different task
        /// 
        /// Only use is for updating the existing task with the new values from EditTaskWindow
        /// 
        /// NOTE: Need to sort out the flows
        /// </summary>
        /// <param name="newValues"></param>
        public void CopyFrom(RbTask newValues)
        {
            this.Duration = newValues.Duration;
            this.description = newValues.description;
            this.manualStartTime = newValues.manualStartTime;
            this.Notes = newValues.Notes;
            this.owners = newValues.owners;
            this.preReqs = newValues.preReqs;
            this.startTime = newValues.startTime;
            this.tags = newValues.tags;
            this.RecalculateTimes();

            RaiseEvent(REFRESH_ALL);
        }

        /// <summary>
        /// Recalculates the Task's times
        /// </summary>
        public void RecalculateTimes()
        {
            DateTime newStartTime;
            
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
            }

            TasksService.Service.NotifyTaskChanged(this);
            
            RaiseEvent(PROP_STARTDATE);
            RaiseEvent(PROP_ENDDATE);
            RaiseEvent(PROP_DURATION);
        }


        public override bool Equals(object obj)
        {
            return obj is RbTask ? ((RbTask)obj).ID == this.ID : false;
        }

        public override int GetHashCode()
        {
            return ID == null ? -1 : ID.Value;
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

        /// <summary>
        /// Sets the task's owner(s)
        /// </summary>
        /// <param name="owners"></param>
        public void SetOwners(IEnumerable<RbOwner> owners)
        {
            this.owners.Clear();

            foreach (var o in owners)
            {
                TasksService.Service.AddNewOwner(o);

                this.owners.Add(o);
            }

            RaiseEvent("Owners");
        }

        /// <summary>
        /// Adds a single owner to the task
        /// </summary>
        /// <param name="owner"></param>
        public void AddOwner(RbOwner owner)
        {
            if (!owners.Contains(owner))
            {
                owners.Add(owner);
                RaiseEvent("Owners");
            }
        }

        /// <summary>
        /// Removes the specified owner from the task
        /// </summary>
        /// <param name="owner"></param>
        public void RemoveOwner(RbOwner owner)
        {
            if (owners.Remove(owner))
            {
                RaiseEvent("Owners");
            }
        }

        /// <summary>
        /// Removes all owners
        /// </summary>
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

        public void SetTags(IEnumerable<RbTag> tags)
        {
            Tags.Clear();

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

        /// <summary>
        /// Adds a single prerequisite
        /// </summary>
        /// <param name="task"></param>
        public void AddPreReq(RbTask task)
        {
            if (!this.preReqs.Contains(task))
            {
                var newPreReqs = new List<RbTask>(preReqs);
                newPreReqs.Add(task);

                if (!Utilities.HasCircular(this, newPreReqs))
                {
                    this.preReqs.Add(task);

                    RecalculateTimes();

                }
            }

            //Need to call this even if fails so that it at least updates the PreReqString value back to the original value
            NotifyPreReqsChanged();
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

                RecalculateTimes();
            }


            NotifyPreReqsChanged();
        }

        public void RemovePreReq(RbTask task)
        {
            if (PreReqs.Remove(task))
            {
                RecalculateTimes();
            }

            NotifyPreReqsChanged();
        }

        public void NotifyPreReqsChanged()
        {
            RaiseEvent(PROP_PREREQ);
        }

        #endregion


        public void SetID(int id)
        {
            ID = id;
        }

        public void SetDuration(int duration)
        {
            if (duration > 0)
            {
                Duration = duration;

                RecalculateTimes();
            }
        }

        public DateTime StartTime
        {
            get
            {
                return startTime;
            }
        }

        public DateTime? GetManualStartTime()
        {
            return manualStartTime;
        }

        public void SetManualStartTime(DateTime dateTime)
        {
            manualStartTime = dateTime;

            RaiseEvent(PROP_MANUAL_START_TIME);


            RecalculateTimes();
        }

        public void ClearManualStartTime()
        {
            manualStartTime = null;

            RecalculateTimes();
        }

        public DateTime EndTime
        {
            get
            {
                return StartTime.AddMinutes(Duration);
            }
        }

        private string notes;
        public string Notes
        {
            get
            {
                return notes;
            }
            set
            {
                notes = value;

                RaiseEvent(PROP_NOTES);
            }
        }







    }
}
