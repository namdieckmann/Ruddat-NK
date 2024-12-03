// using Microsoft.Office.Interop.Excel;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace Ruddat_NK
{
    internal class Afterfetch
    {
        static string lsSql = "";

        // Datensätze Rechnungen
        static DataTable TblRechnungen;          // Rechnungen
        static DataTable TblRechnungenTimeline;      // Rechnungen für TimelineCreate
        // static DataTable tableTwo;
        static DataTable ATblTimeline;
        static DataTable TblTimeLineSet;
        static DataTable TblObjektParts;
        static DataTable TblTimelineGet;
        static DataTable TblTaxGet;
        static DataTable TblTimeLineGet;
        static DataTable TblZlg;
        static DataTable TblZlgNew;
        static DataTable TblTml;
        static DataTable TblRgId;
        static DataTable TblConSumObj;
        static DataTable TblConSumObjT;
        static DataTable TblCnt;
        static DataTable TblCntNew;
        static DataTable TblZlInfo;
        static DataTable TblParts;              // objekt_mix_parts
        static DataTable TblTmlCheckRgNr;       // Hier checken, ob schon eine Rechnungsnmmerfür das Anschreiben drin ist
        static DataTable TblTimeline;           // Timeline
        static DataTable TblTimelineObjKst;     // Kosten des Objektes darstellen 
        static DataTable TblContent;            // Content

        static MySqlDataAdapter mysda;
        static MySqlDataAdapter mysdb;
        static MySqlDataAdapter mysdc;
        // static MySqlDataAdapter mysdd;
        static MySqlDataAdapter mysde;
        static MySqlDataAdapter mysdf;
        static MySqlDataAdapter mysdg;
        static MySqlDataAdapter mysdh;
        static MySqlDataAdapter mysdZlg;
        static MySqlDataAdapter mysdZlgNew;
        static MySqlDataAdapter mysdTimeline;
        static MySqlDataAdapter mysdRgId;
        static MySqlDataAdapter mysdConSumObj;
        static MySqlDataAdapter mysdConSumObjT;
        static MySqlDataAdapter mysdCnt;
        static MySqlDataAdapter mysdCntNew;
        static MySqlDataAdapter mysdZlInfo;
        // static MySqlDataAdapter mysdObjTeil;
        static MySqlDataAdapter mysdParts;
        static MySqlDataAdapter myadp;

        // Delegates
        private delegate void DelPassDb(int giDb);

        // ----------------------------------------------------------------------------------------
        // Datenbankaktionen nach Fetchdata
        // Rechnungen direkt für Objekte, Teilobjekte und Mieter
        // ----------------------------------------------------------------------------------------
        public static int MakeAfterFetch(int aiArt, int aiTeil, int ai1, int ai2, string asConnect, 
            MySqlDataAdapter ASdaRechnungen, DataTable ATblRechnungen,
            MySqlDataAdapter AsdaObjektTeile, DataTable ATblObjektTeile,
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
            int liSave = 1;  // Freigabe
            int liArtRelation = 0;          // 1= Rechnung, 2=Zahlung, 3=Zähler
            int LiSwitch = 0;               // Auswahl Obj, TeilObj, Mieter

            decimal ldBetragNetto = 0;
            decimal ldBetragSollNetto = 0;
            decimal ldBetragBrutto = 0;
            decimal ldBetragSollBrutto = 0;
            decimal ldGesamtflaeche = 0;
            decimal ldZs = 0;               // Zählerstand
            decimal ldVerbrauch = 0;        // Zähler Verbrauch
            decimal[] ladBetraege = new decimal[12];

            int zl = 0;
            int liZlgOrRg = 0;
            int LiSourceId = 0;
            int liRechnungId = 0;
            int liVerteilungId = 0;
            int liZahlungId = 0;
            int liZaehlerstandId = 0;
            int liOk = 0;
            int liAnzPersonenObj = 0;
            int liAnzPersonenObt = 0;
            int liFlTml = 0;            // Flag TimeLine in Zahlungen
            int liImportId = 0;         // Import Id
            int liRgId = 0;             // Rechnungs ID
            int liZsId = 0;             // Zähler Id

            int aiDb = 2;

            string lsVerteilung = "";
            string LsSql = "";
            //string lsObjektBez = "", lsObjektTeilBez = "";
            //string lsObjektBezS = "";
            int LiReturn = 0;

            // Daten zuordnen
            ASdaRechnungen.Fill(ATblRechnungen);
            AsdaObjektTeile.Fill(ATblObjektTeile);
            AsdaTimeline.Fill(ATblTimeline);


            // ID aus der Rechnung ermitteln 
            if (ATblRechnungen.Rows[0].ItemArray.GetValue(14) != DBNull.Value)
            {
                // Die Original Id der Rechnung
                LiSourceId = int.Parse(ATblRechnungen.Rows[0].ItemArray.GetValue(0).ToString());

                // Erzeugte Untergeordnete Rechnungen löschen
                // Alle mit der Id der Hauptrechnung in id_rechnung_source
                liOk = Timeline.DeleteRechnung(LiSourceId, "R", asConnect);
                // Delete Timeline mit dieser Rechnungs id
                liOk = Timeline.DeleteTimeline(LiSourceId, "R", asConnect);

                // Objekt aus der Rechnung > Timeline erzeugen
                if (ATblRechnungen.Rows[0].ItemArray.GetValue(8) != DBNull.Value )
                    if ((int)ATblRechnungen.Rows[0].ItemArray.GetValue(8) > 0)
                    {
                        LiKsa = (int)ATblRechnungen.Rows[0].ItemArray.GetValue(1);                  // Kostenart
                        liObjekt = (int)ATblRechnungen.Rows[0].ItemArray.GetValue(8);               // Objekt
                        LiSwitch = 1;
                    }
                if (ATblRechnungen.Rows[0].ItemArray.GetValue(9) != DBNull.Value)
                    if ((int)ATblRechnungen.Rows[0].ItemArray.GetValue(9) > 0)                      
                    {
                        LiKsa = (int)ATblRechnungen.Rows[0].ItemArray.GetValue(1);                  // Kostenart
                        liObjektTeil = (int)ATblRechnungen.Rows[0].ItemArray.GetValue(9);           // ObjektTeil
                        LiSwitch = 2;
                    }
                if (ATblRechnungen.Rows[0].ItemArray.GetValue(10) != DBNull.Value)
                    if ((int)ATblRechnungen.Rows[0].ItemArray.GetValue(10) > 0)
                    {
                        LiKsa = (int)ATblRechnungen.Rows[0].ItemArray.GetValue(1);                  // Kostenart
                        liObjektTeil = (int)ATblRechnungen.Rows[0].ItemArray.GetValue(10);          // Mieter
                        LiSwitch = 3;
                    }

                // Timeline für Objekte, Teilobjekte Mieter erzeugen
                if (CreateTimeline(LiSourceId, liObjekt, liObjektTeil, liMieter, liArtRelation,
                    ASdaRechnungen, ATblRechnungen,
                    AsdaObjektTeile, ATblObjektTeile,
                    AsdaTimeline, ATblTimeline,
                    asConnect
                    ) == 1)
                {

                    switch (LiSwitch)
                    {
                        case 1:         // Objekte
                            // Weiterleitung an ObjektTeil aus der Kostenart ermitteln
                            // 1 = Weiterleitung an Teilobjekt
                            if (Timeline.GetWeiterleitung(1, LiKsa, asConnect) == 1)
                            {
                                liArtRelation = 1;          // Rechnung

                                // Rechnungen und Timeline für alle zugehörigen Objektteile erzeugen
                                if (CreateRechnungenObjTeile(LiSourceId, liObjekt, liObjektTeil, liMieter, liArtRelation,
                                    ASdaRechnungen, ATblRechnungen,
                                    AsdaObjektTeile, ATblObjektTeile,
                                    AsdaTimeline, ATblTimeline,
                                    asConnect
                                    ) == 1)
                                {
                                    liOk = 1;
                                }
                            }
                    break;
                        case 2:         // Teilobjekte
                            // Weiterleitung an den aktiven Mieter
                            // 2 = Weiterleitung an Mieter
                            if (Timeline.GetWeiterleitung(2, LiKsa, asConnect) == 1)
                            {
                                liArtRelation = 1;              // Rechnung

                                // Rechnunge für Mieter erzeugen
                                if (CreateRechnungenMieter(LiSourceId, liObjekt, liObjektTeil, liMieter, liArtRelation,
                                    ASdaRechnungen, ATblRechnungen,
                                    AsdaObjektTeile, ATblObjektTeile,
                                    AsdaTimeline, ATblTimeline,
                                    asConnect
                                    ) == 1)
                                {
                                    liOk = 1;
                                }

                            }
                                break;
                        case 3:         // Mieter

                            break;
                        default:
                            break;
                    }
                }
            }
            return LiReturn;
        }

        private static int CreateRechnungenMieter(int liSourceId, int liObjekt, int liObjektTeil, int liMieter, int liArtRelation, MySqlDataAdapter aSdaRechnungen, DataTable aTblRechnungen, MySqlDataAdapter asdaObjektTeile, DataTable aTblObjektTeile, MySqlDataAdapter asdaTimeline, DataTable aTblTimeline, string asConnect)
        {
            throw new NotImplementedException();
        }

        // Einfache Timeline erzeugen
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
            int LiArtRelation = 0;      // 1= Rechnung, 2=Zahlung, 3=Zähler

            decimal ldBetragNetto = 0;
            decimal ldBetragSollNetto = 0;
            decimal ldBetragBrutto = 0;
            decimal ldBetragSollBrutto = 0;
            decimal ldGesamtflaeche = 0;
            decimal ldZs = 0;            // Zählerstand
            decimal ldVerbrauch = 0;    // Zähler Verbrauch
            decimal[] ladBetraege = new decimal[12];

            int zl = 0;
            int LiMwstId = 0;
            int liZlgOrRg = 0;
            int LiSourceId = 0;
            int liRechnungId = 0;
            int liZahlungId = 0;
            int liZaehlerstandId = 0;
            int liOk = 0;
            int liAnzPersonenObj = 0;
            int liAnzPersonenObt = 0;
            int liFlTml = 0;            // Flag TimeLine in Zahlungen
            int liImportId = 0;         // Import Id
            int liVerteilungId = 0;     // Id Kostenverteilung
            int liRgId = 0;             // Rechnungs ID
            int liZsId = 0;             // Zähler Id

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
                if (ATblRechnungen.Rows[0].ItemArray.GetValue(3) != DBNull.Value)
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
                ladBetraege = Timeline.GetBetraege(liMonths, liDaysStart, liDaysEnd,
                ldBetragNetto, ldBetragBrutto,
                        ldBetragSollNetto, ldBetragSollBrutto, liZlgOrRg, LdtStart, LdtEnd);

                // Den ersten Monat ermitteln
                string dt = (LdtStart.Year.ToString()) + "-" + LdtStart.Month.ToString() + "-01";
                ldtMonat = DateTime.Parse(dt);                 // Datetime mit erstem Tag des Monats

                // Ermitteln der VerteilungsId aus Tabelle Rechnungen
                liVerteilungId = Timeline.GetVerteilungsId(AsConnect, LiSourceId);
                // Ermitteln, wie verteilt werden soll aus der Tabelle art_verteilung
                LsVerteilung = Timeline.GetVerteilung(AsConnect, liVerteilungId);
                // Gesamtfläche aus Tabelle Objekt holen
                ldGesamtflaeche = Timeline.GetObjektflaeche(liObjekt, liObjektTeil, liMieter, AsConnect);

                // Timeline für das Objekt oder Objektteil erzeugen
                for (int ii = 1; ii <= liMonths; ii++)
                {
                    DataRow DrTimeline = ATblTimeline.NewRow();

                    DrTimeline[1] = LiSourceId;
                    DrTimeline[4] = liObjekt;
                    DrTimeline[5] = liObjektTeil;              // (int)ATblTeilobjekte.Rows[i].ItemArray.GetValue(0); // Objekt Id
                    DrTimeline[6] = liMieter;
                    DrTimeline[7] = LiKsa;
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
                        DrTimeline[8] = ladBetraege[1];
                        DrTimeline[10] = ladBetraege[2];
                    }
                    //---------------------------------------------
                    DrTimeline[9] = ladBetraege[3];
                    DrTimeline[11] = ladBetraege[4];
                    DrTimeline[12] = ldZs;                  // Zählerstand

                    if (ii == 1)                            // erster Monat
                        DrTimeline[13] = LdtStart;
                    else if (ii == liMonths)                // letzter Monat Letzten Tag eintragen
                        DrTimeline[13] = LdtEnd;
                    else
                        DrTimeline[13] = ldtMonat;          // Der Timelinemonat

                    DrTimeline[14] = 0;
                    DrTimeline[15] = 0;
                    ATblTimeline.Rows.Add(DrTimeline);
                    // + Monat 
                    ldtMonat = ldtMonat.AddMonths(1);

                }
                // Timeline in die Datenbank schreiben
                Timeline.FetchData("", "", "", 32, AsConnect);

                LiOk = 1;
                // Todo Die Rechnungsverschiebung testen
            }
            else
            {
                MessageBox.Show("Verarbeitungsfehler ERROR fetchdata fetchdata RdFunctions\n CreateRechnungen = ",
                            "Achtung");
                LiOk = 0;
            }
            return LiOk;
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
            int LiArtRelation = 0;      // 1= Rechnung, 2=Zahlung, 3=Zähler

            decimal ldBetragNetto = 0;
            decimal ldBetragSollNetto = 0;
            decimal ldBetragBrutto = 0;
            decimal ldBetragSollBrutto = 0;
            decimal ldGesamtflaeche = 0;
            decimal ldZs = 0;            // Zählerstand
            decimal ldVerbrauch = 0;     // Zähler Verbrauch
            decimal[] ladBetraege = new decimal[12];

            int zl = 0;
            int LiMwstId = 0;
            int liZlgOrRg = 0;
            int LiSourceId = 0;
            int liRechnungId = 0;
            int liZahlungId = 0;
            int liZaehlerstandId = 0;
            int liOk = 0;
            int liAnzPersonenObj = 0;
            int liAnzPersonenObjTeil = 0;
            int liFlTml = 0;            // Flag TimeLine in Zahlungen
            int liImportId = 0;         // Import Id
            int liVerteilungId = 0;     // Id Kostenverteilung
            int liRgId = 0;             // Rechnungs ID
            int liZsId = 0;             // Zähler Id

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
                if (ATblRechnungen.Rows[0].ItemArray.GetValue(3) != DBNull.Value)
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
                ladBetraege = Timeline.GetBetraege(liMonths, liDaysStart, liDaysEnd,
                ldBetragNetto, ldBetragBrutto,
                        ldBetragSollNetto, ldBetragSollBrutto, liZlgOrRg, LdtStart, LdtEnd);

                // Den ersten Monat ermitteln
                string dt = (LdtStart.Year.ToString()) + "-" + LdtStart.Month.ToString() + "-01";
                ldtMonat = DateTime.Parse(dt);                 // Datetime mit erstem Tag des Monats

                // Ermitteln der VerteilungsId aus der übergordneten Rechnungen
                liVerteilungId = Timeline.GetVerteilungsId(AsConnect, LiSourceId);
                // Ermitteln, wie verteilt werden soll aus der Tabelle art_verteilung
                LsVerteilung = Timeline.GetVerteilung(AsConnect, liVerteilungId);
                // Gesamtfläche aus Tabelle Objekt holen
                ldGesamtflaeche = Timeline.GetObjektflaeche(liObjekt, 0, 0, AsConnect);


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
                    // DrRechnung[8] = AiObjektId;      // Darf hier nicht eingesetzt werden
                    DrRechnung[9] = (int)ATblTeilobjekte.Rows[i].ItemArray.GetValue(0);
                    DrRechnung[10] = AiMieterid;
                    DrRechnung[11] = LsRgNr;
                    DrRechnung[12] = LsFirma;
                    DrRechnung[15] = 1;                 // Flag für Timelinebearbeitung erzeugen
                    DrRechnung[16] = liVerteilungId;
                    DrRechnung[17] = AiSourceId;

                    if (ATblTeilobjekte.Rows[i].ItemArray.GetValue(6) != DBNull.Value)
                    {
                        if ((decimal)ATblTeilobjekte.Rows[i].ItemArray.GetValue(6) > 0) // Fläche Teilobjekt
                        {
                            if (liObjekt > 0)
                            {
                                DrRechnung[5] = ldBetragNetto / (ldGesamtflaeche / (decimal)ATblTeilobjekte.Rows[i].ItemArray.GetValue(6));           // Netto    
                                DrRechnung[6] = ldBetragBrutto / (ldGesamtflaeche / (decimal)ATblTeilobjekte.Rows[i].ItemArray.GetValue(6));          // Brutto
                            }
                        }
                        ATblRechnungen.Rows.Add(DrRechnung);
                    }
                }
                // Daten in die Datenbank schreiben
                LiOk = Timeline.FetchData("", "", "", 33, AsConnect);
            }
            return LiOk;
        }

    }
}
