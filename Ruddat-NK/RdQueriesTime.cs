using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruddat_NK
{
    class RdQueriesTime
    {
        // ZeitQuery von bis
        internal static string GetDateQueryTwo(String asField, DateTime adtStart, DateTime adtEnd, int aiDb)
        {
            String LsSql = "";

            switch (aiDb)
            {
                case 1:         // MsSql
                    LsSql = asField + " >= Convert(DateTime, " + "\'" + adtStart + "', 104) AND "
                                          + asField +" <= Convert(DateTime," + "\'" + adtEnd + "',104)";
                    break;
                case 2:         // MySql
                    LsSql = asField + " >= convert('" + adtStart.ToString()  + "',datetime) AND "
                                          + asField + " <= convert('" + adtEnd.ToString() + "',datetime)";
                    break;
                default:
                    break;
            }
            return (LsSql);
        }

        // ZeitQuery 
        internal static string GetDateQueryOne(String asFieldOne, DateTime adtStart, int aiDb)
        {
            String LsSql = "";

            switch (aiDb)
            {
                case 1:         // MsSql
                    LsSql = asFieldOne + " >= Convert(DateTime, " + "\'" + adtStart + "', 104) ";
                    break;
                case 2:         // MySql
                    LsSql = asFieldOne + " >= convert('" + adtStart.ToString() + "',datetime) ";
                    break;
                default:
                    break;
            }
            return (LsSql);
        }

        // Komplettes Ermitteln des DateQueries
        internal static string GetDateQueryResult(DateTime adtWtStart, DateTime adtWtEnd, DateTime adtStart, DateTime adtEnd, string asField, string asAnd, int aiOne, int aiDb)
        {
            string lsWhere = "";
            DateTime ldtAdd;

            switch (aiOne)  
            {
                case 1:   // Es wird die Funktion One aufgerufen. Es gibt nur eine Zeit Beginn
                    if ((adtWtStart > DateTime.MinValue) && (adtWtEnd == DateTime.MinValue))
                    {
                        ldtAdd = adtWtStart.AddDays(1);

                        lsWhere = RdQueriesTime.GetDateQueryOne(asField, adtWtStart, aiDb);
                        lsWhere = asAnd + lsWhere;
                    }
                    // Start und EndeDatum 
                    if ((adtWtStart > DateTime.MinValue) && (adtWtEnd > DateTime.MinValue))
                    {
                        lsWhere = RdQueriesTime.GetDateQueryOne(asField, adtWtStart, aiDb);
                        lsWhere = asAnd + lsWhere;
                    }
                    // Wurde kein Datum gewählt, aktuelles Jahr zeigen
                    else
                    {
                        lsWhere = RdQueriesTime.GetDateQueryOne(asField, adtStart, aiDb);
                        lsWhere = asAnd + lsWhere;
                    }
                    break;
                case 2:     // Es wird die Funktion Two aufgerufen. Es gibt zei Zeitargumente
                            // Nur StartDatum angegegben, Ende ist ein Tag plus
                    if ((adtWtStart > DateTime.MinValue) && (adtWtEnd == DateTime.MinValue))
                    {
                        ldtAdd = adtWtStart.AddDays(1);

                        lsWhere = RdQueriesTime.GetDateQueryTwo(asField, adtWtStart, ldtAdd, aiDb);
                        lsWhere = asAnd + lsWhere;
                        //" timeline.dt_monat >= Convert(DateTime," + "\'" + adtWtStart + "',104) "
                        //            + "And timeline.dt_monat <= Convert(DateTime," + "\'" + ldtAdd + "',104)";
                    }

                    // Start und EndeDatum 
                    if ((adtWtStart > DateTime.MinValue) && (adtWtEnd > DateTime.MinValue))
                    {
                        lsWhere = RdQueriesTime.GetDateQueryTwo(asField, adtWtStart, adtWtEnd, aiDb);
                        lsWhere = asAnd + lsWhere;
                        //    " timeline.dt_monat >= Convert(DateTime," + "\'" + adtWtStart + "',104) "
                        //+ "And timeline.dt_monat <= Convert(DateTime," + "\'" + adtWtEnd + "',104)";
                    }
                    // Wurde kein Datum gewählt, aktuelles Jahr zeigen
                    else
                    {
                        lsWhere = RdQueriesTime.GetDateQueryTwo(asField, adtStart, adtEnd, aiDb);
                        lsWhere = asAnd + lsWhere;
                        //lsWhereAdd2 = lsAnd + " timeline.dt_monat >= Convert(DateTime," + "\'" + ldtStart + "',104) "
                        //    + "And timeline.dt_monat <= Convert(DateTime," + "\'" + ldtEnd + "',104)";
                    }
                    break;
                default:
                    break;
            }


            return (lsWhere);
        }

        // AbrechnungsInfos zurückgeben
        internal static string GetAbrInfo(int aiFiliale, int liIdObj, int piId, int liIdObjTeil, DateTime ldtStartTmp, DateTime ldtEndTmp, int aiArt, int aiDb)
        {
            string lsSql = "";

            switch (aiDb)
            {
                case 1:     // MsSql
                    switch (aiArt)
                    {
                        case 201:
                            lsSql = @"Delete from x_abr_info;
                        Insert into x_abr_info (id_filiale,id_objekt,abr_dat_von,abr_dat_bis) 
                        values (" + aiFiliale + "," + piId.ToString() + ", Convert(DateTime," + "\'" + ldtStartTmp + "',104) , Convert(DateTime," + "\'" + ldtEndTmp + "',104))";
                            break;
                        case 202:
                            lsSql = @"Delete from x_abr_info;
                        Insert into x_abr_info (id_filiale,id_objekt,id_objekt_teil,abr_dat_von,abr_dat_bis) 
                        values (" + aiFiliale + "," + liIdObj.ToString() + "," + piId.ToString() + ", Convert(DateTime," + "\'" + ldtStartTmp + "',104) , Convert(DateTime," + "\'" + ldtEndTmp + "',104))";
                            break;
                        case 203:
                            lsSql = @"Delete from x_abr_info;
                        Insert into x_abr_info (id_filiale,id_mieter,id_objekt,id_objekt_teil,abr_dat_von,abr_dat_bis) 
                        values (" + aiFiliale + "," + piId.ToString() + "," + liIdObj.ToString() + "," + liIdObjTeil.ToString() + ", Convert(DateTime," + "\'" + ldtStartTmp + "',104) , Convert(DateTime," + "\'" + ldtEndTmp + "',104))";
                            break;
                        default:
                            break;
                    }
                    break;
                case 2:     // MySql
                    switch (aiArt)
                    {
                        case 201:
                            lsSql = @"Delete from x_abr_info;
                        Insert into x_abr_info (id_filiale,id_objekt,abr_dat_von,abr_dat_bis) 
                        values (" + aiFiliale + "," + piId.ToString() + ", convert('" + ldtStartTmp.ToString() + "',datetime) , convert('" + ldtEndTmp.ToString() + "',datetime))";
                            break;
                        case 202:
                            lsSql = @"Delete from x_abr_info;
                        Insert into x_abr_info (id_filiale,id_objekt,id_objekt_teil,abr_dat_von,abr_dat_bis) 
                        values (" + aiFiliale + "," + liIdObj.ToString() + "," + piId.ToString() + ", convert('" + ldtStartTmp.ToString() + "',datetime) , convert('" + ldtEndTmp.ToString() + "',datetime))";
                            break;
                        case 203:
                            lsSql = @"Delete from x_abr_info;
                        Insert into x_abr_info (id_filiale,id_mieter,id_objekt,id_objekt_teil,abr_dat_von,abr_dat_bis) 
                        values (" + aiFiliale + "," + piId.ToString() + "," + liIdObj.ToString() + "," + liIdObjTeil.ToString() + ", convert('" + ldtStartTmp.ToString() + "',datetime), convert('" + ldtEndTmp.ToString() + "',datetime))";
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
            return (lsSql);
        }
    }
}
