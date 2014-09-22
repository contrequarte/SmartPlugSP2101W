using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contrequarte.SmartPlug
{
    public class ScheduledEntry
    {
        #region public properties
        public TimePeriod Period { get; set; }
        public bool Enabled;
        #endregion public properties

        #region constructor
        public ScheduledEntry(TimePeriod period, bool enabled)
        {
            Period = period;
            Enabled = enabled;
        }
        #endregion constructor

        #region public methods

        public string AsEdimaxScheduledEntry()
        {
            char[] edimaxScheduledEntry = {'0','0','0','0','0'};
            edimaxScheduledEntry[0] = NumberToChar(Period.Begin.Hour);
            edimaxScheduledEntry[1] = NumberToChar(Period.Begin.Minute);
            edimaxScheduledEntry[2] = NumberToChar(Period.End.Hour);
            edimaxScheduledEntry[3] = NumberToChar(Period.End.Minute);
            edimaxScheduledEntry[4] = Enabled ? '1' : '0';

            return new string(edimaxScheduledEntry);
        }

        #endregion public methods

        #region private methods

        private static char NumberToChar(int numberToConvert)
        {
            if ((numberToConvert < 0) || (numberToConvert < 0))
            {
                throw new Exception(string.Format(@"Not supported parameter value: ""{0}"" for NumberToChar(int numberToConvert)", numberToConvert));
            }

            return "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".Substring(numberToConvert, 1)[0];
        }

        #endregion private methods
    }


}
