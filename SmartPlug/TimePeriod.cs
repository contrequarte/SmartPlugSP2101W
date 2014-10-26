using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contrequarte.SmartPlug.Core
{
    public class TimePeriod
    {
        #region public properties
        public PointInTime Begin
        {
            get;
            private set;
        }
        public PointInTime End
        {
            get;
            private set;
        }
        #endregion public properties

        #region constructor
        public TimePeriod(PointInTime begin, PointInTime end)
        {
            // automatically use the smaller value as period start
            if ((begin.Hour * 100 + begin.Minute) > (end.Hour * 100 + end.Minute))
            {
                Begin = end;
                End = begin;
            }
            else
            {
                Begin = begin;
                End = end;
            }
        }
        #endregion constructor

        #region public methods
        /// <summary>
        /// Checking, if an enumeration of TimePeriod objects doesn't contain intersecting TimePeriods
        /// </summary>
        /// <param name="timePeriods">enumeration of Timeperiod objects to check</param>
        /// <returns>returns true, if no intersecting teime periods are found, else return false</returns>
        public static bool IsIntersectionFree(IEnumerable<TimePeriod> timePeriods)
        {

            TimePeriod[] testPeriods = timePeriods.OrderBy(tp => tp.Begin.Hour * 100 + tp.Begin.Minute).ToArray();
            for (int i = 0; i < testPeriods.Length - 1; i++)
            {
                if (PointInTime.Compare(testPeriods[i].End, testPeriods[i + 1].Begin) != -1)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Converting the value of an Edimax <Device.System.Power.Schedule.1.List/> node 
        /// to a List<ScheduledEntry>.
        /// the node value looks like following e.g.: 00011-0c0m1-m0mu1-n0nP1
        /// </summary>
        /// <param name="edimaxScheduleList">string representing the schedule list from an Edimax SmartPlug</param>
        /// <returns></returns>
        public static IEnumerable<ScheduledEntry> EdimaxScheduleList2ScheduledEntries(string edimaxScheduleList)
        {
            List<ScheduledEntry> scheduledEntries = new List<ScheduledEntry>();
            string[] entries = edimaxScheduleList.Split('-');
            foreach(string entry in entries)
            {
                scheduledEntries.Add(EdimaxScheduledEntryPart2ScheduledEntry(entry));
            }
            return scheduledEntries.AsEnumerable();
        }

        #endregion public methods
        #region private methods

        /// <summary>
        /// Converting a single schedule entry of the smartplug to a ScheduledEntry object
        /// such a single schedule entry consists of 5 characters
        /// [hour][minute][hour][minute][on/Off]
        /// [hour] is one single char representing the numbers from 0 to 23/24
        /// [minute] is one single char representing the numbers from 0 to 59
        /// [on/off] char '1' stands for 'on' char '0' represents 'off'
        /// </summary>
        /// <param name="scheduledListPart">string value of a singele scheduled entry, always has a length of 5 chars</param>
        /// <returns>A ScheduledEntry object representing the given Edimax schedule entry</returns>
        private static ScheduledEntry EdimaxScheduledEntryPart2ScheduledEntry(string scheduledListPart)
        {
            int[] numericValues = { -1, -1, -1, -1 };
            for (int pos = 0; pos < 4; pos++)
            {
                numericValues[pos] = CharToNumber(scheduledListPart[pos]);
            }
            TimePeriod timePeriod = new TimePeriod(new PointInTime(numericValues[0], numericValues[1]),
                                                   new PointInTime(numericValues[2], numericValues[3]));

            return new ScheduledEntry(timePeriod, (scheduledListPart[4] == '1'));
        }

        /// <summary>
        /// Converting the characters used in <Device.System.Power.Schedule.?.List/>
        /// to the corresponding number to get the times coded mor human readable
        /// </summary>
        /// <param name="charToConvert">character whose equivalent number has to be found</param>
        /// <returns>equivalent number for the char given</returns>
        private static int CharToNumber(char charToConvert)
        {
            int numberToReturn = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(charToConvert);
            if (numberToReturn < 0)
            {
                throw new Exception(string.Format(@"Not supported parameter value: ""{0}"" for CharToNumber(char charToConvert)", charToConvert));
            }
            return numberToReturn;
        }


        #endregion private methods
    }
}
