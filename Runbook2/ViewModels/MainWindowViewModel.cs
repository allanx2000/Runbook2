using Innouvous.Utils;
using Innouvous.Utils.MVVM;
using Runbook2.Models;
using System;
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

        #region Commands

        public RbTaskViewModel SelectedTask {get; set;}

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
            }

            RaisePropertyChanged("Tasks");
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

        public ICollectionView Tasks
        {
            get
            {
                return TasksService.Service.Tasks;
            }
        }
    }
}
