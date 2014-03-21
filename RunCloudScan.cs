using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using System.Xml.XPath;
using ActiveUp.Net.Mail;
using System.Threading;
using Quartz;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
namespace CloudScan
{
    class RunCloudScan
    {
        
        //Results XML file
        public XDocument resultsFile;
        public XElement cloud;
        public XElement notification;
        public string cloudUrl, cloudUsername, cloudPassword, cloudExpectedCradles;
        public TaskbarIcon tb;
        public delegate void BallonTip(TaskbarIcon tb, string title, string body);
        

        public void sendMail(Object stateInfo, XElement result, string email)
        {
            SmtpMessage mail = new SmtpMessage();

            mail.Subject = "CloudScan Test Failure - " + result.Element("deviceName").Value;
            mail.BodyHtml.Text = "<html><body style='font-family:Arial; font-size:10pt;'><h1>CloudScan failed for " + result.Element("deviceName").Value + "</h1> <b>Device ID:   </b>" + result.Element("deviceId").Value + "<br><b>Error:    </b>" + result.Element("finalResult").Element("execution").Element("output").Element("status").Element("description").Value + "</body></html>";
            mail.From = new Address("CloudScan@Orasi.com");

        mail.To.Add(new Address(email));

        mail.BuildMimePartTree();
        
            mail.DirectSend();
        }
        public void sendConnectionMail(Object stateInfo, XElement cradle, string email)
        {
            SmtpMessage mail = new SmtpMessage();

            mail.Subject = "CloudScan Connection Failure - " + cradle.Element("id").Value;
            mail.BodyHtml.Text = "<html><body style='font-family:Arial; font-size:10pt;'><h1>CloudScan connection failure for " + cradle.Element("id").Value + "</h1> <b>Mode:   </b>" + cradle.Element("status").Element("mode").Value + "<br><b>Status:    </b>" + cradle.Element("status").Element("description").Value + "</body></html>";
            mail.From = new Address("CloudScan@Orasi.com");

            mail.To.Add(new Address(email));

            mail.BuildMimePartTree();

            mail.DirectSend();
        }
        public void parseResults()
        {
            XElement root = resultsFile.Root;
           // IEnumerable<XElement> address =
           //   from el in root.Elements("device")
            //  where el.Element("finalResult").Element("execution").Element("output").Element("status").Element("success").Value == "false"
            //  select el;
            IEnumerable<XElement> address = root.Elements("device");
            foreach (XElement el in address)
            {
                if (el.Element("runStatus").Value == "started") { 
                if (el.Element("finalResult").Element("execution").Element("output").Element("status").Element("success").Value == "false")
                    {
                        //XDocument notifyConfig = XDocument.Load("Config\\NotificationConfig.xml");
                        IEnumerable<XElement> notifys = notification.Elements("email");
                        foreach (XElement notify in notifys)
                        {
                            ThreadPool.QueueUserWorkItem(new WaitCallback((stateInfo) => sendMail(stateInfo, el, notify.Value)));
                        }
                    }
                }
            }
           // MainWindow main = (MainWindow)Application.Current.MainWindow;
            //main.showBallonText("CloudScan Finished", "Scan finished for " + cloudUrl);
            //(

            //((MainWindow)Application.Current.MainWindow).taskBar.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => ((MainWindow)Application.Current.MainWindow).showBallonText("CloudScan Started", "Scan started for " + cloudUrl)));
        
            BallonTip tip = showBallonTip;
            object[] myParams = { tb, "CloudScan Finished", "Scan finished for " + cloudUrl };
            tb.Dispatcher.BeginInvoke(tip,myParams);
            Thread.Sleep(0);
        }
        public void ScanConnectionStatus()
        {
            XDocument connectionStatus = XDocument.Load("https://" + cloudUrl + "/services/cradles?operation=list&user=" + cloudUsername + "&password=" + cloudPassword);
            IEnumerable<XElement> cradles = connectionStatus.XPathSelectElements("cradles/cradle");
            foreach (XElement cradle in cradles)
            {
                if (cradle.Element("status").Element("mode").Value != "CONNECTED"){
                    //XDocument notifyConfig = XDocument.Load("Config\\NotificationConfig.xml");

                    IEnumerable<XElement> notifys = notification.Elements("email");
                    foreach (XElement notify in notifys)
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback((stateInfo) => sendConnectionMail(stateInfo, cradle, notify.Value)));
                    }
                    
                }
            }
        }
    
        public RunCloudScan(XElement cloudEl, XElement notificationEl)
        {
            
            cloud = cloudEl;
            notification = notificationEl;
            //Save Cloud Config as class property
            this.cloudUrl = cloudEl.Element("url").Value;
            this.cloudUsername = cloudEl.Element("username").Value;
            this.cloudPassword = cloudEl.Element("password").Value;
            //this.cloudExpectedCradles = cloudEl.Element("expectedCradles").Value;
            try {
                tb = MainWindow.tw;
         
            }
            catch (Exception ex)
            {
                throw ex;
            }
        
            BallonTip tip = showBallonTip;
            object[] myParams = { tb, "CloudScan Started", "Scan started for " + cloudUrl };
            try { 
            tb.Dispatcher.BeginInvoke(tip,myParams);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
              //  tb.Dispatcher.Invoke(showBallonTip(tb, "CloudScan Started", "Scan started for " + cloudUrl));
            ScanConnectionStatus();
            //Create Results XML File
            this.resultsFile = new XDocument(
                new XElement("results"));
            // 17051QW-D0D-C1B94213D
            
        }

        public static void showBallonTip(TaskbarIcon tb, string title, string body)
        {
            tb.ShowBalloonTip(title, body, Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
        }

        //RunScan initianes a scan on a cloud
        public void RunScan()
        {
                //Retrieve The current cloud status to get a list of cradles/status
                XDocument doc = XDocument.Load("https://" + cloudUrl + "/services/handsets?operation=list&user=" + cloudUsername + "&password=" + cloudPassword + "&status=CONNECTED");

                //Create Collection of cradles from current cloud status and cycle through each
                IEnumerable<XElement> devices = doc.XPathSelectElements("handsets/handset");
                foreach (XElement device in devices)
                {   

                    //If Device is Connected and not in use Run CloudScan
                    if (device.Element("inUse").Value == "false") 
                    { 
                        //Build Get Request for Perfecto API 
                        string apiURL = "https://" + cloudUrl + "/services/executions?operation=execute&scriptKey=GROUP:CloudScanTest.xml&user=" + cloudUsername + "&password=" + cloudPassword + "&param.DUT=" + device.Element("deviceId").Value + "&responseFormat=xml";
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiURL);
                        request.Method = WebRequestMethods.Http.Get;
                        request.Timeout = 30000;
                        request.Proxy = null;
                        request.BeginGetResponse(new AsyncCallback((asyncResult) => ReadCallback(asyncResult, device.Element("deviceId").Value)), request);
                        resultsFile.Root.Add(
                            new XElement("device",
                                new XElement("runStatus", "start"),
                                new XElement("deviceName", device.Element("model").Value),
                                new XElement("deviceId", device.Element("deviceId").Value)
                                ));
                    }else
                    {
                        //Do Error Checking and Other Stuff
                        resultsFile.Root.Add(
                           new XElement("device",
                               new XElement("runStatus", "notRun"),
                               new XElement("deviceName", device.Element("model").Value),
                               new XElement("deviceId", device.Element("deviceId").Value)
                               ));
                        //MessageBox.Show(device.Element("status").Element("mode").Value);
                    }
                }
                //waitForResults();
               // MessageBox.Show("CradlesRun: " + cradlesRun + "   CradlesN
        }
        public void waitForResults()
        {
            XElement root = resultsFile.Root;
            IEnumerable<XElement> results =
                from el in root.Elements("device")
                where el.Element("runStatus").Value == "started"
                select el;
            foreach (XElement el in results)
            {
                
                Boolean isError;
                
                do
                {
                    //MessageBox.Show("https://" + cloudUrl + "/services/reports/" + el.Element("reportId").Value + "?operation=download&user=" + cloudUsername + "&password=" + cloudPassword + "&responseFormat=xml");
                    var request = (HttpWebRequest)WebRequest.Create("https://" + cloudUrl + "/services/reports/" + el.Element("reportId").Value + "?operation=download&user=" + cloudUsername + "&password=" + cloudPassword + "&responseFormat=xml");
                    try {
                        using (var response = request.GetResponse() as HttpWebResponse) {
                            if (request.HaveResponse && response != null) {
                                using (var reader = new StreamReader(response.GetResponseStream())) {
                                    XElement finalResult = XElement.Parse(reader.ReadToEnd());
                                    el.Add(new XElement("finalResult",
                                        finalResult));
                                }
                            }

                        }
                        isError = false;
                    }
                    catch (WebException wex) {
                        if (wex.Response != null) {
                            using (var errorResponse = (HttpWebResponse)wex.Response) {
                                using (var reader = new StreamReader(errorResponse.GetResponseStream())) {
                                    string error = reader.ReadToEnd();
                                    //TODO: use JSON.net to parse this string and look at the error message
                                    
                                }
                            }
                        }
                        isError = true;
                    }
                /*
                    WebRequest wrGETURL = WebRequest.Create("https://" + cloudUrl + "/services/reports/" + el.Element("reportId").Value + "?operation=download&user=" + cloudUsername + "&password=" + cloudPassword + "&responseFormat=xml");

                    Stream objStream;
                    
                    objStream = wrGETURL.GetResponse().GetResponseStream();

                    StreamReader objReader = new StreamReader(objStream);
                    string testVAr;
                    XDocument doc = XDocument.Load();
                    IEnumerable<XElement> errors = doc.XPathSelectElements("response/errorMessage");
                    count = 1;*/
                } while (isError);
               // MessageBox.Show("YAY");
            }
            //MessageBox.Show("It's Done");
            parseResults();
        }
        public void addResultsElement(string deviceID, string reportId)
        {
            XElement root = resultsFile.Root;
            IEnumerable<XElement> address =
              from el in root.Elements("device")
              where el.Element("deviceId").Value == deviceID
              select el;
            foreach (XElement el in address){
                el.SetElementValue("runStatus", "started");
                el.Add(new XElement("reportId", reportId));
                }
        }
        private void ReadCallback(IAsyncResult asyncResult, string deviceID)
        {
            
        /*  */
             HttpWebRequest request = (HttpWebRequest)asyncResult.AsyncState;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asyncResult))
                {
                   
                    //MessageBox.Show(testVar);
                    
                    Stream responseStream = response.GetResponseStream();
                    using (StreamReader sr = new StreamReader(responseStream))
                    {
                        
                        XElement responseElement = XElement.Parse(sr.ReadToEnd());
                        addResultsElement(deviceID, responseElement.Element("reportKey").Value);
                        XElement root = resultsFile.Root;
                        IEnumerable<XElement> results =
                            from el in root.Elements("device")
                            where el.Element("runStatus").Value == "start"
                            select el;
                        if (results.Count() == 0)
                        {
                            waitForResults();
                        }
                        string strContent = sr.ReadToEnd();
                        //MessageBox.Show(strContent);
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


    }
}
