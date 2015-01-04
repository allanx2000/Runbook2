using Innouvous.Utils.MVVM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Runbook2
{
    /*public class SelectControl<T> : UserControl
    {
        
        public SelectControlViewModel<T> ViewModel
        {
            get;
            private set;
        }

        public SelectControl(IEnumerable<T> deselectedItems, IEnumerable<T> selectedItems)
        {
            ViewModel = new SelectControlViewModel<T>(deselectedItems, selectedItems);

            this.DataContext = ViewModel;
        }
    }*/

    public class SelectControl<T> : ObservableClass
    {
        private ObservableCollection<T> deselectedItems;
        private ObservableCollection<T> selectedItems;

        private CollectionViewSource deselectedItemsViewSource = null, selectedItemsViewSource = null;


        #region Properties
        public ICollectionView DeselectedItems
        {
            get
            {
                return deselectedItemsViewSource.View;
            }
        }


        public ICollectionView SelectedItems
        {
            get
            {
                return selectedItemsViewSource.View;
            }
        }


        #endregion

        #region Commands

        public ICommand SelectItemsCommand
        {
            get
            {
                return new CommandHelper(AddItems);
            }
        }

        public ICommand DeselectItemsCommand
        {
            get
            {
                return new CommandHelper(RemoveItems);
            }
        }

        private void AddItems(object items)
        {
            if (items is IList)
            {
                List<T> tempList = new List<T>();

                foreach (T i in (IList) items)
                {
                    tempList.Add(i);
                }

                foreach (T i in tempList)
                {
                    deselectedItems.Remove((T)i);
                    selectedItems.Add((T)i);
                }

                DeselectedItems.Refresh();
                SelectedItems.Refresh();
            }
        }

        private void RemoveItems(object items)
        {
            if (items is IList)
            {
                List<T> tempList = new List<T>();

                foreach (T i in (IList)items)
                {
                    tempList.Add(i);
                }

                foreach (T i in tempList)
                {
                    selectedItems.Remove(i);
                    deselectedItems.Add(i);

                }
            
                DeselectedItems.Refresh();
                SelectedItems.Refresh();
            }
        }

        #endregion 

        public SelectControl(IEnumerable<T> unselectedItems, IEnumerable<T> selectedItems)
        {
            SetDeselectedItems(unselectedItems);
            SetSelectedItems(selectedItems);
        }

        private void SetSelectedItems(IEnumerable<T> selectedItems)
        {

            this.selectedItems = new ObservableCollection<T>(selectedItems);
            selectedItemsViewSource = new CollectionViewSource();
            selectedItemsViewSource.Source = this.selectedItems;

            RaiseEvent("SelectedItems");
        }

        private void SetDeselectedItems(IEnumerable<T> deselectedItems)
        {
            this.deselectedItems = new ObservableCollection<T>(deselectedItems);
            deselectedItemsViewSource = new CollectionViewSource();
            deselectedItemsViewSource.Source = this.deselectedItems;

            RaiseEvent("DeselectedItems");          
        }

        public bool AddNewItem(T item)
        {
            if (!deselectedItems.Contains(item) && !selectedItems.Contains(item))
            {
                selectedItems.Add(item);
                RaiseEvent("SelectedItems");

                return true;
            }
            else return false;
        }
       
    }
}
