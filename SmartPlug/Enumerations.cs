using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contrequarte.SmartPlug.Core
{
    /// <summary>
    /// Enumeration of potential states the smartplug could have
    /// </summary>
    public enum SmartPlugState
    {
        Off,
        On,
        UnKnown
    }

    /// <summary>
    /// Enumeration of periods to monitor power consumption currently supported by the smartplugs firmware
    /// </summary>
    public enum EnergyPeriods
    {
        Day,
        Week,
        Month
    }
}
