using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml;
using System.Net;
using System.Windows.Controls;
using System.Threading;

namespace CloudScan
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public TaskbarIcon tb;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
           /* TaskbarIcon tbi = new TaskbarIcon();
            tbi.Icon = CloudScan.Properties.Resources.Logo;
            tbi.ToolTipText = "Orasi - CloudScan";
            */
            
            string appFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string path = Path.Combine(appFolderPath, @"CloudScan\ScheduleConfig.xml");
            if (!File.Exists(path))
            {
                string directoryPath = Path.Combine(appFolderPath, "CloudScan");
                
                System.IO.Directory.CreateDirectory(directoryPath);
                XElement root = new XElement("schedules", null);
                root.Save(path);
            }
            
            Thread.Sleep(4000);
            tb = (TaskbarIcon)FindResource("MyNotifyIcon");
            bool cloudExist;
            XDocument scheduleConfig = XDocument.Load(path);
            var cloudNode = scheduleConfig.XPathSelectElements("schedules/schedule/clouds");
            var cloudCount = cloudNode.Count();

            //var elements = (from e in xdoc.Root.Elements("row").Elements("unit")
                          //  select e).GroupBy(x => x.Value).Select(x => x.First());


            cloudExist = File.Exists("cloud.xml");
            if (cloudCount>0)
            {
                MainWindow mainView = new MainWindow(tb);
                mainView.Hide();
                tb.ShowBalloonTip("CloudScan Engaged", "CloudScan is currently running", BalloonIcon.Info);
               
            }
            else
            {
                //MainWindow mainView = new MainWindow();
                MainWindow mainView = new MainWindow(tb);
                mainView.Show();
                mainView.ShowInTaskbar = true;
                
            }
        }

        private void OpenCloudScan_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Show();
            MainWindow.ShowInTaskbar = true;
        }

        private void ExitCloudScan_Click(object sender, RoutedEventArgs e)
        {
            tb.Visibility = Visibility.Collapsed;
            Application.Current.Shutdown();
        }

        private void Run_Cloud_Scan_Click(object sender, RoutedEventArgs e)
        {
            if ((((MenuItem)e.OriginalSource).Header).ToString() != "Run CloudScan Now")
            { 
            XElement xCloud = XElement.Parse(((XmlElement)((MenuItem)e.OriginalSource).Header).OuterXml);
            //XmlElement cloudNode = (XmlElement)((MenuItem)sender).Tag;
                        //XDocument cloudsConfig = XDocument.Load("Config\\CloudConfig.xml");
            //IEnumerable<XElement> cloudNodes = cloudsConfig.XPathSelectElements("clouds/cloud");
            //foreach (XElement cloud in cloudNodes)
            //{

            //XDocument doc = XDocument.Load("https://" + xCloud.Element("url").Value + "/services/cradles?operation=list&user=" + xCloud.Element("username").Value + "&password=" + xCloud.Element("password").Value);
              //  var testValue = doc.Root.Elements("cradles").Count();
                RunCloudScan scan = new RunCloudScan(xCloud.Element("clouds"), xCloud.Element("notifications"));
                scan.RunScan();

            }
           // }
            
        }

      
    }
}
