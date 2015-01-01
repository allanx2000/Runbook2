using Innouvous.Utils.DialogWindow.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xceed.Wpf.Toolkit;

namespace Runbook2
{
    class DateTimePickerComponent : ValueComponent        
    {
        public const string DefaultDateTimeFormat = "ddd, MMM d, yyyy hh:MMK";
        private string dateTimeFormat;
        private DateTimePicker picker;
        public DateTimePickerComponent(ComponentArgs options, string dateTimeFormat = DefaultDateTimeFormat) : base (options)
        {
            this.dateTimeFormat = dateTimeFormat;

            //Create Components

            picker = new DateTimePicker();

            picker.Margin = new System.Windows.Thickness(3);
            picker.ValueChanged += SetDateTime;
            
            if (Data is DateTime?)
            {
                picker.Value = (DateTime?)Data;
            }

            this.Content = picker;
        }

        private void SetDateTime(object sender, RoutedEventArgs e)
        {
            Value = picker.Value;
        }

        public DateTime? Value
        {
            get
            {
                return base.Data == null? null : (DateTime?) base.Data;
            }
            set
            {
                base.Data = value;
                RaisePropertyChanged("Value");
            }
        }
    }
}
