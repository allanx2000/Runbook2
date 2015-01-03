﻿using Innouvous.Utils.MVVM;
using Runbook2.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runbook2.ViewModels
{
    public class RbTaskViewModel : ViewModel
    {
        private RbTask data;

        //TODO: need to hide
        public RbTask Data {
            get
            {
                return data;
            }
        }
                
        public string Description
        {
            get
            {
                return data.Description;
            }
            set
            {
                data.Description = value;

                RaisePropertyChanged("Description");
            }
        }

        public static string MakeTagsString(IEnumerable<RbTag> tags)
        {
            return String.Join(", ", from i in tags orderby i.Name ascending select i);
        }

        public string TagsString
        {
            get
            {
                return MakeTagsString(data.Tags);
            }
        }

        private string preReqs;
        
        public static string MakePreReqsString(IEnumerable<RbTask> tasks)
        {
            return String.Join(",", from p in tasks orderby p.ID ascending select p.ID);
        }

        public string PreReqsString
        {
            get
            {
                if (preReqs == null)
                {
                    preReqs = MakePreReqsString(data.PreReqs);
                }

                return preReqs;
            }
            set
            {
                var numbers = value.Split(Utilities.CommaDelim, StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToInt32(x)).ToList();

                List<RbTask> tasks = TasksService.Service.GetTasks(numbers);

                if (!Utilities.HasCircular(this.Data, tasks))
                {
                    this.Data.SetPreReqs(tasks);
                    preReqs = String.Join(",", tasks.Select(x => x.ID.ToString()));
                }
                
                RaisePropertyChanged("PreReqsString");
            }
        }

        public int Duration
        {
            get
            {
                return data.Duration;
            }
            set
            {
                data.SetDuration(value);

                RaisePropertyChanged("Duration");
                
                //Recalculate();
            }
        }

        private string ToDateString(DateTime dt)
        {
            return dt.ToShortDateString() + " " + dt.ToShortTimeString();
        }

        public string StartTimeString
        {
            get
            {
                return ToDateString(Data.StartTime);
            }
        }

        public void SetManualStartTime(DateTime dt)
        {
            data.SetManualStartTime(dt);
        }

        public void SetManualStartTime(string dateTime)
        {
            SetManualStartTime(DateTime.Parse(dateTime));
        }

        public string EndTimeString
        {
            get
            {
                return ToDateString(data.EndTime);
            }
        }

        public RbTaskViewModel(RbTask task)
        {
            this.data = task;
            task.PropertyChanged += Data_PropertyChanged;
        }


        private void Data_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == RbTask.REFRESH_ALL)
                this.RefreshViewModel();
            else
            {
                //Reroute for certain Properties
                switch (e.PropertyName)
                {
                    case RbTask.PROP_ENDDATE:
                        RaisePropertyChanged("EndDateString");
                        break;
                    case RbTask.PROP_MANUAL_START_TIME:
                        RaisePropertyChanged("HasManualStart");
                        break;
                    case RbTask.PROP_NOTES:
                        RaisePropertyChanged("HasNots");
                        break;
                    default:
                        RaisePropertyChanged(e.PropertyName);
                        break;
                }
            }
        }

        /*public void Recalculate()
        {
            //Data.Recalculate();

            RefreshViewModel();

            //TasksService.Service.NotifyTaskChanged(this);
        }*/

        public bool HasNotes
        {
            get
            {
                return !String.IsNullOrEmpty(data.Notes);
            }
        }

        public bool HasManualStart
        {
            get
            {
                return data.GetManualStartTime() != null;
            }
        }

        public string Owners {
            get
            {
                return MakeOwnersString(Data.Owners);
            } 
        }

        private static string MakeOwnersString(IEnumerable<RbOwner> owners)
        {
            return String.Join(", ", from i in owners orderby i.Name ascending select i.Name);
        }

    }
}
