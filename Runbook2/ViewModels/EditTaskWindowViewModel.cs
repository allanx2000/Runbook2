using Innouvous.Utils;
using Innouvous.Utils.MVVM;
using Runbook2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Runbook2.ViewModels
{
    public class EditTaskWindowViewModel : ViewModel
    {
        private RbTask task;
        private bool isEdit;

        private EditTaskWindow window;

        public RbTaskViewModel ViewModel
        {
            get;
            set;
        }

        private void Initialize(RbTask task)
        {
            this.task = task;

            this.ViewModel = new RbTaskViewModel(task, TasksService.Service);

        }

        public EditTaskWindowViewModel(EditTaskWindow window, RbTask existingTask = null)
        {
            this.window = window;

            isEdit = existingTask != null;

            Initialize(isEdit? existingTask : new RbTask());
        }


        public string OkText { 
            get
            {
                return isEdit? "Edit" : "Add";
            }
        }

        public Visibility IdVisible
        {
            get
            {
                return task.ID == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }


        public ICommand EditOwnersCommand
        {
            get
            {
                return new CommandHelper(EditOwners);
            }
        }


        private void EditOwners()
        {
            ViewModel.Data.AddOwner(new RbOwner(1, "test"));
            ViewModel.RefreshViewModel();
        }

        public ICommand SaveCommand
        {
            get
            {
                return new CommandHelper(SaveTask);
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return new CommandHelper(() => window.Close());
            }
        }

        private void SaveTask()
        {
            try
            {
                var optionData = window.OptionsControl.GetOptionsData();
                
                //TODO: Prereqs in TaskService not updated

                foreach (var kv in optionData)
                {
                    switch (kv.Key)
                    {
                        case EditTaskWindow.FldTask:
                            task.Description = kv.Value.ToString();
                            break;
                        case EditTaskWindow.FldDuration:
                            task.SetDuration(Convert.ToInt32(kv.Value));
                            break;
                        case EditTaskWindow.FldStartTime:
                            if (kv.Value != null)
                                task.SetManualStartTime(((DateTime?)kv.Value).Value);
                            else
                                task.ClearManualStartTime();
                            break;
                    }
                }

                if (isEdit)
                {
                    TasksService.Service.UpdateTask(task);
                }
                else
                    TasksService.Service.AddNewTask(task);

                window.Close();
            }
            catch (Exception e)
            {
                MessageBoxFactory.ShowError(e);
            }

        }

        public RbTask GetTask()
        {
            return task;
        }
    }
}
