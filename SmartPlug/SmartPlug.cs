using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Contrequarte.SmartPlug
{
    public class SmartPlug
    {
        #region constants
        const int port = 10000;
        const string landingPage = "smartplug.cgi"; //http://192.168.2.61:10000/smartplug.cgi
        #endregion constants

        #region public properties
        public string IpAddress { get; private set; }
        public string UserName { get; private set; }
        public SmartPlugState Status
        {
            get
            {
                XDocument plugRequest = SmartPlugMessages.GetPlugState();
                XDocument plugResponse = SendMessage(plugRequest);
                switch (plugResponse.Descendants("Device.System.Power.State").First().Value.ToUpper())
                {
                    case "ON":
                        return SmartPlugState.On;
                    case "OFF":
                        return SmartPlugState.Off;
                    default:
                        return SmartPlugState.UnKnown;
                }
            }
            set
            {
                XDocument plugRequest = SmartPlugMessages.TogglePlugTo(value);
                XDocument plugResponse = SendMessage(plugRequest);
            }
        }
        #endregion public properties

        #region private properties
        private string password;
        private string targetUri { get { return string.Format("http://{0}:{1}/{2}", IpAddress, port, landingPage); } }
        #endregion private properties

        #region constructor
        public SmartPlug(string ipAddress, string userName, string password)
        {
            IpAddress = ipAddress;
            UserName = userName;
            this.password = password;
        }
        #endregion constructor

        #region public methods

        public void GetEntireSchedule()
        {
             XDocument plugRequest = SmartPlugMessages.GetEntireScheduling();
             XDocument plugResponse = SendMessage(plugRequest);
             int i = 3;
        }
        public IEnumerable<ScheduledEntry> GetScheduleForWeekDay(DayOfWeek dayOfWeek)
        {
            /* expected return looks like that:
             *<SMARTPLUG id="edimax">
             *   <CMD id="get">
             *      <SCHEDULE>
             *         <Device.System.Power.Schedule.0.List>00011-0c0m1-m0mu1-n0nP1</Device.System.Power.Schedule.0.List> 
             *      </SCHEDULE>
             *   </CMD>
             *</SMARTPLUG>
             */

            XDocument plugRequest = SmartPlugMessages.GetScheduledListForDayOfWeek(dayOfWeek);
            XDocument plugResponse = SendMessage(plugRequest);

            string scheduledList = plugResponse.Descendants("SCHEDULE").First().Elements().First().Value;

            return TimePeriod.EdimaxScheduleList2ScheduledEntries(scheduledList);
        }

        #endregion public methods 

        #region private methods
        private XDocument SendMessage(XDocument xmlMessageToSend)
        {
            XDocument xmlResponse = null;
            System.Net.ServicePointManager.Expect100Continue = false;
            byte[] xmlMessageAsByteArray = System.Text.Encoding.UTF8.GetBytes(xmlMessageToSend.ToString());

            WebRequest req = WebRequest.Create(targetUri);
            //((HttpWebRequest)req).UserAgent = ".NET Framework Example Client";

            req.Method = "POST";
            req.Credentials = new NetworkCredential(UserName, password);
            req.ContentLength = xmlMessageAsByteArray.Length;
            //req.ContentType = "application/x-www-form-urlencoded";
            req.ContentType = "text/xml";

            Stream requestStream = req.GetRequestStream();
            requestStream.Write(xmlMessageAsByteArray, 0, xmlMessageAsByteArray.Length);
            requestStream.Close();

            using (WebResponse response = req.GetResponse())
            {
                if (((HttpWebResponse)response).StatusDescription.ToUpper() != "OK")
                {
                    throw new Exception(string.Format("SmartPlug at: {0} returned: {1} instead of \"OK\"!",
                                                       IpAddress,((HttpWebResponse)response).StatusDescription));
                }
                using (StreamReader responseReader = new StreamReader(response.GetResponseStream()))
                {
                    xmlResponse = XDocument.Parse(responseReader.ReadToEnd());
                }
            }
            return xmlResponse;
        }


        #endregion private methods
    }
}