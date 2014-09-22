using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Contrequarte.SmartPlug
{
    public class SmartPlugMessages
    {
        public static XDocument TogglePlugTo(SmartPlugState stateToSet)
        {
            switch (stateToSet)
            {
                case SmartPlugState.Off:
                    return XDocument.Parse(@"<?xml version=""1.0"" encoding=""utf-8""?>
                                             <SMARTPLUG id=""edimax"">
                                                <CMD id=""setup"">
                                                   <Device.System.Power.State>OFF</Device.System.Power.State>
                                                </CMD>
                                             </SMARTPLUG>");
                case SmartPlugState.On:
                    return XDocument.Parse(@"<?xml version=""1.0"" encoding=""utf-8""?>
                                             <SMARTPLUG id=""edimax"">
                                                <CMD id=""setup"">
                                                   <Device.System.Power.State>ON</Device.System.Power.State>
                                                </CMD>
                                             </SMARTPLUG>");
                default:
                    throw new Exception(string.Format(@"Not supported parameter value: ""{0}"" for SmartPlugMessages.TurnSwitchTo(SmartPlugState stateToSet)",
                                               stateToSet.ToString()));
            }
        }

        public static XDocument GetPlugState()
        {
            return XDocument.Parse(@"<?xml version=""1.0"" encoding=""UTF8""?>
                                     <SMARTPLUG id=""edimax"">
	                                    <CMD id=""get"">
		                                   <Device.System.Power.State/>
	                                    </CMD>
                                     </SMARTPLUG>");
        }

        public static XDocument GetEntirePowerInfo()
        {
            return XDocument.Parse(@"<?xml version=""1.0"" encoding=""UTF8""?>
                                     <SMARTPLUG id=""edimax"">
                                        <CMD id=""get"">
                                           <NOW_POWER/>
                                        </CMD>
                                     </SMARTPLUG>");
        }

        public static XDocument GetLastToggleTime()
        {
            XDocument toggleTimeRequest = GetEntirePowerInfo();
            toggleTimeRequest.Descendants("NOW_POWER").First().Add(new XElement("Device.System.Power.LastToggleTime"));
            return toggleTimeRequest;
        }

        public static XDocument GetNowCurrent()
        {
            XDocument currentRequest = GetEntirePowerInfo();
            currentRequest.Descendants("NOW_POWER").First().Add(new XElement("Device.System.Power.NowCurrent"));
            return currentRequest;
        }

        public static XDocument GetNowPower()
        {
            XDocument powerRequest = GetEntirePowerInfo();
            powerRequest.Descendants("NOW_POWER").First().Add(new XElement("Device.System.Power.NowPower"));
            return powerRequest;
        }

        public static XDocument GetNowEnergy(EnergyPeriods periodToRequest)
        {
            XDocument energyPeriodRequest = GetEntirePowerInfo();

            XElement periodRequestElement = null;
            switch (periodToRequest)
                {
                case  EnergyPeriods.Day:
                        periodRequestElement = new XElement("Device.System.Power.NowEnergy.Day");
                        break;
                case EnergyPeriods.Week:
                        periodRequestElement = new XElement("Device.System.Power.NowEnergy.Week");
                        break;
                case EnergyPeriods.Month:
                        periodRequestElement = new XElement("Device.System.Power.NowEnergy.Month");
                        break;
                default:
                        throw new Exception(string.Format(@"Not supported parameter value: ""{0}"" for SmartPlugMessages.GetNowEnergy(EnergyPeriods periodToRequest)",
                               periodToRequest.ToString()));
                }
            energyPeriodRequest.Descendants("NOW_POWER").First().Add(periodRequestElement);

            return energyPeriodRequest;
        }

        /// <summary>
        /// Returns a XDocument as template for getting/setting schedules
        /// </summary>
        /// <param name="isSetup">true returns a "setuo" template, false a "get" template</param>
        /// <returns>Xdocument as template for getting/setting schedules</returns>
        public static XDocument GetEntireScheduling(bool isSetup)
        {
            return XDocument.Parse(string.Format(@"<?xml version=""1.0"" encoding=""UTF8""?>
                                                   <SMARTPLUG id=""edimax"">
                                                      <CMD id=""{0}"">
                                                         <SCHEDULE/>
                                                      </CMD>
                                                   </SMARTPLUG>",isSetup?"setup":"get"));
        }

        public static XDocument GetScheduledListForDayOfWeek(DayOfWeek dayOfWeek)
        {
            XDocument dayScheduleRequest = GetEntireScheduling(false);
            XElement weekDayListElement = null;
            switch (dayOfWeek)
            {
                case DayOfWeek.Sunday:
                    weekDayListElement = new XElement("Device.System.Power.Schedule.0.List");
                    break;
                case DayOfWeek.Monday:
                    weekDayListElement = new XElement("Device.System.Power.Schedule.1.List");
                    break;
                case DayOfWeek.Tuesday:
                    weekDayListElement = new XElement("Device.System.Power.Schedule.2.List");
                    break;
                case DayOfWeek.Wednesday:
                    weekDayListElement = new XElement("Device.System.Power.Schedule.3.List");
                    break;
                case DayOfWeek.Thursday:
                    weekDayListElement = new XElement("Device.System.Power.Schedule.4.List");
                    break;
                case DayOfWeek.Friday:
                    weekDayListElement = new XElement("Device.System.Power.Schedule.5.List");
                    break;
                case DayOfWeek.Saturday:
                    weekDayListElement = new XElement("Device.System.Power.Schedule.6.List");
                    break;
                default:
                    throw new Exception(string.Format(@"Not supported parameter value: ""{0}"" for GetScheduledListForDayOfWeek(DayOfWeek dayOfWeek)",
                           dayOfWeek.ToString()));
            }

            dayScheduleRequest.Descendants("SCHEDULE").First().Add(weekDayListElement);

            return dayScheduleRequest;
        }

        public static XDocument SetScheduleForDayOfWeek(DayOfWeek dayOfWeek)
        {
            XDocument dayScheduleRequest = GetEntireScheduling(true);

            XElement weekDayPowerScheduleList = null;
            XElement weekDayPowerSchedule = null;
            switch (dayOfWeek)
            {
                case DayOfWeek.Sunday:
                    weekDayPowerScheduleList = new XElement("Device.System.Power.Schedule.0.List");
                    weekDayPowerSchedule = new XElement("Device.System.Power.Schedule.0");
                    break;
                case DayOfWeek.Monday:
                    weekDayPowerScheduleList = new XElement("Device.System.Power.Schedule.1.List");
                    weekDayPowerSchedule = new XElement("Device.System.Power.Schedule.1");
                    break;
                case DayOfWeek.Tuesday:
                    weekDayPowerScheduleList = new XElement("Device.System.Power.Schedule.2.List");
                    weekDayPowerSchedule = new XElement("Device.System.Power.Schedule.2");
                    break;
                case DayOfWeek.Wednesday:
                    weekDayPowerScheduleList = new XElement("Device.System.Power.Schedule.3.List");
                    weekDayPowerSchedule = new XElement("Device.System.Power.Schedule.3");
                    break;
                case DayOfWeek.Thursday:
                    weekDayPowerScheduleList = new XElement("Device.System.Power.Schedule.4.List");
                    weekDayPowerSchedule = new XElement("Device.System.Power.Schedule.4");
                    break;
                case DayOfWeek.Friday:
                    weekDayPowerScheduleList = new XElement("Device.System.Power.Schedule.5.List");
                    weekDayPowerSchedule = new XElement("Device.System.Power.Schedule.5");
                    break;
                case DayOfWeek.Saturday:
                    weekDayPowerScheduleList = new XElement("Device.System.Power.Schedule.6.List");
                    weekDayPowerSchedule = new XElement("Device.System.Power.Schedule.6");
                    break;
                default:
                    throw new Exception(string.Format(@"Not supported parameter value: ""{0}"" forSetScheduleForDayOfWeek(DayOfWeek dayOfWeek)",
                           dayOfWeek.ToString()));
            }
            
            dayScheduleRequest.Descendants("SCHEDULE").First().Add(weekDayPowerSchedule);
            dayScheduleRequest.Descendants("SCHEDULE").First().AddFirst(weekDayPowerScheduleList);


            return dayScheduleRequest;
        }
    }
}
