using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Contrequarte.SmartPlug.Core;
using Contrequarte.SmartPlugFinder;
using System.Net.Sockets;

namespace SmartPlugExample
{
    class Program
    {
        const long LISTENINGPORT = 54520;
        const long SENDINGPORT = 20560;

        static void Main(string[] args)
        {
            List<IPAddress> ipAdresses = GetLoacalIPAddresses();
            if (ipAdresses.Count() <= 0)
            {
                System.Console.WriteLine("No valid IP address found, please check your network seetings!");
            }
            else
            {
                if (ipAdresses.Count() > 1)
                {
                    System.Console.WriteLine("More than one IP address found!");
                    for (int i = 0; i < ipAdresses.Count(); i++)
                    {
                        System.Console.WriteLine(ipAdresses[i].ToString());
                    }
                }
                System.Console.WriteLine(string.Format("Local IP address used for this demo: {0}", ipAdresses[0].ToString()));
                
                SmartPlug plugToPlayWith = DeviceFinderDemo(ipAdresses[0]);
                plugToPlayWith.UserName = "admin";
                plugToPlayWith.PassWord = "1234";
                System.Console.WriteLine(string.Format("SmartPlug used for further demo: Name: {0}  IP: {1}", plugToPlayWith.Details.Name, plugToPlayWith.IpAddress.ToString()));
                PlugPlayDemo(plugToPlayWith);
            }
            int j = ipAdresses.Count();
        }
        static SmartPlug DeviceFinderDemo(IPAddress ipAddressToUse)
        {

            DeviceFinder deviceFinder = new DeviceFinder(SENDINGPORT, LISTENINGPORT);

            // finding devices in the network(s), the computer is located, as these could be
            // more than one, tell the FindDevices method, which IP to use.
            IEnumerable<Contrequarte.SmartPlug.Core.SmartPlug> smartPlugs = deviceFinder.FindDevices(ipAddressToUse);

            foreach(var smartP in smartPlugs)
            {
                System.Console.WriteLine(string.Format("Name: {0} IP:{1} model: {2} sw version: {3} "
                          , smartP.Details.Name, smartP.IpAddress, smartP.Details.Model, smartP.Details.SoftwareVersion));
            }

            int i = smartPlugs.Count();
            return smartPlugs.First();
        }

        static void PlugPlayDemo(SmartPlug smartPlug )
        {
            // initializing adapter
            //SmartPlug smartPlug = new SmartPlug(new IPAddress(new byte[] {a, b, c, d}), "lala", "9999"); //Please use your IP, UserName, PassWord...       

            //displaying current state on the 
            System.Console.WriteLine(string.Format("Current state of the smartplug is: {0}. Hit Enter key to continue!", smartPlug.Status.ToString()));
            System.Console.ReadLine();
            if (smartPlug.Status == SmartPlugState.Off)
            {
                smartPlug.Status = SmartPlugState.On;
            }
            else
            {
                smartPlug.Status = SmartPlugState.Off;
            }
            System.Console.WriteLine(string.Format("Now the state of the smartplug is: {0}. Hit Enter key to continue!", smartPlug.Status.ToString()));
            System.Console.ReadLine();

            //smartPlug.GetEntireSchedule();

            System.Console.WriteLine("Old schedule for Monday:");
            ShowSchedule(DayOfWeek.Monday, smartPlug);
            System.Console.WriteLine("Hit Enter key to continue!");
            System.Console.ReadLine();

            List<ScheduledEntry> newSchedule = new List<ScheduledEntry>();
            newSchedule.Add(new ScheduledEntry(new TimePeriod(new PointInTime(2, 0), new PointInTime(3, 0)),true));
            newSchedule.Add(new ScheduledEntry(new TimePeriod(new PointInTime(12, 0), new PointInTime(14, 30)),true));
            newSchedule.Add(new ScheduledEntry(new TimePeriod(new PointInTime(17, 10), new PointInTime(17, 45)),false));
            newSchedule.Add(new ScheduledEntry(new TimePeriod(new PointInTime(20, 20), new PointInTime(23, 13)),true));

            smartPlug.SetScheduleForWeekDay(DayOfWeek.Monday, newSchedule);

            System.Console.WriteLine("New schedule for Monday:");
            ShowSchedule(DayOfWeek.Monday, smartPlug);
            System.Console.WriteLine("Hit Enter key to continue!");
            System.Console.ReadLine();

            decimal thisMonthPowerConsumption = smartPlug.GetCummulatedPowerConsumption(EnergyPeriods.Month);
            System.Console.WriteLine(string.Format("The power consumption during this month has been {0} [kWh]. Hit Enter key to continue!",thisMonthPowerConsumption));
            System.Console.ReadLine();

        }

        static void ShowSchedule(DayOfWeek dayOfWeek, SmartPlug smartPlug)
        {
            IEnumerable<ScheduledEntry> entriesForDay = smartPlug.GetScheduleForWeekDay(dayOfWeek);
            foreach (ScheduledEntry entry in entriesForDay)
            {
                System.Console.WriteLine(string.Format("Entry\tStart:{0}:{1}\tEnd:{2}:{3}\t{4}",
                                          entry.Period.Begin.Hour.ToString("D2"), entry.Period.Begin.Minute.ToString("D2"),
                                          entry.Period.End.Hour.ToString("D2"), entry.Period.End.Minute.ToString("D2"),
                                          entry.Enabled ? "enabled" : "disabled"));
            }
        }

        static List<IPAddress> GetLoacalIPAddresses()
        {
            List<IPAddress> ipList = new List<IPAddress>();
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipList.Add(ip);
                    break;
                }
            }
            return ipList;
        }
    }
}
