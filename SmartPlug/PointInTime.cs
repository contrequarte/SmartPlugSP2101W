using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contrequarte.SmartPlug.Core
{
    public class PointInTime
    {

        #region public properties
        public int Minute
        {
            get;
            private set;
        }

        public int Hour
        {
            get;
            private set;
        }
        #endregion private properties

        #region constructor
        /// <summary>
        /// PontInTime represents a point in time
        /// </summary>
        /// <param name="hour">hour of this point in time</param>
        /// <param name="minute">minute of this point in time</param>
        public PointInTime(int hour, int minute)
        {
            Hour = 0;
            Minute = 0;
            if ((hour < 0) || (hour > 24) || (minute < 0) || (minute > 59) || (hour == 24) && (minute != 00))
                throw new Exception(string.Format(@"Invalid value for point in time given: {0}:{1}", hour, minute));
            Hour = hour;
            Minute = minute;
        }
        #endregion constructor

        #region public methods
        /// <summary>
        /// Compare method for PointInTime objects
        /// </summary>
        /// <param name="pitA">first PointInTime object</param>
        /// <param name="pitB">second PointInTime object</param>
        /// <returns>-1, if first point in time is earlier than the second one,
        ///           0, if both objects represent the same point in time
        ///           1, if first point in time is later than the second one</returns>
        public static int Compare(PointInTime pitA, PointInTime pitB)
        {
            int pA = pitA.Hour * 100 + pitA.Minute;
            int pB = pitB.Hour * 100 + pitB.Minute;
            return (pA < pB) ? -1 : (pA == pB) ? 0 : 1;
        }
        #endregion public methods

    }
}
