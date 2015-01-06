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

namespace Runbook2.ViewModels
{
    
    /// <summary>
    /// This class encapsulates the logic and basic ViewModel functionality of the ListBox-based Selection Control
    /// 
    /// TODO: To be honest, it's not really that useful by itself, probably should just be part of SelectWindowViewModel directly
    /// 
    /// Maybe should just be a model instead
    /// </summary>
    /// <typeparam name="T"></typeparam>
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


        public void ClearSortDescriptions()
        {
            if (selectedItemsViewSource != null && deselectedItems != null)
            {
                deselectedItemsViewSource.SortDescriptions.Clear();
                selectedItemsViewSource.SortDescriptions.Clear();
            }
        }
        public void AddSortDescription(SortDescription description)
        {
            if (selectedItemsViewSource != null && deselectedItems != null)
            {
                deselectedItemsViewSource.SortDescriptions.Add(description);
                selectedItemsViewSource.SortDescriptions.Add(description);
         
            }
        }
        public void SetSortDescription(IEnumerable<SortDescription> descriptions)
        {
            if (selectedItemsViewSource != null && deselectedItems != null)
            {
                ClearSortDescriptions();

                foreach (var d in descriptions)
                {
                    AddSortDescription(d);
                }
            }
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
