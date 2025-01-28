// using Microsoft.Office.Interop.Excel
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using static System.Windows.Forms.LinkLabel;

namespace Ruddat_NK
{
    internal class RdAfterfetch
    {
        // ----------------------------------------------------------------------------------------
        // Datenbankaktionen nach Fetchdata
        // Rechnungen direkt für Objekte, Teilobjekte und Mieter
        // aiArt wird für Art 1 = Rechnung
        // AiTeil wird Art der Aufteilung?
        // ----------------------------------------------------------------------------------------
        public static int MakeAfterFetch(int aiArt, int aiTeil, int ai1, int ai2, string asConnect, 
            MySqlDataAdapter ASdaRechnungenTmp, DataTable ATblRechnungen,
            MySqlDataAdapter AsdaObjektTeile, DataTable ATblObjektTeile,
            MySqlDataAdapter AsdaMieter, DataTable ATblMieter,
            MySqlDataAdapter AsdaTimeline, DataTable ATblTimeline
            )
        {
            DateTime ldtStart = DateTime.MinValue;
            DateTime ldtEnd = DateTime.MinValue;
            DateTime ldtMonat = DateTime.MinValue;
            DateTime ldtVertrag = DateTime.MinValue;

            int liObjekt = 0;
            int liObjektTeil = 0;
            int liMieter = 0;
            int LiKsa = 0;                  // Kostenstellenart
            int liMonths = 0;               //Anzahl der einzutragenden Monate
            int liDaysStart = 0;            // Anzahl der Tages Startmonats
            int liDaysEnd = 0;              // Anzahl der Tages EndMonats
            // int liDaysInMonth = 0;       // Tage im Monat aus Vertrag
            int liSave = 1;                 // Freigabe
            int liArtRelation = 0;          // 1= Rechnung, 2=Zahlung, 3=Zähler
            int LiSwitch = 0;               // Auswahl Obj, TeilObj, Mieter

            //decimal ldBetragNetto = 0;
            //decimal ldBetragSollNetto = 0;
            //decimal ldBetragBrutto = 0;
            //decimal ldBetragSollBrutto = 0;
            //decimal ldGesamtflaeche = 0;
            //decimal ldZs = 0;               // Zählerstand
            //decimal ldVerbrauch = 0;        // Zähler Verbrauch
            decimal[] ladBetraege = new decimal[12];

            //int zl = 0;
            //int liZlgOrRg = 0;
            int LiSourceId = 0;
            //int liRechnungId = 0;
            //int liVerteilungId = 0;
            //int liZahlungId = 0;
            //int liZaehlerstandId = 0;
            int liOk = 0;
            //int liAnzPersonenObj = 0;
            //int liAnzPersonenObt = 0;
            //int liFlTml = 0;            // Flag TimeLine in Zahlungen
            //int liImportId = 0;         // Import Id
            //int liRgId = 0;             // Rechnungs ID
            //int liZsId = 0;             // Zähler Id

            //string lsVerteilung = "";
            //string LsSql = "";
            //string lsObjektBez = "", lsObjektTeilBez = "";
            //string lsObjektBezS = "";
            int LiReturn = 0;

            for (int i = 0; i < ATblRechnungen.Rows.Count; i++)
            {
                // ID aus der Rechnung ermitteln 
                if (ATblRechnungen.Rows[i].ItemArray.GetValue(14) != DBNull.Value)
                {
                    if (int.Parse(ATblRechnungen.Rows[i].ItemArray.GetValue(14).ToString()) == ai1)     // Nur der zugefügte oder editierte Datensatz
                    {
                        // Die Original Id der Rechnung
                        LiSourceId = int.Parse(ATblRechnungen.Rows[i].ItemArray.GetValue(0).ToString());

                        // Erzeugte Untergeordnete Rechnungen löschen

                        // Alle mit der Id der Hauptrechnung in id_rechnung_source löschen
                        Timeline.DeleteRechnung(LiSourceId, "R", asConnect);

                        // Objekt Rechnung
                        if (ATblRechnungen.Rows[i].ItemArray.GetValue(8) != DBNull.Value)
                            if ((int)ATblRechnungen.Rows[i].ItemArray.GetValue(8) > 0)
                            {
                                LiKsa = (int)ATblRechnungen.Rows[i].ItemArray.GetValue(1);                  // Kostenart
                                liObjekt = (int)ATblRechnungen.Rows[i].ItemArray.GetValue(8);               // Objekt
                                liArtRelation = 1;                                                             // Rechnung

                                if (Timeline.GetWeiterleitung(1, LiKsa, asConnect) == 1)
                                {
                                    liArtRelation = 1;          // Rechnung
                                                                // Rechnungen und Timeline für alle zugehörigen Objektteile erzeugen
                                    CreateRechnungenObjTeile(LiSourceId, liObjekt, liObjektTeil, liMieter, liArtRelation,
                                        ASdaRechnungenTmp, ATblRechnungen,
                                        AsdaObjektTeile, ATblObjektTeile,
                                        AsdaTimeline, ATblTimeline,
                                        asConnect
                                        );
                                }
                            }
                        // Teilobjekt Rechnung
                        if (ATblRechnungen.Rows[i].ItemArray.GetValue(9) != DBNull.Value)
                            if ((int)ATblRechnungen.Rows[i].ItemArray.GetValue(9) > 0)
                            {
                                LiKsa = (int)ATblRechnungen.Rows[i].ItemArray.GetValue(1);                  // Kostenart
                                liObjektTeil = (int)ATblRechnungen.Rows[i].ItemArray.GetValue(9);           // ObjektTeil
                                liArtRelation = 1;                                                             // Rechnung
                            }
                        // Mieter Rechnung
                        if (ATblRechnungen.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                            if ((int)ATblRechnungen.Rows[i].ItemArray.GetValue(10) > 0)
                            {
                                LiKsa = (int)ATblRechnungen.Rows[i].ItemArray.GetValue(1);                   // Kostenart
                                liMieter = (int)ATblRechnungen.Rows[i].ItemArray.GetValue(10);               // Mieter
                                liArtRelation = 1;                                                              // Rechnung
                            }



                        //switch (LiSwitch)
                        //{
                        //    case 1:         // Objekte nach Anlegen einer Rechnung in Objekten
                        //                    // Timeline für Objekte
                        //        if (CreateTimeline(LiSourceId, liObjekt, liObjektTeil, liMieter, liArtRelation,
                        //            ASdaRechnungenTmp, ATblRechnungenTmp,
                        //            AsdaObjektTeile, ATblObjektTeile,
                        //            AsdaTimeline, ATblTimeline,
                        //            asConnect
                        //            ) == 1)
                        //        {
                        //            // Weiterleitung an ObjektTeil aus der Kostenart ermitteln
                        //            // 1 = Weiterleitung an Teilobjekt
                        //            if (Timeline.GetWeiterleitung(1, LiKsa, asConnect) == 1)
                        //            {
                        //                liArtRelation = 1;          // Rechnung
                        //                                            // Rechnungen und Timeline für alle zugehörigen Objektteile erzeugen
                        //                CreateRechnungenObjTeile(LiSourceId, liObjekt, liObjektTeil, liMieter, liArtRelation,
                        //                    ASdaRechnungenTmp, ATblRechnungenTmp,
                        //                    AsdaObjektTeile, ATblObjektTeile,
                        //                    AsdaTimeline, ATblTimeline,
                        //                    asConnect
                        //                    );
                        //            }
                        //        }
                        //        break;
                        //    case 2:         // Teilobjekte nach Anlegen oder ändern einer Rechnung Timeline erzeugen
                        //        // Timeline erstellen
                        //        CreateTimeline(LiSourceId, liObjekt, liObjektTeil, liMieter, liArtRelation,
                        //            ASdaRechnungenTmp, ATblRechnungenTmp,
                        //            AsdaObjektTeile, ATblObjektTeile,
                        //            AsdaTimeline, ATblTimeline,
                        //            asConnect);
                        //        break;
                        //    case 3:         // Mieterkosten direkt, der Mieter ist klar, deshalb diese Funktion
                        //        CreateTimeline(LiSourceId, liObjekt, liObjektTeil, liMieter, liArtRelation,
                        //            ASdaRechnungenTmp, ATblRechnungenTmp,
                        //            AsdaObjektTeile, ATblObjektTeile,
                        //            AsdaTimeline, ATblTimeline,
                        //            asConnect);
                        //        break;
                        //    default:
                        //        break;
                        //}
                    }
                }
            }
            return LiReturn;
        }

        // Einfache Timeline für eine Rechnung neu erzeugen
        // AiArtRelation ist 1 für Objekt, 2 für Teilobjekt
        internal static int CreateTimeline(int AiSourceId, int AiObjektId, int AiObjektTeilId,
                int AiMieterid, int AiArtRelation, 
                MySqlDataAdapter ASdaRechnungen, System.Data.DataTable ATblRechnungen,
                MySqlDataAdapter ASdaTeilobjekte, System.Data.DataTable ATblTeilobjekte,
                MySqlDataAdapter ASdaTimeline, System.Data.DataTable ATblTimeline,
                string AsConnect)
        {
            int LiOk = 0;

            DateTime LdtStart = DateTime.MinValue;
            DateTime LdtEnd = DateTime.MinValue;
            DateTime ldtMonat = DateTime.MinValue;
            DateTime ldtVertrag = DateTime.MinValue;
            DateTime Ldtrechnung = DateTime.MinValue;

            int liObjekt = 0;
            int liObjektTeil = 0;
            int liMieter = 0;
            int LiKsa = 0; // Kostenstellenart
            int liMonths = 0; //Anzahl der einzutragenden Monate
            int liDaysStart = 0; // Anzahl der Tages Startmonats
            int liDaysEnd = 0; // Anzahl der Tages EndMonats
            // int liDaysInMonth = 0; // Tage im Monat aus Vertrag
            int liSave = 1;  // Freigabe
            int LiArtRelation = AiArtRelation;      // 1= Rechnung, 2=Zahlung, 3=Zähler
            int LiRgIdSource = 0;

            decimal ldBetragNetto = 0;
            decimal ldBetragSollNetto = 0;
            decimal ldBetragBrutto = 0;
            decimal ldBetragSollBrutto = 0;
            decimal ldGesamtflaeche = 0;
            decimal ldZs = 0;            // Zählerstand
            // decimal ldVerbrauch = 0;    // Zähler Verbrauch
            decimal[] ladBetraege = new decimal[12];

            int zl = 0;
            int LiMwstId = 0;
            int liZlgOrRg = 0;
            int liVerteilungId = 0;     // Id Kostenverteilung
            int LiRechnungId = 0;         // RechnnungsId
            //int liRechnungId = 0;
            //int liZahlungId = 0;
            //int liZaehlerstandId = 0;
            //int liOk = 0;
            //int liAnzPersonenObj = 0;
            //int liAnzPersonenObt = 0;
            //int liFlTml = 0;            // Flag TimeLine in Zahlungen
            //int liImportId = 0;         // Import Id

            //int liRgId = 0;             // Rechnungs ID
            //int liZsId = 0;             // Zähler Id

            string LsVerteilung = "";
            string LsRgNr = "";
            string LsFirma = "";

            for (int i = 0; i < ATblRechnungen.Rows.Count; i++)
            {
                if (ATblRechnungen.Rows[i].ItemArray.GetValue(0) != DBNull.Value)
                {
                    int LiWtlObjekt = 0;            // Weiterleitungen
                    int LiWtlObjektTeil = 0;

                    // Eine Rechnung aus dem Teilobjekt?
                    if (ATblRechnungen.Rows[i].ItemArray.GetValue(17) != DBNull.Value)          // Rechnungs Source Id Quelle Weiterleitung 
                    {
                        if (AiObjektTeilId > 0)
                        {
                            LiWtlObjekt = 1;
                            // Timeline mit der SouceId löschen
                            Timeline.DeleteTimeline((int)ATblRechnungen.Rows[i].ItemArray.GetValue(17), "S", AsConnect);
                        }
                    }
                    else if (ATblRechnungen.Rows[i].ItemArray.GetValue(0) != DBNull.Value)     // Rechnungs Id Objekt
                    {
                        // Timeline löschen
                        Timeline.DeleteTimeline((int)ATblRechnungen.Rows[i].ItemArray.GetValue(0), "R", AsConnect);
                        // Weiterleitungen Rechnung aus Objekt
                        if (AiObjektId > 0)
                        {

                            LiWtlObjekt = 1;
                        }
                        // Rechnung aus Teilobjekt
                        if (AiObjektTeilId > 0 && LiWtlObjekt == 0)
                        {
                           LiWtlObjektTeil = 1;
                        }
                    }
                    
                    // RechnungId
                    LiRechnungId = (int)ATblRechnungen.Rows[i].ItemArray.GetValue(0);

                    if (ATblRechnungen.Rows[i].ItemArray.GetValue(7) != DBNull.Value)
                        LiMwstId = int.Parse(ATblRechnungen.Rows[i].ItemArray.GetValue(7).ToString());
                    if (ATblRechnungen.Rows[i].ItemArray.GetValue(8) != DBNull.Value)
                        liObjekt = (int)ATblRechnungen.Rows[i].ItemArray.GetValue(8);
                    if (ATblRechnungen.Rows[i].ItemArray.GetValue(9) != DBNull.Value)
                        liObjektTeil = (int)ATblRechnungen.Rows[i].ItemArray.GetValue(9);
                    if (ATblRechnungen.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                        liMieter = (int)ATblRechnungen.Rows[i].ItemArray.GetValue(10);
                    if (ATblRechnungen.Rows[i].ItemArray.GetValue(11) != DBNull.Value)
                        LsRgNr = ATblRechnungen.Rows[i].ItemArray.GetValue(11).ToString();
                    if (ATblRechnungen.Rows[i].ItemArray.GetValue(12) != DBNull.Value)
                        LsFirma = ATblRechnungen.Rows[i].ItemArray.GetValue(12).ToString();
                    if (ATblRechnungen.Rows[i].ItemArray.GetValue(5) != DBNull.Value)
                        ldBetragNetto = (decimal)ATblRechnungen.Rows[i].ItemArray.GetValue(5);
                    if (ATblRechnungen.Rows[i].ItemArray.GetValue(6) != DBNull.Value)
                        ldBetragBrutto = (decimal)ATblRechnungen.Rows[i].ItemArray.GetValue(6);
                    if (ATblRechnungen.Rows[i].ItemArray.GetValue(2) != DBNull.Value)
                        Ldtrechnung = (DateTime)ATblRechnungen.Rows[i].ItemArray.GetValue(2);
                    if (ATblRechnungen.Rows[i].ItemArray.GetValue(3) != DBNull.Value)
                        LdtStart = (DateTime)ATblRechnungen.Rows[i].ItemArray.GetValue(3);
                    if (ATblRechnungen.Rows[i].ItemArray.GetValue(4) != DBNull.Value)
                        LdtEnd = (DateTime)ATblRechnungen.Rows[i].ItemArray.GetValue(4);
                    if (ATblRechnungen.Rows[i].ItemArray.GetValue(1) != DBNull.Value)
                        LiKsa = (int)ATblRechnungen.Rows[i].ItemArray.GetValue(1);
                    if (ATblRechnungen.Rows[i].ItemArray.GetValue(16) != DBNull.Value)
                        liVerteilungId = (int)ATblRechnungen.Rows[i].ItemArray.GetValue(16);
                    if (ATblRechnungen.Rows[i].ItemArray.GetValue(17) != DBNull.Value)
                        LiRgIdSource = (int)ATblRechnungen.Rows[i].ItemArray.GetValue(17);

                    // Anzahl der Tage des ersten Monats        99 ist der volle Monat
                    liDaysStart = Timeline.GetDaysStart(LdtStart);
                    // Anzahl der Tage des letzten Monats       99 ist der volle Monat
                    liDaysEnd = Timeline.GetDaysEnd(LdtEnd);
                    // Anzahl der einzutragenden Monate ermitteln
                    liMonths = Timeline.GetMonths(LdtStart, LdtEnd);
                    // Zahlung oder Rechnung 1= Zahlung 2= Rechnung
                    liZlgOrRg = 2;

                    // Monatsbeträge ermitteln (Brutto und Netto) und evtl. erster und letzter Monat nicht voll
                    ladBetraege = Timeline.GetBetraege(liMonths, liDaysStart, liDaysEnd,
                    ldBetragNetto, ldBetragBrutto,
                            ldBetragSollNetto, ldBetragSollBrutto, liZlgOrRg, LdtStart, LdtEnd);

                    // Den ersten Monat ermitteln
                    string dt = (LdtStart.Year.ToString()) + "-" + LdtStart.Month.ToString() + "-01";
                    ldtMonat = DateTime.Parse(dt);                 // Datetime mit erstem Tag des Monats

                    // Ermitteln der VerteilungsId aus Tabelle Rechnungen
                    liVerteilungId = Timeline.GetVerteilungsId(AsConnect, LiRechnungId);
                    // Ermitteln, wie verteilt werden soll aus der Tabelle art_verteilung
                    LsVerteilung = Timeline.GetVerteilung(AsConnect, liVerteilungId);
                    // Gesamtfläche aus Tabelle Objekt holen
                    ldGesamtflaeche = Timeline.GetObjektflaeche(liObjekt, liObjektTeil, liMieter, AsConnect);

                    // Timeline für das Objekt, Objektteil oder Mieter erzeugen
                    for (int ii = 1; ii <= liMonths; ii++)
                    {
                        DataRow DrTimeline = ATblTimeline.NewRow();

                        DrTimeline[1] = LiRechnungId;
                        DrTimeline[4] = liObjekt;
                        DrTimeline[5] = liObjektTeil;              // (int)ATblTeilobjekte.Rows[i].ItemArray.GetValue(0); // Objekt Id
                        DrTimeline[6] = liMieter;
                        DrTimeline[7] = LiKsa;                     // Kostenart
                        //---------------------------------------------
                        if (liDaysStart != 99 && zl == 1)
                        {
                            DrTimeline[8] = ladBetraege[5];         // Netto erster Monat bei späterem Beginn
                            DrTimeline[10] = ladBetraege[6];         // Brutto
                        }
                        //---------------------------------------------
                        else if (liDaysEnd != 99 && zl == liMonths)
                        {
                            DrTimeline[8] = ladBetraege[9];         // Netto letzter Monat bei früherem Ende
                            DrTimeline[10] = ladBetraege[10];         // Brutto
                        }
                        else
                        {
                            DrTimeline[8] = ladBetraege[0];
                            DrTimeline[10] = ladBetraege[1];
                        }
                        //---------------------------------------------  
                        DrTimeline[9] = ladBetraege[3];
                        DrTimeline[11] = ladBetraege[4];
                        DrTimeline[12] = ldZs;                  // Zählerä.fgöl,stand

                        if (ii == 1)                            // erster Monat
                            DrTimeline[13] = LdtStart;
                        else if (ii == liMonths)                // letzter Monat Letzten Tag eintragen
                            DrTimeline[13] = LdtEnd;
                        else
                            DrTimeline[13] = ldtMonat;          // Der Timelinemonat

                        DrTimeline[14] = LiWtlObjekt;           // Weiterleitung aus Objekt
                        DrTimeline[15] = LiWtlObjektTeil;       // weiterleitung aus Teilobjekt
                        DrTimeline[18] = LiRgIdSource;          // Rechnunsquelle

                        ATblTimeline.Rows.Add(DrTimeline);
                        // + Monat 
                        ldtMonat = ldtMonat.AddMonths(1);

                    }
                    // Ab in die Datenbank
                    MySqlCommandBuilder commandBuilder = new MySqlCommandBuilder(ASdaTimeline);
                    ASdaTimeline.Update(ATblTimeline);

                    LiOk = 1;
                }
                else
                {
                    MessageBox.Show("Verarbeitungsfehler ERROR fetchdata fetchdata RdFunctions\n CreateRechnungen = ",
                                "Achtung");
                    LiOk = 0;
                }
            }

            return LiOk;
        }

        // Timeline für Mieter erzeugen
        internal static void CreateTimelineMieter(int AiReserve, int AiObjektId, int AiObjektTeilId, int AiMieterId, int AiArtRelation,
            MySqlDataAdapter ASdRechnungen, DataTable ATblRechnungen,
            MySqlDataAdapter ASdTimeLineObjTeile, DataTable ATblTimeLineObjTeile,
            MySqlDataAdapter ASdTimeLine, DataTable ATblTimeLine,
            string AsConnect)
        {
            int LiMieterId = 0;
            int LiArtVerteilungId = 0;
            decimal LdProzent = 0;
            string LsVerteilung = ""; // Kurzstring verteilung

            DateTime LdtVertragsMonat = DateTime.MinValue;
            DateTime LdtVertragsInfo = DateTime.MinValue;

            // Erstmal alle Einträge der Timeline auf Mieterebene löschen
            if (Timeline.DeleteTimeline(AiMieterId, "M", AsConnect) == 1)
            {
                // Ermitteln, welche Kostenart für prozentuale Weiterleitung zum Mieter id 2?
                LsVerteilung = "pz";
                LiArtVerteilungId = Timeline.GetVerteilungIdAusArtVerteilung(LsVerteilung, AsConnect);

                // Hier wird die ganze Timeline des Teilobjekts auf den Mieter übertragen
                // Einschränkung: Gucken, ob der Vertrag existiert
                // Todo später prüfen: Es werden auch Zählerwerte miterfasst 
                for (int i = 0; i < ATblTimeLineObjTeile.Rows.Count; i++)
                {
                    LiMieterId = 0;
                    LdProzent = 0;
                    // Hat der Mieter einen Vertrag
                    LiMieterId = Timeline.GetAktMieter((int)ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(5),
                                                        (DateTime)ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(13), AsConnect, 2);
                    // Teilobjekt Id
                    if (ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(5) != DBNull.Value && ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(1) != DBNull.Value)
                    {
                        if (Timeline.GetVerteilungsId(AsConnect, (int)ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(1)) == LiArtVerteilungId)
                        {
                            // Gibt es einen Prozentsatz in dem Teilobjekt
                            // Welcher Prozentsatz ist bei der Mietfläche hinterlegt?
                            LdProzent = Timeline.GetProzentFromObjTeil((int)ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(5), AsConnect);
                        }
                    }

                    if (AiMieterId == LiMieterId)
                    {
                        DataRow DrTimeline = ATblTimeLine.NewRow();

                        DrTimeline[1] = ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(1);
                        DrTimeline[2] = ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(2);
                        DrTimeline[3] = ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(3);
                        DrTimeline[4] = 0;                                                      // kein Objekt
                        DrTimeline[5] = 0;                                                      // Kein Teilobjekt
                        DrTimeline[6] = AiMieterId;                                             // Mieter
                        DrTimeline[7] = ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(7);


                        if (LdProzent > 0)  // Prozentuale Weiterleitung
                        {
                            DrTimeline[8] = ((decimal)ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(8) / 100) * LdProzent;     // NettoBetrag
                            DrTimeline[9] = ((decimal)ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(9) / 100) * LdProzent;
                            DrTimeline[10] = ((decimal)ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(10) / 100) * LdProzent;    // BruttoBetrag
                            DrTimeline[11] = ((decimal)ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(11) / 100) * LdProzent;
                        }
                        else
                        {
                            DrTimeline[8] = ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(8);     // NettoBetrag
                            DrTimeline[9] = ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(9);
                            DrTimeline[10] = ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(10);    // BruttoBetrag
                            DrTimeline[11] = ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(11);
                        }

                        DrTimeline[12] = ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(12);
                        DrTimeline[13] = ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(13);
                        DrTimeline[14] = ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(14);
                        DrTimeline[15] = ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(15);
                        DrTimeline[16] = ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(16);
                        DrTimeline[18] = ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(18);

                        ATblTimeLine.Rows.Add(DrTimeline);
                    }
                    else
                    {
                        if (LiMieterId == 0)            // Todo Leerstand buchen
                        {
                            DataRow DrTimeline = ATblTimeLine.NewRow();

                            DrTimeline[1] = ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(1);
                            DrTimeline[2] = ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(2);
                            DrTimeline[3] = ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(3);
                            DrTimeline[4] = 0;                                                      // kein Objekt
                            DrTimeline[5] = 0;                                                      // kein Teilobjekt
                            DrTimeline[6] = 0;                                                      // kein Mieter
                            DrTimeline[7] = ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(7);
                            DrTimeline[8] = ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(8);
                            DrTimeline[9] = ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(9);
                            DrTimeline[10] = ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(10);
                            DrTimeline[11] = ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(11);
                            DrTimeline[12] = ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(12);
                            DrTimeline[13] = ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(13);
                            DrTimeline[14] = ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(14);
                            DrTimeline[15] = ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(15);
                            DrTimeline[16] = AiObjektTeilId;                                        // Leerstand auf das Teilobjekt buchen
                            DrTimeline[18] = ATblTimeLineObjTeile.Rows[i].ItemArray.GetValue(18);   // Rechnungsquelle
                            ATblTimeLine.Rows.Add(DrTimeline);
                        }
                    }
                }
                // Ab in die Datenbank
                MySqlCommandBuilder commandBuilder = new MySqlCommandBuilder(ASdTimeLine);
                ASdTimeLine.Update(ATblTimeLine);
            }
            else
            {
                MessageBox.Show("Löschen von Daten fehlgeschlagen (TimeLLine Mieter)", "Datenfehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Aus Rechnungen Objekt Rechnung für jedes ObjektTeil erzeugen
        private static int CreateRechnungenObjTeile(int AiSourceId, int AiObjektId, int AiObjektTeilId,
                int AiMieterid, int AiArtRelation,
                MySqlDataAdapter ASdaRechnungen, System.Data.DataTable ATblRechnungen,
                MySqlDataAdapter ASdaTeilobjekte, System.Data.DataTable ATblTeilobjekte,
                MySqlDataAdapter ASdaTimeline, System.Data.DataTable ATblTimeline,
                string AsConnect)
        {
            int LiOk = 0;

            DateTime LdtStart = DateTime.MinValue;
            DateTime LdtEnd = DateTime.MinValue;
            DateTime ldtMonat = DateTime.MinValue;
            DateTime ldtVertrag = DateTime.MinValue;
            DateTime Ldtrechnung = DateTime.MinValue;

            int liObjekt = 0;
            int liObjektTeil = 0;
            int liMieter = 0;
            int LiKsa = 0; // Kostenstellenart
            int liMonths = 0; //Anzahl der einzutragenden Monate
            int liDaysStart = 0; // Anzahl der Tages Startmonats
            int liDaysEnd = 0; // Anzahl der Tages EndMonats
            // int liDaysInMonth = 0; // Tage im Monat aus Vertrag
            int liSave = 1;  // Freigabe
            int LiArtRelation = AiArtRelation;      // 1= Rechnung, 2=Zahlung, 3=Zähler
            int LiPersonenFlag = 0;     // Für Message Personen

            decimal ldBetragNetto = 0;
            decimal ldBetragSollNetto = 0;
            decimal ldBetragBrutto = 0;
            decimal ldBetragSollBrutto = 0;
            decimal LdGesamtflaeche = 0;
            decimal ldZs = 0;            // Zählerstand
            decimal ldVerbrauch = 0;     // Zähler Verbrauch
            decimal[] LadBetragMonat = new decimal[12];

            int zl = 0;
            int LiMwstId = 0;
            int liZlgOrRg = 0;
            int LiSourceId = 0;
            //int liRechnungId = 0;
            //int liZahlungId = 0;
            //int liZaehlerstandId = 0;
            //int liOk = 0;
            //int liAnzPersonenObj = 0;
            //int liAnzPersonenObjTeil = 0;
            //int liFlTml = 0;            // Flag TimeLine in Zahlungen
            //int liImportId = 0;         // Import Id
            int liVerteilungId = 0;         // Id Kostenverteilung
            int LiVerteilungsIdNew = 0;     // VerteilungsId muss evtl. umgewandelt werden
            //int liRgId = 0;             // Rechnungs ID
            //int liZsId = 0;             // Zähler Id

            string LsVerteilung = "";
            string LsRgNr = "";
            string LsFirma = "";

            if (ATblRechnungen.Rows[0].ItemArray.GetValue(0) != DBNull.Value)
            {
                LiSourceId = (int)ATblRechnungen.Rows[0].ItemArray.GetValue(0);        // Rechnungs Id Quelle

                if (ATblRechnungen.Rows[0].ItemArray.GetValue(7) != DBNull.Value)
                    LiMwstId = int.Parse(ATblRechnungen.Rows[0].ItemArray.GetValue(7).ToString());
                if (ATblRechnungen.Rows[0].ItemArray.GetValue(8) != DBNull.Value)
                    liObjekt = (int)ATblRechnungen.Rows[0].ItemArray.GetValue(8);
                if (ATblRechnungen.Rows[0].ItemArray.GetValue(9) != DBNull.Value)
                    liObjektTeil = (int)ATblRechnungen.Rows[0].ItemArray.GetValue(9);
                if (ATblRechnungen.Rows[0].ItemArray.GetValue(10) != DBNull.Value)
                    liMieter = (int)ATblRechnungen.Rows[0].ItemArray.GetValue(10);
                if (ATblRechnungen.Rows[0].ItemArray.GetValue(11) != DBNull.Value)
                    LsRgNr = ATblRechnungen.Rows[0].ItemArray.GetValue(11).ToString();
                if (ATblRechnungen.Rows[0].ItemArray.GetValue(12) != DBNull.Value)
                    LsFirma = ATblRechnungen.Rows[0].ItemArray.GetValue(12).ToString();
                if (ATblRechnungen.Rows[0].ItemArray.GetValue(5) != DBNull.Value)
                    ldBetragNetto = (decimal)ATblRechnungen.Rows[0].ItemArray.GetValue(5);
                if (ATblRechnungen.Rows[0].ItemArray.GetValue(6) != DBNull.Value)
                    ldBetragBrutto = (decimal)ATblRechnungen.Rows[0].ItemArray.GetValue(6);
                if (ATblRechnungen.Rows[0].ItemArray.GetValue(2) != DBNull.Value)
                    Ldtrechnung = (DateTime)ATblRechnungen.Rows[0].ItemArray.GetValue(2);
                if (ATblRechnungen.Rows[0].ItemArray.GetValue(3) != DBNull.Value)
                    LdtStart = (DateTime)ATblRechnungen.Rows[0].ItemArray.GetValue(3);
                if (ATblRechnungen.Rows[0].ItemArray.GetValue(4) != DBNull.Value)
                    LdtEnd = (DateTime)ATblRechnungen.Rows[0].ItemArray.GetValue(4);
                if (ATblRechnungen.Rows[0].ItemArray.GetValue(1) != DBNull.Value)
                    LiKsa = (int)ATblRechnungen.Rows[0].ItemArray.GetValue(1);
                if (ATblRechnungen.Rows[0].ItemArray.GetValue(16) != DBNull.Value)              
                    liVerteilungId = (int)ATblRechnungen.Rows[0].ItemArray.GetValue(16);

                // Anzahl der Tage des ersten Monats        99 ist der volle Monat
                liDaysStart = Timeline.GetDaysStart(LdtStart);
                // Anzahl der Tage des letzten Monats       99 ist der volle Monat
                liDaysEnd = Timeline.GetDaysEnd(LdtEnd);
                // Anzahl der einzutragenden Monate ermitteln
                liMonths = Timeline.GetMonths(LdtStart, LdtEnd);
                // Zahlung oder Rechnung 1= Zahlung 2= Rechnung
                liZlgOrRg = 2;

                // Monatsbeträge ermitteln (Brutto und Netto) und evtl. erster und letzter Monat nicht voll
                LadBetragMonat = Timeline.GetBetraege(liMonths, liDaysStart, liDaysEnd,
                ldBetragNetto, ldBetragBrutto,
                        ldBetragSollNetto, ldBetragSollBrutto, liZlgOrRg, LdtStart, LdtEnd);

                // Den ersten Monat ermitteln
                string dt = (LdtStart.Year.ToString()) + "-" + LdtStart.Month.ToString() + "-01";
                ldtMonat = DateTime.Parse(dt);                 // Datetime mit erstem Tag des Monats

                // Ermitteln der VerteilungsId aus der übergordneten Rechnungen
                liVerteilungId = Timeline.GetVerteilungsId(AsConnect, LiSourceId);
                // Ermitteln, wie verteilt werden soll aus der Tabelle art_verteilung
                LsVerteilung = Timeline.GetVerteilung(AsConnect, liVerteilungId);

                // Wenn vom Objekt eine Verteilung Fläche oder Prozent kommt,
                // mus das im ObjektTeil zu direkter Kostenverteilung werden
                switch (LsVerteilung)
                {
                    case "fl":
                        LiVerteilungsIdNew =  Timeline.GetVerteilungIdAusArtVerteilung("di", AsConnect);
                        break;
                    case "pz":
                        LiVerteilungsIdNew = Timeline.GetVerteilungIdAusArtVerteilung("di", AsConnect);
                        break;
                    default:
                        LiVerteilungsIdNew = liVerteilungId;
                        break;
                }

                // Gesamtfläche aus Tabelle Objekt holen
                LdGesamtflaeche = Timeline.GetObjektflaeche(liObjekt, 0, 0, AsConnect);

                // Schleife durch alle Teilobjekte
                for (int i = 0; i < ATblTeilobjekte.Rows.Count; i++)
                {
                    // Neue Rechnung erzeugen
                    DataRow DrRechnung = ATblRechnungen.NewRow();
                    DrRechnung[1] = LiKsa;
                    DrRechnung[2] = Ldtrechnung;
                    DrRechnung[3] = LdtStart;
                    DrRechnung[4] = LdtEnd;
                    DrRechnung[7] = LiMwstId;
                    // DrRechnung[8] = AiObjektId;          // Darf hier nicht eingesetzt werden
                    DrRechnung[9] = (int)ATblTeilobjekte.Rows[i].ItemArray.GetValue(0);
                    DrRechnung[10] = AiMieterid;
                    DrRechnung[11] = LsRgNr;
                    DrRechnung[12] = LsFirma;
                    DrRechnung[15] = 1;                     // Flag für Timelinebearbeitung erzeugen
                    DrRechnung[16] = LiVerteilungsIdNew;
                    DrRechnung[17] = AiSourceId;

                    switch (LsVerteilung)
                    {
                        case "fl":          // Berechnung Kosten MietFläche
                            if (ATblTeilobjekte.Rows[0].ItemArray.GetValue(6) != DBNull.Value)
                            {
                                if ((decimal)ATblTeilobjekte.Rows[0].ItemArray.GetValue(6) > 0) // Fläche Teilobjekt
                                {
                                    decimal LdteilFlaeche = (decimal)ATblTeilobjekte.Rows[0].ItemArray.GetValue(6);
                                    decimal[] LdaBetrag = new decimal[2];

                                    LdaBetrag[0] = LadBetragMonat[0] / (LdGesamtflaeche / LdteilFlaeche);           // Netto    
                                    LdaBetrag[1] = LadBetragMonat[1] / (LdGesamtflaeche / LdteilFlaeche);           // Brutto

                                    DrRechnung[5] = LdaBetrag[0];           // Netto    
                                    DrRechnung[6] = LdaBetrag[1];           // Brutto

                                    ATblRechnungen.Rows.Add(DrRechnung);
                                }
                            }
                            break;
                        case "pz":          // Berechung nach Prozent
                            if (ATblTeilobjekte.Rows[0].ItemArray.GetValue(7) != DBNull.Value)
                            {
                                if ((decimal)ATblTeilobjekte.Rows[0].ItemArray.GetValue(7) > 0)     //  Prozent
                                {
                                    decimal LdteilProzent = (decimal)ATblTeilobjekte.Rows[0].ItemArray.GetValue(7);
                                    decimal[] LdaBetrag = new decimal[2];

                                    LdaBetrag[0] = LadBetragMonat[0] / (100 / LdteilProzent);           // Netto    
                                    LdaBetrag[1] = LadBetragMonat[1] / (100 / LdteilProzent);           // Brutto

                                    DrRechnung[5] = LdaBetrag[0];           // Netto    
                                    DrRechnung[6] = LdaBetrag[1];           // Brutto

                                    ATblRechnungen.Rows.Add(DrRechnung);
                                }
                            }
                            break;
                        case "ps":
                            LiPersonenFlag = 1;
                            break;
                        case "zl":      // Verteilung nach Zählerwert
                            break;
                        case "nl":      // Keine Verteilung
                            break;
                        case "di":      // Direkt und alles
                            break;
                        case "fa":      // Verteilung nach Flächenauswahl
                            break;
                        default:
                            break;
                    }
                }

                // Meldung Personenverteilung
                if (LiPersonenFlag == 1)
                {
                    MessageBox.Show("Keine Beredchnung nach Personenanzahl möglich", "Verteilung auf Personen");
                    LiPersonenFlag = 0;
                }
                // Daten in die Datenbank schreiben
                MySqlCommandBuilder commandBuilder = new MySqlCommandBuilder(ASdaRechnungen);
                ASdaRechnungen.Update(ATblRechnungen);
            }
            return LiOk;
        }
        // Nach Anwahl der Teilobjekte im Treeview die Mieterrechnungen erzeugen
        // Todo > nein Mieterrechnugen müssen aus der Timeline erzeugt werden
        // Jeder Monat muss einzeln betrachtet werden (Mietervertrag)
        internal static void CreateRechnungenMieter(int AiMieterId, int AiArtRelation,
            MySqlDataAdapter aSdRechnungen, DataTable aTblRechnungen,
            MySqlDataAdapter aSdObjektTeile, DataTable aTblObjektTeile,
            MySqlDataAdapter aSdTimeline, DataTable aTblTimeline,
            string asConnect)
        {
            int LiOk = 0;

            DateTime LdtStart = DateTime.MinValue;
            DateTime LdtEnd = DateTime.MinValue;
            DateTime ldtMonat = DateTime.MinValue;
            DateTime ldtVertrag = DateTime.MinValue;
            DateTime Ldtrechnung = DateTime.MinValue;

            int liObjekt = 0;
            int liObjektTeil = 0;
            int liMieter = 0;
            int LiKsa = 0; // Kostenstellenart
            int liMonths = 0; //Anzahl der einzutragenden Monate
            int liDaysStart = 0; // Anzahl der Tages Startmonats
            int liDaysEnd = 0; // Anzahl der Tages EndMonats
            int liDaysInMonth = 0; // Tage im Monat aus Vertrag
            int liSave = 1;  // Freigabe
            int LiArtRelation = AiArtRelation;      // 1= Rechnung, 2=Zahlung, 3=Zähler

            decimal ldBetragNetto = 0;
            //decimal ldBetragSollNetto = 0;
            decimal ldBetragBrutto = 0;
            //decimal ldBetragSollBrutto = 0;
            //decimal ldGesamtflaeche = 0;
            //decimal ldZs = 0;            // Zählerstand
            //decimal ldVerbrauch = 0;    // Zähler Verbrauch
            decimal[] ladBetraege = new decimal[12];

            int LiMwstId = 0;
            //int liZlgOrRg = 0;
            int LiSourceId = 0;
            //int liRechnungId = 0;
            //int liZahlungId = 0;
            //int liZaehlerstandId = 0;
            //int liOk = 0;
            //int liAnzPersonenObj = 0;
            //int liAnzPersonenObt = 0;
            //int liFlTml = 0;            // Flag TimeLine in Zahlungen
            //int liImportId = 0;         // Import Id
            int liVerteilungId = 0;     // Id Kostenverteilung
            //int liRgId = 0;             // Rechnungs ID
            //int liZsId = 0;             // Zähler Id

            string LsVerteilung = "";
            string LsRgNr = "";
            string LsFirma = "";

            // Durch alle Rechnungen
            for (int i = 0; i < aTblRechnungen.Rows.Count; i++)
            {
                LiSourceId = (int)aTblRechnungen.Rows[i].ItemArray.GetValue(0);        // Rechnungs Id Quelle

                if (aTblRechnungen.Rows[i].ItemArray.GetValue(7) != DBNull.Value)
                    LiMwstId = int.Parse(aTblRechnungen.Rows[i].ItemArray.GetValue(7).ToString());
                if (aTblRechnungen.Rows[i].ItemArray.GetValue(8) != DBNull.Value)
                    liObjekt = (int)aTblRechnungen.Rows[i].ItemArray.GetValue(8);
                if (aTblRechnungen.Rows[i].ItemArray.GetValue(9) != DBNull.Value)
                    liObjektTeil = (int)aTblRechnungen.Rows[i].ItemArray.GetValue(9);
                //if (aTblRechnungen.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                //    liMieter = (int)aTblRechnungen.Rows[i].ItemArray.GetValue(10);
                if (aTblRechnungen.Rows[i].ItemArray.GetValue(11) != DBNull.Value)
                    LsRgNr = aTblRechnungen.Rows[i].ItemArray.GetValue(11).ToString();
                if (aTblRechnungen.Rows[i].ItemArray.GetValue(12) != DBNull.Value)
                    LsFirma = aTblRechnungen.Rows[i].ItemArray.GetValue(12).ToString();
                if (aTblRechnungen.Rows[i].ItemArray.GetValue(5) != DBNull.Value)
                    ldBetragNetto = (decimal)aTblRechnungen.Rows[i].ItemArray.GetValue(5);
                if (aTblRechnungen.Rows[i].ItemArray.GetValue(6) != DBNull.Value)
                    ldBetragBrutto = (decimal)aTblRechnungen.Rows[i].ItemArray.GetValue(6);
                if (aTblRechnungen.Rows[i].ItemArray.GetValue(2) != DBNull.Value)
                    Ldtrechnung = (DateTime)aTblRechnungen.Rows[i].ItemArray.GetValue(2);
                if (aTblRechnungen.Rows[i].ItemArray.GetValue(3) != DBNull.Value)
                    LdtStart = (DateTime)aTblRechnungen.Rows[i].ItemArray.GetValue(3);
                if (aTblRechnungen.Rows[i].ItemArray.GetValue(4) != DBNull.Value)
                    LdtEnd = (DateTime)aTblRechnungen.Rows[i].ItemArray.GetValue(4);
                if (aTblRechnungen.Rows[i].ItemArray.GetValue(1) != DBNull.Value)
                    LiKsa = (int)aTblRechnungen.Rows[i].ItemArray.GetValue(1);
                if (aTblRechnungen.Rows[i].ItemArray.GetValue(16) != DBNull.Value)
                    liVerteilungId = (int)aTblRechnungen.Rows[i].ItemArray.GetValue(16);

                // Neue Rechnung erzeugen
                DataRow DrRechnung = aTblRechnungen.NewRow();
                DrRechnung[1] = LiKsa;
                DrRechnung[2] = Ldtrechnung;
                DrRechnung[3] = LdtStart;
                DrRechnung[4] = LdtEnd;

                //DrRechnung[5] = ldBetragNetto / (ldGesamtflaeche / (decimal)aTblObjektTeile.Rows[i].ItemArray.GetValue(6));           // Netto    
                //DrRechnung[6] = ldBetragBrutto / (ldGesamtflaeche / (decimal)aTblObjektTeile.Rows[i].ItemArray.GetValue(6));          // Brutto

                DrRechnung[7] = LiMwstId;
                // DrRechnung[8] = AiObjektId;      // Darf hier nicht eingesetzt werden
                // DrRechnung[9] = (int)aTblTeilobjekte.Rows[i].ItemArray.GetValue(0);
                DrRechnung[10] = 999; // AiMieterid;                                                   // Todo einstzen
                DrRechnung[11] = LsRgNr;
                DrRechnung[12] = LsFirma;
                DrRechnung[15] = 1;                 // Flag für Timelinebearbeitung erzeugen
                DrRechnung[16] = liVerteilungId;
                DrRechnung[17] = LiSourceId;

                aTblRechnungen.Rows.Add(DrRechnung);
            }
        }
    }
}
