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
        static DataTable TblTimelineNew;
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
        static SqlDataAdapter sda;
        static SqlDataAdapter sdb;
        static SqlDataAdapter sdc;
        static SqlDataAdapter sde;
        static SqlDataAdapter sdf;
        static SqlDataAdapter sdg;
        static SqlDataAdapter sdh;
        static SqlDataAdapter sdZlg;
        static SqlDataAdapter sdZlgNew;
        static SqlDataAdapter sdTml;
        static SqlDataAdapter sdRgId;
        static SqlDataAdapter sdConSumObj;
        static SqlDataAdapter sdConSumObjT;
        static SqlDataAdapter sdCnt;
        static SqlDataAdapter sdCntNew;
        static SqlDataAdapter sdZlInfo;
        //static SqlDataAdapter sdObjTeil;
        static SqlDataAdapter sdParts;
        static SqlDataAdapter adp;

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
        static MySqlDataAdapter mysdTml;
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

        // Datenbankaktionen nach Fetchdata
        public static int MakeAfterFetch(int aiArt, int aiTeil, int ai1, int ai2, string asConnect, 
            MySqlDataAdapter ASdaRechnungen, DataTable ATblRechnungen,
            MySqlDataAdapter AsdaObjektTeile, DataTable ATblObjektTeile
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

            switch (aiArt)
            {
                case 1:
                    // Externe ID aus der Rechnung ermitteln 
                    if (ATblRechnungen.Rows[0].ItemArray.GetValue(14) != DBNull.Value)
                    {
                        // Die Original Id der Rechnung
                        LiSourceId = int.Parse(ATblRechnungen.Rows[0].ItemArray.GetValue(0).ToString());

                        // Hier die Liste der Teilobjekte dazuholen


                        // Erzeugte Unterrechnunen erstmal löschen Datenbank übergeben
                        // Alle mit der Id der Hauptrechnung
                        liOk = Timeline.DeleteRechnungen(LiSourceId, "R", asConnect);

                        // Objekt > Rechnungen für Objektteile
                        if (ATblRechnungen.Rows[0].ItemArray.GetValue(8) != DBNull.Value)
                            if ((int)ATblRechnungen.Rows[0].ItemArray.GetValue(8) > 0)
                            {
                                liObjekt = (int)ATblRechnungen.Rows[0].ItemArray.GetValue(8);
                                LiKsa    = (int)ATblRechnungen.Rows[0].ItemArray.GetValue(1);

                                // Weiterleitung an ObjektTeil aus der Kostenart ermitteln
                                // 1 = Weiterleitung an Teilobjekt
                                if (Timeline.GetWeiterleitung(1, LiKsa, asConnect) == 1)
                                {
                                    liObjektTeil = 0;           // Weterleitung kommt vom Objekt
                                    liArtRelation = 1;          // Rechnung

                                    // Rechnungen für alle zugehörigen Objektteile erzeugen
                                    liOk = Timeline.CreateRechnungen(LiSourceId, liObjekt, liObjektTeil, liMieter, liArtRelation,
                                        ASdaRechnungen, ATblRechnungen,
                                        AsdaObjektTeile, ATblObjektTeile,
                                        asConnect
                                        );
                                }
                            }

                        // Todo : Das hier nichr mehr machen

                        // ObjektTeil
                        //if (TblRechnungen.Rows[0].ItemArray.GetValue(9) != DBNull.Value)
                        //    if ((int)TblRechnungen.Rows[0].ItemArray.GetValue(9) > 0)
                        //    {
                        //        liObjektTeil = (int)TblRechnungen.Rows[0].ItemArray.GetValue(9);
                        //        // Timeline neu erzeugen Objektteile aus Rechnungen
                        //        // Todo erstmal keine Timeline
                        //        liOk = Timeline.TimelineCreate(LiSourceId, "id_rechnung", asConnect, aiDb);
                        //        // Weiterleitung an ObjektTeil aus der Kostenart ermitteln
                        //        // 2 = Weiterleitung an Mieter
                        //        if (Timeline.GetWeiterleitung(2, LiSourceId, asConnect) == 1)
                        //        {
                        //            liArtRelation = 1;
                        //            // Todo erstmal keine Timeline
                        //            // Timeline neu erzeugen für Relationen
                        //            // liOk = TimelineCreateRelations(liExternId, liObjekt, liObjektTeil, liMieter, liArtRelation, asConnect, aiDb);
                        //        }
                        //    }

                        // Mieter
                        //if (TblRechnungen.Rows[0].ItemArray.GetValue(10) != DBNull.Value)
                        //    if ((int)TblRechnungen.Rows[0].ItemArray.GetValue(10) > 0)
                        //    {
                        //        liMieter = (int)TblRechnungen.Rows[0].ItemArray.GetValue(10);
                        //        // Timeline neu erzeugen Mieter aus Rechnungen
                        //        // Todo erstmal keine Timeline
                        //        // liOk = TimelineCreate(liExternId, "id_rechnung", asConnect, aiDb);
                        //    }
                    }
                    else
                    {
                        MessageBox.Show("Verarbeitungsfehler ERROR fetchdata fetchdata RdFunctions 0001\n piArt = " + aiArt.ToString(),
                                    "Achtung");
                        break;
                    }

                    break;
                case 3:
                    switch (aiTeil)
                    {
                        case 1:
                            for (int i = 0; TblRechnungen.Rows.Count > i; i++)
                            {
                                if (TblRechnungen.Rows[i].ItemArray.GetValue(14) != DBNull.Value)
                                {
                                    LiSourceId = (int)TblRechnungen.Rows[i].ItemArray.GetValue(14);
                                    LiReturn = LiSourceId;
                                }
                                else
                                {
                                    MessageBox.Show("Verarbeitungsfehler ERROR fetchdata fetchdata RdFunctions 0002\n piArt = " + aiArt.ToString(),
                                                "Achtung");
                                    break;
                                }
                            }
                            break;
                        case 2:
                            //for (int i = 0; TblRechnungen.Rows.Count > i; i++)
                            //{
                            //    if (TblRechnungen.Rows[i].ItemArray.GetValue(14) != DBNull.Value)
                            //    {
                            //        LiSourceId = (int)TblRechnungen.Rows[i].ItemArray.GetValue(14);
                            //        if (TblRechnungen.Rows[i].ItemArray.GetValue(8) != DBNull.Value)
                            //            liObjekt = (int)TblRechnungen.Rows[i].ItemArray.GetValue(8);
                            //        if (TblRechnungen.Rows[i].ItemArray.GetValue(9) != DBNull.Value)
                            //            liObjektTeil = (int)TblRechnungen.Rows[i].ItemArray.GetValue(9);
                            //        if (TblRechnungen.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                            //            liMieter = (int)TblRechnungen.Rows[i].ItemArray.GetValue(10);
                            //        if (TblRechnungen.Rows[i].ItemArray.GetValue(5) != DBNull.Value)
                            //            ldBetragNetto = (decimal)TblRechnungen.Rows[i].ItemArray.GetValue(5);
                            //        if (TblRechnungen.Rows[i].ItemArray.GetValue(6) != DBNull.Value)
                            //            ldBetragBrutto = (decimal)TblRechnungen.Rows[i].ItemArray.GetValue(6);
                            //        if (TblRechnungen.Rows[i].ItemArray.GetValue(3) != DBNull.Value)
                            //            ldtStart = (DateTime)TblRechnungen.Rows[i].ItemArray.GetValue(3);
                            //        if (TblRechnungen.Rows[i].ItemArray.GetValue(4) != DBNull.Value)
                            //            ldtEnd = (DateTime)TblRechnungen.Rows[i].ItemArray.GetValue(4);
                            //        if (TblRechnungen.Rows[i].ItemArray.GetValue(1) != DBNull.Value)
                            //            liKsa = (int)TblRechnungen.Rows[i].ItemArray.GetValue(1);

                            //        zl = 1;         // Anzahl der Monate = Anzahl der Datensätze in Timeline

                            //        // Anzahl der Tage des ersten Monats        99 ist der volle Monat
                            //        liDaysStart = Timeline.GetDaysStart(ldtStart);
                            //        // Anzahl der Tage des letzten Monats       99 ist der volle Monat
                            //        liDaysEnd = Timeline.GetDaysEnd(ldtEnd);
                            //        // Anzahl der einzutragenden Monat ermitteln
                            //        liMonths = Timeline.GetMonths(ldtStart, ldtEnd);
                            //        // Zahlung oder Rechnung 1= Zahlung 2= Rechnung
                            //        liZlgOrRg = 2;
                            //        // Monatsbeträge ermitteln (Brutto und Netto) und evtl. erster und letzter Monat nicht voll
                            //        ladBetraege = Timeline.GetBetraege(liMonths, liDaysStart, liDaysEnd,
                            //                ldBetragNetto, ldBetragBrutto,
                            //                ldBetragSollNetto, ldBetragSollBrutto, liZlgOrRg, ldtStart, ldtEnd);
                            //        // Den ersten Monat ermitteln
                            //        string dt = (ldtStart.Year.ToString()) + "-" + ldtStart.Month.ToString() + "-01";
                            //        ldtMonat = DateTime.Parse(dt);                 // Datetime mit erstem Tag des Monats

                            //        do
                            //        {
                            //            DataRow dr = TblTimelineNew.NewRow();

                            //            dr[1] = LiSourceId;
                            //            dr[4] = liObjekt;
                            //            dr[5] = liObjektTeil;
                            //            dr[6] = liMieter;
                            //            dr[7] = liKsa;
                            //            //---------------------------------------------
                            //            if (liDaysStart != 99 && zl == 1)
                            //            {
                            //                dr[8] = ladBetraege[5];         // Netto erster Monat bei späterem Beginn
                            //                dr[10] = ladBetraege[6];         // Brutto
                            //            }
                            //            //---------------------------------------------
                            //            else if (liDaysEnd != 99 && zl == liMonths)
                            //            {
                            //                dr[8] = ladBetraege[9];         // Netto letzter Monat bei früherem Ende
                            //                dr[10] = ladBetraege[10];         // Brutto
                            //            }
                            //            else
                            //            {
                            //                dr[8] = ladBetraege[1];
                            //                dr[10] = ladBetraege[2];
                            //            }
                            //            //---------------------------------------------
                            //            dr[9] = ladBetraege[3];
                            //            dr[11] = ladBetraege[4];
                            //            dr[12] = ldZs;                  // Zählerstand

                            //            if (zl == 1)                    // erster Monat
                            //                dr[13] = ldtStart;
                            //            else if (zl == liMonths)        // letzter Monat
                            //                dr[13] = ldtEnd;
                            //            else
                            //                dr[13] = ldtMonat;      // Der Timelinemonat
                            //            dr[14] = 0;
                            //            dr[15] = 0;
                            //            TblTimelineNew.Rows.Add(dr);
                            //            // + Monat 
                            //            ldtMonat = ldtMonat.AddMonths(1);
                            //            // + Zähler
                            //            zl++;

                            //        } while (zl <= liMonths);

                            //        // und alles ab in die Datenbank
                            //        Timeline.MakeCommand(aiDb, 1);
                            //    }
                            //    else
                            //    {
                            //        MessageBox.Show("Verarbeitungsfehler ERROR fetchdata fetchdata RdFunctions 0003\n piArt = " + aiArt.ToString(),
                            //                    "Achtung");
                            //        break;
                            //    }
                            //}
                            break;
                        default:
                            break;
                    }
                    break;
                case 4:
                    // Schleife durch Timeline
                    // Jeder Datensatz muss hier auch für jeden Objektteil einen Datensatz erzeugen
                    // Die Beträge werden nach der Flächenaufteilung eingetragen
                    // Aufteilung nach Personen kann hier nicht gemacht werden. 
                    // Geschieht erst beim Verteilen auf die Mieter

                    TblTimeLineSet.Rows.Clear();     // Timeline leeren

                    // Ein Popup öffnen
                    WndTimelineCalc frmTml = new WndTimelineCalc();
                    frmTml.Show();

                    // Timeline
                    for (int i = 0; TblTimelineGet.Rows.Count > i; i++)
                    {
                        if (TblTimelineGet.Rows[i].ItemArray.GetValue(1) != DBNull.Value)
                        {
                            liRechnungId = (int)TblTimelineGet.Rows[i].ItemArray.GetValue(1);
                            if (TblTimelineGet.Rows[i].ItemArray.GetValue(4) != DBNull.Value)
                                liObjekt = (int)TblTimelineGet.Rows[i].ItemArray.GetValue(4);
                            if (TblTimelineGet.Rows[i].ItemArray.GetValue(5) != DBNull.Value)
                                liObjektTeil = (int)TblTimelineGet.Rows[i].ItemArray.GetValue(5);
                            if (TblTimelineGet.Rows[i].ItemArray.GetValue(6) != DBNull.Value)
                                liMieter = (int)TblTimelineGet.Rows[i].ItemArray.GetValue(6);
                            if (TblTimelineGet.Rows[i].ItemArray.GetValue(7) != DBNull.Value)
                                LiKsa = (int)TblTimelineGet.Rows[i].ItemArray.GetValue(7);
                            if (TblTimelineGet.Rows[i].ItemArray.GetValue(8) != DBNull.Value)
                                ldBetragNetto = (decimal)TblTimelineGet.Rows[i].ItemArray.GetValue(8);
                            if (TblTimelineGet.Rows[i].ItemArray.GetValue(9) != DBNull.Value)
                                ldBetragSollNetto = (decimal)TblTimelineGet.Rows[i].ItemArray.GetValue(9);
                            if (TblTimelineGet.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                                ldBetragBrutto = (decimal)TblTimelineGet.Rows[i].ItemArray.GetValue(10);
                            if (TblTimelineGet.Rows[i].ItemArray.GetValue(11) != DBNull.Value)
                                ldBetragSollBrutto = (decimal)TblTimelineGet.Rows[i].ItemArray.GetValue(11);
                            if (TblTimelineGet.Rows[i].ItemArray.GetValue(12) != DBNull.Value)
                                ldZs = (decimal)TblTimelineGet.Rows[i].ItemArray.GetValue(12);
                            if (TblTimelineGet.Rows[i].ItemArray.GetValue(13) != DBNull.Value)
                                ldtMonat = (DateTime)TblTimelineGet.Rows[i].ItemArray.GetValue(13);
                            if (TblTimelineGet.Rows[i].ItemArray.GetValue(17) != DBNull.Value)
                                liImportId = (int)TblTimelineGet.Rows[i].ItemArray.GetValue(17);

                            // Ermitteln der VerteilungsId aus Tabelle Rechnungen
                            // Achtung nbüschen gepfuscht liRechnungId ist die externTimeline Id
                            liVerteilungId = Timeline.GetVerteilungsId(asConnect, liRechnungId);

                            // Ermitteln, wie verteilt werden soll aus der Tabelle art_verteilung
                            lsVerteilung = Timeline.GetVerteilung(asConnect, liVerteilungId);

                            // Alle Objektteile zu dem Objekt
                            for (int ii = 0; TblObjektParts.Rows.Count > ii; ii++)
                            {
                                // Timeline schreiben
                                DataRow dr = TblTimeLineSet.NewRow();

                                dr[1] = liRechnungId;
                                // dr[4] = liObjekt; nicht eintragen
                                if (TblObjektParts.Rows[ii].ItemArray.GetValue(0) != DBNull.Value)
                                {
                                    dr[5] = (int)TblObjektParts.Rows[ii].ItemArray.GetValue(0);   // id ObjektTeil
                                    liObjektTeil = (int)TblObjektParts.Rows[ii].ItemArray.GetValue(0);
                                    dr[6] = liMieter;
                                    dr[7] = LiKsa;

                                    switch (lsVerteilung)
                                    {
                                        case "fl":
                                            if (TblObjektParts.Rows[ii].ItemArray.GetValue(6) != DBNull.Value)
                                            {
                                                if ((decimal)TblObjektParts.Rows[ii].ItemArray.GetValue(6) > 0)
                                                {
                                                    // Gesamtfläche aus Tabelle Objekt holen
                                                    if (liObjekt > 0)
                                                    {
                                                        ldGesamtflaeche = Timeline.GetObjektflaeche(liObjekt, 0, 0, asConnect);
                                                        dr[8] = ldBetragNetto / (ldGesamtflaeche / (decimal)TblObjektParts.Rows[ii].ItemArray.GetValue(6));          // Netto    
                                                        dr[10] = ldBetragBrutto / (ldGesamtflaeche / (decimal)TblObjektParts.Rows[ii].ItemArray.GetValue(6));         // Brutto
                                                    }
                                                }
                                                else
                                                {
                                                    liSave = 0;
                                                }
                                            }
                                            break;
                                        // Prozentanteil rechnen
                                        case "pz":
                                            if (TblObjektParts.Rows[ii].ItemArray.GetValue(7) != DBNull.Value)
                                            {
                                                if ((decimal)TblObjektParts.Rows[ii].ItemArray.GetValue(7) > 0)
                                                {
                                                    dr[8] = (ldBetragNetto / 100) * (decimal)TblObjektParts.Rows[ii].ItemArray.GetValue(7);           // Netto    
                                                    dr[10] = (ldBetragBrutto / 100) * (decimal)TblObjektParts.Rows[ii].ItemArray.GetValue(7);         // Brutto                                                		 
                                                }
                                                else
                                                {
                                                    liSave = 0;
                                                }
                                            }
                                            break;
                                        // Personenanzahl für den aktuellen Monat berechnen
                                        case "ps":
                                            // Anzahl der Personen in einem Objekt ermitteln
                                            // Information aus aktiven Verträgen
                                            // liAnzPersonenObj = getAktPersonen(liObjekt, ldtMonat, 0);
                                            // liAnzPersonenObt = getAktPersonen(0, ldtMonat, liObjektTeil);

                                            if (TblObjektParts.Rows[ii].ItemArray.GetValue(8) != DBNull.Value)
                                            {
                                                if ((int)TblObjektParts.Rows[ii].ItemArray.GetValue(8) > 0)
                                                {
                                                    // Anzahl der Personen in einem Objekt ermitteln
                                                    // Aktive Verträge
                                                    liAnzPersonenObj = Convert.ToInt32(Timeline.GetAktPersonen(liObjekt, 0, 0, ldtMonat.ToString(), ldtMonat.ToString(), 0, asConnect));
                                                    // Anzahl der Personen in einem Objektteil ermitteln
                                                    liAnzPersonenObt = Convert.ToInt32(Timeline.GetAktPersonen(0, liObjektTeil, liObjektTeil, ldtMonat.ToString(), ldtMonat.ToString(), liObjektTeil, asConnect));

                                                    if (liAnzPersonenObj > 0 && liAnzPersonenObt > 0)
                                                    {
                                                        dr[8] = (ldBetragNetto / liAnzPersonenObj) * liAnzPersonenObt;          // Netto    
                                                        dr[10] = (ldBetragBrutto / liAnzPersonenObj) * liAnzPersonenObt;        // Brutto                                                		 
                                                    }
                                                    else
                                                    {
                                                        liSave = 0;
                                                    }
                                                }
                                                else
                                                {
                                                    liSave = 0;
                                                }
                                            }
                                            break;
                                        // Direkte Verteilung 1:1 weiterleiten   31.5.2018
                                        case "di":
                                            dr[8] = ldBetragNetto;          // Netto    
                                            dr[10] = ldBetragBrutto;        // Brutto                                                		 
                                            break;
                                        // Nix wird verteilt                    31.5.2018
                                        case "nl":
                                            liSave = 0;
                                            break;
                                        case "zl":
                                            // Zähleranteil ermitteln 
                                            // Zähler werden immer direkt auf die Wohnung bzw den Mieter gebucht  
                                            liSave = 0;
                                            break;
                                        // Verteilung Bedingt mit Anwahl für gewünschte Wohnungen
                                        // Die Gesamtfläche für die Auswahl wird ermittelt
                                        case "fa":
                                            if ((decimal)TblObjektParts.Rows[ii].ItemArray.GetValue(6) > 0)
                                            {
                                                // Gesamtfläche der ausgewählten Wohnungen aus Tabelle Objekt_mix_parts holen
                                                if (liObjekt > 0)
                                                {
                                                    int liArt = 0;
                                                    // Gesamtfläche der Auswahl = 0 oder Gesamtfläche = 1
                                                    liArt = Timeline.GetObjektflaecheAuswFlag(liObjekt, asConnect);
                                                    ldGesamtflaeche = Timeline.GetObjektflaecheAuswahl(liObjekt, liRechnungId, asConnect, liArt);  // RechnungsId ist Timeline ID
                                                    if (Timeline.GetObjektTeilAuswahl((int)TblObjektParts.Rows[ii].ItemArray.GetValue(0), asConnect) > 0)
                                                    {
                                                        // decimal ldtest = ldBetragNetto / (ldGesamtflaeche / (decimal)tableFive.Rows[ii].ItemArray.GetValue(6)); 
                                                        dr[8] = ldBetragNetto / (ldGesamtflaeche / (decimal)TblObjektParts.Rows[ii].ItemArray.GetValue(6));          // Netto    
                                                        dr[10] = ldBetragBrutto / (ldGesamtflaeche / (decimal)TblObjektParts.Rows[ii].ItemArray.GetValue(6));         // Brutto                                                                                                                                                    
                                                    }
                                                    else
                                                    {
                                                        dr[8] = 0;
                                                        dr[10] = 0;
                                                        liSave = 0;     // nur in diesem Fall Datensatz verwerfen
                                                    }
                                                }
                                            }
                                            break;
                                        default:
                                            break;
                                    }

                                    dr[12] = ldZs;                  // Zählerstand
                                    dr[13] = ldtMonat;              // Der Timelinemonat

                                    // Kennzeichnen der Timeline, ob es eine Weiterleitung vom Objekt ist
                                    if (liObjekt > 0)
                                    {
                                        dr[14] = 1;
                                    }
                                    else
                                    {
                                        dr[14] = 0;
                                    }
                                    // Kennzeichnen der Timeline, ob es eine Weiterleitung vom ObjektTeil ist
                                    if (liObjektTeil > 0)
                                    {
                                        dr[15] = 1;
                                    }
                                    else
                                    {
                                        dr[15] = 0;
                                    }
                                    // Import ID schreiben
                                    dr[17] = liImportId;
                                }
                                if (liSave == 1)
                                {
                                    TblTimeLineSet.Rows.Add(dr);
                                }
                                liSave = 1;
                                // und alle TimelineEinträge ab in die Datenbank
                                Timeline.MakeCommand(2);
                                TblTimeLineSet.Rows.Clear();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Verarbeitungsfehler ERROR fetchdata fetchdata RdFunctions 0004\n piArt = " + aiArt.ToString(),
                                        "Achtung");
                            break;
                        }
                    }

                    frmTml.Close();

                    //Application.Current.Dispatcher.Invoke(() =>
                    //{
                    //});

                    break;
                case 5:         // Mieter schreiben
                    // Schleife durch Timeline
                    // Jeder Datensatz aus Timeline Objektteile muss hier einen Datensatz für den Mieter erzeugen
                    TblTimelineNew.Rows.Clear();    // TimeLine leeren

                    for (int i = 0; TblTimeLineGet.Rows.Count > i; i++)
                    {
                        liSave = 1;
                        if (TblTimeLineGet.Rows[i].ItemArray.GetValue(1) != DBNull.Value || TblTimeLineGet.Rows[i].ItemArray.GetValue(2) != DBNull.Value || TblTimeLineGet.Rows[i].ItemArray.GetValue(3) != DBNull.Value)
                        {
                            // Rechnung
                            if (TblTimeLineGet.Rows[i].ItemArray.GetValue(1) != DBNull.Value)
                            {
                                liRechnungId = (int)TblTimeLineGet.Rows[i].ItemArray.GetValue(1);
                            }
                            // Zahlung
                            if (TblTimeLineGet.Rows[i].ItemArray.GetValue(2) != DBNull.Value)
                            {
                                liZahlungId = (int)TblTimeLineGet.Rows[i].ItemArray.GetValue(2);
                            }
                            // Zählerstand
                            if (TblTimeLineGet.Rows[i].ItemArray.GetValue(3) != DBNull.Value)
                            {
                                liZaehlerstandId = (int)TblTimeLineGet.Rows[i].ItemArray.GetValue(3);
                            }

                            if (TblTimeLineGet.Rows[i].ItemArray.GetValue(4) != DBNull.Value)
                                liObjekt = (int)TblTimeLineGet.Rows[i].ItemArray.GetValue(4);
                            if (TblTimeLineGet.Rows[i].ItemArray.GetValue(5) != DBNull.Value)
                                liObjektTeil = (int)TblTimeLineGet.Rows[i].ItemArray.GetValue(5);
                            if (TblTimeLineGet.Rows[i].ItemArray.GetValue(7) != DBNull.Value)
                                LiKsa = (int)TblTimeLineGet.Rows[i].ItemArray.GetValue(7);
                            if (TblTimeLineGet.Rows[i].ItemArray.GetValue(8) != DBNull.Value)
                                ldBetragNetto = (decimal)TblTimeLineGet.Rows[i].ItemArray.GetValue(8);
                            if (TblTimeLineGet.Rows[i].ItemArray.GetValue(9) != DBNull.Value)
                                ldBetragSollNetto = (decimal)TblTimeLineGet.Rows[i].ItemArray.GetValue(9);
                            if (TblTimeLineGet.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                                ldBetragBrutto = (decimal)TblTimeLineGet.Rows[i].ItemArray.GetValue(10);
                            if (TblTimeLineGet.Rows[i].ItemArray.GetValue(11) != DBNull.Value)
                                ldBetragSollBrutto = (decimal)TblTimeLineGet.Rows[i].ItemArray.GetValue(11);
                            if (TblTimeLineGet.Rows[i].ItemArray.GetValue(12) != DBNull.Value)
                                ldZs = (decimal)TblTimeLineGet.Rows[i].ItemArray.GetValue(12);
                            if (TblTimeLineGet.Rows[i].ItemArray.GetValue(13) != DBNull.Value)
                                ldtMonat = (DateTime)TblTimeLineGet.Rows[i].ItemArray.GetValue(13);
                            if (TblTimeLineGet.Rows[i].ItemArray.GetValue(17) != DBNull.Value)
                                liImportId = (int)TblTimeLineGet.Rows[i].ItemArray.GetValue(17);

                            DataRow dr = TblTimelineNew.NewRow();
                            dr[1] = liRechnungId;
                            dr[2] = liZahlungId;
                            dr[3] = liZaehlerstandId;
                            // dr[4] = liObjekt; nicht eintragen
                            // dr[5] = liObjektTeil; nicht eintragen

                            // Aktuellen Mieter ermitteln
                            liMieter = Timeline.GetAktMieter(liObjektTeil, ldtMonat, asConnect, aiDb);

                            // Mieter gefunden
                            if (liMieter > 0)
                            {
                                ldtVertrag = DateTime.MinValue;
                                liDaysStart = 0;
                                liDaysEnd = 0;

                                // Hier nur, wenn ein Monat auch noch geteilt werden soll
                                // Beginnt der Vertrag in diesem Monat?
                                // ldtVertrag = getVertragInfo(1, ldtMonat, liMieter, asConnect, aiDb);

                                // Todo Tage anteilig berechnen
                                //// Tageszahl von Monatsbeginn an ermitteln
                                //if (ldtVertrag > DateTime.MinValue)
                                //{
                                //    liDaysStart = ldtVertrag.Day;
                                //    liDaysInMonth = System.DateTime.DaysInMonth(ldtVertrag.Year, ldtVertrag.Month);
                                //    liDaysInMonth = liDaysInMonth - liDaysStart;
                                //    ldBetragNetto = (ldBetragNetto / liDaysInMonth) * liDaysInMonth;
                                //    ldBetragBrutto = (ldBetragBrutto / liDaysInMonth) * liDaysInMonth;
                                //}

                                //// Endet der Vetrag in diesem Monat?
                                //ldtVertrag = getVertragInfo(2, ldtMonat, liMieter, asConnect);

                                //// Tageszahl zum Monatsende ermitteln
                                //if (ldtVertrag > DateTime.MinValue)
                                //{
                                //    liDaysStart = ldtVertrag.Day;
                                //    liDaysInMonth = System.DateTime.DaysInMonth(ldtVertrag.Year, ldtVertrag.Month);
                                //    ldBetragNetto = (ldBetragNetto / liDaysInMonth) * liDaysStart;
                                //    ldBetragBrutto = (ldBetragBrutto / liDaysInMonth) * liDaysStart;
                                //}

                                dr[6] = liMieter;

                            }
                            else // sonst auf Leerstand buchen
                            {
                                dr[4] = liObjekt;
                                dr[5] = liObjektTeil;
                                // Mieter für Leerstand ermiteln und eintragen
                                // ObjektTeil ist vorhanden
                                liMieter = Timeline.GetMieterLeerstand(liObjektTeil, asConnect, aiDb);
                                if (liMieter > 0)
                                {
                                    dr[6] = liMieter;       // Mieter Leerstand existiert und wird genutzt
                                }
                                dr[16] = liObjektTeil;         // Auf Leerstand wird die TeilObjekt ID geschrieben Feld 16
                            }
                            dr[7] = LiKsa;

                            if (ldBetragNetto > 0 || ldBetragBrutto > 0)
                            {
                                dr[8] = ldBetragNetto;          // Netto                                        
                                dr[10] = ldBetragBrutto;        // Brutto                                                                                    
                            }
                            else
                            {
                                liSave = 0;
                            }

                            dr[12] = ldZs;                  // Zählerstand
                            dr[13] = ldtMonat;              // Der TimelineMonat

                            // Kennzeichnen der Timeline, ob es eine Weiterleitung vom Objekt ist
                            if (liObjekt > 0)
                            {
                                dr[14] = 1;
                            }
                            else
                            {
                                dr[14] = 0;
                            }
                            // Kennzeichnen der Timeline, ob es eine Weiterleitung vom ObjektTeil ist
                            if (liObjektTeil > 0)
                            {
                                dr[15] = 1;
                            }
                            else
                            {
                                dr[15] = 0;
                            }
                            // Import ID schreiben
                            dr[17] = liImportId;

                            if (liSave == 1)
                            {
                                TblTimelineNew.Rows.Add(dr);            // Timeline                                     
                            }
                            liSave = 1;
                            // und alle TimelineEinträge ab in die Datenbank
                            Timeline.MakeCommand(3);
                            TblTimelineNew.Rows.Clear();
                        }
                        else
                        {
                            MessageBox.Show("Verarbeitungsfehler ERROR fetchdata fetchdata RdFunctions 0005\n piArt = " + aiArt.ToString(),
                                        "Achtung");
                            break;
                        }
                    }

                    break;
                case 8:
                    // MwstSatz holen
                    if (TblTaxGet.Rows.Count > 0)
                    {
                        if (TblTaxGet.Rows[0].ItemArray.GetValue(2) != DBNull.Value)
                        {
                            // Hier wird liRows ausnahmsweise mit dem Mwst-Satz belegt
                            decimal ldMwst = (decimal)TblTaxGet.Rows[0].ItemArray.GetValue(2);
                            LiReturn = (int)ldMwst;
                        }
                    }
                    break;
                case 11:
                    // Externe ID aus der Zahlung ermitteln 
                    for (int i = 0; TblZlg.Rows.Count > i; i++)
                    {
                        if (TblZlg.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                        {
                            LiSourceId = (int)TblZlg.Rows[i].ItemArray.GetValue(10);
                            // Timeline löschen
                            // liOk = Timeline.TimelineDelete(LiSourceId, "A", asConnect, aiDb);

                            // Objekt
                            if (TblZlg.Rows[i].ItemArray.GetValue(2) != DBNull.Value)
                                if ((int)TblZlg.Rows[i].ItemArray.GetValue(2) > 0)
                                {
                                    liObjekt = (int)TblZlg.Rows[i].ItemArray.GetValue(2);
                                    // Timeline neu erzeugen Objekte aus Rechnungen
                                    liOk = Timeline.TimelineCreate(LiSourceId, "id_vorauszahlung", asConnect, aiDb);
                                }
                            // ObjektTeil
                            if (TblZlg.Rows[i].ItemArray.GetValue(3) != DBNull.Value)
                                if ((int)TblZlg.Rows[i].ItemArray.GetValue(3) > 0)
                                {
                                    liObjektTeil = (int)TblZlg.Rows[i].ItemArray.GetValue(3);
                                    ldtMonat = Convert.ToDateTime(TblZlg.Rows[i].ItemArray.GetValue(4));
                                    // Timeline neu erzeugen Objektteile aus Rechnungen
                                    liOk = Timeline.TimelineCreate(LiSourceId, "id_vorauszahlung", asConnect, aiDb);

                                    // Weiterleitung an aktiven Mieter
                                    liMieter = 0;
                                    liMieter = Timeline.GetAktMieter(liObjektTeil, ldtMonat, asConnect, aiDb);

                                    if (liMieter > 0)
                                    {
                                        liArtRelation = 2;
                                        // Timeline neu erzeugen für Relationen
                                        liOk = Timeline.TimelineCreateRelations(LiSourceId, liObjekt, liObjektTeil, liMieter, liArtRelation, asConnect, aiDb);
                                    }
                                }

                            // Mieter
                            if (TblZlg.Rows[i].ItemArray.GetValue(1) != DBNull.Value)
                                if ((int)TblZlg.Rows[i].ItemArray.GetValue(1) > 0)
                                {
                                    liMieter = (int)TblZlg.Rows[i].ItemArray.GetValue(1);
                                    // Timeline neu erzeugen Mieter aus Zahlungen
                                    // TODO ACHTUNG hier Kontrolle einbauen, ob Mietvertrag gültig ist
                                    liOk = Timeline.TimelineCreate(LiSourceId, "id_vorauszahlung", asConnect, aiDb);
                                }
                        }
                        else
                        {
                            MessageBox.Show("Verarbeitungsfehler ERROR fetchdata RdFunctions fetchdata\n piArt = " + aiArt.ToString(),
                                        "Achtung");
                            break;
                        }
                    }
                    break;
                case 13:
                    switch (aiTeil)
                    {
                        case 1:
                            // Externe ID aus der Zahlung ermitteln 
                            for (int i = 0; TblZlgNew.Rows.Count > i; i++)
                            {
                                if (TblZlgNew.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                                {
                                    LiSourceId = (int)TblZlgNew.Rows[i].ItemArray.GetValue(10);
                                }
                                else
                                {
                                    MessageBox.Show("Verarbeitungsfehler ERROR fetchdata RdFunctions 0002\n piArt = " + aiArt.ToString(),
                                                "Achtung");
                                    break;
                                }
                            }
                            break;
                        case 2:
                            // Timeline Datensätze erzeugen
                            for (int i = 0; TblZlgNew.Rows.Count > i; i++)
                            {
                                if (TblZlgNew.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                                {
                                    LiSourceId = (int)TblZlgNew.Rows[i].ItemArray.GetValue(10);
                                    if (TblZlgNew.Rows[i].ItemArray.GetValue(1) != DBNull.Value)
                                        liMieter = (int)TblZlgNew.Rows[i].ItemArray.GetValue(1);
                                    if (TblZlgNew.Rows[i].ItemArray.GetValue(2) != DBNull.Value)
                                        liObjekt = (int)TblZlgNew.Rows[i].ItemArray.GetValue(2);
                                    if (TblZlgNew.Rows[i].ItemArray.GetValue(3) != DBNull.Value)
                                        liObjektTeil = (int)TblZlgNew.Rows[i].ItemArray.GetValue(3);
                                    if (TblZlgNew.Rows[i].ItemArray.GetValue(4) != DBNull.Value)
                                        ldtStart = (DateTime)TblZlgNew.Rows[i].ItemArray.GetValue(4);
                                    if (TblZlgNew.Rows[i].ItemArray.GetValue(5) != DBNull.Value)
                                        ldtEnd = (DateTime)TblZlgNew.Rows[i].ItemArray.GetValue(5);
                                    if (TblZlgNew.Rows[i].ItemArray.GetValue(6) != DBNull.Value)
                                        ldBetragNetto = (decimal)TblZlgNew.Rows[i].ItemArray.GetValue(6);
                                    if (TblZlgNew.Rows[i].ItemArray.GetValue(7) != DBNull.Value)
                                        ldBetragBrutto = (decimal)TblZlgNew.Rows[i].ItemArray.GetValue(7);
                                    if (TblZlgNew.Rows[i].ItemArray.GetValue(8) != DBNull.Value)
                                        ldBetragSollNetto = (decimal)TblZlgNew.Rows[i].ItemArray.GetValue(8);
                                    if (TblZlgNew.Rows[i].ItemArray.GetValue(9) != DBNull.Value)
                                        ldBetragSollBrutto = (decimal)TblZlgNew.Rows[i].ItemArray.GetValue(9);
                                    if (TblZlgNew.Rows[i].ItemArray.GetValue(11) != DBNull.Value)
                                        liFlTml = (int)TblZlgNew.Rows[i].ItemArray.GetValue(11);
                                    if (TblZlgNew.Rows[i].ItemArray.GetValue(12) != DBNull.Value)
                                        LiKsa = (int)TblZlgNew.Rows[i].ItemArray.GetValue(12);
                                    if (TblZlgNew.Rows[i].ItemArray.GetValue(13) != DBNull.Value)
                                        liImportId = (int)TblZlgNew.Rows[i].ItemArray.GetValue(13);
                                    zl = 1;         // Anzahl der Monate = Anzahl der Datensätze in Timeline

                                    // Den erstenTag des Monats einsetzen
                                    string dt = (ldtStart.Year.ToString()) + "-" + ldtStart.Month.ToString() + "-01";
                                    ldtMonat = DateTime.Parse(dt);                 // Datetime mit erstem Tag des Monats

                                    do
                                    {
                                        DataRow dr = TblTml.NewRow();

                                        dr[2] = LiSourceId;
                                        dr[4] = liObjekt;
                                        dr[5] = liObjektTeil;
                                        dr[6] = liMieter;
                                        dr[7] = LiKsa;
                                        dr[8] = ldBetragNetto * -1;             // Alles * -1 wegen Zahlungen
                                        dr[9] = ldBetragSollNetto * -1;
                                        dr[10] = ldBetragBrutto * -1;
                                        dr[11] = ldBetragSollBrutto * -1;
                                        dr[12] = ldZs;                          // Zählerstand
                                        dr[13] = ldtStart;
                                        dr[14] = 0;
                                        dr[15] = 0;
                                        dr[17] = liImportId;

                                        TblTml.Rows.Add(dr);
                                        // + Monat 
                                        ldtMonat = ldtMonat.AddMonths(1);
                                        // + Zähler
                                        zl++;

                                    } while (zl <= liMonths);

                                    // und alles ab in die Datenbank
                                    liOk = Timeline.FetchData("", "", 32, asConnect);
                                }
                                else
                                {
                                    MessageBox.Show("Verarbeitungsfehler ERROR fetchdata RdFunctions 0003\n piArt = " + aiArt.ToString(),
                                                "Achtung");
                                    break;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case 16:
                    // Die Rechnungs Id aus der Timeline ermitteln
                    if (TblRgId.Rows.Count >= 0)
                    {
                        if (TblRgId.Rows[0].ItemArray.GetValue(6) != DBNull.Value)
                        {
                            liRgId = (int)TblRgId.Rows[0].ItemArray.GetValue(6);
                        }
                        else
                        {
                            liRgId = 0;
                        }
                    }
                    break;
                case 21:
                    // Externe ID aus der Zählerstand ermitteln 
                    for (int i = 0; TblCnt.Rows.Count > i; i++)
                    {
                        if (TblCnt.Rows[i].ItemArray.GetValue(8) != DBNull.Value)
                        {
                            LiSourceId = (int)TblCnt.Rows[i].ItemArray.GetValue(8);
                            // Timeline löschen
                            // liOk = Timeline.TimelineDelete(LiSourceId, "Z", asConnect, aiDb);

                            // Objekt
                            if (TblCnt.Rows[i].ItemArray.GetValue(9) != DBNull.Value)
                                if ((int)TblCnt.Rows[i].ItemArray.GetValue(9) > 0)
                                {
                                    liObjekt = (int)TblCnt.Rows[i].ItemArray.GetValue(9);
                                    // Timeline neu erzeugen Objekte aus Zählerständen
                                    liOk = Timeline.TimelineCreate(LiSourceId, "id_zaehlerstand", asConnect, aiDb);
                                }

                            // ObjektTeil
                            if (TblCnt.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                                if ((int)TblCnt.Rows[i].ItemArray.GetValue(10) > 0)
                                {
                                    liObjektTeil = (int)TblCnt.Rows[i].ItemArray.GetValue(10);
                                    ldtMonat = Convert.ToDateTime(TblCnt.Rows[i].ItemArray.GetValue(4));
                                    // Timeline neu erzeugen Objektteile aus Zählerständen
                                    liOk = Timeline.TimelineCreate(LiSourceId, "id_zaehlerstand", asConnect, aiDb);

                                    // Weiterleitung an aktiven Mieter
                                    liMieter = Timeline.GetAktMieter(liObjektTeil, ldtMonat, asConnect, aiDb);

                                    if (liMieter > 0)
                                    {
                                        liArtRelation = 3;
                                        // Timeline neu erzeugen für Relationen
                                        liOk = Timeline.TimelineCreateRelations(LiSourceId, liObjekt, liObjektTeil, liMieter, liArtRelation, asConnect, aiDb);
                                    }
                                }

                            //// Mieter
                            //if ( tableCnt.Rows[i].ItemArray.GetValue(1) != DBNull.Value)
                            //    if ((int)tableCnt.Rows[i].ItemArray.GetValue(1) > 0)
                            //    {
                            //        liMieter = (int)tableCnt.Rows[i].ItemArray.GetValue(1);
                            //        // Timeline neu erzeugen Mieter aus Zählerstände
                            //        // ACHTUNG hier Kontrolle einbauen, ob Mietvertrag gültig ist ULF!
                            //        liOk = TimelineCreate(liExternId, "id_zs");
                            //    }
                        }
                        else
                        {
                            MessageBox.Show("Verarbeitungsfehler ERROR fetchdata RdFunctions fetchdata\n piArt = " + aiArt.ToString(),
                                        "Achtung");
                            break;
                        }
                    }
                    break;
                case 23:            // Zähler

                    for (int i = 0; TblCntNew.Rows.Count > i; i++)
                    {
                        if (TblCntNew.Rows[i].ItemArray.GetValue(8) != DBNull.Value)
                        {
                            LiSourceId = (int)TblCntNew.Rows[i].ItemArray.GetValue(8);

                            if (TblCntNew.Rows[i].ItemArray.GetValue(0) != DBNull.Value)
                                liZsId = (int)TblCntNew.Rows[i].ItemArray.GetValue(0);            // Id Zählerstand
                            if (TblCntNew.Rows[i].ItemArray.GetValue(4) != DBNull.Value)
                                ldtStart = (DateTime)TblCntNew.Rows[i].ItemArray.GetValue(4);     // Datum
                            if (TblCntNew.Rows[i].ItemArray.GetValue(5) != DBNull.Value)
                                ldVerbrauch = (decimal)TblCntNew.Rows[i].ItemArray.GetValue(5);   // Verbrauch
                            if (TblCntNew.Rows[i].ItemArray.GetValue(6) != DBNull.Value)
                                ldBetragNetto = (decimal)TblCntNew.Rows[i].ItemArray.GetValue(6);     // Preis Einheit Netto
                            if (TblCntNew.Rows[i].ItemArray.GetValue(7) != DBNull.Value)
                                ldBetragBrutto = (decimal)TblCntNew.Rows[i].ItemArray.GetValue(7);    // Preis Einheit Brutto
                            if (TblCntNew.Rows[i].ItemArray.GetValue(9) != DBNull.Value)
                                liObjekt = (int)TblCntNew.Rows[i].ItemArray.GetValue(9);          // Objekt
                            if (TblCntNew.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                                liObjektTeil = (int)TblCntNew.Rows[i].ItemArray.GetValue(10);     // Obj Teil
                            if (TblCntNew.Rows[i].ItemArray.GetValue(11) != DBNull.Value)
                                LiKsa = (int)TblCntNew.Rows[i].ItemArray.GetValue(11);            // Kostenstellenart

                            DataRow dr = TblTml.NewRow();

                            dr[3] = LiSourceId;     // id Zählerstand
                            dr[4] = liObjekt;
                            dr[5] = liObjektTeil;
                            dr[6] = liMieter;
                            dr[7] = LiKsa;
                            dr[8] = ldBetragNetto * ldVerbrauch;
                            dr[10] = ldBetragBrutto * ldVerbrauch;
                            dr[13] = ldtStart;
                            dr[14] = 0;
                            dr[15] = 0;
                            // dr[17] = 99; für Testzwecke, um Zählerdaten wiederzufinden

                            TblTml.Rows.Add(dr);

                            // und alles ab in die Datenbank
                            Timeline.MakeCommand(4);
                        }
                        else
                        {
                            MessageBox.Show("Verarbeitungsfehler ERROR fetchdata RdFunctions 0003\n piArt = " + aiArt.ToString(),
                                        "Achtung");
                            break;
                        }
                    }
                    break;
                default:
                    break;

            }

            // mainWindow.ProgressBar.IsIndeterminate = false;
            return LiReturn;
        }
    }
}
