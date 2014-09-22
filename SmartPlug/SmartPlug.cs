using System;
using System.Collections.Generic;
using System.Globalization;
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

        private const int MinutesOfADay = 24 * 60;

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

        public void SetScheduleForWeekDay(DayOfWeek dayOfWeek, IEnumerable<ScheduledEntry> entriesToSchedule)
        {
            XDocument xmlSetScheduleMessage = SmartPlugMessages.SetScheduleForDayOfWeek(dayOfWeek);

            XElement scheduleNode = xmlSetScheduleMessage.Descendants("SCHEDULE").First();
            foreach (XElement xElement in scheduleNode.Elements())
            {
                if(xElement.Name.ToString().Contains("Device.System.Power.Schedule."))
                {
                    if(xElement.Name.ToString().Contains(".List"))
                    {
                        xElement.Value = PreparePowerScheduleList(entriesToSchedule);
                    }
                    else
                    {
                       xElement.Value =  PreparePowerSchedule(entriesToSchedule);
                    }
                }
            }

            XDocument xmlResponse = SendMessage(xmlSetScheduleMessage);
            int i = 3;
        }

        public decimal GetCummulatedPowerConsumption(EnergyPeriods energyPeriod)
        {
            /*
             * <Device.System.Power.NowEnergy.Month>0.107</Device.System.Power.NowEnergy.Month> 
             */
            XDocument plugRequest = SmartPlugMessages.GetNowEnergy(energyPeriod);
            XDocument plugResponse = SendMessage(plugRequest);

            string powerConsumption =  plugResponse.Descendants("NOW_POWER").First().Elements().First().Value;

            return Convert.ToDecimal(powerConsumption, new CultureInfo("en-US"));
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

        private string PreparePowerSchedule(IEnumerable<ScheduledEntry> entriesToSchedule)
        {
            int[] minuteSchedule = InitMinuteSchedule(); // an array containing one element for each minute of a day

            
            foreach (ScheduledEntry entry in entriesToSchedule)
            {
                // filling in scheduled minutes for each scheduled entry
                FillMinuteSchedule(entry, ref minuteSchedule);
            }

            //Transfer it to a 360 elements based hex schedule

            char[] hexSchedule = InitHexSchedule();

            int schedulePosition = 0;
            for (int quadrupletStart = 0; quadrupletStart < MinutesOfADay; quadrupletStart += 4)
            {
                hexSchedule[schedulePosition] = QuadrupletToHex(minuteSchedule, quadrupletStart);
                schedulePosition++;
            }

            return new string(hexSchedule); // string with length 360, each position contains a value between '0' and 'F'
        }

        private string PreparePowerScheduleList(IEnumerable<ScheduledEntry> entriesToSchedule)
        {
            StringBuilder entireList = new StringBuilder();
            foreach (ScheduledEntry entry in entriesToSchedule)
            {
                entireList.Append(entry.AsEdimaxScheduledEntry() + "-");
            }
            string result = entireList.ToString();

            return result.Substring(0, result.Length - 1);
        }

        private static char[] InitHexSchedule()
        {
            char[] scheduleTemplate = new char[360];
            for (int i = 0; i < 360; i++)
            {
                scheduleTemplate[i] = '0';
            }
            return scheduleTemplate;
        }

        private static int[] InitMinuteSchedule()
        {
            int[] scheduleTemplate = new int[MinutesOfADay];
            for (int i = 0; i < MinutesOfADay; i++)
            {
                scheduleTemplate[i] = 0;
            }
            return scheduleTemplate;
        }

        static void FillMinuteSchedule(ScheduledEntry entry, ref int[] minuteSchedule)
        {
            FillMinuteSchedule(entry.Period.Begin.Hour, entry.Period.Begin.Minute, 
                               entry.Period.End.Hour, entry.Period.End.Minute, ref minuteSchedule);

        }
        static void FillMinuteSchedule(int fromHour, int fromMinute, int toHour, int toMinute, ref int[] minuteSchedule)
        {
            int begin = fromHour * 60 + fromMinute;
            int end = toHour * 60 + toMinute;
            for (int pos = begin; pos < end; pos++)
            {
                minuteSchedule[pos] = 1;
            }
        }

        private static char QuadrupletToHex(int[] minuteSchedule, int quadrupletBaseAdress)
        {
            int quadrupletSum = 0;
            for (int position = quadrupletBaseAdress; position < (quadrupletBaseAdress + 4); position++)
            {
                quadrupletSum = quadrupletSum * 2 + minuteSchedule[position];
            }
            return quadrupletSum.ToString("X")[0];
        }

        #endregion private methods
    }
}