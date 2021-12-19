using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruddat_NK
{
    class RdQueriesTime
    {
        internal static string GetDateQuery(DateTime adttStart, DateTime adtEnd, int aiDb)
        {
            String LsSql = "";

            switch (aiDb)
            {
                case 1:         // MsSql
                    LsSql = @" timeline.dt_monat >= Convert(DateTime, " + "\'" + adttStart + "', 104) "
                                          + "And timeline.dt_monat <= Convert(DateTime," + "\'" + adtEnd + "',104)";
                    break;
                case 2:         // MySql
                    LsSql = @" timeline.dt_monat >= convert('" + adtEnd.ToString()  + "',datetime) "
                                          + "And timeline.dt_monat <= convert('" + adtEnd.ToString() + "',datetime)";
                    break;
                default:
                    break;
            }
            return (LsSql);
        }
    }
}
