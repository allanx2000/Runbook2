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

                ts.owners.Add(owner);
                ownerLookup.Add(o.ID, owner);

                ts.nextOwner = o.ID + 1;
            }

            //Tags
            foreach (TasksServiceState.Tag t in serviceState.Tags)
            {
                RbTag tag = new RbTag(t.ID, t.Name);

                ts.tags.Add(tag);
                tagLookup.Add(t.ID, tag);

                ts.nextTag = t.ID + 1;
            }

            //Tasks
            foreach (TasksServiceState.Task t in serviceState.Tasks)
            {
                RbTask task = new RbTask();
                
                //Basic
                task.Description = t.Name;
                task.SetID(t.ID.Value);
                task.SetDuration(t.Duration);

                task.Notes = t.Notes;

                if (t.ManualStartTime != null)
                    task.SetManualStartTime(t.ManualStartTime.Value);

                ts.nextTask = task.ID.Value + 1;
                
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

                ts.tasks.Add(task);
                tasksLookup.Add(task.ID.Value, task);
            }

            //Link PreReqs
            foreach (TasksServiceState.Task t in serviceState.Tasks)
            {
                var task = tasksLookup[t.ID.Value];

                if (!String.IsNullOrEmpty(t.PreReqs))
                {
                    task.SetPreReqs(ts.GetTasks(Utilities.GetInts(t.PreReqs).ToList()));
                }
            }

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

            _xser.Serialize(sw, TasksServiceState.FromTaskService(tasks, owners, tags));

            sw.Close();
        }

        private ObservableCollection<RbTask> tasks = new ObservableCollection<RbTask>();
        private ObservableCollection<RbOwner> owners = new ObservableCollection<RbOwner>();
        private ObservableCollection<RbTag> tags = new ObservableCollection<RbTag>();

        private int nextTask = 1, nextOwner = 1, nextTag = 1;

        private DateTime minDate = DateTime.Now;

        #region Views
        private CollectionViewSource tasksView, ownersView, tagsView;
                
        public ICollectionView Tags
        {
            get
            {
                return tagsView.View;
            }
        }

        public List<RbTag> TagsList
        {
            get
            {
                return new List<RbTag>(tags);
            }
        }

        public List<RbOwner> OwnersList
        {
            get
            {
                return new List<RbOwner>(owners);
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

        public List<RbTask> TasksList
        {
            get
            {
                return new List<RbTask>(tasks);
            }
        }

        private void CreateViewSources()
        {
            tasksView = new CollectionViewSource();
            tasksView.Source = tasks;

            ownersView = new CollectionViewSource();
            ownersView.Source = owners;

            tagsView = new CollectionViewSource();
            tagsView.Source = tags;
        }
        #endregion


        
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


        
        private void RenumberIds()
        {
            throw new NotImplementedException();
        }

        #region CRUD

        #region Tasks
        public void DeleteTask(RbTask task)
        {
            List<RbTask> affectedTasks = new List<RbTask>();
            foreach (var t in tasks)
            {
                if (t.PreReqs.Contains(task))
                {
                    t.PreReqs.Remove(task);
                    affectedTasks.Add(t);
                }
            }

            foreach (var t in affectedTasks)
            {
                t.RecalculateTimes();
            }

            tasks.Remove(task);
        }

        public void AddNewTask(RbTask task)
        {
            task.SetID(nextTask++);

            ValidateTask(task);

            tasks.Add(task);

        }

        

        public void UpdateTask(RbTask existingTask, RbTask task)
        {
            if (existingTask.ID != task.ID)
            {
                throw new Exception("Task IDs does not match!");
            }

            existingTask.CopyFrom(task);
            NotifyTaskChanged(existingTask);

            
            /*

            var existing = tasks.First(x => x.ID == task.ID);

            if (existing != null)
            {
                existing.CopyFrom(task);
                this.NotifyTaskChanged(existing);
            }
            else
                throw new Exception("Error Occurred");*/
        }

        public void NotifyTaskChanged(RbTask task)
        {
            foreach (var t in tasks)
            {
                if (t.PreReqs.Contains(task))
                {
                    t.RecalculateTimes();
                }
            }
        }

        /*public void NotifyTaskChanged(RbTaskViewModel taskVM)
        {
            NotifyTaskChanged(taskVM.Data);
        }*/

        public void ValidateTask(RbTask task)
        {
            //Check Circular PreReqs
            if (Utilities.HasCircular(task, task.PreReqs))
            {
                throw new Exception("The task causes has dependencies.");
            }
        }
        #endregion

        #region Owners
        
        #endregion

        #region Tags

        #endregion


        #endregion

        /// <summary>
        /// Sorts the input IDs and returns a list of RbTask
        /// </summary>
        /// <param name="numbers"></param>
        /// <returns></returns>
        public List<RbTask> GetTasks(List<int> numbers)
        {
            numbers.Sort();

            List<RbTask> results = tasks.Where(x => numbers.Contains(x.ID.Value)).ToList();

            /*
            foreach (var t in this.tasks)
            {
                if (numbers.Contains(t.ID.Value))
                {
                    tasks.Add(t);
                    numbers.Remove(t.ID.Value);
                }
            }*/

            return results;
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

        #endregion


        /// <summary>
        /// Adds the Owner to the master list, if not present
        /// </summary>
        /// <param name="o"></param>
        public void AddNewTag(RbTag o)
        {
            if (o.ID == null
                && tags.FirstOrDefault(x => x.Name == o.Name) == null)
            {
                o.SetID(nextTag++);
                tags.Add(o);
            }
        }

        /// <summary>
        /// Adds the Owner to the master list, if not present
        /// </summary>
        /// <param name="o"></param>
        public void AddNewOwner(RbOwner o)
        {
            if (o.ID == null 
                && owners.FirstOrDefault(x => x.Name == o.Name) == null)
            {
                o.SetID(nextOwner++);
                owners.Add(o);
            }
        }

        public void SetMinDate(DateTime dt)
        {
            this.minDate = dt;

            foreach (var t in tasks)
            {
                t.RecalculateTimes();
            }
        }
    }
}