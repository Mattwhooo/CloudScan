using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using Quartz;
using System.Xml.Linq;
using System.IO;

namespace CloudScan
{
    class WriteToConsoleJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.Trigger.JobDataMap;

            XElement notificationNode = XElement.Parse(dataMap.GetString("notifications"));
            XElement cloudUrl = XElement.Parse(dataMap.GetString("cloud"));
            XElement cloudNode = null;
            string appFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string path = Path.Combine(appFolderPath, @"CloudScan\ScheduleConfig.xml");
            XDocument cloudConfig = XDocument.Load(path);
            IEnumerable<XElement> clouds = cloudConfig.Root.Element("schedule").Elements("clouds");
            foreach (XElement cloud in clouds)
            {
                if (cloud.Element("url").Value == cloudUrl.Value)
                {
                    cloudNode = cloud;
                }
            }
            //Console.WriteLine("Trigger {0} in group {1} was fired", context.Trigger.Key.Name, context.Trigger.Key.Group);*/

             RunCloudScan scan = new RunCloudScan(cloudNode, notificationNode);
            scan.RunScan();
        }
    }
}