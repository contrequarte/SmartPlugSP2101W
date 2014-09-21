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
    }
}
