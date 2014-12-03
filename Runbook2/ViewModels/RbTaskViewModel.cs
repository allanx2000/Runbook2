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
                data = value;

                RefreshViewModel();
            }
        }

        //TODO: need to hide?
        public int ID
        {
            get
            {
                return data.ID;
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
                RaisePropertyChanged("Name");
            }
        }

        private string preReqs;

        public string PreReqs
        {
            get
            {
                if (preReqs == null)
                {
                    preReqs = string.Join(",", from p in data.PreReqs orderby p.ID ascending select p.ID);
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
            this.Data = task;
            this.taskService = service;

            task.PropertyChanged += Data_PropertyChanged;
        }

        void Data_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(e.PropertyName);
        }

        public void Recalculate()
        {
            Data.Recalculate();

            RefreshViewModel();

            TasksService.Service.NotifyTaskChanged(this);
        }
    }
}
