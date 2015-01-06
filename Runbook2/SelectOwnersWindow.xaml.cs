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
    /// Interaction logic for SelectOwnersWindow.xaml
    /// </summary>
    public partial class SelectOwnersWindow : Window
    {
        
        private SelectWindowViewModel<RbOwnerViewModel> viewModel;

        public SelectOwnersWindow(List<RbOwner> allOwners, List<RbOwner> existingOwners)
        {
            InitializeComponent();

            //Remove Existing from All
            foreach (var i in existingOwners)
            {
                allOwners.Remove(i);
            }

            var selected = from i in existingOwners select new RbOwnerViewModel(i);
            var unselected = from i in allOwners select new RbOwnerViewModel(i);

            viewModel = new SelectWindowViewModel<RbOwnerViewModel>(unselected, selected,
                OnClose, UpdateTextbox, ConvertToList,
                CreateNewOwner, MakeSelectedString
                );

            this.DataContext = viewModel;

            viewModel.SelectControl.AddSortDescription(new System.ComponentModel.SortDescription("Name", System.ComponentModel.ListSortDirection.Ascending));

        }

        private void UpdateTextbox(bool success)
        {
            if (success)
                NewOwnerNameTextbox.Text = null;
        }

        private void OnClose()
        {
            //Add to TaskService
            if (!IsCancelled)
            {
                foreach (RbOwnerViewModel o in viewModel.SelectControl.SelectedItems)
                {
                    if (o.Data.ID == null)
                        TasksService.Service.AddNewOwner(o.Data);
                }
            }
            this.Close();
        }

        private string MakeSelectedString(List<RbOwnerViewModel> items)
        {
            var names = (from i in items 
                                  orderby i.Data.Name ascending 
                                  select i.Data.Name);

            return String.Join(", ", names);
        }
        private RbOwnerViewModel CreateNewOwner(object paramz)
        {
            string name = NewOwnerNameTextbox.Text;

            return new RbOwnerViewModel(new RbOwner(null, name));
        }

        private List<RbOwnerViewModel> ConvertToList(IEnumerable arg)
        {
            List<RbOwnerViewModel> items = new List<RbOwnerViewModel>();

            foreach (RbOwnerViewModel i in arg)
            {
                items.Add(i);
            }

            return items;
        }

        public bool IsCancelled
        {
            get
            {
                return viewModel.IsCancelled;
            }
        }

        public IEnumerable<RbOwner> GetSelectedOwners()
        {
            return from i in viewModel.SelectedItems select i.Data;
        }

    }


 
/*
    public class SelectOwnersViewModel : ViewModel
    {
        #region Properties

        private string newOwnerName;
        public string NewOwnerName
        {
            get
            {
                return newOwnerName;
            }
            set
            {
                newOwnerName = value;

                RaisePropertyChanged("NewOwnerName");
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

        public SelectControl<RbOwnerViewModel> SelectControl
        {
            get;
            private set;
        }

        public string SelectedOwnersString
        {
            get
            {
                return RbTaskViewModel.MakeOwnersString(SelectedOwners);
            }
        }

        public List<RbOwner> SelectedOwners
        {
            get
            {
                List<RbOwner> owners = new List<RbOwner>();

                foreach (RbOwnerViewModel o in SelectControl.SelectedItems.SourceCollection)
                {
                    owners.Add(o.Data);
                }

                return owners;
            }
        }

        #endregion

        #region Commands

        public ICommand AddNewOwnerCommand
        {
            get
            {
                return new CommandHelper(AddNewOwner);
            }
        }

        public void AddNewOwner()
        {
            if (!String.IsNullOrEmpty(newOwnerName) && newOwnerName.Trim() != "")
            {
                bool success = SelectControl.AddNewItem(new RbOwnerViewModel(new RbOwner(null, newOwnerName.Trim())));     
                if (success)
                {
                    UpdateSelectedString();
                }
            }

            NewOwnerName = "";
        }

        private void UpdateSelectedString()
        {
            RaisePropertyChanged("SelectedOwners");
            RaisePropertyChanged("SelectedOwnersString");
              
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
        public SelectOwnersViewModel(IEnumerable<RbOwner> unselectedOwners, IEnumerable<RbOwner> selectedOwners, Action onClose)
        {
            var unselectedVMs = from i in unselectedOwners select new RbOwnerViewModel(i);
            var selectedVMs = from i in selectedOwners select new RbOwnerViewModel(i);

            Initialize(unselectedVMs, selectedVMs, onClose);
        }

        public SelectOwnersViewModel(IEnumerable<RbOwnerViewModel> unselectedOwners, IEnumerable<RbOwnerViewModel> selectedOwners, Action onClose)
        {
            Initialize(unselectedOwners, selectedOwners, onClose);
        }

        private void Initialize(IEnumerable<RbOwnerViewModel> unselectedOwners, IEnumerable<RbOwnerViewModel> selectedOwners, Action onClose)
        {
            SelectControl = new SelectControl<RbOwnerViewModel>(unselectedOwners, selectedOwners);

            SelectControl.PropertyChanged += SelectControl_PropertyChanged;

            this.onClose = onClose;
        }


        private void SelectControl_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //Refresh SelectedOwners
            RaisePropertyChanged("SelectedOwners");
        }
    }
*/
}
