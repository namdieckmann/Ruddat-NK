using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruddat_NK
{
    class RdQueriesTime
    {
        // ZeitQuery von bis auf 2 Felder
        private static string GetDateQueryThree(string asFieldFrom, string asFieldTo, DateTime adtStart, DateTime adtEnd, int aiDb)
        {
            String LsSql = "";

            switch (aiDb)
            {
                case 1:         // MsSql
                    LsSql = asFieldFrom + " >= Convert(DateTime, " + "\'" + adtStart + "', 104) AND "
                                          + asFieldTo + " <= Convert(DateTime," + "\'" + adtEnd + "',104)";
                    break;
                case 2:         // MySql   AAACCHTUNG Format ist %Y   großes Y
                    LsSql = asFieldFrom + " >= str_to_date(\"" + adtStart.ToString("dd.MM.yyyy") + "\",\"%d.%m.%Y %H:%i:%s\") AND "
                                          + asFieldTo + " <= str_to_date(\"" + adtEnd.ToString("dd.MM.yyyy") + "\",\"%d.%m.%Y %H:%i:%s\")";
                    break;
                default:
                    break;
            }
            return (LsSql);
        }

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
                case 2:         // MySql   AAACCHTUNG Format ist %Y   großes Y
                    LsSql = asField + " >= str_to_date(\"" + adtStart.ToString("dd.MM.yyyy") + "\",\"%d.%m.%Y %H:%i:%s\") AND "
                                          + asField + " <= str_to_date(\"" + adtEnd.ToString("dd.MM.yyyy") + "\",\"%d.%m.%Y %H:%i:%s\")";
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
                    LsSql = asFieldOne + " >= str_to_date(\"" + adtStart.ToString("dd.MM.yyyy") + "\",\"%d.%m.%Y %H:%i:%s\") ";
                    break;
                default:
                    break;
            }
            return (LsSql);
        }

        // Komplettes Ermitteln des DateQueries
        internal static string GetDateQueryResult(DateTime adtWtStart, DateTime adtWtEnd, DateTime adtStart, DateTime adtEnd, string asFieldFrom, string asFieldTo, string asAnd, int aiOne, int aiDb)
        {
            string lsWhere = "";
            DateTime ldtAdd;

            switch (aiOne)  
            {
                case 1:   // Es wird die Funktion One aufgerufen. Es gibt nur eine Zeit Beginn
                    if ((adtWtStart > DateTime.MinValue) && (adtWtEnd == DateTime.MinValue))
                    {
                        ldtAdd = adtWtStart.AddDays(1);

                        lsWhere = RdQueriesTime.GetDateQueryOne(asFieldFrom, adtWtStart, aiDb);
                        lsWhere = asAnd + lsWhere;
                    }
                    // Start und EndeDatum 
                    if ((adtWtStart > DateTime.MinValue) && (adtWtEnd > DateTime.MinValue))
                    {
                        lsWhere = RdQueriesTime.GetDateQueryOne(asFieldFrom, adtWtStart, aiDb);
                        lsWhere = asAnd + lsWhere;
                    }
                    // Wurde kein Datum gewählt, aktuelles Jahr zeigen
                    else
                    {
                        lsWhere = RdQueriesTime.GetDateQueryOne(asFieldFrom, adtStart, aiDb);
                        lsWhere = asAnd + lsWhere;
                    }
                    break;
                case 2:     // Es wird die Funktion Two aufgerufen. Es gibt zei Zeitargumente
                            // Nur StartDatum angegegben, Ende ist ein Tag plus
                    if ((adtWtStart > DateTime.MinValue) && (adtWtEnd == DateTime.MinValue))
                    {
                        ldtAdd = adtWtStart.AddDays(1);

                        lsWhere = RdQueriesTime.GetDateQueryTwo(asFieldFrom, adtWtStart, ldtAdd, aiDb);
                        lsWhere = asAnd + lsWhere;
                        //" timeline.dt_monat >= Convert(DateTime," + "\'" + adtWtStart + "',104) "
                        //            + "And timeline.dt_monat <= Convert(DateTime," + "\'" + ldtAdd + "',104)";
                    }

                    // Start und EndeDatum 
                    if ((adtWtStart > DateTime.MinValue) && (adtWtEnd > DateTime.MinValue))
                    {
                        lsWhere = RdQueriesTime.GetDateQueryTwo(asFieldFrom, adtWtStart, adtWtEnd, aiDb);
                        lsWhere = asAnd + lsWhere;
                        //    " timeline.dt_monat >= Convert(DateTime," + "\'" + adtWtStart + "',104) "
                        //+ "And timeline.dt_monat <= Convert(DateTime," + "\'" + adtWtEnd + "',104)";
                    }
                    // Wurde kein Datum gewählt, aktuelles Jahr zeigen
                    else
                    {
                        lsWhere = RdQueriesTime.GetDateQueryTwo(asFieldFrom, adtStart, adtEnd, aiDb);
                        lsWhere = asAnd + lsWhere;
                        //lsWhereAdd2 = lsAnd + " timeline.dt_monat >= Convert(DateTime," + "\'" + ldtStart + "',104) "
                        //    + "And timeline.dt_monat <= Convert(DateTime," + "\'" + ldtEnd + "',104)";
                    }
                    break;
                case 3:         // Es gibt das Feld from und To
                                // Start und EndeDatum 
                    if ((adtWtStart > DateTime.MinValue) && (adtWtEnd > DateTime.MinValue))
                    {
                        lsWhere = RdQueriesTime.GetDateQueryThree(asFieldFrom, asFieldTo, adtWtStart, adtWtEnd, aiDb);
                        lsWhere = asAnd + lsWhere;
                        //    " timeline.dt_monat >= Convert(DateTime," + "\'" + adtWtStart + "',104) "
                        //+ "And timeline.dt_monat <= Convert(DateTime," + "\'" + adtWtEnd + "',104)";
                    }
                    // Wurde kein Datum gewählt, aktuelles Jahr zeigen
                    else
                    {
                        lsWhere = RdQueriesTime.GetDateQueryThree(asFieldFrom, asFieldTo, adtStart, adtEnd, aiDb);
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
        internal static string GetAbrInfo(int aiFiliale, int liIdObj, int liIdObjTeil, int liMieterId, DateTime ldtStartTmp, DateTime ldtEndTmp, int aiArt, int aiDb)
        {
            string lsSql = "";

            switch (aiDb)
            {
                case 1:     // MsSql
                    switch (aiArt)
                    {
                        case 201:       // Objekt
                            lsSql = @"Delete from x_abr_info;
                        Insert into x_abr_info (id_filiale,id_objekt,abr_dat_von,abr_dat_bis) 
                        values (" + aiFiliale + "," + liIdObj.ToString() + ", Convert(DateTime," + "\'" + ldtStartTmp + "',104) , Convert(DateTime," + "\'" + ldtEndTmp + "',104))";
                            break;
                        case 202:       // ObjektTeil
                            lsSql = @"Delete from x_abr_info;
                        Insert into x_abr_info (id_filiale,id_objekt,id_objekt_teil,abr_dat_von,abr_dat_bis) 
                        values (" + aiFiliale + "," + liIdObj.ToString() + "," + liIdObjTeil.ToString() + ", Convert(DateTime," + "\'" + ldtStartTmp + "',104) , Convert(DateTime," + "\'" + ldtEndTmp + "',104))";
                            break;
                        case 203:       // Mieter
                            lsSql = @"Delete from x_abr_info;
                        Insert into x_abr_info (id_filiale,id_mieter,id_objekt,id_objekt_teil,abr_dat_von,abr_dat_bis) 
                        values (" + aiFiliale + "," + liMieterId.ToString() + "," + liIdObj.ToString() + "," + liIdObjTeil.ToString() + ", Convert(DateTime," + "\'" + ldtStartTmp + "',104) , Convert(DateTime," + "\'" + ldtEndTmp + "',104))";
                            break;
                        default:
                            break;
                    }
                    break;
                case 2:     // MySql
                    switch (aiArt)
                    {
                        case 201:           // Objekt
                            lsSql = @"Delete from x_abr_info Where Id_info > 0;
                                        Insert into x_abr_info (id_filiale,id_objekt,abr_dat_von,abr_dat_bis) 
                                        values (" + aiFiliale + "," + liIdObj.ToString() + ", " +
                                        "str_to_date(\"" + ldtStartTmp.ToString("dd.MM.yyyy") + "\",\"%d.%m.%Y %H:%i:%s\"), " +
                                        "str_to_date(\"" + ldtEndTmp.ToString("dd.MM.yyyy") + "\",\"%d.%m.%Y %H:%i:%s\"))";
                            break;
                        case 202:           // ObjektTeil
                            lsSql = @"Delete from x_abr_info Where Id_info > 0;
                                        Insert into x_abr_info (id_filiale,id_objekt,id_objekt_teil,abr_dat_von,abr_dat_bis) 
                                        values(" + aiFiliale + ", " + liIdObj.ToString() + ", " + liIdObjTeil.ToString() + ", " +
                                        "str_to_date(\"" + ldtStartTmp.ToString("dd.MM.yyyy") + "\",\"%d.%m.%Y %H:%i:%s\"), " +
                                        "str_to_date(\"" + ldtEndTmp.ToString("dd.MM.yyyy") + "\",\"%d.%m.%Y %H:%i:%s\"))";
                                                        break;
                        case 203:           // Mieter
                            lsSql = @"Delete from x_abr_info Where Id_info > 0;
                                        Insert into x_abr_info (id_filiale,id_mieter,id_objekt,id_objekt_teil,abr_dat_von,abr_dat_bis) 
                                        values (" + aiFiliale + "," + liMieterId.ToString() + "," + liIdObj.ToString() + "," + liIdObjTeil.ToString() + ", " +
                                        "str_to_date(\"" + ldtStartTmp.ToString("dd.MM.yyyy") + "\",\"%d.%m.%Y %H:%i:%s\"), " +
                                        "str_to_date(\"" + ldtEndTmp.ToString("dd.MM.yyyy") + "\",\"%d.%m.%Y %H:%i:%s\"))";
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

        // Datumsstring zusammenbauen
        internal static string GetDateQueryResultVertrag(DateTime adtStart, string AsFieldFrom, string AsFieldTo, int aiOne, int aiDb)
        {
            string LsWhereAdd = "";
            string lsAnd = " AND ";

            switch (aiDb)
            {
                case 1:
                    LsWhereAdd = lsAnd + AsFieldFrom + " >= Convert(DateTime, " + "\'" + adtStart + "', 104) ";
                    break;
                case 2:
                    LsWhereAdd = lsAnd + " (" + AsFieldFrom +     " <= str_to_date(\"" + adtStart.ToString("dd.MM.yyyy") + "\",\"%d.%m.%Y %H:%i:%s %H:%i:%s\") ";
                    LsWhereAdd = LsWhereAdd + lsAnd + AsFieldTo + " >= str_to_date(\"" + adtStart.ToString("dd.MM.yyyy") + "\",\"%d.%m.%Y %H:%i:%s %H:%i:%s\") " + ") ";                      
                    break;
                default:
                    break;
            }
            return (LsWhereAdd);
        } 
    }
}