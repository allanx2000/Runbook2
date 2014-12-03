using Runbook2.Models;
using Runbook2.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml.Serialization;

namespace Runbook2
{
    public class TasksService
    {
        #region Singleton Constructor
        
        private static TasksService _service = new TasksService();
        private static XmlSerializer _xser = new XmlSerializer(typeof(TasksServiceState));

        public static void LoadFromXML(string file)
        {
            TasksServiceState serviceState = (TasksServiceState)_xser.Deserialize(new StreamReader(file));
            
            RestoreFromServiceState(serviceState);

            _service.savePath = file;
        }

        private static void RestoreFromServiceState(TasksServiceState serviceState)
        {
            var ts = new TasksService();

            Dictionary<int, RbTask> tasksLookup = new Dictionary<int,RbTask>();
            Dictionary<int, RbOwner> ownerLookup = new Dictionary<int,RbOwner>();
            Dictionary<int, RbTag> tagLookup = new Dictionary<int,RbTag>();

            //Owners
            foreach (TasksServiceState.Owner o in serviceState.Owners)
            {
                RbOwner owner = new RbOwner(o.ID, o.Name);

                ts.owners.Add(new RbOwnerViewModel(owner));
                ownerLookup.Add(o.ID, owner);

                ts.nextOwner = o.ID + 1;
            }

            //Tags
            foreach (TasksServiceState.Tag t in serviceState.Tags)
            {
                RbTag tag = new RbTag(t.ID, t.Name);

                ts.tags.Add(new RbTagViewModel(tag));
                tagLookup.Add(t.ID, tag);

                ts.nextTag = t.ID + 1;
            }

            //Tasks
            foreach (TasksServiceState.Task t in serviceState.Tasks)
            {
                RbTask task = new RbTask();
                
                //Basic
                task.Name = t.Name;
                task.SetID(t.ID);
                task.SetDuration(t.Duration);
                
                if (t.ManualStartTime != null)
                    task.SetManualStartTime(t.ManualStartTime.Value);

                ts.nextTask = task.ID + 1;
                
                //Tags
                foreach (int i in Utilities.GetInts(t.Tags))
                {
                    task.Tags.Add(tagLookup[i]);
                }

                //Owners
                foreach (int i in Utilities.GetInts(t.Owners))
                {
                    task.Owners.Add(ownerLookup[i]);
                }

                ts.tasks.Add(new RbTaskViewModel(task, ts));
                tasksLookup.Add(task.ID, task);
            }

            //Link PreReqs
            foreach (TasksServiceState.Task t in serviceState.Tasks)
            {
                var task = tasksLookup[t.ID];

                if (!String.IsNullOrEmpty(t.PreReqs))
                {
                    task.SetPreReqs(ts.GetTasks(Utilities.GetInts(t.PreReqs).ToList()));
                }
            }

            ts.CreateViewSources();

            _service = ts;
        }

        public static TasksService Service
        {
            get
            {
                if (_service == null)
                    _service = new TasksService();

                return _service;
            }
        }

        #endregion


        private string savePath;
        
        public string SavePath
        {
            get
            {
                return savePath;
            }
            set
            {
                try
                {
                    new FileInfo(value);

                    savePath = value;    
                }
                catch (Exception e)
                {
                    throw new Exception("Path invalid: " + value);
                }

            }
        }

        public void Save()
        {
            if (String.IsNullOrEmpty(SavePath))
                throw new Exception("No Save Path Set");

            StreamWriter sw = new StreamWriter(savePath);

            _xser.Serialize(sw, TasksServiceState.FromTaskService());

            sw.Close();
        }

        private ObservableCollection<RbTaskViewModel> tasks = new ObservableCollection<RbTaskViewModel>();
        private ObservableCollection<RbOwnerViewModel> owners = new ObservableCollection<RbOwnerViewModel>();
        private ObservableCollection<RbTagViewModel> tags = new ObservableCollection<RbTagViewModel>();

        private int nextTask = 1, nextOwner = 1, nextTag = 1;

        private DateTime minDate = DateTime.Now;

        private CollectionViewSource tasksView, ownersView, tagsView;

        private Dictionary<int, List<RbTask>> preReqLookup;

        private TasksService()
        {
            CreateViewSources();
        }

        /*
        private void RestoreState(TasksServiceState state)
        {
            minDate = state.MinDate;

            nextTask = tasks.Max(x => x.ID) + 1;

            //Load Tags
            foreach (var tag in state.Tags)
            {
                tags.Add(new RbTagViewModel(tag));
            }

            //Load Owners
            foreach (var owner in state.Owners)
            {
                owners.Add(new RbOwnerViewModel(owner));
            }

            nextTag = tags.Max(x => x.ID) + 1;

            //Load Tasks
            foreach (var task in state.Tasks)
            {
                tasks.Add(new RbTaskViewModel(task, this));
            }


            CreateViewSources();
        }*/


        private void CreateViewSources()
        {
            tasksView = new CollectionViewSource();
            tasksView.Source = tasks;

            ownersView = new CollectionViewSource();
            ownersView.Source = owners;

            tagsView = new CollectionViewSource();
            tagsView.Source = tags;
        }

        private void RenumberIds()
        {
            throw new NotImplementedException();
        }


        public void AddNewTask(RbTask task)
        {
            task.SetID(nextTask++);

            ValidateTask(task);

            tasks.Add(new RbTaskViewModel(task, this));
        }

        public void NotifyTaskChanged(RbTaskViewModel taskViewModel)
        {
            foreach (var t in tasks)
            {
                if (t.Data.PreReqs.Contains(taskViewModel.Data))
                {
                    t.Recalculate();
                }
            }
        }


        public void ValidateTask(RbTask task)
        {
            //Check Tag
            //Check Circular PreReqs
        }


        
        /// <summary>
        /// Sorts the input IDs and returns a list of RbTask
        /// </summary>
        /// <param name="numbers"></param>
        /// <returns></returns>
        public List<RbTask> GetTasks(List<int> numbers)
        {
            List<RbTask> tasks = new List<RbTask>();

            numbers.Sort();

            foreach (var t in this.tasks)
            {
                if (numbers.Contains(t.ID))
                {
                    tasks.Add(t.Data);
                    numbers.Remove(t.ID);
                }
            }

            return tasks;
        }

        #region Properties

        public bool HasUnsavedChanges { get; private set; }

        public DateTime MinDate
        {
            get
            {
                return minDate;
            }
        }

        public ICollectionView Tags
        {
            get
            {
                return tagsView.View;
            }
        }

        public ICollectionView Owners
        {
            get
            {
                return ownersView.View;
            }
        }

        public ICollectionView Tasks
        {
            get
            {
                return tasksView.View;
            }
        }

        #endregion

    }
}