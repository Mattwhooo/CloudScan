using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;
using Hardcodet.Wpf.TaskbarNotification;
using System.Xml.Linq;
using System.Xml;
using Quartz;
using Quartz.Impl;
using System.IO;


namespace CloudScan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Window main;
        public Window parent;
        public ISchedulerFactory schedulerFactory;
        public IScheduler scheduler;
        public List<JobKey> JobList;
        internal static TaskbarIcon tw;
        public TaskbarIcon taskBar
        {
            get;
            set;
        }
        public void showBallonTip(TaskbarIcon taskBarIcon, string title, string body)
        {
            taskBarIcon.ShowBalloonTip(title, body, Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
        }
         public MainWindow()
        {
            Window form1 = new Window();
           
            form1.WindowStyle = WindowStyle.ToolWindow;
            form1.ShowInTaskbar = false;
            JobList = new List<JobKey>();
            //this.Owner = form1;
           /*
            TaskbarIcon tbi = new TaskbarIcon();
            tbi.Icon = CloudScan.Properties.Resources.Logo;
            tbi.ToolTipText = "Orasi - CloudScan";*/
            
            InitializeComponent();

            string appFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string path = Path.Combine(appFolderPath, @"CloudScan\ScheduleConfig.xml");
            XmlDataProvider xmlData = Application.Current.FindResource("ScheduleListSource") as XmlDataProvider;
             XmlDocument doc = new XmlDocument();
             doc.Load(path);
            xmlData.Document= doc;
            xmlData.Refresh();

            //Create the scheduler factory
            schedulerFactory = new StdSchedulerFactory();

            //Ask the scheduler factory for a scheduler
            scheduler = schedulerFactory.GetScheduler();
            
            //Start the scheduler so that it can start executing jobs
            scheduler.Start();

            startJobs();
        }
        public MainWindow(TaskbarIcon tb) : this()
        {
            taskBar = tb;
            tw = taskBar;
        }

        public void stopJobs()
        {
            foreach (JobKey jobKey in JobList)
            {
                scheduler.DeleteJob(jobKey);
            }
        }

        public void startJobs()
        {
            string appFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string path = Path.Combine(appFolderPath, @"CloudScan\ScheduleConfig.xml");
            XDocument scheduleConfig = XDocument.Load(path);
            IEnumerable<XElement> schedules = scheduleConfig.Root.Elements("schedule");
            foreach (XElement schedule in schedules)
            {
                //schedule.Element("time").Element("startTime").Value

                // Create a job of Type WriteToConsoleJob
                IJobDetail job = JobBuilder.Create(typeof(WriteToConsoleJob)).Build();
                JobList.Add(job.Key);

                //Schedule this job to execute every second, a maximum of 10 times
                //ITrigger trigger = TriggerBuilder.Create().WithSchedule(SimpleScheduleBuilder.RepeatMinutelyForever()).StartNow().WithIdentity("MyJobTrigger", "MyJobTriggerGroup").Build();
                //scheduler.ScheduleJob(job, trigger);
                CronScheduleBuilder csb = CronScheduleBuilder.DailyAtHourAndMinute(int.Parse(schedule.Element("time").Element("startTimeHour").Value), int.Parse(schedule.Element("time").Element("startTimeMinute").Value));
                ITrigger trigger = TriggerBuilder.Create().WithSchedule(csb).StartNow()
                    .UsingJobData("cloud", schedule.Element("clouds").Element("url").ToString())
                    .UsingJobData("notifications", schedule.Element("notifications").ToString())
                    .Build();
                scheduler.ScheduleJob(job, trigger);
                //Wait for a key press. If we don't wait the program exits and the scheduler gets destroyed
                //Console.ReadKey();
            }
            //A nice way to stop the scheduler, waiting for jobs that are running to finish
            // scheduler.Shutdown(true);

        }
        void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
                
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            this.ShowInTaskbar = false;
            stopJobs();
            startJobs();
        }

        private void Add_Schedule_Click(object sender, RoutedEventArgs e)
        {
            AddNewSchedule anp = new AddNewSchedule();
            anp.ShowDialog();
            XmlDataProvider xmlData = Application.Current.FindResource("ScheduleListSource") as XmlDataProvider;
            string appFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string path = Path.Combine(appFolderPath, @"CloudScan\ScheduleConfig.xml");
            //XmlDataProvider xmlData = Application.Current.FindResource("ScheduleListSource") as XmlDataProvider;
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            xmlData.Document = doc;
            xmlData.Refresh();
            anp = null;
            
        }

        private void Edit_Schedule_Click(object sender, RoutedEventArgs e)
        {
            if (ScheduleListBox.SelectedItem != null)
            {
                AddNewSchedule ep = new AddNewSchedule(this.ScheduleListBox.SelectedItem);
                ep.ShowDialog();
                string appFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string path = Path.Combine(appFolderPath, @"CloudScan\ScheduleConfig.xml");
                XmlDataProvider xmlData = Application.Current.FindResource("ScheduleListSource") as XmlDataProvider;
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                xmlData.Document = doc;
                xmlData.Refresh();
            }
        }

        private void Delete_Schedule_Click(object sender, RoutedEventArgs e)
        {
            if (ScheduleListBox.SelectedItem != null)
            {
                //XElement doc = XElement.Parse(el.OuterXml);
                //XDocument ScheduleConfig = XDocument.Load("Config\\ScheduleConfig.xml");
                XmlElement scheduleNode = (XmlElement)ScheduleListBox.SelectedItem;
                XElement node = XElement.Parse(scheduleNode.OuterXml);
                string appFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string path = Path.Combine(appFolderPath, @"CloudScan\ScheduleConfig.xml");
                XDocument scheduleConfig = XDocument.Load(path);

                IEnumerable<XElement> schedules = scheduleConfig.Root.Elements("schedule");
                foreach (XElement schedule in schedules)
                {
                    if (schedule.Value == node.Value)
                    {
                        schedule.Remove();
                        scheduleConfig.Save(path);
                        break;
                    }
                }
                /*
                XDocument notifyConfig = XDocument.Load("Config\\NotificationConfig.xml");
                IEnumerable<XElement> notifys = notifyConfig.Root.Elements("notify");
                XmlElement elem = (XmlElement)ScheduleListBox.SelectedItem;
                XDocument doc = XDocument.Parse(elem.OuterXml);
                foreach (XElement notify in notifys)
                    if (notify.Element("email").Value == doc.Root.Element("email").Value)
                    {
                        notify.Remove();
                        notifyConfig.Save("Config\\NotificationConfig.xml");
                    }*/
                
                XmlDataProvider xmlData = Application.Current.FindResource("ScheduleListSource") as XmlDataProvider;
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                xmlData.Document = doc;
                xmlData.Refresh();
            }
        }

      
        public void showBallonText(string title, string body)
        {
            taskBar.ShowBalloonTip(title, body, BalloonIcon.Info);
        }

        public TaskbarIcon getTaskBar()
        {
            return taskBar;
        }

    }
}
