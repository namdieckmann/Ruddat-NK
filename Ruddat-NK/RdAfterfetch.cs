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
        // aiArt wird für Art 1 = Rechnung  2= ZählerStand
        // AiTeil wird Art der Aufteilung?
        // ----------------------------------------------------------------------------------------
        public static int MakeAfterFetch(int aiArt, int aiTeil, int ai1, int ai2, string asConnect, 
            MySqlDataAdapter ASdaRechnungen, DataTable ATblRechnungen,
            MySqlDataAdapter AsdaObjektTeile, DataTable ATblObjektTeile,
            MySqlDataAdapter AsdaMieter, DataTable ATblMieter,
            MySqlDataAdapter AsdaTimeline, DataTable ATblTimeline,
            MySqlDataAdapter AsdaZaehlerWerte, DataTable ATblZaehlerWerte
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

            switch (aiArt)
            {
                case 1:     // Rechnungen
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
                                        liArtRelation = 1;                                                          // Rechnung

                                        if (Timeline.GetWeiterleitung(1, LiKsa, asConnect) == 1)
                                        {
                                            // Rechnungen und Timeline für alle zugehörigen Objektteile erzeugen
                                            CreateRechnungenObjTeile(LiSourceId, liObjekt, liObjektTeil, liMieter, liArtRelation,
                                                ASdaRechnungen, ATblRechnungen,
                                                AsdaZaehlerWerte, ATblZaehlerWerte,
                                                AsdaObjektTeile, ATblObjektTeile,
                                                AsdaTimeline, ATblTimeline,
                                                asConnect, liObjektTeil);
                                        }
                                    }
                                // Teilobjekt Rechnung
                                if (ATblRechnungen.Rows[i].ItemArray.GetValue(9) != DBNull.Value)
                                    if ((int)ATblRechnungen.Rows[i].ItemArray.GetValue(9) > 0)
                                    {
                                        LiKsa = (int)ATblRechnungen.Rows[i].ItemArray.GetValue(1);                  // Kostenart
                                        liObjektTeil = (int)ATblRechnungen.Rows[i].ItemArray.GetValue(9);           // ObjektTeil
                                        liArtRelation = 1;                                                          // Rechnung
                                    }
                                // Mieter Rechnung
                                if (ATblRechnungen.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                                    if ((int)ATblRechnungen.Rows[i].ItemArray.GetValue(10) > 0)
                                    {
                                        LiKsa = (int)ATblRechnungen.Rows[i].ItemArray.GetValue(1);                   // Kostenart
                                        liMieter = (int)ATblRechnungen.Rows[i].ItemArray.GetValue(10);               // Mieter
                                        liArtRelation = 1;                                                           // Rechnung
                                    }
                            }
                        }
                    }

                    break;
                case 2:             // Zählerstände
                    for (int j = 0; j < ATblZaehlerWerte.Rows.Count; j++)
                    {
                        // ID aus der Zählerablesung ermitteln 
                        if (ATblZaehlerWerte.Rows[j].ItemArray.GetValue(0) != DBNull.Value)
                        {
                            if (int.Parse(ATblZaehlerWerte.Rows[j].ItemArray.GetValue(13).ToString()) == 1)     // Nur der zugefügte oder editierte Datensatz
                            {

                                LiSourceId = (int)ATblZaehlerWerte.Rows[j][0];
                                // Erzeugte Zählerrechnung > Rechnungen löschen
                                // Alle mit der Id der Hauptrechnung in id_zaehler löschen
                                Timeline.DeleteRechnung(LiSourceId, "Z", asConnect);


                                // Umgang mit flag_timeline in Zählerwerten?
                                // Rechnung erzeugen auf gleicher Ebene 
                                // Untergeordnete Rechnungen erzeugen
                                // Timline erzeugen aber nur auf der Zählerstandsebene
                                // Untergeordnete Timlines werden bei Anwahl erzeugt

                                // Objekt Zählerrechnung
                                if (ATblZaehlerWerte.Rows[j].ItemArray.GetValue(8) != DBNull.Value)
                                    if ((int)ATblZaehlerWerte.Rows[j].ItemArray.GetValue(8) > 0)
                                    {
                                        CreateRechnungenZaehler(ASdaRechnungen, ATblRechnungen,
                                                                AsdaZaehlerWerte, ATblZaehlerWerte,
                                                                AsdaObjektTeile, ATblObjektTeile,
                                                                LiSourceId, asConnect);

                                        if (ATblRechnungen.Rows[j][20] != DBNull.Value)                                 // Zählerwert Id vorhenden
                                        {
                                            LiKsa = (int)ATblRechnungen.Rows[j].ItemArray.GetValue(1);                  // Kostenart

                                            // Untergeordnete Rechnungen erzeugen
                                            // Parameter: 3 = Zähler
                                            if (Timeline.GetWeiterleitung(3, LiKsa, asConnect) == 1)
                                            {
                                                // Alle mit der id_zähler löschen außer die Hauptrechnung
                                                Timeline.DeleteRechnung((int)ATblRechnungen.Rows[0].ItemArray.GetValue(20), "U", asConnect);

                                                // Rechnungs Id aus Zaehlerwert Id (id steht u.u. noch nicht in der Tabelle Rechnungen)
                                                LiSourceId = Timeline.GetRgZlwId((int)ATblRechnungen.Rows[0].ItemArray.GetValue(20), asConnect, 2);

                                                // Rechnungen und Timeline für alle zugehörigen Objektteile erzeugen
                                                CreateRechnungenObjTeile(LiSourceId, liObjekt, liObjektTeil, liMieter, liArtRelation,
                                                    ASdaRechnungen, ATblRechnungen,
                                                    AsdaZaehlerWerte, ATblZaehlerWerte,
                                                    AsdaObjektTeile, ATblObjektTeile,
                                                    AsdaTimeline, ATblTimeline,
                                                    asConnect, liObjektTeil);
                                            }
                                        }
                                    }

                                // Teilobjekt ZählerRechnung keine weitere Verteilung
                                if (ATblZaehlerWerte.Rows[j].ItemArray.GetValue(9) != DBNull.Value)
                                    if ((int)ATblZaehlerWerte.Rows[j].ItemArray.GetValue(9) > 0)
                                    {
                                        CreateRechnungenZaehler(ASdaRechnungen, ATblRechnungen,
                                                                AsdaZaehlerWerte, ATblZaehlerWerte,
                                                                AsdaObjektTeile, ATblObjektTeile,
                                                                LiSourceId, asConnect);
                                    }
                            }
                        }
                    }
                    break;
                default:
                    break;
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
            int LiRgZaehlerWertSource = 0;          // Zählereintrag Id Source

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
            int LiFlagTml = 0;              // Flag TimeLine neu erzeugen
            //int liImportId = 0;         // Import Id

            //int liRgId = 0;             // Rechnungs ID
            //int liZsId = 0;             // Zähler Id

            string LsVerteilung = "";
            string LsRgNr = "";
            string LsFirma = "";

            for (int i = 0; i < ATblRechnungen.Rows.Count; i++)
            {
                if (ATblRechnungen.Rows[i].ItemArray.GetValue(0) != DBNull.Value)       // Todo und timelineflag in Rechnungen = 1
                {
                    if ((int)ATblRechnungen.Rows[i].ItemArray.GetValue(15) == 1)        // Timeline Flag ist gesetzt > Timeline neu erzeugen
                    {
                        int LiWtlObjekt = 0;            // Weiterleitungen
                        int LiWtlObjektTeil = 0;

                        // Eine Rechnung aus dem Objekt weitergeleitet hat source id? kann auch Zählerwert sein
                        if (ATblRechnungen.Rows[i].ItemArray.GetValue(17) != DBNull.Value)          // Rechnungs Source Id Quelle Weiterleitung 
                        {
                            if (AiObjektTeilId > 0)
                            {
                                LiWtlObjekt = 1;        // Weiterleitung aus Objekt
                                // Timeline mit der SouceId löschen
                                Timeline.DeleteTimeline((int)ATblRechnungen.Rows[i].ItemArray.GetValue(17), "S", AsConnect);
                            }
                        }

                        // Rechnungs Id Objekt oder Teilobjekt
                        if (ATblRechnungen.Rows[i].ItemArray.GetValue(0) != DBNull.Value)     // Rechnungs Id Objekt
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

                        // Wenn es eine Zählerwert ID gibt, die Timeline löschen
                        if (ATblRechnungen.Rows[i].ItemArray.GetValue(20) != DBNull.Value)
                        {
                            if ((int)ATblRechnungen.Rows[i].ItemArray.GetValue(20) > 0)
                            {
                                Timeline.DeleteTimeline((int)ATblRechnungen.Rows[i].ItemArray.GetValue(20), "Z", AsConnect);
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
                        if (ATblRechnungen.Rows[i].ItemArray.GetValue(20) != DBNull.Value)
                            LiRgZaehlerWertSource = (int)ATblRechnungen.Rows[i].ItemArray.GetValue(20);

                        // Anzahl der Tage des ersten Monats        99 ist der volle Monat
                        liDaysStart = Timeline.GetDaysStart(LdtStart);
                        // Anzahl der Tage des letzten Monats       99 ist der volle Monat
                        liDaysEnd = Timeline.GetDaysEnd(LdtEnd);
                        // Anzahl der einzutragenden Monate ermitteln
                        liMonths = Timeline.GetMonths(LdtStart, LdtEnd);
                        // Zahlung oder Rechnung 2 = Rechnung 1 = Zahlung 
                        liZlgOrRg = 2;

                        // Monatsbeträge ermitteln (Brutto und Netto) und evtl. erster und letzter Monat nicht voll
                        ladBetraege = Timeline.GetMonatsBetraege(liMonths, liDaysStart, liDaysEnd,
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
                            DrTimeline[3] = LiRgZaehlerWertSource;
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
                            DrTimeline[9] = ladBetraege[3];         // Soll Netto
                            DrTimeline[11] = ladBetraege[4];        // Soll Brutto
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
                    }
                }

                // TimelineFlag Reset
                ATblRechnungen.Rows[i][15] = 0;
            }

            try
            {
                // Timeline Ab in die Datenbank
                MySqlCommandBuilder commandBuilder = new MySqlCommandBuilder(ASdaTimeline);
                ASdaTimeline.Update(ATblTimeline);
                LiOk = 1;
            }
            catch (Exception)
            {
                MessageBox.Show("Verarbeitungsfehler ERROR fetchdata fetchdata RdFunctions\n CreateTimeline = ",
                "Achtung");
                LiOk = 0;
                throw;
            }

            try
            {
                // Rechnungsänderungen ab in die Datenbank
                MySqlCommandBuilder commandBuilder2 = new MySqlCommandBuilder(ASdaRechnungen);
                ASdaRechnungen.Update(ATblRechnungen);
                LiOk = 1;
            }
            catch (Exception)
            {
                MessageBox.Show("Verarbeitungsfehler ERROR fetchdata fetchdata RdFunctions\n UpdateRechnungen = ",
                "Achtung");
                LiOk = 0;
                throw;
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
                        if (LiMieterId == 0)            // Todo Leerstand buchen !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
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
                try
                {
                    // Ab in die Datenbank
                    MySqlCommandBuilder commandBuilder = new MySqlCommandBuilder(ASdTimeLine);
                    ASdTimeLine.Update(ATblTimeLine);
                }
                catch (Exception)
                {
                    MessageBox.Show("Löschen von Daten fehlgeschlagen (TimeLine Mieter)", "Datenfehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw;
                }
            }
        }


        // Aus Rechnungen Objekt Rechnung für jedes ObjektTeil erzeugen
        private static int CreateRechnungenObjTeile(int AiSourceId, int AiObjektId, int AiObjektTeilId,
                int AiMieterid, int AiArtRelation,
                MySqlDataAdapter ASdaRechnungen, System.Data.DataTable ATblRechnungen,
                MySqlDataAdapter ASdaZaehlerWerte, System.Data.DataTable ATblZaehlerWerte,
                MySqlDataAdapter ASdaTeilobjekte, System.Data.DataTable ATblTeilobjekte,
                MySqlDataAdapter ASdaTimeline, System.Data.DataTable ATblTimeline,
                string AsConnect, int liObjektTeil)
        {
            int LiOk = 0;

            DateTime LdtStart = DateTime.MinValue;
            DateTime LdtEnd = DateTime.MinValue;
            DateTime ldtMonat = DateTime.MinValue;
            DateTime ldtVertrag = DateTime.MinValue;
            DateTime Ldtrechnung = DateTime.MinValue;

            int liObjekt = 0;
            // int liObjektTeil = 0;
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
            decimal LdAnzPersonenObj = 0;
            decimal LdAnzPersonenObjTeil = 0;
            decimal ldZs = 0;            // Zählerstand
            decimal ldVerbrauch = 0;     // Zähler Verbrauch
            decimal[] LadBetragteil = new decimal[12];

            int zl = 0;
            int LiMwstId = 0;
            int liZlgOrRg = 0;
            int LiSourceId = 0;
            //int liRechnungId = 0;
            //int liZahlungId = 0;
            //int liZaehlerstandId = 0;
            //int liOk = 0;
            //int liFlTml = 0;            // Flag TimeLine in Zahlungen
            //int liImportId = 0;         // Import Id
            int liVerteilungId = 0;         // Id Kostenverteilung
            int LiVerteilungsIdNew = 0;     // VerteilungsId muss evtl. umgewandelt werden
            //int liRgId = 0;             // Rechnungs ID
            int liZaehlerWertId = 0;      // Zählerwert Id

            string LsVerteilung = "";
            string LsRgNr = "";
            string LsFirma = "";
            string LsText = "";

            if ((int)ATblRechnungen.Rows[0].ItemArray.GetValue(15) == 1)                    // Timeline Flag ist gesetzt
            {
                if (ATblRechnungen.Rows[0].ItemArray.GetValue(7) != DBNull.Value)
                    LiMwstId = int.Parse(ATblRechnungen.Rows[0].ItemArray.GetValue(7).ToString());
                if (ATblRechnungen.Rows[0].ItemArray.GetValue(8) != DBNull.Value)
                    liObjekt = (int)ATblRechnungen.Rows[0].ItemArray.GetValue(8);
                if (ATblRechnungen.Rows[0].ItemArray.GetValue(9) != DBNull.Value)
                    _ = (int)ATblRechnungen.Rows[0].ItemArray.GetValue(9);
                if (ATblRechnungen.Rows[0].ItemArray.GetValue(10) != DBNull.Value)
                    _ = (int)ATblRechnungen.Rows[0].ItemArray.GetValue(10);
                if (ATblRechnungen.Rows[0].ItemArray.GetValue(11) != DBNull.Value)
                    LsRgNr = ATblRechnungen.Rows[0].ItemArray.GetValue(11).ToString();
                if (ATblRechnungen.Rows[0].ItemArray.GetValue(12) != DBNull.Value)
                    LsFirma = ATblRechnungen.Rows[0].ItemArray.GetValue(12).ToString();
                if (ATblRechnungen.Rows[0].ItemArray.GetValue(13) != DBNull.Value)
                    LsText = ATblRechnungen.Rows[0].ItemArray.GetValue(13).ToString();
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
                if (ATblRechnungen.Rows[0].ItemArray.GetValue(20) != DBNull.Value)
                    liZaehlerWertId = (int)ATblRechnungen.Rows[0].ItemArray.GetValue(20);

                // Anzahl der Tage des ersten Monats        99 ist der volle Monat
                liDaysStart = Timeline.GetDaysStart(LdtStart);
                // Anzahl der Tage des letzten Monats       99 ist der volle Monat
                liDaysEnd = Timeline.GetDaysEnd(LdtEnd);
                // Anzahl der einzutragenden Monate ermitteln
                liMonths = Timeline.GetMonths(LdtStart, LdtEnd);
                // Zahlung oder Rechnung 1= Zahlung 2= Rechnung
                liZlgOrRg = 2;

                // Ermitteln, wie verteilt werden soll aus der Tabelle art_verteilung
                LsVerteilung = Timeline.GetVerteilung(AsConnect, liVerteilungId);

                // Wenn vom Objekt eine Verteilung Fläche, Zählerwert oder Prozent kommt,
                // mus das im ObjektTeil zu direkter Kostenverteilung werden
                switch (LsVerteilung)
                {
                    case "fl":      // Fläche wird zu direkt
                        LiVerteilungsIdNew = Timeline.GetVerteilungIdAusArtVerteilung("di", AsConnect);
                        break;
                    case "pz":      // Prozent wird zu direkt
                        LiVerteilungsIdNew = Timeline.GetVerteilungIdAusArtVerteilung("di", AsConnect);
                        break;
                    case "ps":      // Personen wird zu direkt
                        LiVerteilungsIdNew = Timeline.GetVerteilungIdAusArtVerteilung("di", AsConnect);
                        break;
                    case "zl":      // Zählerwert wird zu direkt
                        LiVerteilungsIdNew = Timeline.GetVerteilungIdAusArtVerteilung("di", AsConnect);
                        break;
                    default:
                        LiVerteilungsIdNew = liVerteilungId;
                        break;
                }

                // Gesamtfläche aus Tabelle Objekt holen
                LdGesamtflaeche = Timeline.GetObjektflaeche(liObjekt, 0, 0, AsConnect);
                // Anzahl der Personen aus Tabelle Objekt holen Flag 0 für Objekt Id
                LdAnzPersonenObj = Timeline.GetAktPersonen(liObjekt, 0, 0, LdtStart, LdtEnd, 0, AsConnect);

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
                    // DrRechnung[10] = AiMieterid;         // nicht einsetzen
                    DrRechnung[11] = LsRgNr;
                    DrRechnung[12] = LsFirma;
                    DrRechnung[13] = LsText;
                    DrRechnung[15] = 1;                     // Flag für Timelinebearbeitung erzeugen
                    DrRechnung[16] = LiVerteilungsIdNew;
                    DrRechnung[17] = AiSourceId;
                    DrRechnung[20] = liZaehlerWertId;

                    switch (LsVerteilung)
                    {
                        case "fl":          // Berechnung Kosten MietFläche
                            if (ATblTeilobjekte.Rows[i].ItemArray.GetValue(6) != DBNull.Value)
                            {
                                if ((decimal)ATblTeilobjekte.Rows[i].ItemArray.GetValue(6) > 0) // Fläche Teilobjekt
                                {
                                    decimal LdteilFlaeche = (decimal)ATblTeilobjekte.Rows[i].ItemArray.GetValue(6);

                                    DrRechnung[5] = ldBetragNetto / (LdGesamtflaeche / LdteilFlaeche);           // Netto    
                                    DrRechnung[6] = ldBetragBrutto / (LdGesamtflaeche / LdteilFlaeche);           // Brutto
                                }
                            }
                            break;
                        case "pz":          // Berechung nach Prozent
                            if (ATblTeilobjekte.Rows[i].ItemArray.GetValue(7) != DBNull.Value)
                            {
                                if ((decimal)ATblTeilobjekte.Rows[i].ItemArray.GetValue(7) > 0)     //  Prozent
                                {
                                    decimal LdteilProzent = (decimal)ATblTeilobjekte.Rows[i].ItemArray.GetValue(7);

                                    DrRechnung[5] = ldBetragNetto / (100 / LdteilProzent);           // Netto    
                                    DrRechnung[6] = ldBetragBrutto / (100 / LdteilProzent);           // Brutto
                                }
                            }
                            break;
                        case "ps":      // Berechnung nach Personenzahl
                            if (ATblTeilobjekte.Rows[i].ItemArray.GetValue(8) != DBNull.Value)
                            {
                                if (int.Parse(ATblTeilobjekte.Rows[i].ItemArray.GetValue(8).ToString()) == 1)
                                {
                                    LdAnzPersonenObjTeil = Timeline.GetAktPersonen(0, (int)ATblTeilobjekte.Rows[i].ItemArray.GetValue(0), 0, LdtStart, LdtEnd, 0, AsConnect);

                                    if (LdAnzPersonenObj > LdAnzPersonenObjTeil)
                                    {
                                        decimal LdTeilPersonen = LdAnzPersonenObj / LdAnzPersonenObjTeil;

                                        DrRechnung[5] = ldBetragNetto / LdTeilPersonen;           // Netto    
                                        DrRechnung[6] = ldBetragBrutto / LdTeilPersonen;           // Brutto
                                    }
                                }
                            }
                            break;
                        case "zl":      // Verteilung nach Zählerwert
                            break;
                        case "nl":      // Keine Verteilung
                            break;
                        case "di":      // Direkt auf Anzahl der Objekte

                            int LiRows = ATblTeilobjekte.Rows.Count;

                            DrRechnung[5] = ldBetragNetto / LiRows;           // Netto    
                            DrRechnung[6] = ldBetragBrutto / LiRows;           // Brutto

                            break;
                        case "fa":      // Verteilung nach Flächenauswahl
                                        // Todo Parts: Nur die betroffnenen Räume speichern mit Flag Fläche beibehalten
                                        // Bezug ist die Rechnungsnummer also um id_rechnung erweitern
                                        // Schleife durch Teilobjekte
                                        // Prüfen, ob es einen Eintrag der Rechungsnummer in objekt_mix_parts gibt
                                        // Wenn die Gesamtfläche bleiben soll sofort den Teil rechnen
                                        // Sonst zunächst Gesamtfläche der Auswahl rechnen 

                            break;
                        default:
                            break;
                    }
                    ATblRechnungen.Rows.Add(DrRechnung);
                }
            }
            try
            {
                // Daten in die Datenbank schreiben
                MySqlCommandBuilder commandBuilder = new MySqlCommandBuilder(ASdaRechnungen);
                ASdaRechnungen.Update(ATblRechnungen);
            }
            catch (Exception)
            {
                MessageBox.Show("Erzeugen von Rechnungen fehlgeschlagen (Rechnungen ObjektTeile)", "Datenfehler", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
            return LiOk;
        }
        // Nach Anwahl der Teilobjekte im Treeview die Mieterrechnungen erzeugen
        // Todo !!!!!> nein Mieterrechnugen müssen aus der Timeline erzeugt werden !!!!!!!!!!!!!!!!!!!!!!!!!!!!
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
                DrRechnung[15] = 1;                                                                    // Flag für Timelinebearbeitung erzeugen
                DrRechnung[16] = liVerteilungId;
                DrRechnung[17] = LiSourceId;

                aTblRechnungen.Rows.Add(DrRechnung);
            }
        }

        // Rechnung aus Zählerwert erzeugen AiSwitch> 1= Objekt 2=Teilobjekt
        internal static void CreateRechnungenZaehler(
                MySqlDataAdapter ASdaRechnungen, System.Data.DataTable ATblRechnungen,
                MySqlDataAdapter ASdaZaehlerWerte, System.Data.DataTable ATblZaehlerWerte,
                MySqlDataAdapter ASdaTeilobjekte, System.Data.DataTable ATblTeilobjekte,
                int AiSourceId, string asConnect)
        {
            int LiZaehlerId = 0;
            int LiMwstId = 0;
            int LiEinheitId = 0;
            string LsRgNr = "";
            string LsEinheit = "";
            string LsText = "";
            DateTime LdtStart = DateTime.MinValue;
            DateTime LdtEnd = DateTime.MinValue;


            for (int i = 0; i < ATblZaehlerWerte.Rows.Count; i++)
            {
                if ((int)ATblZaehlerWerte.Rows[i][13] == 1 && (int)ATblZaehlerWerte.Rows[i][0] == AiSourceId)
                {
                    // Zählernummer ermitteln
                    LsRgNr = Timeline.GetZlName((int)ATblZaehlerWerte.Rows[i][10], asConnect, 2);
                    LiZaehlerId = (int)ATblZaehlerWerte.Rows[i][10];
                    LdtEnd = (DateTime)ATblZaehlerWerte.Rows[i][1];             // Datum Ablesung
                    LdtStart = Timeline.GetZlStartDatum(LiZaehlerId, LdtEnd, asConnect);                  // Startdatum
                    LiMwstId = Timeline.GetZlMwstId(LiZaehlerId, asConnect, 2);      // MwstId
                    LiEinheitId = (int)ATblZaehlerWerte.Rows[i][4];
                    LsEinheit = Timeline.GetEinheit(LiEinheitId, asConnect, 2);

                    // Rechnungstext für Zählerrechnung
                    LsText = @"Verbrauch: " + ATblZaehlerWerte.Rows[i][3].ToString()  
                                        + " " + LsEinheit 
                                        + " - "
                                        + ATblZaehlerWerte.Rows[i][5].ToString() + "€ Netto " 
                                        + ATblZaehlerWerte.Rows[i][6].ToString() + "€ Brutto";

                    // Neue Rechnung erzeugen
                    DataRow DrRechnung = ATblRechnungen.NewRow();

                    DrRechnung[1] = (int)ATblZaehlerWerte.Rows[i][11];      // id Ksa
                    DrRechnung[2] = DateTime.Now;  // Datum
                    DrRechnung[3] = LdtStart;  // Datum Von vorherige Ablesung
                    DrRechnung[4] = LdtEnd;  // Datum Bis Ablesung
                    DrRechnung[5] = (decimal)ATblZaehlerWerte.Rows[i][3] * (decimal)ATblZaehlerWerte.Rows[i][5];    // Netto
                    DrRechnung[6] = (decimal)ATblZaehlerWerte.Rows[i][3] * (decimal)ATblZaehlerWerte.Rows[i][6];    // Brutto
                    DrRechnung[7] = LiMwstId;
                    DrRechnung[8] = (int)ATblZaehlerWerte.Rows[i][8];           // Objekt
                    DrRechnung[9] = (int)ATblZaehlerWerte.Rows[i][9];           // TeilObjekt
                    DrRechnung[10] = 0;
                    DrRechnung[11] = LsRgNr;
                    DrRechnung[12] = "Zählerrechnung";                          // LsFirma;
                    DrRechnung[13] = LsText;
                    DrRechnung[15] = (int)ATblZaehlerWerte.Rows[i][13];         // Flag für Timelinebearbeitung
                    DrRechnung[16] = (int)ATblZaehlerWerte.Rows[i][12];         // VerteilungsId
                    DrRechnung[20] = (int)ATblZaehlerWerte.Rows[i][0];          // LiSourceId wird auf id Zaehlerwert gesetzt

                    ATblRechnungen.Rows.Add(DrRechnung);
                }
            }
            try
            {
                // Daten in die Datenbank schreiben
                MySqlCommandBuilder commandBuilder = new MySqlCommandBuilder(ASdaRechnungen);
                ASdaRechnungen.Update(ATblRechnungen);
            }
            catch (Exception)
            {
                MessageBox.Show("Erzeugen von Rechnungen fehlgeschlagen (Rechnungen Zähler)", "Datenfehler", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }
    }
}
