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
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using System.Xml;
 
namespace CloudScan
{
    /// <summary>
    /// Interaction logic for AddNewProfile.xaml
    /// </summary>
    public partial class AddNewSchedule : Window
    {
        public XElement currentSchedule;
        public XDocument cancelConfig;
        public XDocument scheduleConfig;
        public XElement emailList;
        public XmlDataProvider xdp;
         public AddNewSchedule(object notify)
        {
            
            InitializeComponent();
           
            //Load Class properties
            string appFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string path = Path.Combine(appFolderPath, @"CloudScan\ScheduleConfig.xml");
            scheduleConfig = XDocument.Load(path);
            cancelConfig = XDocument.Parse(scheduleConfig.Root.ToString());
        

            XmlElement el = (XmlElement)notify;
            XElement doc = XElement.Parse(el.OuterXml);

            currentSchedule = getCurrentSchedule(doc);
            emailList = currentSchedule.Element("notifications");

            refreshEmailList();
          
            urlTextBox.Text = currentSchedule.Element("clouds").Element("url").Value;
            usernameTextBox.Text = currentSchedule.Element("clouds").Element("username").Value;
            passwordTextbox.Text = currentSchedule.Element("clouds").Element("password").Value;
             if (int.Parse(currentSchedule.Element("time").Element("startTimeHour").Value) > 12)
             {
                 Hours.Text = (int.Parse(currentSchedule.Element("time").Element("startTimeHour").Value) - 12).ToString();
             }
             else
             {
                 Hours.Text = currentSchedule.Element("time").Element("startTimeHour").Value;
             }
             Minutes.Text = currentSchedule.Element("time").Element("startTimeMinute").Value;
             if (int.Parse(currentSchedule.Element("time").Element("startTimeHour").Value) > 11 && int.Parse(currentSchedule.Element("time").Element("startTimeHour").Value) < 24)
             {
                 pmRadio.IsChecked = true;
             }
             else
             {
                 amRadio.IsChecked = true;
             }
        
        }

        public AddNewSchedule()
        {
            
            InitializeComponent();
            string appFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string path = Path.Combine(appFolderPath, @"CloudScan\ScheduleConfig.xml");
            scheduleConfig = XDocument.Load(path);
            currentSchedule = null;

            XElement el = new XElement("notifications");
            XmlDocument email = new XmlDocument();
            email.Load(el.CreateReader());

            ScheduleListBox.DataContext = email;
        }
        public void refreshEmailList()
        {
            
            
            var xmlDocument = new XmlDocument();
            using (var xmlReader = emailList.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            
            //ScheduleListBox.Document = xmlDocument;
            xdp = new XmlDataProvider();
            xdp.XPath = "notifications";
            xdp.Document = xmlDocument;
            ScheduleListBox.DataContext = xdp;
            xdp.Refresh();
        }
        private void Validate_Button_Click(object sender, RoutedEventArgs e)
        {


            if (urlTextBox.Text != string.Empty & usernameTextBox.Text != string.Empty & passwordTextbox.Text != string.Empty & Hours.Text != string.Empty & Minutes.Text != string.Empty)
            {
                if (((bool)amRadio.IsChecked) | ((bool)pmRadio.IsChecked))
                {
                    if (int.Parse(Hours.Text) > 0 && int.Parse(Hours.Text) < 13)
                    {
                        if (int.Parse(Minutes.Text) >= 0 && int.Parse(Minutes.Text) <= 59)
                        {

                            int cradleCount;
                            try
                            {
                                XDocument cradles = XDocument.Load("https://" + urlTextBox.Text + "/services/cradles?operation=list&user=" + usernameTextBox.Text + "&password=" + passwordTextbox.Text);
                                cradleCount = cradles.XPathSelectElements("cradles/cradle").Count();
                            }
                            catch(Exception ex)
                            {
                                cradleCount = 0;
                                MessageBox.Show("Could not Validate Cloud Credentials \n"+ ex.Message.ToString());
                            }
                    
                            if (cradleCount > 0)
                            {
                                if (currentSchedule != null)
                                {
                                    currentSchedule.Remove();
                                }

                                string hourStart;
                                string minuteStart;

                                if (pmRadio.IsChecked == true)
                                {
                                    if (int.Parse(Hours.Text) != 12)
                                    {
                                        hourStart = (int.Parse(Hours.Text) + 12).ToString();
                                    }
                                    else
                                    {
                                        hourStart = Hours.Text;
                                    }
                                }
                                else
                                {
                                    if (int.Parse(Hours.Text) == 12)
                                    {
                                        hourStart = (int.Parse(Hours.Text) - 12).ToString();
                                    }
                                    else
                                    {
                                        hourStart = Hours.Text;
                                    }
                                }

                                minuteStart = Minutes.Text;
                                if (emailList == null)
                                {
                                    emailList = new XElement("notifications", null);
                                }
                                scheduleConfig.Root.Add(new XElement("schedule",
                                    new XElement("clouds",
                                        new XElement("url", urlTextBox.Text),
                                        new XElement("username", usernameTextBox.Text),
                                        new XElement("password", passwordTextbox.Text)
                                        ),
                                    new XElement("time",
                                        new XElement("startTimeHour", hourStart),
                                        new XElement("startTimeMinute", minuteStart)),
                                    emailList));
                                string appFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                                string path = Path.Combine(appFolderPath, @"CloudScan\ScheduleConfig.xml");
                                scheduleConfig.Save(path);

                                this.Close();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Minutes must be between 0-59");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Hours must be between 1-12");
                    }
  
                }
                else
                {
                    MessageBox.Show("Must select AM or PM");
                }
            }
            else
            {
                MessageBox.Show("All Fields are required. Please verify input");
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void Plus_Button_Click(object sender, RoutedEventArgs e)
        {
            //Load and show New Email Window
            AddEmail ae = new AddEmail();
            ae.ShowDialog();

            //Retrieve Email from window as string
            string email = ae.emailString;


            if (emailList != null)
            {    
                if (email != null)
                {
                    emailList.Add(new XElement(("email"), email));
                }
             //   scheduleConfig.Save("Config\\ScheduleConfig.xml");
                refreshEmailList();
            }
            else
            {
                XElement newXElement = new XElement("notifications",
                    new XElement("email", email));
                emailList = newXElement;
                refreshEmailList();
            
            }
            
            

        }

        private void Minus_Button_Click(object sender, RoutedEventArgs e)
        {
            XmlElement selectedEmailXml = (XmlElement)ScheduleListBox.SelectedItem;
            

            if (selectedEmailXml != null)
            {
                XElement selectedEmail = XElement.Parse(selectedEmailXml.OuterXml);
                IEnumerable<XElement> emails = currentSchedule.Element("notifications").Elements("email");
                foreach (XElement email in emails)
                {
                    if (email.Value == selectedEmail.Value)
                    {
                        email.Remove();
                    }
                }
            }
            string appFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string path = Path.Combine(appFolderPath, @"CloudScan\ScheduleConfig.xml");
            scheduleConfig.Save(path);
            refreshEmailList();
        }

        public XElement getCurrentSchedule(XElement node)
        {
            IEnumerable<XElement> schedules = scheduleConfig.Root.Elements("schedule");
            foreach (XElement schedule in schedules)
            {
                if (schedule.Value == node.Value)
                {
                    return schedule;
                    
                }
            }
            return null;
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            if (cancelConfig != null)
            {
                string appFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string path = Path.Combine(appFolderPath, @"CloudScan\ScheduleConfig.xml");
                cancelConfig.Save(path);
            }
            
            this.Close();
            
        }
        

       
    }
    
    
}
