using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Contrequarte.SmartPlug;

namespace SmartPlugExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // initializing adapter
            SmartPlug smartPlug = new SmartPlug("192.168.2.61", "admin", "1234");

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

            smartPlug.GetEntireSchedule();
            IEnumerable<ScheduledEntry> mondayEntries = smartPlug.GetScheduleForWeekDay(DayOfWeek.Monday);
            foreach (ScheduledEntry entry in mondayEntries)
            {
                System.Console.WriteLine(string.Format("Entry\tStart:{0}:{1}\tEnd:{2}:{3}\t{4}",
                                          entry.Period.Begin.Hour.ToString("D2"), entry.Period.Begin.Minute.ToString("D2"),
                                          entry.Period.End.Hour.ToString("D2"), entry.Period.End.Minute.ToString("D2"),
                                          entry.Enabled?"enabled":"disabled"));
            }
            System.Console.WriteLine("Hit Enter key to continue!");
            System.Console.ReadLine();
        }
    }
}
