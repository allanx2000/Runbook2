using Innouvous.Utils.MVVM;
using Runbook2.Models;
using Runbook2.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Runbook2
{
    /// <summary>
    /// Interaction logic for SelectTagsWindow.xaml
    /// </summary>
    public partial class SelectPreReqsWindow : Window
    {
        private SelectPreReqsViewModel viewModel;

        public SelectPreReqsWindow(List<RbTask> allTasks, List<RbTask> existingTasks, RbTask currentTask)
        {
            InitializeComponent();

            if (currentTask != null)
                allTasks.Remove(currentTask);

            //Remove Existing from All
            foreach (var i in existingTasks)
            {
                allTasks.Remove(i);
            }

            var selected = from i in existingTasks select new RbTaskViewModel(i);
            var unselected = from i in allTasks select new RbTaskViewModel(i);

            viewModel = new SelectPreReqsViewModel(this, unselected, selected);

            this.DataContext = viewModel;
        }

        //These are for the Window itself to retrieve the values
        public bool IsCancelled
        {
            get
            {
                return viewModel.IsCancelled;
            }
        }
        public IEnumerable<RbTask> GetSelectedTasks()
        {
            return from i in viewModel.SelectedItems select i.Data;
        }

    }

    /// <summary>
    /// Select Tags-specific implementation of SelectWindowViewModel
    /// </summary>
    public class SelectPreReqsViewModel : SelectWindowViewModel<RbTaskViewModel>
    {
        private SelectPreReqsWindow window;

        public SelectPreReqsViewModel(SelectPreReqsWindow owner, IEnumerable<RbTaskViewModel> unselectedTags, IEnumerable<RbTaskViewModel> existingTags)
            : base(unselectedTags, existingTags)
        {
            window = owner;
            base.OnClose = OnClose;
            base.OnMakeSelectedString = MakeSelectedString;
            base.OnSelectedToList = ConvertToList;

            this.SelectControl.AddSortDescription(new System.ComponentModel.SortDescription("ID", System.ComponentModel.ListSortDirection.Ascending));

        }

        private void OnClose()
        {  
            window.Close();
        }

        private string MakeSelectedString(List<RbTaskViewModel> items)
        {
            var names = (from i in items
                         orderby i.Data.ID.Value ascending
                         select i.Data.ID.Value);

            return String.Join(", ", names);
        }

        private List<RbTaskViewModel> ConvertToList(IEnumerable arg)
        {
            List<RbTaskViewModel> items = new List<RbTaskViewModel>();

            foreach (RbTaskViewModel i in arg)
            {
                items.Add(i);
            }

            return items;
        }

    }

}
