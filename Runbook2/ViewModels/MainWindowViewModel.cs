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

        private CommandHelper addNewTaskCommand;
        public CommandHelper AddNewTaskCommand
        {
            get
            {
                if (addNewTaskCommand == null)
                    addNewTaskCommand = new CommandHelper(new Action<object>(AddNewTask));

                return addNewTaskCommand;
            }
        }


        private void AddNewTask(object obj)
        {
            RbTask test = new RbTask();
            test.Name = "test";

            TasksService.Service.AddNewTask(test);
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
