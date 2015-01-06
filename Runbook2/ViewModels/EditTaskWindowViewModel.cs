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
        private RbTask existingTask;
        private RbTask task;
        private bool IsEdit
        {
            get
            {
                return existingTask != null;
            }
        }

        private EditTaskWindow window;

        public RbTaskViewModel ViewModel
        {
            get;
            set;
        }

        public RbTask Data
        {
            get
            {
                return task;
            }
        }

        private void Initialize(RbTask task)
        {
            if (task != null)
            {
                this.existingTask = task;
                this.task = task.Clone();
            }
            else
                this.task = new RbTask();
            
            this.ViewModel = new RbTaskViewModel(this.task);
        }

        public EditTaskWindowViewModel(EditTaskWindow window, RbTask existingTask = null)
        {
            this.window = window;

            Initialize(existingTask);
        }


        public string OkText { 
            get
            {
                return IsEdit? "Edit" : "Add";
            }
        }

        public Visibility IdVisible
        {
            get
            {
                return task.ID == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public ICommand EditTagsCommand
        {
            get
            {
                return new CommandHelper(EditTags);
            }
        }


        private void EditTags()
        {
            var allTags = TasksService.Service.TagsList;
            var selectedTags = Data.Tags;

            SelectTagsWindow dlg = new SelectTagsWindow(allTags, selectedTags);
            dlg.ShowDialog();

            if (!dlg.IsCancelled)
            {
                ViewModel.Data.SetTags(dlg.GetSelectedTags());
            }

            ViewModel.RefreshViewModel();
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
            var allOwners = TasksService.Service.OwnersList;
            var selectedOwners = Data.Owners;

            SelectOwnersWindow dlg = new SelectOwnersWindow(allOwners, selectedOwners);
            dlg.ShowDialog();

            if (!dlg.IsCancelled)
            {
                ViewModel.Data.SetOwners(dlg.GetSelectedOwners());
            }

            ViewModel.RefreshViewModel();
        }

        public ICommand EditPreReqsCommand
        {
            get
            {
                return new CommandHelper(EditPreReqs);
            }
        }


        private void EditPreReqs()
        {
            var allTasks = TasksService.Service.TasksList;
            var selectedTasks = Data.PreReqs;

            SelectPreReqsWindow dlg = new SelectPreReqsWindow(allTasks, selectedTasks, existingTask);
            dlg.ShowDialog();

            if (!dlg.IsCancelled)
            {
                ViewModel.Data.SetPreReqs(dlg.GetSelectedTasks().ToList());
            }

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

                if (IsEdit)
                {
                    TasksService.Service.UpdateTask(existingTask, task);
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
