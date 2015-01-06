using Innouvous.Utils.DialogWindow.Windows;
using Innouvous.Utils.MVVM;
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
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        public OptionsWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            ConfigureOptionsControl();
        }

        private const string MinimumDate = "MinDate";

        private void ConfigureOptionsControl()
        {
            
            var options = DialogControlOptions.SetDataInputOptions(new List<ValueComponent>()
                {
                    new DateTimePickerComponent(new ComponentArgs()
                        {
                             FieldName=MinimumDate,
                             DisplayName="Minimum Date",
                             InitialData=TasksService.Service.MinDate
                        })
                });

            OptionsControl.SetupControl(options);

        }

        public ICommand SaveChangesCommand
        {
            get
            {
                return new CommandHelper(SaveChanges);
            }
        }

        private void SaveChanges()
        {
            var data = OptionsControl.GetOptionsData();

            foreach (var kv in data)
            {
                switch (kv.Key)
                {
                    case MinimumDate:

                        DateTime? dt = (DateTime?) kv.Value;
                        if (dt == null)
                        {
                            throw new Exception("Minimum Date cannot be null");
                        }
                        else
                        {
                            TasksService.Service.SetMinDate(dt.Value);
                        }
                        break;
                    default:
                        throw new Exception("Field not recognized: " + kv.Key);
                }
            }

            this.Close();
        }


    }
}
