using Innouvous.Utils.MVVM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Runbook2.ViewModels
{
    public class SelectWindowViewModel<T> : ViewModel
    {
        private Action OnClose;

        private Func<object, T> OnCreateNewItem;
        private Func<IEnumerable, List<T>> OnSelectedToList;
        private Func<List<T>, string> OnMakeSelectedString;

        public SelectWindowViewModel(Func<IEnumerable, List<T>> onSelectedToList, Func<object, T> onCreateNewItem = null, Func<List<T>, string> onMakeSelectedString = null)
        {
            this.OnCreateNewItem = onCreateNewItem;
            this.OnSelectedToList = onSelectedToList;
            this.OnMakeSelectedString = onMakeSelectedString;
        }

        #region Properties
        public SelectControl<T> SelectControl
        {
            get;
            private set;
        }

        private bool isCancelled = true;
        public bool IsCancelled
        {
            get
            {
                return isCancelled;
            }
            private set
            {
                isCancelled = value;
            }
        }

        public void SetIsCancelled(bool value)
        {
            isCancelled = value;
        }

        public string SelectedString
        {
            get
            {
                if (OnMakeSelectedString == null)
                    return "OnMakeSelectedString not implemented";
                else
                    return OnMakeSelectedString.Invoke(SelectedItems);
            }
        }

        public List<T> SelectedItems
        {
            get
            {
                List<T> items = new List<T>();

                foreach (T item in SelectControl.SelectedItems.SourceCollection)
                {
                    items.Add(item);
                }

                return items;
            }
        }

        #endregion

        #region Commands
        public ICommand AddNewItemCommand
        {
            get
            {
                return new CommandHelper(AddNewItem);
            }
        }

        public void AddNewItem(object paramz)
        {
            if (OnCreateNewItem == null)
                throw new Exception("OnCreateNewItem not implemented");

            var newItem = OnCreateNewItem.Invoke(paramz);
            bool success = SelectControl.AddNewItem(newItem);

            if (success)
            {
                UpdateSelectedString();
            }
        }

        private void UpdateSelectedString()
        {
            RaisePropertyChanged("SelectedItems");
            RaisePropertyChanged("SelectedString");
        }

        public ICommand OKCommand
        {
            get
            {
                return new CommandHelper(() =>
                {
                    isCancelled = false;

                    if (OnClose != null)
                        OnClose.Invoke();
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

                    if (OnClose != null)
                        OnClose.Invoke();
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
    }

}
