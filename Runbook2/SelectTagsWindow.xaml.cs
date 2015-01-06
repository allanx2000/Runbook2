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
    public partial class SelectTagsWindow : Window
    {
        private SelectTagsViewModel viewModel;

        public SelectTagsWindow(List<RbTag> allTags, List<RbTag> existingTags)
        {
            InitializeComponent();

            //Remove Existing from All
            foreach (var i in existingTags)
            {
                allTags.Remove(i);
            }

            var selected = from i in existingTags select new RbTagViewModel(i);
            var unselected = from i in allTags select new RbTagViewModel(i);

            viewModel = new SelectTagsViewModel(this, unselected, selected);

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
        public IEnumerable<RbTag> GetSelectedTags()
        {
            return from i in viewModel.SelectedItems select i.Data;
        }

    }

    /// <summary>
    /// Select Tags-specific implementation of SelectWindowViewModel
    /// </summary>
    public class SelectTagsViewModel : SelectWindowViewModel<RbTagViewModel>
    {
        private SelectTagsWindow window;

        public SelectTagsViewModel(SelectTagsWindow owner, IEnumerable<RbTagViewModel> unselectedTags, IEnumerable<RbTagViewModel> existingTags)
            : base(unselectedTags, existingTags)
        {
            window = owner;
            base.OnCreateNewItem = CreateNewTag;
            base.OnMakeSelectedString = MakeSelectedString;
            base.OnNewItemAdded = UpdateTextbox;
            base.OnSelectedToList = ConvertToList;
            base.OnClose = DoOnClose;

            this.SelectControl.AddSortDescription(new System.ComponentModel.SortDescription("Name", System.ComponentModel.ListSortDirection.Ascending));
        }

        private void UpdateTextbox(bool success)
        {
            if (success)
                window.NewTagNameTextbox.Text = null;
        }

        private void DoOnClose()
        {
            //Add to TaskService
            if (!IsCancelled)
            {
                foreach (RbTagViewModel t in SelectControl.SelectedItems)
                { 
                    if (t.Data.ID == null)
                        TasksService.Service.AddNewTag(t.Data);
                }
            }

            window.Close();
        }

        private string MakeSelectedString(List<RbTagViewModel> items)
        {
            var names = (from i in items
                         orderby i.Data.Name ascending
                         select i.Data.Name);

            return String.Join(", ", names);
        }
        private RbTagViewModel CreateNewTag(object paramz)
        {
            string name = window.NewTagNameTextbox.Text;

            return new RbTagViewModel(new RbTag(null, name));
        }

        private List<RbTagViewModel> ConvertToList(IEnumerable arg)
        {
            List<RbTagViewModel> items = new List<RbTagViewModel>();

            foreach (RbTagViewModel i in arg)
            {
                items.Add(i);
            }

            return items;
        }

    }

}
