using Innouvous.Utils.DialogWindow.Windows;
using Innouvous.Utils.DialogWindow.Windows.Components;
using Innouvous.Utils.MVVM;
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
    /// Interaction logic for EditTaskWindow.xaml
    /// </summary>
    public partial class EditTaskWindow : Window
    {
        private RbTask existingTask;

        private EditTaskWindowViewModel viewModel;

        public const string FldTask = "Task";
        public const string FldDuration = "Duration";
        public const string FldStartTime = "StartTime";

        public EditTaskWindow()
        {
            InitializeComponent();
            SetupDialogControl();

            this.viewModel = new EditTaskWindowViewModel(this);
            this.DataContext = this.viewModel;
        }

        public EditTaskWindow(RbTask existingTask)
        {
            InitializeComponent();

            this.existingTask = existingTask;
            SetupDialogControl();

            this.viewModel = new EditTaskWindowViewModel(this, existingTask);
            this.DataContext = this.viewModel;
        }

        private void SetupDialogControl()
        {
            var components = new List<ValueComponent>()
            {
                new TextBoxComponent(
                    new ComponentArgs()
                    {
                        DisplayName = "Description",
                        FieldName = FldTask,
                        InitialData = existingTask != null? existingTask.Description : null
                    }),
                new DateTimePickerComponent(
                    new ComponentArgs()
                    {
                        DisplayName = "Explicit Start (Optional)",
                        FieldName = FldStartTime,
                        InitialData = existingTask != null? existingTask.GetManualStartTime() : null
                    }),
                    new TextBoxComponent(
                        new ComponentArgs()
                    {
                        DisplayName = "Duration (Minutes)",
                        FieldName = FldDuration,
                        InitialData = existingTask != null? existingTask.Duration : 1
                    }, TextBoxComponent.FieldType.Integer, 4),
            };
            
            
            DialogControlOptions options = DialogControlOptions.SetDataInputOptions(components);
            options.BoldLabels = true;

            OptionsControl.SetupControl(options);
        }

    }

   
}
