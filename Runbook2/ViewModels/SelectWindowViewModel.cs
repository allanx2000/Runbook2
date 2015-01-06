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
        protected Action OnClose;

        protected Func<object, T> OnCreateNewItem;
        protected Func<IEnumerable, List<T>> OnSelectedToList;
        protected Func<List<T>, string> OnMakeSelectedString;
        protected Action<bool> OnNewItemAdded;

        protected SelectWindowViewModel(IEnumerable<T> unselectedItems, IEnumerable<T> selectedItems)
        {
            this.SelectControl = new SelectControl<T>(unselectedItems, selectedItems);
        }


        public SelectWindowViewModel(IEnumerable<T> unselectedItems, IEnumerable<T> selectedItems, 
            Action onClose, Action<bool> onNewItemAdded, Func<IEnumerable, List<T>> onSelectedToList,
            Func<object, T> onCreateNewItem = null, Func<List<T>, string> onMakeSelectedString = null
            ): this(unselectedItems,selectedItems)
        {

            this.OnCreateNewItem = onCreateNewItem;
            this.OnSelectedToList = onSelectedToList;
            this.OnMakeSelectedString = onMakeSelectedString;

            this.OnClose = onClose;
            this.OnNewItemAdded = onNewItemAdded;
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

            OnNewItemAdded.Invoke(success);
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
