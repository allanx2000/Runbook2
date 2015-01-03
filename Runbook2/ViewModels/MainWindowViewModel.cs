using Innouvous.Utils;
using Innouvous.Utils.MVVM;
using Runbook2.Models;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace Runbook2.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        #region Task View
        private ObservableCollection<RbTaskViewModel> tasks;
        private CollectionViewSource tasksView;

        public RbTaskViewModel SelectedTask { get; set; }

        public ICollectionView TasksView
        {
            get
            {
                return tasksView.View;
            }
        }

        public MainWindowViewModel()
        {
            LoadFromTaskService();
        }

        private void LoadFromTaskService()
        {
            tasks = new ObservableCollection<RbTaskViewModel>();
            foreach (RbTask t in TasksService.Service.Tasks)
            {
                tasks.Add(new RbTaskViewModel(t));
            }

            tasksView = new CollectionViewSource();
            tasksView.Source = tasks;

            TasksService.Service.Tasks.CollectionChanged += Tasks_CollectionChanged;
        }

        private void Tasks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (RbTask t in e.NewItems)
                    {
                        tasks.Add(new RbTaskViewModel(t));
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:

                    var viewsToRemove = new List<RbTaskViewModel>();

                    foreach (RbTask t in e.NewItems)
                    {
                        foreach (var vm in tasks)
                        {
                            if (vm.Data == t)
                            {
                                viewsToRemove.Add(vm);
                            }
                        }
                    }

                    foreach (var t in viewsToRemove)
                    {
                        tasks.Remove(t);
                    }

                    break;
            }
        }

        #endregion


        #region Commands

        
        private CommandHelper addNewTaskCommand;
        public CommandHelper AddNewTaskCommand
        {
            get
            {
                if (addNewTaskCommand == null)
                    addNewTaskCommand = new CommandHelper(AddNewTask);

                return addNewTaskCommand;
            }
        }

        public CommandHelper EditTaskCommand
        {
            get
            {
                return new CommandHelper(EditTask);
            }
        }

        private void EditTask()
        {
            if (SelectedTask != null)
            {
                EditTaskWindow window = new EditTaskWindow(SelectedTask.Data);
                window.ShowDialog();
            }
        }

        private void AddNewTask()
        {

            EditTaskWindow window = new EditTaskWindow();
            window.ShowDialog();
        }


        private CommandHelper loadTaskServiceCommand;
        public CommandHelper LoadTaskServiceCommand
        {
            get
            {
                if (loadTaskServiceCommand == null)
                    loadTaskServiceCommand = new CommandHelper(new Action<object>(LoadTaskService));

                return loadTaskServiceCommand;
            }
        }

        private void LoadTaskService(object sender)
        {
            var selectDialog = DialogsUtility.CreateOpenFileDialog("Load Tasks File");
            DialogsUtility.AddExtension(selectDialog, "Tasks File", "*.xml");

            selectDialog.ShowDialog();

            if (!String.IsNullOrEmpty(selectDialog.FileName))
            {
                TasksService.LoadFromXML(selectDialog.FileName);

                //Rebind to new TaskServices
                LoadFromTaskService();

                RefreshViewModel();
            }
        }

        private CommandHelper saveTaskServiceCommand;
        public CommandHelper SaveTaskServiceCommand
        {
            get
            {
                if (saveTaskServiceCommand == null)
                    saveTaskServiceCommand = new CommandHelper(new Action<object>(SaveTaskService));

                return saveTaskServiceCommand;
            }
        }

        private void SaveTaskService(object sender)
        {
            try
            {
                if (String.IsNullOrEmpty(TasksService.Service.SavePath))
                {
                    var dlg = DialogsUtility.CreateSaveFileDialog("Save Tasks File");
                    DialogsUtility.AddExtension(dlg, "Tasks File", "*.xml");

                    dlg.ShowDialog();

                    if (String.IsNullOrEmpty(dlg.FileName))
                        throw new Exception("No save file selected");

                    TasksService.Service.SavePath = dlg.FileName;
                }

                TasksService.Service.Save();
            }
            catch (Exception e)
            {
                MessageBoxFactory.ShowError(e, "Saving Error");
            }
        }

        #endregion


    }
}


