using Innouvous.Utils.MVVM;
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

        private TasksService taskService;

        //TODO: need to hide
        public RbTask Data {
            get
            {
                return data;
            }
            private set
            {
                if (data != null)
                    data.CopyFrom(value);
                else 
                    data = value;

                RefreshViewModel();
            }
        }

        public void SetUpdatedData(RbTask data)
        {
            Data = data;
        }

        //TODO: need to hide?
        public int? ID
        {
            get
            {
                return data.ID;
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

        public string Tags
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

        public string PreReqs
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

                List<RbTask> tasks = taskService.GetTasks(numbers);

                if (!Utilities.HasCircular(this.Data, tasks))
                {
                    this.Data.SetPreReqs(tasks);
                    preReqs = String.Join(",", tasks.Select(x => x.ID.ToString()));
                }
               
                RaisePropertyChanged("PreReqs");

                Recalculate();
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
                Recalculate();

                taskService.NotifyTaskChanged(this);
            }
        }

        private string ToDateString(DateTime dt)
        {
            return dt.ToShortDateString() + " " + dt.ToShortTimeString();
        }

        public string StartTime
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

        public string EndTime
        {
            get
            {
                return ToDateString(data.EndTime);
            }
        }

        public RbTaskViewModel(RbTask task, TasksService service)
        {
            this.taskService = service;
            this.Data = task;
            
        }


        void Data_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(e.PropertyName);

            TasksService.Service.NotifyTaskChanged(this);
        }

        public void Recalculate()
        {
            Data.Recalculate();

            RefreshViewModel();

            TasksService.Service.NotifyTaskChanged(this);
        }

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
