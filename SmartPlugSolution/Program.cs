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
            SmartPlug smartPlug = new SmartPlug("192.168.2.61", "admin", "1234"); //Please use your IP, UserName, PassWord...

            

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
    }
}
