﻿using Innouvous.Utils.MVVM;
using Runbook2.Models;
using Runbook2.ViewModels;
using System;
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

            viewModel = new SelectTagsViewModel(allTags, existingTags, () => this.Close());

            this.DataContext = viewModel;
        }

        public bool IsCancelled
        {
            get
            {
                return viewModel.IsCancelled;
            }
        }

        public IEnumerable<RbTag> GetSelectedTags()
        {
            return viewModel.SelectedTags;
        }

    }

    public class SelectTagsViewModel : ViewModel
    {
        #region Properties

        private string newTagName;
        public string NewTagName
        {
            get
            {
                return newTagName;
            }
            set
            {
                newTagName = value;

                RaisePropertyChanged("NewTagName");
            }
        }

        
        private bool isCancelled = true;
        public bool IsCancelled
        {
            get
            {
                return isCancelled;
            }
        }

        public SelectControl<RbTagViewModel> SelectControl
        {
            get;
            private set;
        }

        public string SelectedTagsString
        {
            get
            {
                return RbTaskViewModel.MakeTagsString(SelectedTags);
            }
        }

        public List<RbTag> SelectedTags
        {
            get
            {
                List<RbTag> Tags = new List<RbTag>();

                foreach (RbTagViewModel o in SelectControl.SelectedItems.SourceCollection)
                {
                    Tags.Add(o.Data);
                }

                return Tags;
            }
        }

        #endregion

        #region Commands

        public ICommand AddNewTagCommand
        {
            get
            {
                return new CommandHelper(AddNewTag);
            }
        }

        public void AddNewTag()
        {
            if (!String.IsNullOrEmpty(newTagName) && newTagName.Trim() != "")
            {
                bool success = SelectControl.AddNewItem(new RbTagViewModel(new RbTag(null, newTagName.Trim())));     
                if (success)
                {
                    UpdateSelectedString();
                }
            }

            NewTagName = "";
        }

        private void UpdateSelectedString()
        {
            RaisePropertyChanged("SelectedTags");
            RaisePropertyChanged("SelectedTagsString");
              
        }


        public ICommand OKCommand
        {
            get
            {
                return new CommandHelper(() =>
                {
                    isCancelled = false;

                    if (onClose != null)
                        onClose.Invoke();
                });
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return new CommandHelper(() =>
                {
                    isCancelled = true;

                    if (onClose != null)
                        onClose.Invoke();
                });
            }
        }

        public ICommand AddSelectedCommand
        {
            get
            {
                return new CommandHelper((paramz) =>
                {
                    SelectControl.SelectItemsCommand.Execute(paramz);
                    UpdateSelectedString();
                });
            }
        }

        public ICommand RemoveSelectedCommand
        {
            get
            {
                return new CommandHelper((paramz) =>
                    {
                        SelectControl.DeselectItemsCommand.Execute(paramz);
                        UpdateSelectedString();
                    });
            }
        }

        #endregion


        private Action onClose;
        public SelectTagsViewModel(IEnumerable<RbTag> unselectedTags, IEnumerable<RbTag> selectedTags, Action onClose)
        {
            var unselectedVMs = from i in unselectedTags select new RbTagViewModel(i);
            var selectedVMs = from i in selectedTags select new RbTagViewModel(i);

            Initialize(unselectedVMs, selectedVMs, onClose);
        }

        public SelectTagsViewModel(IEnumerable<RbTagViewModel> unselectedTags, IEnumerable<RbTagViewModel> selectedTags, Action onClose)
        {
            Initialize(unselectedTags, selectedTags, onClose);
        }

        private void Initialize(IEnumerable<RbTagViewModel> unselectedTags, IEnumerable<RbTagViewModel> selectedTags, Action onClose)
        {
            SelectControl = new SelectControl<RbTagViewModel>(unselectedTags, selectedTags);

            SelectControl.PropertyChanged += SelectControl_PropertyChanged;

            this.onClose = onClose;
        }


        private void SelectControl_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //Refresh SelectedTags
            RaisePropertyChanged("SelectedTags");
        }
    }
}