using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Globalization;


namespace Ruddat_NK
{
    public class Timeline
    {
        // Datensätze Rechnungen
        static DataTable tableOne;
        // static DataTable tableTwo;
        static DataTable tableThree;
        static DataTable tableFour;
        static DataTable tableFive;
        static DataTable tableSix;
        static DataTable tableSeven;
        static DataTable tableEight;
        static DataTable tableZlg;
        static DataTable tableZlgNew;
        static DataTable tableTml;
        static DataTable tableRgId;
        static DataTable tableConSumObj;
        static DataTable tableConSumObjT;
        static DataTable tableCnt;
        static DataTable tableCntNew;
        static DataTable tableZlInfo;
        static DataTable tableObjTeil;      // Objektteile
        static DataTable tableParts;        // objekt_mix_parts
        static SqlDataAdapter sda;
        // static SqlDataAdapter sdb;
        static SqlDataAdapter sdc;
        // static SqlDataAdapter sdd;
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
        static SqlDataAdapter sdObjTeil;
        static SqlDataAdapter sdParts;
        static string gsConnectString = "";
        static string lsSql = "";

        // Todo Timeline für 2.te Datenbank erweitern und testen. Sind Datumseinträge enthalten?

        // Neue Id für Timeline ermitteln
        public static int getTimelineId(string asConnectString, int asArt)
        {
            Int32 liGetLastTempId = 0;

            switch (asArt)
            {
                case 1:
                    lsSql = "Select max(id_extern_timeline )from rechnungen";
                    break;
                case 2:
                    lsSql = "Select max(id_extern_timeline )from zahlungen";
                    break;
                case 3:
                    lsSql = "Select max(id_extern_timeline )from zaehlerstaende";
                    break;
                default:
                    break;
            }

            SqlConnection connect;
            connect = new SqlConnection(asConnectString);
            SqlCommand command = new SqlCommand(lsSql, connect);

            try
            {
                // Db open
                connect.Open();

                var lvGetLastTempId = command.ExecuteScalar();

                if (lvGetLastTempId != null)
                {
                    Int32.TryParse(lvGetLastTempId.ToString(), out liGetLastTempId);    // Ulf! TODO testen
                }
                else
                {
                    liGetLastTempId = 0;
                }

                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es wurden keine GetLastTempId gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (rdfunctions.getTimelineId id extern timeline)",
                         MessageBoxButton.OK);
            }
            return (liGetLastTempId);
        }

        // Timeline wurde geändert (Löschen und neu anlegen)
        // Parameter: Timeline ID für ändern, Flag nach zufügen
        // Rechnungen
        // Flag = 1 > ändern
        // Flag = 2 > löschen
        // Zahlungen
        // Flag = 11 > ändern
        // Flag = 12 > löschen
        // Zählerstädene
        // Flag = 21 > ändern
        // Flag = 22 > löschen
        public static void editTimeline(int liTimelineId, int liFlagAdd , string asConnectString)
        {
            string lsSql = "";
            int liRows = 0;
            int liOk = 0;
            gsConnectString = asConnectString;

            switch (liFlagAdd)
            {
                case 1:
                    // Rechnungen Daten holen mit id extern timeline
                    lsSql = Timeline.getSql(1, liTimelineId, "", "",0);
                    // Sql, Art = 1 
                    liRows = Timeline.fetchData(lsSql,"", 1, asConnectString);
                    break;
                case 2:
                    // Rechnung Timeline löschen
                    liOk = Timeline.TimelineDelete(liTimelineId);
                    break;
                case 11:
                    // Zahlungen Daten holen mit id extern timeline
                    lsSql = Timeline.getSql(12, liTimelineId, "", "",0);
                    // Sql, Art = 11 
                    liRows = Timeline.fetchData(lsSql,"", 11, asConnectString);
                    break;
                case 12:
                    // Zahlungen Timeline löschen 
                    liOk = Timeline.TimelineDelete(liTimelineId);
                    break;
                case 13:
                    // Zahlungen importieren. Nur anderes SQL Statement, sonst wie Case 11
                    lsSql = Timeline.getSql(13, liTimelineId, "", "",0);
                    // Sql, Art = 11 
                    liRows = Timeline.fetchData(lsSql,"", 11, asConnectString);
                    break;
                case 21:
                    // Zählerstände Daten holen mit id extern timeline
                    lsSql = Timeline.getSql(21, liTimelineId, "", "",0);
                    // Sql, Art = 21 
                    liRows = Timeline.fetchData(lsSql, "", 21, asConnectString);
                    break;
                case 22:
                    // Zählerstände Timeline löschen
                    liOk = Timeline.TimelineDelete(liTimelineId);
                    break;
                default:
                    break;
            }
        }

        // Sql Statements zusammenbauen
        public static string getSql(int piArt, int piId, string ps2, string ps3, int piId2)
        {
            String lsSql = "";
            String lsWhereAdd = "";
            String lsWhereAdd2 = "";
            String lsGroup = "";
            String lsOrder = "";
            DateTime ldtAdd = DateTime.MinValue;
            DateTime ldtEnd = DateTime.Today;                       // Heute
            string dt = (DateTime.Now.Year.ToString()) + "-01-01";
            DateTime ldtStart = DateTime.Parse(dt);                 // Jahresanfang

            // Rechnungen mit definierter id_extern_timeline
            if (piArt == 1)
            {
                lsWhereAdd = piId.ToString() + " ";

                lsSql = @"select id_rechnungen,
                                    id_ksa,
                                    datum_rechnung as datum,
                                    datum_von as von,
                                    datum_bis as bis,
                                    betrag_netto netto,
                                    betrag_brutto brutto,
                                    id_mwst_art,
                                    id_objekt,
                                    id_objekt_teil,
                                    id_mieter,
                                    rg_nr,
                                    firma,
                                    text,
                                    id_extern_timeline,
                                    flag_timeline,
                                    id_verteilung
                            from rechnungen
					        where id_extern_timeline = " + lsWhereAdd +
            " Order by rechnungen.datum_rechnung desc";
            }

            // Timeline löschen
            if (piArt == 2)
            {
                lsWhereAdd = piId.ToString() + " ";

                lsSql = @"delete from timeline
					        where id_rechnung = " + lsWhereAdd + " or id_vorauszahlung = " + lsWhereAdd + " or id_zaehlerstand = " + lsWhereAdd;
            }

            // Timeline neu erzeugen in ps2 steht, welches Feld beschrieben werden soll
            if (piArt == 3)
            {
                lsWhereAdd = piId.ToString() + " ";
                lsSql = @"select 
                                id_timeline,     
                                id_rechnung,     
                                id_vorauszahlung,
                                id_zaehlerstand, 
                                id_objekt,       
                                id_objekt_teil,  
                                id_mieter,       
                                id_ksa,          
                                betrag_netto,          
                                betrag_soll_netto,     
                                betrag_brutto,          
                                betrag_soll_brutto,     
                                zs,              
                                dt_monat,
                                wtl_aus_objekt,
                                wtl_aus_objteil,
                                leerstand,
                                id_import
                            from timeline
                            where " + ps2 + " = " + " \'" + lsWhereAdd + "\'";            
            }

            // Timeline neu erzeugen in ps2 steht, welches Feld beschrieben werden soll
            if (piArt == 31)
            {
                lsWhereAdd = piId.ToString() + " ";
                lsSql = @"select 
                                id_timeline,     
                                id_rechnung,     
                                id_vorauszahlung,
                                id_zaehlerstand, 
                                id_objekt,       
                                id_objekt_teil,  
                                id_mieter,       
                                id_ksa,          
                                betrag_netto,          
                                betrag_soll_netto,     
                                betrag_brutto,          
                                betrag_soll_brutto,     
                                zs,              
                                dt_monat,
                                wtl_aus_objekt,
                                wtl_aus_objteil,
                                leerstand,
                                id_import
                            from timeline
                            where " + ps2 + " = " + lsWhereAdd ;
            }

            // TimelineRelations sollen geschrieben werden
            // Hier auf Grundlage des Objektes
            // Beschrieben werden die Kosten für Objektteile
            if (piArt == 4)
            {
                lsWhereAdd = "id_rechnung = " + piId.ToString() + " ";
                lsWhereAdd2 = " id_objekt = " + ps2 + " ";

                lsSql = @"select 
                                id_timeline,     
                                id_rechnung,     
                                id_vorauszahlung,
                                id_zaehlerstand, 
                                id_objekt,       
                                id_objekt_teil,  
                                id_mieter,       
                                id_ksa,          
                                betrag_netto,          
                                betrag_soll_netto,     
                                betrag_brutto,          
                                betrag_soll_brutto,     
                                zs,              
                                dt_monat,
                                wtl_aus_objekt,
                                wtl_aus_objteil,
                                leerstand,
                                id_import
                            from timeline
                                where " + lsWhereAdd + " and " + lsWhereAdd2 + " order by dt_monat";
            }

            // TimelineRelations sollen geschrieben werden
            // Hier auf Grundlage des ObjektTeils
            // Beschrieben werden die Kosten für Mieter
            if (piArt == 5)
            {
                lsWhereAdd = " (id_rechnung = " + piId.ToString() + " or id_vorauszahlung = " + piId.ToString() + " or id_zaehlerstand = " + piId.ToString() + ") ";
                lsWhereAdd2 = " id_objekt_teil > 0 "; // + ps2 + " ";

                lsSql = @"select 
                                id_timeline,     
                                id_rechnung,     
                                id_vorauszahlung,
                                id_zaehlerstand, 
                                id_objekt,       
                                id_objekt_teil,  
                                id_mieter,       
                                id_ksa,          
                                betrag_netto,          
                                betrag_soll_netto,     
                                betrag_brutto,          
                                betrag_soll_brutto,     
                                zs,              
                                dt_monat,
                                wtl_aus_objekt,
                                wtl_aus_objteil,
                                leerstand,
                                id_import
                            from timeline
                                where " + lsWhereAdd + " and " + lsWhereAdd2 + "order by dt_monat" ;
            }

            // für die TimelineRelation Objektteile holen
            if (piArt == 6)
            {
                lsWhereAdd = "id_objekt = " + piId.ToString() + " ";
                lsSql = @"select id_objekt_teil,
                                id_objekt,
                                bez,
                                geschoss,
                                lage,
                                id_adresse,
                                flaeche_anteil,
                                prozent_anteil,
                                personen_anteil_flag
                            from objekt_teil
                            where " + lsWhereAdd; 
            }

            if (piArt == 7)
            {
                lsWhereAdd = "id_mieter = " + piId.ToString() + " ";
                lsSql = @"select id_mieter,
                                id_vertrag,
                                bez
                            from mieter
                            where " + lsWhereAdd;
            }

            // MwstSatz holen Art ist bebekannt
            if (piArt == 8)
            {
                lsWhereAdd = "Id_mwst_art = " + piId.ToString() + " ";
                lsSql = @"select Id_mwst_art,
                                 bez,
                                 mwst
                            from art_mwst
                            where " + lsWhereAdd;
            }

            // MwstSatz holen Bezeichnung ist bekannt Bsp. "normal"
            if (piArt == 9)
            {
                lsWhereAdd = "bez = " + " \'" + ps2 + "\' ";
                lsSql = @"select Id_mwst_art,
                                 bez,
                                 mwst
                            from art_mwst
                            where " + lsWhereAdd;
            }

            // Zahlungen
            if (piArt == 11)
	        {
		        lsWhereAdd = "id_vz = " + piId.ToString() + " ";
                lsSql = @"select id_vz,
                                    id_mieter,
                                    id_objekt,
                                    id_objekt_teil,
                                    datum_von,
                                    datum_bis,
                                    betrag_netto,
                                    betrag_brutto, 
                                    betrag_netto_soll,
                                    betrag_brutto_soll, 
                                    id_extern_timeline,
                                    flag_timeline,
                                    id_ksa
                            from zahlungen where " + lsWhereAdd;
	        }

            // Zahlungen mit definierter Timeline
            if (piArt == 12)
            {
                lsWhereAdd = "id_extern_timeline = " + piId.ToString() + " ";
                lsSql = @"select id_vz,
                                    id_mieter,
                                    id_objekt,
                                    id_objekt_teil,
                                    datum_von,
                                    datum_bis,
                                    betrag_netto,
                                    betrag_brutto, 
                                    betrag_netto_soll,
                                    betrag_brutto_soll, 
                                    id_extern_timeline,
                                    flag_timeline,
                                    id_ksa,
                                    id_import
                            from zahlungen where " + lsWhereAdd;
            }
            // Zahlungen aus automatischem Import. Alle mit flag_timeline = 1 und der übergebenen Import ID
            if (piArt == 13)
            {
                lsWhereAdd = "id_import = " + piId.ToString() + " ";
                lsSql = @"select id_vz,
                                    id_mieter,
                                    id_objekt,
                                    id_objekt_teil,
                                    datum_von,
                                    datum_bis,
                                    betrag_netto,
                                    betrag_brutto, 
                                    betrag_netto_soll,
                                    betrag_brutto_soll, 
                                    id_extern_timeline,
                                    flag_timeline,
                                    id_ksa,
                                    id_import
                            from zahlungen where flag_timeline = 1 and " + lsWhereAdd;
            }

            // Aus der Rechnungs ID die untergeordneten Summen der Timeline ermitteln
            if (piArt == 14 || piArt == 15 || piArt == 16 || piArt == 17)
            {
                lsSql = @"Select Sum(timeline.betrag_netto) as betrag_netto,
						    Sum(timeline.betrag_brutto) as betrag_brutto,
							rechnungen.betrag_netto as rg_netto,
							rechnungen.betrag_brutto as rg_brutto,
							timeline.wtl_aus_objekt as wtl_obj,
                            timeline.wtl_aus_objteil as wtl_objt,
							timeline.id_rechnung,
							timeline.id_vorauszahlung,
							timeline.id_objekt,
							timeline.id_objekt_teil,
							timeline.id_mieter,
							rechnungen.Rg_nr,
							rechnungen.datum_rechnung as rgdat,
							rechnungen.firma as firma,
							art_kostenart.bez as kbez,
							art_kostenart.sort as sort,
                            timeline.id_ksa,
                            rechnungen.id_verteilung,
                            timeline.id_zaehlerstand
                        from timeline
						Left Join rechnungen on rechnungen.id_extern_timeline = timeline.id_rechnung
						Right Join art_kostenart on timeline.id_ksa = art_kostenart.id_ksa";
                lsGroup = @" Group by timeline.id_rechnung,timeline.id_vorauszahlung,timeline.id_objekt,
							timeline.id_objekt_teil,timeline.id_mieter,rechnungen.Rg_nr,art_kostenart.bez,
							rechnungen.betrag_netto,rechnungen.betrag_brutto,art_kostenart.sort,timeline.wtl_aus_objekt,
                            timeline.wtl_aus_objteil,rechnungen.datum_rechnung,rechnungen.firma,timeline.id_ksa,
                            rechnungen.id_verteilung,timeline.id_zaehlerstand ";
                lsOrder = " Order by art_kostenart.sort ";

                switch (piArt)
                {
                    case 14:
                        lsWhereAdd = " Where timeline.id_rechnung = " + piId.ToString() + " and timeline.id_objekt > 0 ";         // Objekte
                        break;
                    case 15:
                        lsWhereAdd = " Where timeline.id_rechnung = " + piId.ToString() + " and timeline.id_objekt_teil = " + piId2.ToString() ;    // Teilobjekte
                        break;
                    case 16:
                        lsWhereAdd = " Where timeline.id_zaehlerstand = " + piId.ToString() + " and timeline.id_objekt > 0 ";         // Objekte
                        break;
                    case 17:
                        lsWhereAdd = " Where timeline.id_zaehlerstand = " + piId.ToString() + " and timeline.id_objekt_teil = " + piId2.ToString();    // Teilobjekte
                        break;
                    default:
                        break;
                }
                lsSql = lsSql + lsWhereAdd + lsWhereAdd2;
                lsSql = lsSql + lsGroup + lsOrder;
            }

            // Zählerstände mit definierter Timeline
            if (piArt == 21)
            {
                lsWhereAdd = "id_extern_timeline = " + piId.ToString() + " ";
                lsSql = @"select Id_zs,               
                            id_zaehler,          
                            id_einheit,          
                            zs,           
                            datum_von,
                            verbrauch,
                            preis_einheit_netto,
                            preis_einheit_brutto,
                            id_extern_timeline,
                            id_objekt,
                            id_objekt_teil,
                            id_ksa
                        from zaehlerstaende where " + lsWhereAdd;
            }

            if (piArt == 24)            // Zählerinfo für Report Nebenkosten holen
            {
                lsWhereAdd = " Where id_extern_timeline = " + piId.ToString() + " ";
                lsSql = @"select Id_zs,               
                            zaehlerstaende.id_zaehler,          
                            zaehlerstaende.id_einheit,          
                            zaehlerstaende.zs,           
                            zaehlerstaende.datum_von,
                            zaehlerstaende.verbrauch,
                            zaehlerstaende.preis_einheit_netto,
                            zaehlerstaende.preis_einheit_brutto,
                            zaehlerstaende.id_extern_timeline,
                            zaehlerstaende.id_objekt,
                            zaehlerstaende.id_objekt_teil,
                            zaehlerstaende.id_ksa,
                            zaehler.zaehlernummer,
                            zaehler.zaehlerort,
							art_einheit.bez
                        from zaehlerstaende
                        left join zaehler on zaehler.Id_zaehler = zaehlerstaende.id_zaehler
                        left join art_einheit on zaehler.id_einheit = art_einheit.Id_einheit "
                    + lsWhereAdd;
            }

            if (piArt == 25)        // Zusammenstellungen der gewählten Wohnungen für den Report Nebenkosten
            {
                lsSql = @"select Id_obj_mix_parts,id_objekt_mix,id_objekt,id_objekt_teil,bez,sel,flaeche_anteil,    
                                id_timeline,ges_fl_behalten,erklaerung,geschoss,lage
                                    from objekt_mix_parts";
                lsWhereAdd = " where sel > 0 and id_timeline = " + piId.ToString() + " ";
                // lsWhereAdd2 = " and id_objekt = " + piId2.ToString() + " ";
                lsSql = lsSql + lsWhereAdd + lsWhereAdd2;
            }

            // Schnippsel
            // where " + ps2 + " = " + " \'" + lsWhereAdd + "\'";            

            return lsSql;
        }

        // Daten aus der Db holen
        public static Int32 fetchData(string psSql, string psSql2, int piArt, string asConnectString)
        {
            DateTime ldtStart = DateTime.MinValue;
            DateTime ldtEnd = DateTime.MinValue;
            DateTime ldtMonat = DateTime.MinValue;
            DateTime ldtVertrag = DateTime.MinValue;

            int liObjekt = 0;
            int liObjektTeil = 0;
            int liMieter = 0;
            int liKsa = 0; // Kostenstellenart
            int liMonths = 0; //Anzahl der einzutragenden Monate
            int liDaysStart = 0; // Anzahl der Tages Startmonats
            int liDaysEnd = 0; // Anzahl der Tages EndMonats
            int liDaysInMonth = 0; // Tage im Monat aus Vertrag
            int liSave = 1;  // Freigabe 

            decimal ldBetragNetto = 0;
            decimal ldBetragSollNetto = 0;
            decimal ldBetragBrutto = 0;
            decimal ldBetragSollBrutto = 0;
            decimal ldGesamtflaeche = 0;
            decimal ldZs = 0;            // Zählerstand
            decimal ldVerbrauch = 0;    // Zähler Verbrauch
            decimal[] ladBetraege = new decimal[12];

            Int32 liRows = 0;
            int zl = 0;
            int liZlgOrRg = 0;
            int liExternId = 0;
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

            string lsVerteilung = "";
            //string lsObjektBez = "", lsObjektTeilBez = "";
            //string lsObjektBezS = "";

            SqlConnection connect;
            connect = new SqlConnection(asConnectString);

            try
            {
                // Db open
                connect.Open();

                switch (piArt)
                {
                    case 1:     // Rechnungen > Timeline erzeugen bearbeiten

                        tableOne = new DataTable();         // Rechnung 
                        SqlCommand command = new SqlCommand(psSql, connect);
                        sda = new SqlDataAdapter(command);
                        sda.Fill(tableOne);

                        // Externe ID aus der Rechnung ermitteln 
                        for (int i = 0; tableOne.Rows.Count > i; i++)
                        {
                            if ( tableOne.Rows[i].ItemArray.GetValue(14) != DBNull.Value)
                            {
                                liExternId = (int)tableOne.Rows[i].ItemArray.GetValue(14);
                                // Timeline löschen
                                liOk = TimelineDelete(liExternId);          

                                // Objekt
                                if ( tableOne.Rows[i].ItemArray.GetValue(8) != DBNull.Value)
                                    if ((int)tableOne.Rows[i].ItemArray.GetValue(8) > 0)
                                    {
                                        liObjekt = (int)tableOne.Rows[i].ItemArray.GetValue(8);
                                        // Timeline neu erzeugen Objekte aus Rechnungen
                                        liOk = TimelineCreate(liExternId, "id_rechnung");

                                        // Weiterleitung an ObjektTeil aus der Kostenart ermitteln
                                        // 1 = Weiterleitung an Teilobjekt
                                        if (getWtl(1,liExternId))
                                        {
                                            liObjektTeil = 0;
                                            // Timeline neu erzeugen für Relationen
                                            liOk = TimelineCreateRelations(liExternId, liObjekt, liObjektTeil, liMieter);

                                            // 2 = Weiterleitung an Mieter
                                            if (getWtl(2, liExternId))
                                            {
                                                                   liObjekt = 0;
                                                liObjektTeil = 1;   // Auslöser für das Weiterleiten an Mieter
                                                // Timeline neu erzeugen für Relationen
                                                liOk = TimelineCreateRelations(liExternId, liObjekt, liObjektTeil, liMieter);
                                            }
                                        }
                                    }

                                // ObjektTeil
                                if ( tableOne.Rows[i].ItemArray.GetValue(9) != DBNull.Value)
                                    if ((int)tableOne.Rows[i].ItemArray.GetValue(9) > 0)
                                    {
                                        liObjektTeil = (int)tableOne.Rows[i].ItemArray.GetValue(9);
                                        // Timeline neu erzeugen Objektteile aus Rechnungen
                                        liOk = TimelineCreate(liExternId, "id_rechnung");
                                        // Weiterleitung an ObjektTeil aus der Kostenart ermitteln
                                        // 2 = Weiterleitung an Mieter
                                        if (getWtl(2, liExternId))
                                        {
                                            // Timeline neu erzeugen für Relationen
                                            liOk = TimelineCreateRelations(liExternId, liObjekt, liObjektTeil, liMieter);
                                        }
                                    }

                                // Mieter
                                if ( tableOne.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                                    if ((int)tableOne.Rows[i].ItemArray.GetValue(10) > 0)
                                    {
                                        liMieter = (int)tableOne.Rows[i].ItemArray.GetValue(10);
                                        // Timeline neu erzeugen Mieter aus Rechnungen
                                        // ACHTUNG hier Kontrolle einbauen, ob Mietvertrag gültig ist ULF
                                        liOk = TimelineCreate(liExternId, "id_rechnung");
                                    }
                            }
                            else
                            {
                               MessageBox.Show("Verarbeitungsfehler ERROR fetchdata fetchdata RdFunctions 0001\n piArt = " + piArt.ToString(),
                                        "Achtung");
                               break;   
                            }
                        }

                        break;

                    case 2:     // Timeline löschen
                        // Pass both strings to a new SqlCommand object.
                        SqlCommand command2 = new SqlCommand(psSql, connect);
                        SqlDataReader queryCommandReader = command2.ExecuteReader();
                        break;

                    case 3:     // Rechnungen Timeline Create
                        tableOne = new DataTable();         // Rechnungen
                        SqlCommand command4 = new SqlCommand(psSql2, connect);
                        sda = new SqlDataAdapter(command4);
                        sda.Fill(tableOne);

                        // Externe ID aus der Rechnung ermitteln 
                        for (int i = 0; tableOne.Rows.Count > i; i++)
                        {
                            if ( tableOne.Rows[i].ItemArray.GetValue(14) != DBNull.Value)
                            {
                                liExternId = (int)tableOne.Rows[i].ItemArray.GetValue(14);
                            }
                            else
                            {
                               MessageBox.Show("Verarbeitungsfehler ERROR fetchdata fetchdata RdFunctions 0002\n piArt = " + piArt.ToString(),
                                        "Achtung");
                               break;   
                            }
                        }

                        // Timeline neue Datensätze erzeugen
                        SqlCommand command3 = new SqlCommand(psSql, connect);
                        tableThree = new DataTable();
                        sdc = new SqlDataAdapter(command3);
                        sdc.Fill(tableThree);

                        for (int i = 0; tableOne.Rows.Count > i; i++)
                        {
                            if (tableOne.Rows[i].ItemArray.GetValue(14) != DBNull.Value)
                            {
                                liExternId = (int)tableOne.Rows[i].ItemArray.GetValue(14);
                                if (tableOne.Rows[i].ItemArray.GetValue(8) != DBNull.Value)
                                    liObjekt = (int)tableOne.Rows[i].ItemArray.GetValue(8);
                                if (tableOne.Rows[i].ItemArray.GetValue(9) != DBNull.Value)
                                    liObjektTeil = (int)tableOne.Rows[i].ItemArray.GetValue(9);
                                if (tableOne.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                                    liMieter = (int)tableOne.Rows[i].ItemArray.GetValue(10);
                                if (tableOne.Rows[i].ItemArray.GetValue(5) != DBNull.Value)
                                    ldBetragNetto = (decimal)tableOne.Rows[i].ItemArray.GetValue(5);
                                if (tableOne.Rows[i].ItemArray.GetValue(6) != DBNull.Value)
                                    ldBetragBrutto = (decimal)tableOne.Rows[i].ItemArray.GetValue(6);
                                if (tableOne.Rows[i].ItemArray.GetValue(3) != DBNull.Value)
                                    ldtStart = (DateTime)tableOne.Rows[i].ItemArray.GetValue(3);
                                if (tableOne.Rows[i].ItemArray.GetValue(4) != DBNull.Value)
                                    ldtEnd = (DateTime)tableOne.Rows[i].ItemArray.GetValue(4);
                                if (tableOne.Rows[i].ItemArray.GetValue(1) != DBNull.Value)
                                    liKsa = (int)tableOne.Rows[i].ItemArray.GetValue(1);

                                zl = 1;         // Anzahl der Monate = Anzahl der Datensätze in Timeline

                                // Anzahl der Tage des ersten Monats        99 ist der volle Monat
                                liDaysStart = getDaysStart(ldtStart);
                                // Anzahl der Tage des letzten Monats       99 ist der volle Monat
                                liDaysEnd = getDaysEnd(ldtEnd);
                                // Anzahl der einzutragenden Monat ermitteln
                                liMonths = getMonths(ldtStart, ldtEnd);
                                // Zahlung oder Rechnung 1= Zahlung 2= Rechnung
                                liZlgOrRg = 2;
                                // Monatsbeträge ermitteln (Brutto und Netto) und evtl. erster und letzter Monat nicht voll
                                ladBetraege = getBetraege(liMonths, liDaysStart, liDaysEnd, 
                                        ldBetragNetto, ldBetragBrutto, 
                                        ldBetragSollNetto, ldBetragSollBrutto, liZlgOrRg, ldtStart, ldtEnd);                                
                                // Den ersten Monat ermitteln
                                string dt = (ldtStart.Year.ToString()) + "-" + ldtStart.Month.ToString() + "-01";
                                ldtMonat = DateTime.Parse(dt);                 // Datetime mit erstem Tag des Monats

                                do
                                {
                                    DataRow dr = tableThree.NewRow();

                                    dr[1] = liExternId;
                                    dr[4] = liObjekt;
                                    dr[5] = liObjektTeil;
                                    dr[6] = liMieter;
                                    dr[7] = liKsa;
                                    //---------------------------------------------
                                    if (liDaysStart != 99 && zl == 1)
                                    {
                                        dr[8] = ladBetraege[5];         // Netto erster Monat bei späterem Beginn
                                        dr[10] = ladBetraege[6];         // Brutto
                                    }
                                    //---------------------------------------------
                                    else if (liDaysEnd != 99 && zl == liMonths)
                                    {
                                        dr[8] = ladBetraege[9];         // Netto letzter Monat bei früherem Ende
                                        dr[10] = ladBetraege[10];         // Brutto
                                    }
                                    else
                                    {
                                        dr[8] = ladBetraege[1];
                                        dr[10] = ladBetraege[2];
                                    }
                                    //---------------------------------------------
                                    dr[9] = ladBetraege[3];
                                    dr[11] = ladBetraege[4];
                                    dr[12] = ldZs;                  // Zählerstand
                                    if (zl == 1)                    // erster Monat
                                        dr[13] = ldtStart;
                                    else if (zl == liMonths)        // letzter Monat
                                        dr[13] = ldtEnd;
                                    else
                                        dr[13] = ldtMonat;      // Der Timelinemonat

                                    tableThree.Rows.Add(dr);
                                    // + Monat 
                                    ldtMonat = ldtMonat.AddMonths(1);
                                    // + Zähler
                                    zl++;
                                    
                                } while (zl <= liMonths);

                                // und alles ab in die Datenbank
                                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(sdc);
                                sdc.UpdateCommand = commandBuilder.GetUpdateCommand();
                                sdc.InsertCommand = commandBuilder.GetInsertCommand();

                                sdc.Update(tableThree);
                            }
                            else
                            {
                                MessageBox.Show("Verarbeitungsfehler ERROR fetchdata fetchdata RdFunctions 0003\n piArt = " + piArt.ToString(),
                                         "Achtung");
                                break;
                            }
                        }

                        break;

                    case 4:     // Rechnungen Timeline Create Relations Objektteile schreiben

                        // tableFive beiinhaltet die Objektteile zu einem gewählten Objekt
                        SqlCommand command6 = new SqlCommand(psSql2, connect);
                        tableFive = new DataTable();
                        sde = new SqlDataAdapter(command6);
                        sde.Fill(tableFive);
                        // tableFive ist jetzt mit allen Objektteilen zum Objekt gefüllt

                        // tableSix: Holen der Timeline
                        SqlCommand command5 = new SqlCommand(psSql, connect);
                        tableSix = new DataTable();
                        sdf = new SqlDataAdapter(command5);
                        sdf.Fill(tableSix);

                        // tableFour Timeline schreiben
                        SqlCommand command7 = new SqlCommand(psSql, connect);
                        tableFour = new DataTable();
                        sdc = new SqlDataAdapter(command7);
                        sdc.Fill(tableFour);

                        // Schleife durch Timeline
                        // Jeder Datensatz muss hier auch für jeden Objektteil einen Datensatz erzeugen
                        // Die Beträge werden nach der Flächenaufteilung eingetragen
                        // Aufteilung nach Personen kann hier nicht gemacht werden. 
                        // Geschieht erst beim Verteilen auf die Mieter

                        // Timeline
                        for (int i = 0; tableSix.Rows.Count > i; i++)
                        {
                            if (tableSix.Rows[i].ItemArray.GetValue(1) != DBNull.Value)
                            {
                                liRechnungId = (int)tableSix.Rows[i].ItemArray.GetValue(1);
                                if (tableSix.Rows[i].ItemArray.GetValue(4) != DBNull.Value)
                                    liObjekt = (int)tableSix.Rows[i].ItemArray.GetValue(4);
                                if (tableSix.Rows[i].ItemArray.GetValue(5) != DBNull.Value)
                                    liObjektTeil = (int)tableSix.Rows[i].ItemArray.GetValue(5);
                                if (tableSix.Rows[i].ItemArray.GetValue(6) != DBNull.Value)
                                    liMieter = (int)tableSix.Rows[i].ItemArray.GetValue(6);
                                if (tableSix.Rows[i].ItemArray.GetValue(7) != DBNull.Value)
                                    liKsa = (int)tableSix.Rows[i].ItemArray.GetValue(7);
                                if (tableSix.Rows[i].ItemArray.GetValue(8) != DBNull.Value)
                                    ldBetragNetto = (decimal)tableSix.Rows[i].ItemArray.GetValue(8);
                                if (tableSix.Rows[i].ItemArray.GetValue(9) != DBNull.Value)
                                    ldBetragSollNetto = (decimal)tableSix.Rows[i].ItemArray.GetValue(9);
                                if (tableSix.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                                    ldBetragBrutto = (decimal)tableSix.Rows[i].ItemArray.GetValue(10);
                                if (tableSix.Rows[i].ItemArray.GetValue(11) != DBNull.Value)
                                    ldBetragSollBrutto = (decimal)tableSix.Rows[i].ItemArray.GetValue(11);
                                if (tableSix.Rows[i].ItemArray.GetValue(12) != DBNull.Value)
                                    ldZs = (decimal)tableSix.Rows[i].ItemArray.GetValue(12);
                                if (tableSix.Rows[i].ItemArray.GetValue(13) != DBNull.Value)
                                    ldtMonat = (DateTime)tableSix.Rows[i].ItemArray.GetValue(13);
                                if (tableSix.Rows[i].ItemArray.GetValue(17) != DBNull.Value)
                                    liImportId = (int)tableSix.Rows[i].ItemArray.GetValue(17);

                                // Ermitteln der VerteilungsId aus Tabelle rechnungen
                                // Achtung nbüschen gepfuscht liRechnungId ist die externTimeline Id
                                liVerteilungId = getVerteilungsId(asConnectString, liRechnungId);
                                // Ermitteln, wie verteilt werden soll aus der Tabelle art_verteilung
                                lsVerteilung = getVerteilung(asConnectString, liVerteilungId);

                                // Alle Objektteile zu dem Objekt
                                for (int ii = 0; tableFive.Rows.Count > ii; ii++)
                                {
                                    // Timeline schreiben
                                    DataRow dr = tableFour.NewRow();

                                    dr[1] = liRechnungId;
                                    // dr[4] = liObjekt; nicht eintragen
                                    if (tableFive.Rows[ii].ItemArray.GetValue(0) != DBNull.Value)
                                    {
                                        dr[5] = (int)tableFive.Rows[ii].ItemArray.GetValue(0);   // id ObjektTeil
                                        liObjektTeil = (int)tableFive.Rows[ii].ItemArray.GetValue(0);
                                        dr[6] = liMieter;
                                        dr[7] = liKsa;

                                        // Flächenanteil rechnen
                                        if (lsVerteilung == "fl")
                                        {
                                            if (tableFive.Rows[ii].ItemArray.GetValue(6) != DBNull.Value)
                                            {
                                                if ((decimal)tableFive.Rows[ii].ItemArray.GetValue(6) > 0)
                                                {
                                                    // Gesamtfläche aus Tabelle Objekt holen
                                                    if (liObjekt > 0)
                                                    {
                                                        ldGesamtflaeche = getObjektflaeche(liObjekt, 0, 0, asConnectString);
                                                        dr[8] = ldBetragNetto / (ldGesamtflaeche / (decimal)tableFive.Rows[ii].ItemArray.GetValue(6));          // Netto    
                                                        dr[10] = ldBetragBrutto / (ldGesamtflaeche / (decimal)tableFive.Rows[ii].ItemArray.GetValue(6));         // Brutto                                                                                                                                                        
                                                    }
                                                }
                                                else
                                                {
                                                    liSave = 0;
                                                }
                                            }
                                        }
                                        // Prozentanteil rechnen
                                        // if (tableFive.Rows[ii].ItemArray.GetValue(7) != DBNull.Value)
                                        if (lsVerteilung == "pz")
                                        {
                                            if (tableFive.Rows[ii].ItemArray.GetValue(7) != DBNull.Value)
                                            {
                                                if ((decimal)tableFive.Rows[ii].ItemArray.GetValue(7) > 0)
                                                {
                                                    dr[8] = (ldBetragNetto / 100) * (decimal)tableFive.Rows[ii].ItemArray.GetValue(7);           // Netto    
                                                    dr[10] = (ldBetragBrutto / 100) * (decimal)tableFive.Rows[ii].ItemArray.GetValue(7);         // Brutto                                                		 
                                                }
                                                else
                                                {
                                                    liSave = 0;
                                                }
                                            }
                                        }

                                        // Personenanzahl für den aktuellen Monat berechnen
                                        //if (tableFive.Rows[ii].ItemArray.GetValue(8) != DBNull.Value)
                                        if (lsVerteilung == "ps")
                                        {
                                            // Anzahl der Personen in einem Objekt ermitteln
                                            // Information aus aktiven Verträgen
                                            // liAnzPersonenObj = getAktPersonen(liObjekt, ldtMonat, 0);
                                            // liAnzPersonenObt = getAktPersonen(0, ldtMonat, liObjektTeil);

                                            if (tableFive.Rows[ii].ItemArray.GetValue(8) != DBNull.Value)
                                            {
                                                if ((int)tableFive.Rows[ii].ItemArray.GetValue(8) > 0)
                                                {
                                                    // Anzahl der Personen in einem Objekt ermitteln
                                                    // Aktive Verträge
                                                    liAnzPersonenObj = Convert.ToInt32(getAktPersonen(liObjekt, 0, 0, ldtMonat.ToString(), ldtMonat.ToString(), 0, asConnectString));
                                                    // Anzahl der Personen in einem Objektteil ermitteln
                                                    liAnzPersonenObt = Convert.ToInt32(getAktPersonen(0, liObjektTeil, 0, ldtMonat.ToString(), ldtMonat.ToString(), 0, asConnectString));

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
                                        }

                                        // Direkte Verteilung 1:1 weiterleiten   31.5.2018
                                        if (lsVerteilung == "di")
                                        {
                                                dr[8] = ldBetragNetto;          // Netto    
                                                dr[10] = ldBetragBrutto;        // Brutto                                                		 
                                        }

                                        // Nix wird verteilt                    31.5.2018
                                        if (lsVerteilung == "nl")
                                        {
                                                // dr[8] = 0;          // Netto    
                                                // dr[10] = 0;        // Brutto
                                                liSave = 0;    		 
                                        }

                                        // Zähleranteil ermitteln 
                                        if (lsVerteilung == "zl")
                                        {
                                            // Zähler werden immer direkt auf die Wohnung bzw den Mieter gebucht  
                                            liSave = 0;
                                        }

                                        dr[12] = ldZs;                  // Zählerstand
                                        dr[13] = ldtMonat;              // Der Timelinemonat

                                        // Verteilung Bedingt mit Anwahl für gewünschte Wohnungen
                                        // Die Gesamtfläche für die Auswahl wird ermittelt
                                        if (lsVerteilung == "fa")
                                        {
                                            if ((decimal)tableFive.Rows[ii].ItemArray.GetValue(6) > 0)
                                            {
                                                // Gesamtfläche der ausgewählten Wohnungen aus Tabelle Objekt_mix_parts holen
                                                if (liObjekt > 0)
                                                {
                                                    int liArt = 0;
                                                    // Gesamtfläche der Auswahl = 0 oder Gesamtfläche = 1
                                                    liArt = getObjektflaecheAuswFlag(liObjekt, asConnectString);
                                                    ldGesamtflaeche = getObjektflaecheAuswahl(liObjekt, liRechnungId ,asConnectString, liArt);  // RechnungsId ist Timeline ID
                                                    if (getObjektTeilAuswahl((int)tableFive.Rows[ii].ItemArray.GetValue(0)) > 0)
                                                    {
                                                        // decimal ldtest = ldBetragNetto / (ldGesamtflaeche / (decimal)tableFive.Rows[ii].ItemArray.GetValue(6)); 
                                                        dr[8] = ldBetragNetto / (ldGesamtflaeche / (decimal)tableFive.Rows[ii].ItemArray.GetValue(6));          // Netto    
                                                        dr[10] = ldBetragBrutto / (ldGesamtflaeche / (decimal)tableFive.Rows[ii].ItemArray.GetValue(6));         // Brutto                                                                                                                                                    
                                                    }
                                                    else
                                                    {
                                                        dr[8] = 0;
                                                        dr[10] = 0;
                                                        liSave = 0;     // nur in diesem Fall Datensatz verwerfen
                                                    }                                                    
                                                }
                                            }
                                        }

                                        // Kennzeichnen der Timeline, ob es eine Weiterleitung vom Objekt ist
                                        if (liObjekt > 0)
                                        {
                                            dr[14] = 1;
                                        }
                                        // Kennzeichnen der Timeline, ob es eine Weiterleitung vom ObjektTeil ist
                                        if (liObjektTeil > 0)
                                        {
                                            dr[15] = 1;
                                        }
                                        // Import ID schreiben
                                        dr[17] = liImportId;
                                    }
                                    if (liSave == 1)
                                    {
                                        tableFour.Rows.Add(dr);                                        
                                    }

                                    liSave = 1;
                                }

                                // und alle TimelineEinträge ab in die Datenbank
                                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(sdc);
                                sdc.UpdateCommand = commandBuilder.GetUpdateCommand();
                                sdc.InsertCommand = commandBuilder.GetInsertCommand();

                                sdc.Update(tableFour);
                            }
                            else
                            {
                                MessageBox.Show("Verarbeitungsfehler ERROR fetchdata fetchdata RdFunctions 0004\n piArt = " + piArt.ToString(),
                                         "Achtung");
                                break;
                            }
                        }
                        break;

                    case 5:     // Rechnungen Timeline Create Relations Mieter schreiben

                        // Vorhandene Timeline einlesen
                        SqlCommand command9 = new SqlCommand(psSql, connect);
                        tableEight = new DataTable();
                        sdh = new SqlDataAdapter(command9);
                        sdh.Fill(tableEight);

                        // Timeline neue Datensätze erzeugen
                        SqlCommand command8 = new SqlCommand(psSql, connect);
                        tableThree = new DataTable();
                        sdc = new SqlDataAdapter(command8);
                        sdc.Fill(tableThree);

                        // Schleife durch Timeline
                        // Jeder Datensatz muss hier einen Datensatz für den Mieter erzeugen
                        for (int i = 0; tableEight.Rows.Count > i; i++)
                        {
                            liSave = 1;
                            if (tableEight.Rows[i].ItemArray.GetValue(1) != DBNull.Value || tableEight.Rows[i].ItemArray.GetValue(2) != DBNull.Value || tableEight.Rows[i].ItemArray.GetValue(3) != DBNull.Value)
                            {
                                // Rechnung
                                if (tableEight.Rows[i].ItemArray.GetValue(1) != DBNull.Value)
                                {
                                    liRechnungId = (int)tableEight.Rows[i].ItemArray.GetValue(1);    
                                }
                                // Zahlung
                                if (tableEight.Rows[i].ItemArray.GetValue(2) != DBNull.Value)
                                {
                                    liZahlungId = (int)tableEight.Rows[i].ItemArray.GetValue(2);
                                }
                                // Zählerstand
                                if (tableEight.Rows[i].ItemArray.GetValue(3) != DBNull.Value)
                                {
                                    liZaehlerstandId = (int)tableEight.Rows[i].ItemArray.GetValue(3);
                                }

                                if (tableEight.Rows[i].ItemArray.GetValue(4) != DBNull.Value)
                                    liObjekt = (int)tableEight.Rows[i].ItemArray.GetValue(4);
                                if (tableEight.Rows[i].ItemArray.GetValue(5) != DBNull.Value)
                                    liObjektTeil = (int)tableEight.Rows[i].ItemArray.GetValue(5);
                                if (tableEight.Rows[i].ItemArray.GetValue(7) != DBNull.Value)
                                    liKsa = (int)tableEight.Rows[i].ItemArray.GetValue(7);
                                if (tableEight.Rows[i].ItemArray.GetValue(8) != DBNull.Value)
                                    ldBetragNetto = (decimal)tableEight.Rows[i].ItemArray.GetValue(8);
                                if (tableEight.Rows[i].ItemArray.GetValue(9) != DBNull.Value)
                                    ldBetragSollNetto = (decimal)tableEight.Rows[i].ItemArray.GetValue(9);
                                if (tableEight.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                                    ldBetragBrutto = (decimal)tableEight.Rows[i].ItemArray.GetValue(10);
                                if (tableEight.Rows[i].ItemArray.GetValue(11) != DBNull.Value)
                                    ldBetragSollBrutto = (decimal)tableEight.Rows[i].ItemArray.GetValue(11);
                                if (tableEight.Rows[i].ItemArray.GetValue(12) != DBNull.Value)
                                    ldZs = (decimal)tableEight.Rows[i].ItemArray.GetValue(12);
                                if (tableEight.Rows[i].ItemArray.GetValue(13) != DBNull.Value)
                                    ldtMonat = (DateTime)tableEight.Rows[i].ItemArray.GetValue(13);
                                if (tableEight.Rows[i].ItemArray.GetValue(17) != DBNull.Value)
                                    liImportId = (int)tableEight.Rows[i].ItemArray.GetValue(17);

                                DataRow dr = tableThree.NewRow();

                                dr[1] = liRechnungId;
                                dr[2] = liZahlungId;
                                dr[3] = liZaehlerstandId;
                                // dr[4] = liObjekt; nicht eintragen
                                // dr[5] = liObjektTeil; nicht eintragen

                                // Aktuellen Mieter ermitteln / Ohne Aktivkennzeichen!
                                // Gibt es am Monatsende einen zweiten Mieter muss das hier durch eine 2.te Funtion ermittelt 
                                // werden TODO ULF!
                                liMieter = getAktMieter(liObjektTeil,ldtMonat, asConnectString);

                                // Mieter gefunden
                                if (liMieter > 0)
                                {
                                    ldtVertrag = DateTime.MinValue;
                                    liDaysStart = 0;
                                    liDaysEnd = 0;

                                    // Beginnt der Vertrag in diesem Monat?
                                    ldtVertrag = getVertragInfo(1, ldtMonat, liMieter, gsConnectString);

                                    // Tageszahl von Monatsbeginn an ermitteln
                                    if (ldtVertrag > DateTime.MinValue)
                                    {
                                        liDaysStart = ldtVertrag.Day;
                                        liDaysInMonth = System.DateTime.DaysInMonth(ldtVertrag.Year, ldtVertrag.Month);
                                        liDaysInMonth = liDaysInMonth - liDaysStart;
                                        ldBetragNetto = (ldBetragNetto / liDaysInMonth) * liDaysInMonth;
                                        ldBetragBrutto = (ldBetragBrutto / liDaysInMonth) * liDaysInMonth;
                                    }

                                    // Endet der Vetrag in diesem Monat?
                                    ldtVertrag = getVertragInfo(2, ldtMonat, liMieter, gsConnectString);

                                    // Tageszahl zum Monatsende ermitteln
                                    if (ldtVertrag > DateTime.MinValue)
                                    {
                                        liDaysStart = ldtVertrag.Day;
                                        liDaysInMonth = System.DateTime.DaysInMonth(ldtVertrag.Year, ldtVertrag.Month);
                                        ldBetragNetto = (ldBetragNetto / liDaysInMonth) * liDaysStart;
                                        ldBetragBrutto = (ldBetragBrutto / liDaysInMonth) * liDaysStart;
                                    }

                                    dr[6] = liMieter;
                                }
                                else // sonst auf Leerstand buchen
                                {
                                    // dr[4] = liObjekt; nicht eintragen
                                    // dr[5] = liObjektTeil; nicht eintragen
                                    // Mieter für Leerstand ermiteln und eintragen
                                    // ObjektTeil ist vorhanden 
                                    liMieter = getMieterLeerstand(liObjektTeil,asConnectString);
                                    if (liMieter > 0)
                                    {
                                        dr[6] = liMieter;       // Mieter Leerstand existiert und wird genutzt
                                    }
                                    dr[16] = liObjektTeil;         // Auf Leerstand wird die TeilObjekt ID geschrieben
                                    
                                }
                                dr[7] = liKsa;
                                if (ldBetragNetto > 0)
                                {
                                    dr[8] = ldBetragNetto;          // Netto                                        
                                }
                                else
                                {
                                    liSave = 0;
                                }
                                if (ldBetragBrutto > 0)
                                {
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
                                // Kennzeichnen der Timeline, ob es eine Weiterleitung vom ObjektTeil ist
                                if (liObjektTeil > 0)
                                {
                                    dr[15] = 1;
                                }
                                // Import ID schreiben
                                dr[17] = liImportId;

                                if (liSave == 1)
                                {
                                    tableThree.Rows.Add(dr);            // Timeline                                     
                                }
                                liSave = 1;

                                // und alle TimelineEinträge ab in die Datenbank
                                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(sdc);
                                sdc.UpdateCommand = commandBuilder.GetUpdateCommand();
                                sdc.InsertCommand = commandBuilder.GetInsertCommand();

                                sdc.Update(tableThree);
                            }
                            else
                            {
                                MessageBox.Show("Verarbeitungsfehler ERROR fetchdata fetchdata RdFunctions 0005\n piArt = " + piArt.ToString(),
                                         "Achtung");
                                break;
                            }
                        }
                        break;
                    case 8:     // Mwst Satz holen

                        SqlCommand command10 = new SqlCommand(psSql, connect);
                        sdg = new SqlDataAdapter(command10);
                        tableSeven = new DataTable();
                        sdg.Fill(tableSeven);

                        if (tableSeven.Rows.Count > 0)
                        {
                            if (tableSeven.Rows[0].ItemArray.GetValue(2) != DBNull.Value)
	                        {
                                // Hier wird liRows ausnahmsweise mit dem Mwst-Satz belegt
                                decimal ldMwst = (decimal)tableSeven.Rows[0].ItemArray.GetValue(2);
                                liRows = (int)ldMwst;
	                        }
                        }

                        break;

                    case 11:    // Zahlungen > Timeline erzeugen bearbeiten
                        tableZlg = new DataTable();         // Zahlungen
                        SqlCommand command11 = new SqlCommand(psSql, connect);
                        sdZlg = new SqlDataAdapter(command11);
                        sdZlg.Fill(tableZlg);

                        // Externe ID aus der Zahlung ermitteln 
                        for (int i = 0; tableZlg.Rows.Count > i; i++)
                        {
                            if ( tableZlg.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                            {
                                liExternId = (int)tableZlg.Rows[i].ItemArray.GetValue(10);
                                // Timeline löschen
                                liOk = TimelineDelete(liExternId);          

                                // Objekt
                                if ( tableZlg.Rows[i].ItemArray.GetValue(2) != DBNull.Value)
                                    if ((int)tableZlg.Rows[i].ItemArray.GetValue(2) > 0)
                                    {
                                        liObjekt = (int)tableZlg.Rows[i].ItemArray.GetValue(2);
                                        // Timeline neu erzeugen Objekte aus Rechnungen
                                        liOk = TimelineCreate(liExternId, "id_vorauszahlung");
                                    }

                                // ObjektTeil
                                if ( tableZlg.Rows[i].ItemArray.GetValue(3) != DBNull.Value)
                                    if ((int)tableZlg.Rows[i].ItemArray.GetValue(3) > 0)
                                    {
                                        liObjektTeil = (int)tableZlg.Rows[i].ItemArray.GetValue(3);
                                        ldtMonat = Convert.ToDateTime( tableZlg.Rows[i].ItemArray.GetValue(4));
                                        // Timeline neu erzeugen Objektteile aus Rechnungen
                                        liOk = TimelineCreate(liExternId, "id_vorauszahlung");

                                        // Weiterleitung an aktiven Mieter
                                        liMieter = 0;

                                        liMieter = getAktMieter(liObjektTeil, ldtMonat, asConnectString);
                                        
                                        if (liMieter > 0)
                                        {
                                            // Timeline neu erzeugen für Relationen
                                            liOk = TimelineCreateRelations(liExternId, liObjekt, liObjektTeil, liMieter);
                                        }
                                    }

                                // Mieter
                                if ( tableZlg.Rows[i].ItemArray.GetValue(1) != DBNull.Value)
                                    if ((int)tableZlg.Rows[i].ItemArray.GetValue(1) > 0)
                                    {
                                        liMieter = (int)tableZlg.Rows[i].ItemArray.GetValue(1);
                                        // Timeline neu erzeugen Mieter aus Zahlungen
                                        // ACHTUNG hier Kontrolle einbauen, ob Mietvertrag gültig ist ULF TODO !
                                        liOk = TimelineCreate(liExternId, "id_vorauszahlung");
                                    }
                            }
                            else
                            {
                               MessageBox.Show("Verarbeitungsfehler ERROR fetchdata RdFunctions fetchdata\n piArt = " + piArt.ToString(),
                                        "Achtung");
                               break;   
                            }
                        }

                        break;

                    case 13:        // Zahlungen Timeline neu erzeugen
                        tableZlgNew = new DataTable();         // Zahlungen
                        SqlCommand command13 = new SqlCommand(psSql2, connect);
                        sdZlgNew = new SqlDataAdapter(command13);
                        sdZlgNew.Fill(tableZlgNew);

                        // Externe ID aus der Zahlung ermitteln 
                        for (int i = 0; tableZlgNew.Rows.Count > i; i++)
                        {
                            if ( tableZlgNew.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                            {
                                liExternId = (int)tableZlgNew.Rows[i].ItemArray.GetValue(10);
                            }
                            else
                            {
                               MessageBox.Show("Verarbeitungsfehler ERROR fetchdata RdFunctions 0002\n piArt = " + piArt.ToString(),
                                        "Achtung");
                               break;   
                            }
                        }

                        // Timeline neue Datensätze erzeugen
                        SqlCommand command131 = new SqlCommand(psSql, connect);
                        tableTml = new DataTable();
                        sdTml = new SqlDataAdapter(command131);
                        sdTml.Fill(tableTml);

                        for (int i = 0; tableZlgNew.Rows.Count > i; i++)
                        {
                            if (tableZlgNew.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                            {
                                liExternId = (int)tableZlgNew.Rows[i].ItemArray.GetValue(10);
                                if (tableZlgNew.Rows[i].ItemArray.GetValue(1) != DBNull.Value)
                                    liMieter = (int)tableZlgNew.Rows[i].ItemArray.GetValue(1);
                                if (tableZlgNew.Rows[i].ItemArray.GetValue(2) != DBNull.Value)
                                    liObjekt = (int)tableZlgNew.Rows[i].ItemArray.GetValue(2);
                                if (tableZlgNew.Rows[i].ItemArray.GetValue(3) != DBNull.Value)
                                    liObjektTeil = (int)tableZlgNew.Rows[i].ItemArray.GetValue(3);
                                if (tableZlgNew.Rows[i].ItemArray.GetValue(4) != DBNull.Value)
                                    ldtStart = (DateTime)tableZlgNew.Rows[i].ItemArray.GetValue(4);
                                if (tableZlgNew.Rows[i].ItemArray.GetValue(5) != DBNull.Value)
                                    ldtEnd = (DateTime)tableZlgNew.Rows[i].ItemArray.GetValue(5);
                                if (tableZlgNew.Rows[i].ItemArray.GetValue(6) != DBNull.Value)
                                    ldBetragNetto = (decimal)tableZlgNew.Rows[i].ItemArray.GetValue(6);
                                if (tableZlgNew.Rows[i].ItemArray.GetValue(7) != DBNull.Value)
                                    ldBetragBrutto = (decimal)tableZlgNew.Rows[i].ItemArray.GetValue(7);
                                if (tableZlgNew.Rows[i].ItemArray.GetValue(8) != DBNull.Value)
                                    ldBetragSollNetto = (decimal)tableZlgNew.Rows[i].ItemArray.GetValue(8);
                                if (tableZlgNew.Rows[i].ItemArray.GetValue(9) != DBNull.Value)
                                    ldBetragSollBrutto = (decimal)tableZlgNew.Rows[i].ItemArray.GetValue(9);
                                if (tableZlgNew.Rows[i].ItemArray.GetValue(11) != DBNull.Value)
                                    liFlTml = (int)tableZlgNew.Rows[i].ItemArray.GetValue(11);
                                if (tableZlgNew.Rows[i].ItemArray.GetValue(12) != DBNull.Value)
                                    liKsa = (int)tableZlgNew.Rows[i].ItemArray.GetValue(12);
                                if (tableZlgNew.Rows[i].ItemArray.GetValue(13) != DBNull.Value)
                                    liImportId = (int)tableZlgNew.Rows[i].ItemArray.GetValue(13);

                                zl = 1;         // Anzahl der Monate = Anzahl der Datensätze in Timeline

                                // Den erstenTag des Monats einsetzen
                                string dt = (ldtStart.Year.ToString()) + "-" + ldtStart.Month.ToString() + "-01";
                                ldtMonat = DateTime.Parse(dt);                 // Datetime mit erstem Tag des Monats

                                do
                                {
                                    DataRow dr = tableTml.NewRow();

                                    dr[2] = liExternId;     
                                    dr[4] = liObjekt;
                                    dr[5] = liObjektTeil;
                                    dr[6] = liMieter;
                                    dr[7] = liKsa;
                                    dr[8] = ldBetragNetto * -1;             // Alles * -1 wegen Zahlungen
                                    dr[9] = ldBetragSollNetto  * -1;
                                    dr[10] = ldBetragBrutto * -1;
                                    dr[11] = ldBetragSollBrutto * -1;
                                    dr[12] = ldZs;                          // Zählerstand
                                    dr[13] = ldtStart;
                                    dr[17] = liImportId;

                                    tableTml.Rows.Add(dr);
                                    // + Monat 
                                    ldtMonat = ldtMonat.AddMonths(1);
                                    // + Zähler
                                    zl++;
                                    
                                } while (zl <= liMonths);

                                // und alles ab in die Datenbank
                                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(sdTml);
                                sdTml.UpdateCommand = commandBuilder.GetUpdateCommand();
                                sdTml.InsertCommand = commandBuilder.GetInsertCommand();

                                sdTml.Update(tableTml);
                            }
                            else
                            {
                                MessageBox.Show("Verarbeitungsfehler ERROR fetchdata RdFunctions 0003\n piArt = " + piArt.ToString(),
                                         "Achtung");
                                break;
                            }
                        }
                        break;
                    case 14:        // Summen aus Objekt für Report Content
                        SqlCommand command132 = new SqlCommand(psSql, connect);
                        tableConSumObj = new DataTable();
                        sdConSumObj = new SqlDataAdapter(command132);
                        sdConSumObj.Fill(tableConSumObj);
                        break;
                    case 15:        // Summen aus ObjektTeil für Report Content
                        SqlCommand command133 = new SqlCommand(psSql, connect);
                        tableConSumObjT = new DataTable();
                        sdConSumObjT = new SqlDataAdapter(command133);
                        sdConSumObjT.Fill(tableConSumObjT);
                        break;
                    case 16:        // Die Rechnungs Id aus der Timeline ermitteln
                        SqlCommand command134 = new SqlCommand(psSql, connect);
                        tableRgId = new DataTable();
                        sdRgId = new SqlDataAdapter(command134);
                        sdRgId.Fill(tableRgId);

                        if (tableRgId.Rows.Count >= 0)
                        {
                            if (tableRgId.Rows[0].ItemArray.GetValue(6) != DBNull.Value)
                            {
                                liRgId = (int)tableRgId.Rows[0].ItemArray.GetValue(6);
                            }
                            else
                            {
                                liRgId = 0;
                            }
                        }
                        break;
                    case 21:                               // Zählerstände
                        tableCnt = new DataTable();         
                        SqlCommand command21 = new SqlCommand(psSql, connect);
                        sdCnt = new SqlDataAdapter(command21);
                        sdCnt.Fill(tableCnt);

                        // Externe ID aus der Zählerstand ermitteln 
                        for (int i = 0; tableCnt.Rows.Count > i; i++)
                        {
                            if ( tableCnt.Rows[i].ItemArray.GetValue(8) != DBNull.Value)
                            {
                                liExternId = (int)tableCnt.Rows[i].ItemArray.GetValue(8);
                                // Timeline löschen
                                liOk = TimelineDelete(liExternId);          

                                // Objekt
                                if ( tableCnt.Rows[i].ItemArray.GetValue(9) != DBNull.Value)
                                    if ((int)tableCnt.Rows[i].ItemArray.GetValue(9) > 0)
                                    {
                                        liObjekt = (int)tableCnt.Rows[i].ItemArray.GetValue(9);
                                        // Timeline neu erzeugen Objekte aus Zählerständen
                                        liOk = TimelineCreate(liExternId, "id_zaehlerstand");
                                    }

                                // ObjektTeil
                                if ( tableCnt.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                                    if ((int)tableCnt.Rows[i].ItemArray.GetValue(10) > 0)
                                    {
                                        liObjektTeil = (int)tableCnt.Rows[i].ItemArray.GetValue(10);
                                        ldtMonat = Convert.ToDateTime( tableCnt.Rows[i].ItemArray.GetValue(4));
                                        // Timeline neu erzeugen Objektteile aus Zählerständen
                                        liOk = TimelineCreate(liExternId, "id_zaehlerstand");

                                        // Weiterleitung an aktiven Mieter
                                        liMieter = getAktMieter(liObjektTeil, ldtMonat, asConnectString); 
                                        
                                        if (liMieter > 0)
                                        {
                                            // Timeline neu erzeugen für Relationen
                                            liOk = TimelineCreateRelations(liExternId, liObjekt, liObjektTeil, liMieter);
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
                               MessageBox.Show("Verarbeitungsfehler ERROR fetchdata RdFunctions fetchdata\n piArt = " + piArt.ToString(),
                                        "Achtung");
                               break;   
                            }
                        }

                        break;
                    case 23:        // Zählerstände Timeline Create

                        tableCntNew = new DataTable();         // Zahlungen
                        SqlCommand command23 = new SqlCommand(psSql2, connect);
                        sdCntNew = new SqlDataAdapter(command23);
                        sdCntNew.Fill(tableCntNew);

                         // Timeline neue Datensätze erzeugen
                        SqlCommand command231 = new SqlCommand(psSql, connect);
                        tableTml = new DataTable();
                        sdTml = new SqlDataAdapter(command231);
                        sdTml.Fill(tableTml);

                        for (int i = 0; tableCntNew.Rows.Count > i; i++)
                        {
                            if (tableCntNew.Rows[i].ItemArray.GetValue(8) != DBNull.Value)
                            {
                                liExternId = (int)tableCntNew.Rows[i].ItemArray.GetValue(8);

                                if (tableCntNew.Rows[i].ItemArray.GetValue(0) != DBNull.Value)
                                    liZsId = (int)tableCntNew.Rows[i].ItemArray.GetValue(0);            // Id Zählerstand
                                if (tableCntNew.Rows[i].ItemArray.GetValue(4) != DBNull.Value)
                                    ldtStart = (DateTime)tableCntNew.Rows[i].ItemArray.GetValue(4);     // Datum
                                if (tableCntNew.Rows[i].ItemArray.GetValue(5) != DBNull.Value)
                                    ldVerbrauch = (decimal)tableCntNew.Rows[i].ItemArray.GetValue(5);   // Verbrauch
                                if (tableCntNew.Rows[i].ItemArray.GetValue(6) != DBNull.Value)
                                    ldBetragNetto = (decimal)tableCntNew.Rows[i].ItemArray.GetValue(6);     // Preis Einheit Netto
                                if (tableCntNew.Rows[i].ItemArray.GetValue(7) != DBNull.Value)
                                    ldBetragBrutto = (decimal)tableCntNew.Rows[i].ItemArray.GetValue(7);    // Preis Einheit Brutto
                                if (tableCntNew.Rows[i].ItemArray.GetValue(9) != DBNull.Value)
                                    liObjekt = (int)tableCntNew.Rows[i].ItemArray.GetValue(9);          // Objekt
                                if (tableCntNew.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                                    liObjektTeil = (int)tableCntNew.Rows[i].ItemArray.GetValue(10);     // Obj Teil
                                if (tableCntNew.Rows[i].ItemArray.GetValue(11) != DBNull.Value)         
                                    liKsa = (int)tableCntNew.Rows[i].ItemArray.GetValue(11);            // Kostenstellenart

                                DataRow dr = tableTml.NewRow();

                                dr[3] = liExternId;     // id Zählerstand
                                dr[4] = liObjekt;
                                dr[5] = liObjektTeil;
                                dr[6] = liMieter;
                                dr[7] = liKsa;
                                dr[8] = ldBetragNetto * ldVerbrauch;
                                dr[10] = ldBetragBrutto * ldVerbrauch;
                                dr[13] = ldtStart;
                                // dr[17] = 99; für Testzwecke, um Zählerdaten wiederzufinden

                                tableTml.Rows.Add(dr);

                                // und alles ab in die Datenbank
                                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(sdTml);
                                sdTml.UpdateCommand = commandBuilder.GetUpdateCommand();
                                sdTml.InsertCommand = commandBuilder.GetInsertCommand();

                                sdTml.Update(tableTml);
                            }
                            else
                            {
                                MessageBox.Show("Verarbeitungsfehler ERROR fetchdata RdFunctions 0003\n piArt = " + piArt.ToString(),
                                         "Achtung");
                                break;
                            }
                        }
                        break;
                    case 24:            // Zählerinformationen für Report Nebenkostenabrechnungen
                        SqlCommand command124 = new SqlCommand(psSql, connect);
                        tableZlInfo = new DataTable();
                        sdZlInfo = new SqlDataAdapter(command124);
                        sdZlInfo.Fill(tableZlInfo);
                        liRows = tableZlInfo.Rows.Count;
                        break;
                    case 25:            // Zählerinformationen für Report Nebenkostenabrechnungen
                        SqlCommand command125 = new SqlCommand(psSql, connect);
                        tableParts = new DataTable();
                        sdParts = new SqlDataAdapter(command125);
                        sdParts.Fill(tableParts);
                        liRows = tableParts.Rows.Count;
                        break;

                    default:
                        break;
                }

                // db close
                connect.Close();
            }

            catch
            {
                // Die Anwendung anhalten 
                MessageBox.Show("Verarbeitungsfehler ERROR fetchdata RdFunctions 0006\n piArt = " + piArt.ToString(),
                        "Achtung");
            }
            return (liRows);     
        }

        // Berechnen der monatlichen Beträge für die Timeline
        private static decimal[] getBetraege(int liMonths, int liDaysStart, int liDaysEnd, 
                        decimal ldBetragNetto, decimal ldBetragBrutto, decimal ldBetragSollNetto, decimal ldBetragSollBrutto, 
                        int liZlgOrRg, DateTime ldtStart, DateTime ldtEnd)
        {

            int liDaysCount = 0;
            decimal ldNettoDay = 0;
            decimal ldBruttoDay = 0;
            decimal[] ldBetraege = new decimal[12];
            // Arraybelegung der Beträge:   Netto,                      Brutto, 
            //                              Netto Soll,                 Brutto Soll, 
            //                              Netto erster Monat,         Brutto erster Monat, 
            //                              Netto erster Monat Soll,    Brutto erster Monat Soll, 
            //                              Netto letzter Monat,        Brutto letzter Monat
            //                              Netto letzter Monat Soll,   Brutto letzter Monat Soll
            // Bei Vorrauszahlungen für Nebenkosten wird der Betrag bei unvollständigen Monaten Tageweise gerechnet
            // Bei Rechnungen die nicht mit dem vollen Monat starten oder enden, muss alles Tageweise gerechnet werden

            // Das ist eine Rechnung
            if (liZlgOrRg == 2)
            {
                // volle Monate werden gerechnet
                if (liDaysStart == 99 && liDaysEnd == 99)
                {
                    ldBetraege[1] = ldBetragNetto / liMonths;
                    ldBetraege[2] = ldBetragBrutto / liMonths;
                }
                // Tageweise rechnen, Start oder Ende in der Monatsmitte
                if (liDaysStart != 99 || liDaysEnd != 99)
                {
                    // Anzahl der Tage gesamt
                    // Difference in days, hours, and minutes.
                    TimeSpan ts = ldtEnd - ldtStart;
                    // Anzahl der Tage gesamt
                    int differenceInDays = ts.Days;
                    liDaysCount = ts.Days;

                    // Tagessummen
                    ldNettoDay = ldBetragNetto / liDaysCount;
                    ldBruttoDay = ldBetragBrutto / liDaysCount;

                    // Der Anfangsmonat wird anteilig gerechnet
                    if (liDaysStart != 99)
                    {
                        // Summen für 1. Monat
                        ldBetraege[5] = liDaysStart * ldNettoDay;
                        ldBetraege[6] = liDaysStart * ldBruttoDay;

                        // Anzahl der Monate reduzieren
                        liMonths--;

                        // Beträge um den geteilten ersten Monat reduzieren 
                        ldBetragNetto = ldBetragNetto - ldBetraege[5];
                        ldBetragBrutto = ldBetragBrutto - ldBetraege[6];

                        // Tage korrigieren
                        liDaysCount = liDaysCount - liDaysStart;
                    }

                    // Der Endmonat wird anteilig gerechnet
                    if (liDaysEnd != 99)
                    {
                        // Summen für 1. Monat
                        ldBetraege[9] = liDaysEnd * ldNettoDay;
                        ldBetraege[10] = liDaysEnd * ldBruttoDay;

                        // Anzahl der Monate reduzieren
                        liMonths--;

                        // Beträge um den geteilten ersten Monat reduzieren 
                        ldBetragNetto = ldBetragNetto - ldBetraege[9];
                        ldBetragBrutto = ldBetragBrutto - ldBetraege[10];

                        // Tage korrigieren
                        liDaysCount = liDaysCount - liDaysStart;
                    }

                    // Die verbleibende Summe wird auf die verbleibenden Monate verteilt
                    ldBetraege[1] = ldBetragNetto / liMonths;
                    ldBetraege[2] = ldBetragBrutto / liMonths;
                }
            }

            // Das ist eine Vorrauszahlung
            if (liZlgOrRg == 2)
            {
                
            }

            // Das ist ein Zählerstand
            if (liZlgOrRg == 3)
            {

            }

            return ldBetraege;
        }

        // Timeline für Relationen erzeugen
        private static int TimelineCreateRelations(int liExternId, int liObjekt, int liObjektTeil, int liMieter)
        {
            int liOk = 0;
            string lsSql = "";
            string lsSql2 = "";


            // Dann werden die Kosten verteilt:
            // Nach Objektteil nur nach Quadratmetern oder Anteilig
            // Nach Mieter auch nach Personenzahl


            if (liObjekt > 0 )                       // Timeline Objektteil schreiben
            {
                // in Timeline Objektteil werden alle Monate nach dem Verteilungsschlüssel geschrieben
                lsSql2 = Timeline.getSql(6, liObjekt, "", "",0);       // Objektteile holen
                lsSql = Timeline.getSql(4, liExternId, liObjekt.ToString(), "",0);
                liOk = Timeline.fetchData(lsSql,lsSql2, 4, gsConnectString);
            }

            else if (liObjektTeil > 0)
            {
                // In Timeline Mieter werden alle umlagefähigen Kosten auf den 
                // zu dem TimeLineMonat wohnenden Mieter geschrieben
                lsSql = Timeline.getSql(5, liExternId, liObjektTeil.ToString(), "",0);
                liOk = Timeline.fetchData(lsSql,"", 5, gsConnectString);
            }

            return liOk;
        }

        // Anzahl der Tage bis Monatsende
        private static int getDaysEnd(DateTime ldtEnd)
        {
            int liDaysInMonth = 0;
            int liDays = 0;
            int liDay = 0;

            liDay = ldtEnd.Day;
            liDaysInMonth = DateTime.DaysInMonth(ldtEnd.Year, ldtEnd.Month);

            if (liDay == liDaysInMonth)
            {
                liDays = 99;    // kompletter Monat    
            }
            else
            {
                liDays = liDay;
            }
            return liDays;
        }

        // Anzahl der Tage des ersten Monats (Tag = 1 > voller Monat)
        private static int getDaysStart(DateTime ldtStart)
        {
            int liDaysInMonth = 0;
            int liDays = 0;
            int liDay = 0;

            liDay = ldtStart.Day;

            if (liDay == 1)
            {
                liDays = 99;        // Kompletter Monat
            }
            else                    // Teilmonat (Anzahl der Tage bis Monatsende)
            {
                liDaysInMonth = DateTime.DaysInMonth(ldtStart.Year, ldtStart.Month);
                liDays = liDaysInMonth - liDay;
            }
            
            return liDays;

        }

        // Anzahl der Monate von Start- bis EndeDatum
        private static int getMonths(DateTime ldtStart, DateTime ldtEnd)
        {
            int liMonths = 0;

            liMonths = ((ldtEnd.Year - ldtStart.Year) * 12) + ldtEnd.Month - ldtStart.Month + 1;

            return liMonths;
        }

        // Timeline neu erzeugen
        private static int TimelineCreate(int liExternId, string asField)
        {
            int liOk = 0;
            string lsSql = "";
            string lsSql2 = "";

            if (asField == "id_rechnung") // Rechnung
            {
                lsSql = Timeline.getSql(31, liExternId, asField, "",0);               // Timeline
                lsSql2 = Timeline.getSql(1, liExternId, asField, "",0);               // Rechnung
                liOk = Timeline.fetchData(lsSql, lsSql2, 3, gsConnectString);
            }

            if (asField == "id_vorauszahlung") // Vorrauszahlung                                     
            {
                lsSql = Timeline.getSql(31, liExternId, asField, "",0);               // Timeline
                lsSql2 = Timeline.getSql(12, liExternId, asField, "",0);              // Zahlung mit extern Timeline Id
                liOk = Timeline.fetchData(lsSql, lsSql2, 13, gsConnectString);                
            }

            if (asField == "id_zaehlerstand") // Zähler
            {
                lsSql = Timeline.getSql(31, liExternId, asField, "",0);               // Timeline
                lsSql2 = Timeline.getSql(21, liExternId, asField, "",0);              // Zählerstande mit extern Timeline Id
                liOk = Timeline.fetchData(lsSql, lsSql2, 23, gsConnectString);      //          
            }

            return liOk;
        }

        // Alle Datensätze der Timeline ID zunächst löschen
        private static int TimelineDelete(int liExternId)
        {
            int liOk = 0;
            string lsSql = "";

            // SqlStatement für Timeline löschen
            lsSql = Timeline.getSql(2,liExternId, "", "",0);
            liOk = Timeline.fetchData(lsSql,"", 2, gsConnectString); 

            // Info: hier werden auch alle Datensätze evtl untergeordneter Rubriken 
            // anteilige Kosten von Objektteilen und Mietern gelöscht,
            // weil alle datensätze betr. der Extern Id gelöscht werden

            return liOk;
        }

        // Mehrwertsteuersatz holen, Bezeichnung bez ist bekannt
        public static int getMwstFromBez(string lsBez, string asConnectString)
        {
            String lsSql = "";
            int liMwstSatz = 0;

            lsSql = Timeline.getSql(9, 0,lsBez,"",0);
            // fetchdata gibt hier den MwstSatz zurück
            liMwstSatz = Timeline.fetchData(lsSql, "", 8, asConnectString);

            return liMwstSatz;
        }

        // Mehrwertsteuersatz holen, Art ist bekannt
        public static int getMwstSatz(int liMwstArt, string asConnectString)
        {
            String lsSql = "";
            int liMwstSatz = 0;

            lsSql = Timeline.getSql(8, liMwstArt, "", "",0);
            // fetchdata gibt hier den MwstSatz zurück
            liMwstSatz = Timeline.fetchData(lsSql, "", 8, asConnectString);

            return liMwstSatz;
        }

        // Gesamtfläche eines Objektes holen
        private static decimal getObjektflaeche(int aiObjekt, int aiTObjekt, int aiMieterId, string asConnectString)
        {
            int liObjTeilId = 0;
            int liObjId = 0;
            decimal ldGesamtflaeche = 0;
            String lsSql = "";

            // Mieter ID vorhanden
            if (aiMieterId > 0)
            {
                liObjTeilId = getIdObjTeil(aiMieterId, asConnectString);
                liObjId = getIdObj(liObjTeilId, asConnectString, 2);
                lsSql = "Select flaeche_gesamt from objekt where id_objekt = " + liObjId.ToString();
            }
            // TeilObjekt ID vorhanden
            if (aiTObjekt > 0)
            {
                liObjId = getIdObj(liObjTeilId, asConnectString, 2);
                lsSql = "Select flaeche_gesamt from objekt where id_objekt = " + liObjId.ToString();
            }
            // Objekt ID vorhanden
            if (aiObjekt > 0)
            {
                lsSql = "Select flaeche_gesamt from objekt where id_objekt = " + aiObjekt.ToString();
            }

            SqlConnection connect;
            connect = new SqlConnection(asConnectString);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvGetFlaeche = command.ExecuteScalar();

                if (lvGetFlaeche != null)
                {
                    decimal.TryParse(lvGetFlaeche.ToString(), out ldGesamtflaeche);   // Ulf! TODO testen
                }
                else
                {
                    lvGetFlaeche = 0;
                }

                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es wurden keine Gesamtfläche gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.getObjektFlaeche)",
                         MessageBoxButton.OK);
            }
            return ldGesamtflaeche;
        }

        // Fläche eines TeilObjektes holen
        private static decimal getTObjektflaeche(int aiTObjekt, int aiMieterId, string asConnectString)
        {
            int liObjTeilId = 0;
            decimal ldFlaeche = 0;
            string lsSql = "";

            // Mieter ID vorhanden
            if (aiMieterId > 0)
            {
                liObjTeilId = getIdObjTeil(aiMieterId, asConnectString);
                lsSql = "Select flaeche_anteil from objekt_teil where id_objekt_teil = " + liObjTeilId.ToString();
            }
            // TeilObjekt ID vorhanden
            if (aiTObjekt > 0)
            {
                lsSql = "Select flaeche_anteil from objekt_teil where id_objekt_teil = " + aiTObjekt.ToString();
            }

            SqlConnection connect;
            connect = new SqlConnection(asConnectString);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvGetFlaeche = command.ExecuteScalar();

                if (lvGetFlaeche != null)
                {
                    decimal.TryParse(lvGetFlaeche.ToString(), out ldFlaeche);   // TODO Ulf! testen
                }
                else
                {
                    lvGetFlaeche = 0;
                }

                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es wurden keine Gesamtfläche gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.getTObjektFlaeche)",
                         MessageBoxButton.OK);
            }
            return ldFlaeche;
        }

        // Fläche eines TeilObjektes holen
        private static decimal getTObjektAnteil(int aiTObjekt, int aiMieterId, string asConnectString)
        {
            decimal ldAnteil = 0;
            int liObjTeilId = 0;
            string lsSql = "";

            if (aiTObjekt > 0)
            {
                lsSql = "Select prozent_anteil from objekt_teil where id_objekt_teil = " + aiTObjekt.ToString();                
            }
            if (aiMieterId > 0)
            {
                liObjTeilId = getIdObjTeil(aiMieterId, asConnectString);
                lsSql = "Select prozent_anteil from objekt_teil where id_objekt_teil = " + liObjTeilId.ToString();                
            }

            SqlConnection connect;
            connect = new SqlConnection(asConnectString);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvGetAnteil = command.ExecuteScalar();

                if (lvGetAnteil != null && (liObjTeilId > 0 || aiTObjekt > 0))
                {
                    decimal.TryParse(ldAnteil.ToString(), out ldAnteil);  // TODO Ulf! testen
                }
                else
                {
                    lvGetAnteil = 0;
                }

                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es wurden keine % Anteil gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.getTObjektAnteil)",
                         MessageBoxButton.OK);
            }
            return ldAnteil;
        }


        // Die Gesamtfläche der Objektauswahl aus objekt_part_mix ermitteln
        // Art 1 ist die Gesamtgrundfläche der gewählten Wohnungen
        // Art 2 ist die Gesamtfläche des Objektes
        private static decimal getObjektflaecheAuswahl(int liObjekt, int aiTimelineId, string asConnect, int aiArt)
        {
            decimal ldGesamtflaeche = 0;
            string lsSql = "";

            switch (aiArt)
            {
                case 0:
                    lsSql = @"Select Sum(flaeche_anteil) from objekt_mix_parts where sel = 1 
                                and id_objekt = " + liObjekt.ToString() + " and id_timeline = " + aiTimelineId.ToString();
                    break;
                case 1:
                    lsSql = @"Select Sum(flaeche_anteil) from objekt_mix_parts where ges_fl_behalten = 1 
                                and id_objekt = " + liObjekt.ToString() + " and id_timeline = " + aiTimelineId.ToString();
                    break;
                default:
                    break;
            }

            SqlConnection connect;
            connect = new SqlConnection(asConnect);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvGetFlaeche = command.ExecuteScalar();

                if (lvGetFlaeche != null && liObjekt > 0)
                {
                    decimal.TryParse(lvGetFlaeche.ToString(), out ldGesamtflaeche);  // Ulf! TODO testen
                }
                else
                {
                    lvGetFlaeche = 0;
                }
                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es wurden keine Gesamtfläche gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.getObjektFlaecheAuswahl)",
                         MessageBoxButton.OK);
            }
            return ldGesamtflaeche;
        }

        // Es wird geprüft ob das Objektteil in der Auswahl enthalten ist
        private static int getObjektTeilAuswahl(int aiObjektTeil)
        {
            int liObjektTeil = 0;

            String lsSql = "Select id_objekt_teil from objekt_mix_parts where sel = 1 and id_objekt_teil = " + aiObjektTeil.ToString();

            SqlConnection connect;
            connect = new SqlConnection(gsConnectString);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvGetObjt = command.ExecuteScalar();

                if (lvGetObjt != null)
                {
                    int.TryParse(lvGetObjt.ToString(), out liObjektTeil);   // Ulf! TODO testen
                }
                else
                {
                    liObjektTeil = 0;
                }

                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es wurden keine Objektteil in der Auswahl gefunden gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.getObjektTeilAuswahl)",
                         MessageBoxButton.OK);
            }



            return liObjektTeil;
        }


        // Ist eine Weitergabe der Kosten in art_kostenart eingetragen
        // 1 = Grundlage ist das Objekt > geht an ObjektTeil
        // 2 = Grundlage ist Objektteil > geht an Mieter
        private static bool getWtl(int p, int liExternId)
        {
            bool lbWtl = false;
            string lsSql = "";

            switch (p)
            {
                case 1:
                    // Weiterleitung an Objektteil
                    lsSql = @"Select art_kostenart.wtl_obj_teil from timeline 
                                join art_kostenart on timeline.id_ksa = art_kostenart.id_ksa
                                Where timeline.id_rechnung = " + liExternId.ToString();
                    break;
                    // Weiterleitung an Mieter
                case 2:
                    lsSql = @"Select art_kostenart.wtl_mieter from timeline 
                                join art_kostenart on timeline.id_ksa = art_kostenart.id_ksa
                                Where timeline.id_rechnung = " + liExternId.ToString();
                    break;
                default:
                    break;
            }

            SqlConnection connect;
            connect = new SqlConnection(gsConnectString);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvWtl = command.ExecuteScalar();

                if (lvWtl != DBNull.Value)
                {
                    lbWtl = Convert.ToBoolean(lvWtl);               
                }
                else
                {
                    lbWtl = false;
                }

                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es wurden keine Weiterleitungsinformation gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.getWtl)",
                         MessageBoxButton.OK);
            }
            return lbWtl;
        }

        // Hier wird der aktuelle Mieter für den gegebenen Monat der Timeline ermittelt
        public static int getAktMieter(int aiObjektTeil, DateTime adtMonat, string asConnect)
        {
            String lsSql = "";
            Int32 liMieterId = 0;
            
            // adtMonat umbauen soll immer den ersten des Monats zeigen
            adtMonat = adtMonat.AddDays(- (adtMonat.Day - 1));

            lsSql = @"Select id_mieter from vertrag
                        Where id_objekt_teil = " + aiObjektTeil.ToString() +
                        " AND vertrag.datum_von <= Convert(DateTime," + "\'" + adtMonat + "',104) "
                        + " AND vertrag.datum_bis >= Convert(DateTime," + "\'" + adtMonat + "',104)";
                        // + " AND vertrag_aktiv = 1 "; // Sollteauch ohne Aktiv Kennzeichen gehen TODO ULF!

            SqlConnection connect;
            connect = new SqlConnection(asConnect);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvMieterId = command.ExecuteScalar();

                if (lvMieterId != DBNull.Value)
                {
                    if (lvMieterId != null)
                    {
                        Int32.TryParse(lvMieterId.ToString(), out liMieterId);                        
                    }
                }
                else
                {
                    liMieterId = 0;
                }

                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es wurden keine Mieterinformation gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.getAktMieter)",
                         MessageBoxButton.OK);
            }

            return liMieterId;
        }

        // Den Mieter für Leerstand ermitteln
        // Aus Objekt oder ObjektTeil
        // Für Rechnungen zur Timeline, die nicht auf einen aktiven Mietvertrag gebucht werden können
        private static int getMieterLeerstand( int aiObjektTeil, string asConnect)
        {
            String lsSql = "";
            int liMieterId = 0;

            if (aiObjektTeil > 0)
            {
                lsSql = @"select mieter.Id_mieter as mid
                            from objekt_teil
                        join objekt on objekt_teil.id_objekt = objekt.Id_objekt
                        Join filiale on filiale.id_filiale = objekt.Id_filiale
                        join mieter on mieter.id_filiale = filiale.Id_Filiale
                            where objekt_teil.Id_objekt_teil = " + aiObjektTeil.ToString();                
            } 

            SqlConnection connect;
            connect = new SqlConnection(asConnect);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvMieterId = command.ExecuteScalar();

                if (lvMieterId != DBNull.Value)
                {
                    if (lvMieterId != null)
                    {
                        liMieterId = Convert.ToInt32(lvMieterId);   
                    }
                }
                else
                {
                    liMieterId = 0;
                }

                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es wurden keine Mieterinformation gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.getMieterLeerstand)",
                         MessageBoxButton.OK);
            }

            return liMieterId;
        }

        // Ermitteln der Anzahl der aktuell wohnenden Personen in einem Objekt, Objektteil
        // Gesucht wird nach aktiven Verträgen in einem Objekt, Objektteil
        // Wird benötigt, um eine Kostenaufteilung nach Personen zu machen
        // Das Flag soll die fehlenden Informationen holen 0 = nix; 1 = ObjektId; 2 = TeilobjektId
        private static decimal getAktPersonen(int aiObjekt, int aiObjektTeil, int aiMieterId, string asDatVon, string asDatBis, int aiFlag, string asConnectString)
        {
            int liObjId = 0;
            int liObTId = 0;
            decimal ldAnzahlPersonen = 0;
            String lsSql = "";
            String lsSqlAdd = "";

            // Keine Ids holen
            if (aiFlag == 0)
            {
                if (aiObjekt > 0)
                {
                    lsSql = @"Select sum(vertrag.anzahl_personen) from vertrag where vertrag.vertrag_aktiv = 1 And vertrag.id_objekt = " + aiObjekt.ToString();
                }
                if (aiObjektTeil > 0)
                {
                    lsSql = @"Select sum(vertrag.anzahl_personen) from vertrag where vertrag.vertrag_aktiv = 1 And vertrag.id_objekt_teil = " + aiObjektTeil.ToString();
                }                
            }

            // Objekt ID aus Mieter ID holen
            if (aiFlag == 1)
            {
                liObjId = getIdObj(aiMieterId, asConnectString, 1);
                lsSql = lsSql = @"Select sum(vertrag.anzahl_personen) from vertrag where vertrag.vertrag_aktiv = 1 And vertrag.id_objekt = " + liObjId.ToString();
            }

            // TeilObjekt ID aus Mieter Id holen
            if (aiFlag == 2)
            {
                liObTId = getIdObjTeil(aiMieterId, asConnectString);
                lsSql = @"Select sum(vertrag.anzahl_personen) from vertrag where vertrag.vertrag_aktiv = 1 And vertrag.id_objekt_teil = " + liObTId.ToString();
            }

            lsSqlAdd = " And vertrag.datum_von <= Convert(DateTime," + "\'" + asDatVon + "',104) "
                                 + "And vertrag.datum_bis >= Convert(DateTime," + "\'" + asDatBis + "',104)";

            lsSql = lsSql + lsSqlAdd;

            SqlConnection connect;
            connect = new SqlConnection(asConnectString);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvAnzPersonen = command.ExecuteScalar();

                if (lvAnzPersonen != DBNull.Value)
                {
                    if (lvAnzPersonen != null)
                    {
                        ldAnzahlPersonen = Convert.ToDecimal(lvAnzPersonen);
                    }
                }
                else
                {
                    ldAnzahlPersonen = 0;
                }

                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es wurden keine Personeninformation gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.getAktPersonen)",
                         MessageBoxButton.OK);
            }
            return ldAnzahlPersonen;
        }

        // Die Nebenkosten ID in der Tabelle art_KostenArt ermitteln
        // Art 1 = Zahlung Nebenkosten
        // Art 2 = Zählerstände
        public static int getKsaId(int aiArt, String asConnect)
        {
            int liKsaId = 0;
            String lsSql = "";

            switch (aiArt)
            {
                case 1:
                    lsSql = @"Select id_ksa From art_kostenart Where ksa_zahlung = 1 Order by sort;";
                    break;
                case 2:
                    lsSql = @"Select id_ksa From art_kostenart Where ksa_zaehler = 1 Order by sort;";
                    break;
                default:
                    break;
            }


            SqlConnection connect;
            connect = new SqlConnection(asConnect);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvKsa = command.ExecuteScalar();

                if (lvKsa != DBNull.Value)
                {
                    if (lvKsa != null)
                    {
                        liKsaId = Convert.ToInt32(lvKsa);
                    }
                }
                else
                {
                    liKsaId = 0;
                }

                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es wurde keine Nebenkosten ID gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.getNkId)",
                         MessageBoxButton.OK);
            }

            return liKsaId;
        }

        // Den Verteilungskurzstring aus der Tabelle art_verteilung ermitteln
        public static string getVerteilung(String asConnect, int aiVerteilungId)
        {
            string lsVerteilung = "";
            String lsSql = "";

            lsSql = @"Select kb From art_verteilung Where id_verteilung = " + aiVerteilungId.ToString();

            SqlConnection connect;
            connect = new SqlConnection(asConnect);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvVerteilung = command.ExecuteScalar();

                if (lvVerteilung != DBNull.Value)
                {
                    if (lvVerteilung != null)
                    {
                        lsVerteilung = lvVerteilung.ToString().Trim();
                    }
                }
                else
                {
                    lsVerteilung = "";
                }

                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es wurde kein Verteilungsstring gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.getVerteilung)",
                         MessageBoxButton.OK);
            }

            return lsVerteilung;
        }

        // Die VerteilungsId aus Rechnungen ermitteln
        private static int getVerteilungsId(string asConnect, int aiTimelineId)
        {
            int liVerteilungId = 0;
            String lsSql = "";

            lsSql = @"Select id_verteilung From rechnungen Where id_extern_timeline = " + aiTimelineId.ToString();

            SqlConnection connect;
            connect = new SqlConnection(asConnect);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvRechnungId = command.ExecuteScalar();

                if (lvRechnungId != DBNull.Value)
                {
                    if (lvRechnungId != null)
                    {
                        liVerteilungId = Convert.ToInt32(lvRechnungId);
                    }
                }
                else
                {
                    liVerteilungId = 0;
                }

                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es wurde kein Verteilungsstring gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.getVerteilung)",
                         MessageBoxButton.OK);
            }

            return liVerteilungId;
        }

        // Verteilungs ID aus art_verteilung ermitteln
        private static int getIdArtVerteilung(string asBez, string asConnect)
        {
            int liVerteilungId = 0;
            String lsSql = "";

            lsSql = @"Select id_verteilung From art_verteilung Where kb = '" + asBez.ToString() +"'";

            SqlConnection connect;
            connect = new SqlConnection(asConnect);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var vertId = command.ExecuteScalar();

                if (vertId != DBNull.Value)
                {
                    if (vertId != null)
                    {
                        liVerteilungId = Convert.ToInt32(vertId);
                    }
                }
                else
                {
                    liVerteilungId = 0;
                }

                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es wurde keine VerteilungsID gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.getIdArtVerteilung)",
                         MessageBoxButton.OK);
            }
            return liVerteilungId;
        }




        // Den Verteilungskurzstring aus der Tabelle art_verteilung ermitteln
        public static string getVerteilungFromString(String asConnect, string asVerteilung)
        {
            string lsVerteilung = "";
            String lsSql = "";

            lsSql = @"Select kb From art_verteilung Where bez = '" + asVerteilung.ToString().Trim() + " '";

            SqlConnection connect;
            connect = new SqlConnection(asConnect);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvVerteilung = command.ExecuteScalar();

                if (lvVerteilung != DBNull.Value)
                {
                    if (lvVerteilung != null)
                    {
                        lsVerteilung = lvVerteilung.ToString().Trim();
                    }
                }
                else
                {
                    lsVerteilung = "";
                }

                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es wurde kein Verteilungsstring gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.getVerteilung)",
                         MessageBoxButton.OK);
            }

            return lsVerteilung;
        }

        // Und den Sql Zusatz für Reports in eine xml-Datei speichern
        public static void saveLastSql(string asSqlKostenDirekt, string asSqlContent, string asSqlContSumObj, string asSqlConSumObjt,
            string asSqlZahlungen, string asSqlZahlungenSumme, 
            string asSqlPersonen, string asSqlZaehler, string asReport, string asSqlRgNr)
        {
            String PDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Ruddat\\Nebenkosten";

            // Eintrag in die XML Datei
            try
            {
                XmlTextWriter xmlwriter = new XmlTextWriter(PDataPath + "ruddat_sql.xml", null);
                xmlwriter.Formatting = Formatting.Indented;
                xmlwriter.WriteStartDocument();
                xmlwriter.WriteStartElement("Root");

                xmlwriter.WriteStartElement("LastSqlDirekt");
                xmlwriter.WriteString(asSqlKostenDirekt);
                xmlwriter.WriteEndElement();

                if (asSqlZahlungen.Length>0)
                {
                    xmlwriter.WriteStartElement("LastSqlZahlungen");
                    xmlwriter.WriteString(asSqlZahlungen);
                    xmlwriter.WriteEndElement();
                }

                if (asSqlZahlungenSumme.Length > 0)
                {
                    xmlwriter.WriteStartElement("LastSqlSumme");
                    xmlwriter.WriteString(asSqlZahlungenSumme);
                    xmlwriter.WriteEndElement();
                }

                if (asSqlContent.Length > 0)
                {
                    xmlwriter.WriteStartElement("LastSqlContent");
                    xmlwriter.WriteString(asSqlContent);
                    xmlwriter.WriteEndElement();
                }

                if (asSqlContent.Length > 0)
                {
                    xmlwriter.WriteStartElement("LastSqlContent2");
                    xmlwriter.WriteString(asSqlZaehler);     // Darstellung nur ObjektKosten Zähler
                    xmlwriter.WriteEndElement();
                }


                if (asSqlContent.Length > 0)
                {
                    xmlwriter.WriteStartElement("LastSqlRgNr");
                    xmlwriter.WriteString(asSqlRgNr);     // Rechnungsnummer Anschreiben speichern
                    xmlwriter.WriteEndElement();
                }

                xmlwriter.WriteStartElement("Report");
                xmlwriter.WriteString(asReport);
                xmlwriter.WriteEndElement();

                xmlwriter.WriteEndElement();
                xmlwriter.WriteEndDocument();
                xmlwriter.Close();
            }
            catch
            {
                MessageBox.Show("Sql-Statement konnte nicht geschrieben werden", "Achtung",
                                MessageBoxButton.OK);
            }
        }

        public static void saveLastVal(DateTime adtVon, DateTime adtBis, String asArt)
        {
            String PDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Ruddat\\Nebenkosten";

            // Eintrag in die XML Datei
            try
            {
                XmlTextWriter xmlwriter = new XmlTextWriter(PDataPath + "ruddat_val.xml", null);
                xmlwriter.Formatting = Formatting.Indented;
                xmlwriter.WriteStartDocument();
                xmlwriter.WriteStartElement("Root");

                if (adtVon > DateTime.MinValue)
                {
                    xmlwriter.WriteStartElement("DatumVon");
                    xmlwriter.WriteString(adtVon.ToString());
                    xmlwriter.WriteEndElement();
                }

                if (adtBis < DateTime.MaxValue)
                {
                    xmlwriter.WriteStartElement("DatumBis");
                    xmlwriter.WriteString(adtBis.ToString());
                    xmlwriter.WriteEndElement();
                }

                xmlwriter.WriteStartElement("Datum");
                xmlwriter.WriteString(asArt);
                xmlwriter.WriteEndElement();

                xmlwriter.WriteEndElement();
                xmlwriter.WriteEndDocument();
                xmlwriter.Close();
            }
            catch
            {
                MessageBox.Show("Sql-Statement konnte nicht geschrieben werden", "Achtung",
                                MessageBoxButton.OK);
            }
        }


        // Aus den Verträgen die Teilobjekt ID anhand der Mieter ID ermitteln
        internal static int getIdObjTeil(int aiId, string asConnect)
        {
            int liIdObjTeil = 0;
            String lsSql = "";

            lsSql = @"Select id_objekt_teil From vertrag Where id_mieter = " + aiId.ToString();

            SqlConnection connect;
            connect = new SqlConnection(asConnect);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvMieterId = command.ExecuteScalar();

                if (lvMieterId != DBNull.Value)
                {
                    if (lvMieterId != null)
                    {
                        liIdObjTeil = Convert.ToInt32(lvMieterId);
                    }                    
                }
                else
                {
                    liIdObjTeil = 0;
                }

                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es wurde kein Teilobjekt gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.getIdObjTeil)",
                         MessageBoxButton.OK);
            }
            return liIdObjTeil;
        }

        // Die Objekt ID aus den Vertragsdaten ermitteln aus der Mieter Id = 1 oder der Teilobjekt ID = 2
        internal static int getIdObj(int aiId, string asConnect, int aiArt)
        {
            int liIdObj = 0;
            String lsSql = "";

            switch (aiArt)
            {
                case 1:
                    lsSql = @"Select id_objekt From vertrag Where id_mieter = " + aiId.ToString();
                    break;
                case 2:
                    lsSql = @"Select id_objekt From vertrag Where id_objekt_teil = " + aiId.ToString();
                    break;
                default:
                    break;
            }

            SqlConnection connect;
            connect = new SqlConnection(asConnect);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvRechnungId = command.ExecuteScalar();

                if (lvRechnungId != DBNull.Value)
                {
                    if (lvRechnungId != null)
                    {
                        liIdObj = Convert.ToInt32(lvRechnungId);
                    }
                }
                else
                {
                    liIdObj = 0;
                }

                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es wurde kein Objekt gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.getIdObj)",
                         MessageBoxButton.OK);
            }
            return liIdObj;
        }

        // Die Tabelle x_abr_content wird gefüllt
        // asSql ist die Timeline
        // asSqlContent ist die Zieltabelle. Sie zeigt das Content des Reports Nebenkostenabrechnung
        internal static int fill_content(string asSql, string asSqlContent, string asSql2, string asDatVon, string asDatBis, string asConnect, string asSqlRgNr, int aiAnschreiben)
        {
            int liOk = 0;
            int liIdExternTimeline = 0;
            int liIdZaehlerstand = 0;
            int liIdMieter = 0;
            int liIdObjt = 0;
            int liIdObj = 0;
            int liIdArtVerteilung = 0;
            int liIdExternTimelineZaehlerstand = 0;
            int liIdRgNr = 0;
            // string lsRgNr = "";
            // string lsRgTxt = "";
            DateTime ldtMonat = DateTime.MinValue;
            DateTime ldtRgDat = DateTime.MinValue;

            SqlConnection connect;

            // Tabelle Report Content leeren
            liOk = Timeline.delContent(asConnect);

            // Timeline einlesen
            DataTable tableTimeline = new DataTable();
            DataTable tableTimeline1 = new DataTable();     // Kosten des Objektes darstellen 
            DataTable tableContent = new DataTable();       // Content
            DataTable tableTmlCheckRgNr = new DataTable();  // Hier checken, ob schon eine Rechnungsnmmerfür das Anschreiben drin ist

            connect = new SqlConnection(asConnect);

            try
            {
                // Db open
                connect.Open();

                if (aiAnschreiben == 1)
                {
                    // Rechnunsnummer für Anschreiben prüfen und einsetzen
                    // ist eine id_rg_nr in der Timeline vorhanden?
                    SqlCommand command01 = new SqlCommand(asSql, connect);
                    // Create a SqlDataReader
                    SqlDataReader queryCommandReader01 = command01.ExecuteReader();
                    // Create a DataTable object to hold all the data returned by the query.
                    tableTmlCheckRgNr.Load(queryCommandReader01);
                    if (tableTmlCheckRgNr.Rows.Count > 0)
                    {
                        if (tableTmlCheckRgNr.Rows[0].ItemArray.GetValue(22) != DBNull.Value)
                        {
                            liIdRgNr = Convert.ToInt16(tableTmlCheckRgNr.Rows[0].ItemArray.GetValue(22).ToString());       //  id Rechnungsnummer für Anschreiben
                        }
                    }

                    // In dem Fall muss die Rechnungsnummer Anschreiben und das Besetzt-Kennzeichen in RgNr eingesetzt werden
                    if (liIdRgNr == 0)
                    {
                        liIdRgNr = getRgNrFromPool(asConnect);          // ID Rechnungsnummer aus dem Pool besorgen
                        if (liIdRgNr > 0)
                        {
                            liOk = setRgNrToTml(liIdRgNr, asSqlRgNr, asConnect);       // ID Rechnungsnummer in Timeline einsetzen
                            if (liOk == 1)
                            {
                                liOk = setRgNrFromPool(liIdRgNr, asConnect);    // Die Rechnungsnummer als besetzt kennzeichnen 
                            } 
                        }
                        else
                        {
                            // Keine Rechnungsnummer Image Pool vorhanden, bitte Eintragen
                            MessageBox.Show("Keine Rechnungsnummer im Pool vorhanden, \nbitte Eintragen");
                        }
                    } 
                }
                // Erste Tabelle Timeline holen
                SqlCommand command = new SqlCommand(asSql, connect);
                // Create a SqlDataReader
                SqlDataReader queryCommandReader = command.ExecuteReader();
                // Create a DataTable object to hold all the data returned by the query.
                tableTimeline.Load(queryCommandReader);

                // Zweite Tabelle Timeline ObjektKostendarstellung (Zähler)
                SqlCommand command1 = new SqlCommand(asSql2, connect);
                // Create a SqlDataReader
                SqlDataReader queryCommandReader1 = command1.ExecuteReader();
                // Create a DataTable object to hold all the data returned by the query.
                tableTimeline1.Load(queryCommandReader1);

                // Dritte Tabelle x_abr_content
                SqlCommand command2 = new SqlCommand(asSqlContent, connect);
                SqlDataReader queryCommandReader2 = command2.ExecuteReader();
                tableContent.Load(queryCommandReader2);

                // Schleife durch Timeline asSql und erstmal stumpf an Tabelle Content übertragen
                // Achtung rows.count -1, weil i bei 0 anfängt
                for (int i = 0; i < tableTimeline.Rows.Count; i++)
                {
                    DataRow dr = tableContent.NewRow();

                    if (tableTimeline.Rows[i].ItemArray.GetValue(6) != DBNull.Value)
                    {
                        dr[2] = Convert.ToInt16(tableTimeline.Rows[i].ItemArray.GetValue(6).ToString());        //  Id Extern TimeLine
                        liIdExternTimeline = Convert.ToInt16(tableTimeline.Rows[i].ItemArray.GetValue(6).ToString());
                        
                        dr[27] = getRgInfo(liIdExternTimeline, asConnect, 1).Trim();                                   // Rechnungsnummer
                        dr[28] = getRgInfo(liIdExternTimeline, asConnect, 2).Trim();                                   // Rechnungstext
                        string lsd;
                        lsd = getRgInfo(liIdExternTimeline, asConnect, 3);
                        if (lsd.Length > 0)
                        {
                            dr[29] = lsd;    
                        }
                    }
                    if (tableTimeline.Rows[i].ItemArray.GetValue(7) != DBNull.Value)
                    {
                        dr[3] = Convert.ToInt16(tableTimeline.Rows[i].ItemArray.GetValue(7).ToString());        //  Id Vorrauszahlung
                    }
                    if (tableTimeline.Rows[i].ItemArray.GetValue(18) != DBNull.Value)                            // Id Zählerstand
                    {
                        dr[4] = Convert.ToInt16(tableTimeline.Rows[i].ItemArray.GetValue(18).ToString());         // Id Zählerstand
                        liIdZaehlerstand = Convert.ToInt16(tableTimeline.Rows[i].ItemArray.GetValue(18).ToString());
                        // dr[13] = Convert.ToDecimal(tableTimeline.Rows[i].ItemArray.GetValue(12).ToString());         // Zählerstand
                    }

                    if (tableTimeline.Rows[i].ItemArray.GetValue(8) != DBNull.Value)
                    {
                        dr[5] = Convert.ToInt16(tableTimeline.Rows[i].ItemArray.GetValue(8).ToString());        //  Id Objekt
                        liIdObj = Convert.ToInt16(tableTimeline.Rows[i].ItemArray.GetValue(8).ToString());
                    }
                    if (tableTimeline.Rows[i].ItemArray.GetValue(9) != DBNull.Value)
                    {
                        dr[6] = Convert.ToInt16(tableTimeline.Rows[i].ItemArray.GetValue(9).ToString());        //  Id Teilobjekt
                        liIdObjt = Convert.ToInt16(tableTimeline.Rows[i].ItemArray.GetValue(9).ToString());
                    }
                    if (tableTimeline.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                    {
                        dr[7] = Convert.ToInt16(tableTimeline.Rows[i].ItemArray.GetValue(10).ToString());       //  Id Mieter
                        liIdMieter = Convert.ToInt16(tableTimeline.Rows[i].ItemArray.GetValue(10).ToString());
                    }
                    if (tableTimeline.Rows[i].ItemArray.GetValue(16) != DBNull.Value)
                    {
                        dr[8] = Convert.ToInt16(tableTimeline.Rows[i].ItemArray.GetValue(16).ToString());       //  Id Kostenart
                    }
                    if (tableTimeline.Rows[i].ItemArray.GetValue(0) != DBNull.Value)
                    {
                        dr[9] = Convert.ToDecimal(tableTimeline.Rows[i].ItemArray.GetValue(0).ToString());      //  Netto
                    }
                    if (tableTimeline.Rows[i].ItemArray.GetValue(1) != DBNull.Value)
                    {
                        dr[11] = Convert.ToDecimal(tableTimeline.Rows[i].ItemArray.GetValue(1).ToString());     //  Brutto
                    }

                    if (tableTimeline.Rows[i].ItemArray.GetValue(4) != DBNull.Value)
                    {
                        dr[15] = Convert.ToInt16(tableTimeline.Rows[i].ItemArray.GetValue(4).ToString());       //  Weiterleitung Objekt
                    }
                    if (tableTimeline.Rows[i].ItemArray.GetValue(5) != DBNull.Value)
                    {
                        dr[16] = Convert.ToInt16(tableTimeline.Rows[i].ItemArray.GetValue(5).ToString());       //  Weiterleitung ObjektTeil
                    }
                    if (tableTimeline.Rows[i].ItemArray.GetValue(17) != DBNull.Value)
                    {
                        if (Convert.ToInt16(tableTimeline.Rows[i].ItemArray.GetValue(17)) > 0)
                        {
                            dr[23] = Convert.ToInt16(tableTimeline.Rows[i].ItemArray.GetValue(17).ToString());       //  Art der Verteilung REchnungen
                            liIdArtVerteilung = Convert.ToInt16(tableTimeline.Rows[i].ItemArray.GetValue(17).ToString());                            
                        }
                    }
                    else if (tableTimeline.Rows[i].ItemArray.GetValue(18) != DBNull.Value)                      // Art der Verteilung für Zähler ermitteln "zl"
                    {
                        liIdExternTimelineZaehlerstand = Convert.ToInt16(tableTimeline.Rows[i].ItemArray.GetValue(18));
                        liIdArtVerteilung = Timeline.getIdArtVerteilung("zl",asConnect); 
                    }
                    if (tableTimeline.Rows[i].ItemArray.GetValue(2) != DBNull.Value)
                    {
                        dr[24] = Convert.ToDecimal(tableTimeline.Rows[i].ItemArray.GetValue(2).ToString());       //  Rechnung Netto
                    }
                    if (tableTimeline.Rows[i].ItemArray.GetValue(3) != DBNull.Value)
                    {
                        dr[25] = Convert.ToDecimal(tableTimeline.Rows[i].ItemArray.GetValue(3).ToString());       //  Rechnung Brutto
                    }

                    if (tableTimeline.Rows[i].ItemArray.GetValue(22) != DBNull.Value)
                    {
                        dr[30] = Convert.ToInt16(tableTimeline.Rows[i].ItemArray.GetValue(22).ToString());       //  id Rechnungsnummer für Anschreiben
                    }

                    // Verteilungsinformationen holen
                    if (liIdArtVerteilung > 0)
                    {
                        // Verteilungsinfos ermittlen; letztes Argument ist der Detailgrad 2 = Alles TODO Ulf!!!
                        dr[26] = Timeline.getVerteilungsInfo(asConnect, liIdExternTimeline, liIdArtVerteilung, liIdObj, liIdObjt, liIdMieter, asDatVon, asDatBis, liIdExternTimelineZaehlerstand, 1);
                    }

                    // Rechnung aus Objekt oder Teilobjekt
                    if (liIdExternTimeline > 0 )
                    {
                        // Objektsummen holen
                        lsSql = getSql(14, liIdExternTimeline, "", "",0);
                        liOk = Timeline.fetchData(lsSql, "", 14, asConnect);

                        if (tableConSumObj.Rows.Count > 0)
                        {
                            if (tableConSumObj.Rows[0].ItemArray.GetValue(0) != DBNull.Value)
                            {
                                dr[21] = Convert.ToDecimal(tableConSumObj.Rows[0].ItemArray.GetValue(0));
                            }
                            if (tableConSumObj.Rows[0].ItemArray.GetValue(1) != DBNull.Value)
                            {
                                dr[22] = Convert.ToDecimal(tableConSumObj.Rows[0].ItemArray.GetValue(1));
                            }  
                        }

                        // Teilobjekt ID aus der Mieter ID  ermitteln
                        if (liIdMieter > 0)
                        {
                            liIdObjt = getVertragInfoFromMieter(liIdMieter, asConnect, 1);
                        }
                        lsSql = Timeline.getSql(15, liIdExternTimeline, "", "", liIdObjt);
                        liOk = Timeline.fetchData(lsSql, "", 15, asConnect);

                        if (tableConSumObjT.Rows.Count > 0 && liIdObjt > 0)
                        {
                            if (tableConSumObjT.Rows[0].ItemArray.GetValue(0) != DBNull.Value)
                            {
                                dr[19] = Convert.ToDecimal(tableConSumObjT.Rows[0].ItemArray.GetValue(0));
                            }
                            if (tableConSumObjT.Rows[0].ItemArray.GetValue(1) != DBNull.Value)
                            {
                                dr[20] = Convert.ToDecimal(tableConSumObjT.Rows[0].ItemArray.GetValue(1));
                            }
                        }
                    }

                    // Zählerstand aus Objekt oder ObjektTeil
                    if (liIdZaehlerstand > 0)
                    {
                        // Objektsummen holen
                        lsSql = getSql(16, liIdZaehlerstand, "", "",0);
                        liOk = Timeline.fetchData(lsSql, "", 14, asConnect);

                        if (tableConSumObj.Rows.Count > 0)
                        {
                            if (tableConSumObj.Rows[0].ItemArray.GetValue(0) != DBNull.Value)
                            {
                                dr[21] = Convert.ToDecimal(tableConSumObj.Rows[0].ItemArray.GetValue(0));
                            }
                            if (tableConSumObj.Rows[0].ItemArray.GetValue(1) != DBNull.Value)
                            {
                                dr[22] = Convert.ToDecimal(tableConSumObj.Rows[0].ItemArray.GetValue(1));
                            }

                        }

                        // TeilobjektSummen holen
                        // Teilobjekt ID aus der Mieter ID  ermitteln
                        if (liIdMieter > 0)
                        {
                            liIdObjt = getVertragInfoFromMieter(liIdMieter, asConnect, 1);
                        }
                        lsSql = getSql(17, liIdZaehlerstand, "", "",0);
                        liOk = Timeline.fetchData(lsSql, "", 15, asConnect);

                        if (tableConSumObjT.Rows.Count > 0)
                        {
                            if (tableConSumObjT.Rows[0].ItemArray.GetValue(0) != DBNull.Value)
                            {
                                dr[19] = Convert.ToDecimal(tableConSumObjT.Rows[0].ItemArray.GetValue(0));
                            }
                            if (tableConSumObjT.Rows[0].ItemArray.GetValue(1) != DBNull.Value)
                            {
                                dr[20] = Convert.ToDecimal(tableConSumObjT.Rows[0].ItemArray.GetValue(1));
                            }
                        }
                    }

                    tableContent.Rows.Add(dr);
                }

                // Zweiter Teil, nur ObjektKosten darstellen ( im Moment nur sZähler)
                // Schleife durch Timeline1 asSql2 und erstmal stumpf an Tabelle Content übertragen
                // Achtung rows.count -1, weil i bei 0 anfängt
                for (int i = 0; i < tableTimeline1.Rows.Count; i++)
                {
                    DataRow dr = tableContent.NewRow();

                    if (tableTimeline1.Rows[i].ItemArray.GetValue(6) != DBNull.Value)
                    {
                        dr[2] = Convert.ToInt16(tableTimeline1.Rows[i].ItemArray.GetValue(6).ToString());        //  Id Rechnung
                        liIdExternTimeline = Convert.ToInt16(tableTimeline1.Rows[i].ItemArray.GetValue(6).ToString());
                        dr[27] = getRgInfo(liIdExternTimeline, asConnect, 1).Trim();                                   // Rechnungesnummer
                        dr[28] = getRgInfo(liIdExternTimeline, asConnect, 2).Trim();                                   // Rechnungstext

                        if (tableTimeline1.Rows[i].ItemArray.GetValue(7) != DBNull.Value)
                        {
                            dr[3] = Convert.ToInt16(tableTimeline1.Rows[i].ItemArray.GetValue(7).ToString());        //  Id Vorrauszahlung
                        }
                        if (tableTimeline1.Rows[i].ItemArray.GetValue(18) != DBNull.Value)                            // Id Zählerstand
                        {
                            dr[4] = Convert.ToInt16(tableTimeline1.Rows[i].ItemArray.GetValue(18).ToString());         // Id Zählerstand
                            liIdZaehlerstand = Convert.ToInt16(tableTimeline1.Rows[i].ItemArray.GetValue(18).ToString());
                            // dr[13] = Convert.ToDecimal(tableTimeline1.Rows[i].ItemArray.GetValue(12).ToString());         // Zählerstand
                        }

                        if (tableTimeline1.Rows[i].ItemArray.GetValue(8) != DBNull.Value)
                        {
                            dr[5] = Convert.ToInt16(tableTimeline1.Rows[i].ItemArray.GetValue(8).ToString());        //  Id Objekt
                            liIdObj = Convert.ToInt16(tableTimeline1.Rows[i].ItemArray.GetValue(8).ToString());
                        }
                        if (tableTimeline1.Rows[i].ItemArray.GetValue(9) != DBNull.Value)
                        {
                            dr[6] = Convert.ToInt16(tableTimeline1.Rows[i].ItemArray.GetValue(9).ToString());        //  Id Teilobjekt
                            liIdObjt = Convert.ToInt16(tableTimeline1.Rows[i].ItemArray.GetValue(9).ToString());
                        }
                        if (tableTimeline1.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                        {
                            dr[7] = Convert.ToInt16(tableTimeline1.Rows[i].ItemArray.GetValue(10).ToString());       //  Id Mieter
                            liIdMieter = Convert.ToInt16(tableTimeline1.Rows[i].ItemArray.GetValue(10).ToString());
                        }
                        if (tableTimeline1.Rows[i].ItemArray.GetValue(16) != DBNull.Value)
                        {
                            dr[8] = Convert.ToInt16(tableTimeline1.Rows[i].ItemArray.GetValue(16).ToString());       //  Id Kostenart
                        }
                        //if (tableTimeline1.Rows[i].ItemArray.GetValue(0) != DBNull.Value)
                        //{
                        //    dr[9] = Convert.ToDecimal(tableTimeline1.Rows[i].ItemArray.GetValue(0).ToString());      //  Netto
                        //}
                        //if (tableTimeline1.Rows[i].ItemArray.GetValue(1) != DBNull.Value)
                        //{
                        //    dr[11] = Convert.ToDecimal(tableTimeline1.Rows[i].ItemArray.GetValue(1).ToString());     //  Brutto
                        //}

                        if (tableTimeline1.Rows[i].ItemArray.GetValue(4) != DBNull.Value)
                        {
                            dr[15] = Convert.ToInt16(tableTimeline1.Rows[i].ItemArray.GetValue(4).ToString());       //  Weiterleitung Objekt
                        }
                        if (tableTimeline1.Rows[i].ItemArray.GetValue(5) != DBNull.Value)
                        {
                            dr[16] = Convert.ToInt16(tableTimeline1.Rows[i].ItemArray.GetValue(5).ToString());       //  Weiterleitung ObjektTeil
                        }
                        if (tableTimeline1.Rows[i].ItemArray.GetValue(17) != DBNull.Value)
                        {
                            if (Convert.ToInt16(tableTimeline1.Rows[i].ItemArray.GetValue(17)) > 0)
                            {
                                dr[23] = Convert.ToInt16(tableTimeline1.Rows[i].ItemArray.GetValue(17).ToString());       //  Art der Verteilung REchnungen
                                liIdArtVerteilung = Convert.ToInt16(tableTimeline1.Rows[i].ItemArray.GetValue(17).ToString());
                            }
                        }
                        else if (tableTimeline1.Rows[i].ItemArray.GetValue(18) != DBNull.Value)                      // Art der Verteilung für Zähler ermitteln "zl"
                        {
                            liIdExternTimelineZaehlerstand = Convert.ToInt16(tableTimeline1.Rows[i].ItemArray.GetValue(18));
                            liIdArtVerteilung = Timeline.getIdArtVerteilung("zl", asConnect);
                        }

                        // Hier nicht zeigen
                        //if (tableTimeline1.Rows[i].ItemArray.GetValue(2) != DBNull.Value)
                        //{
                        //    dr[24] = Convert.ToDecimal(tableTimeline1.Rows[i].ItemArray.GetValue(2).ToString());       //  Rechnung Netto
                        //}
                        //if (tableTimeline1.Rows[i].ItemArray.GetValue(3) != DBNull.Value)
                        //{
                        //    dr[25] = Convert.ToDecimal(tableTimeline1.Rows[i].ItemArray.GetValue(3).ToString());       //  Rechnung Brutto
                        //}

                        // Verteilungsinformationen holen
                        if (liIdArtVerteilung > 0)
                        {
                            // Verteilungsinfos ermitteln letztes Argument ist der Detailgrad 2 ist alles
                            dr[26] = Timeline.getVerteilungsInfo(asConnect, liIdExternTimeline, liIdArtVerteilung, liIdObj, liIdObjt, liIdMieter, asDatVon, asDatBis, liIdExternTimelineZaehlerstand, 1);
                        }

                        // Rechnung aus Objekt oder Teilobjekt
                        if (liIdExternTimeline > 0)
                        {
                            // Objektsummen holen
                            lsSql = getSql(14, liIdExternTimeline, "", "", 0);
                            liOk = Timeline.fetchData(lsSql, "", 14, asConnect);

                            if (tableConSumObj.Rows.Count > 0)
                            {
                                if (tableConSumObj.Rows[0].ItemArray.GetValue(0) != DBNull.Value)
                                {
                                    dr[21] = Convert.ToDecimal(tableConSumObj.Rows[0].ItemArray.GetValue(0));
                                }
                                if (tableConSumObj.Rows[0].ItemArray.GetValue(1) != DBNull.Value)
                                {
                                    dr[22] = Convert.ToDecimal(tableConSumObj.Rows[0].ItemArray.GetValue(1));
                                }
                            }

                            //// TeilobjektSummen holen
                            //lsSql = getSql(15, liIdRechnung, "", "");
                            //liOk = Timeline.FetchData(lsSql, "", 15, asConnect);

                            //if (tableConSumObjT.Rows.Count > 0)
                            //{
                            //    if (tableConSumObjT.Rows[0].ItemArray.GetValue(0) != DBNull.Value)
                            //    {
                            //        dr[19] = Convert.ToDecimal(tableConSumObjT.Rows[0].ItemArray.GetValue(0));
                            //    }
                            //    if (tableConSumObjT.Rows[0].ItemArray.GetValue(1) != DBNull.Value)
                            //    {
                            //        dr[20] = Convert.ToDecimal(tableConSumObjT.Rows[0].ItemArray.GetValue(1));
                            //    }
                            //}
                        }

                        // Zählerstand aus Objekt oder ObjektTeil
                        if (liIdZaehlerstand > 0)
                        {
                            // Objektsummen holen
                            lsSql = getSql(16, liIdZaehlerstand, "", "", 0);
                            liOk = Timeline.fetchData(lsSql, "", 14, asConnect);

                            if (tableConSumObj.Rows.Count > 0)
                            {
                                if (tableConSumObj.Rows[0].ItemArray.GetValue(0) != DBNull.Value)
                                {
                                    dr[21] = Convert.ToDecimal(tableConSumObj.Rows[0].ItemArray.GetValue(0));
                                }
                                if (tableConSumObj.Rows[0].ItemArray.GetValue(1) != DBNull.Value)
                                {
                                    dr[22] = Convert.ToDecimal(tableConSumObj.Rows[0].ItemArray.GetValue(1));
                                }

                            }
                        }
                    }

                    tableContent.Rows.Add(dr);
                }

                // Ab in die Datenbank
                SqlDataAdapter adp = new SqlDataAdapter(command2);
                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(adp);

                adp.UpdateCommand = commandBuilder.GetUpdateCommand();
                adp.InsertCommand = commandBuilder.GetInsertCommand();

                adp.Update(tableContent);
                

            }
            catch (Exception)
            {
                // Die Anwendung anhalten
                MessageBox.Show("Verarbeitungsfehler rdFunctions.fillcontent\n" +
                        "Achtung rdfunctions.fillcontent"); 
                throw;
            }
            

            // db close
            connect.Close();

            // ist es eine Mieter ID in Timeline, dann die Summen aus Teilobjekt und Objekt einsetzen
            // Ist es eine Teilobjekt ID, dann die Summen aus Objekt einsetzen

            return (liOk);
        }

        // ReportTabelle vor Gebrauch löschen
        private static int delContent(string asConnect)
        {
            int liOk = 0;

            // kann schonmal gelöscht werden
            String lsSql = "delete from x_abr_content;";

            SqlConnection connect;
            connect = new SqlConnection(asConnect);

            SqlCommand command = new SqlCommand(lsSql, connect);

            // import_file
            try
            {
                // Db open
                connect.Open();
                SqlDataReader queryCommandReader = command.ExecuteReader();
                liOk = 1;
                connect.Close();
            }
            catch
            {
                MessageBox.Show("Tabelle x_abr_content konnte nicht geleert werden\n",
                        "Achtung RdFunctions.delContent",
                         MessageBoxButton.OK);
                liOk = 0;
            }
            return (liOk);

        }

        // Die Rechnungs ID aus dem SqlStatement ermitteln
        internal static int getRechnungsId(string asSqlTimeline, string asConnectString)
        {
            int liIdRechnung = 0;

            liIdRechnung = fetchData(asSqlTimeline,"", 16, asConnectString);

            return (liIdRechnung);
        }

        // Den Verbrauch aus dem Zählerstand ermitteln
        internal static decimal getZlVerbrauch(decimal adZlStand, int aiZlId, string asConnect, int aiFlagNew)
        {
            decimal ldZlStandOld = 0;
            decimal ldZlVerbrauch = 0;
            String lsSql = "";

            if (aiFlagNew == 1)
            {
                lsSql = @"select zs from zaehlerstaende where id_zaehler = " + aiZlId.ToString() + " Order by zs desc";                
            }
            else
            {
                lsSql = @"select zs from zaehlerstaende where id_zaehler = " + aiZlId.ToString() + " Order by zs desc";                
            }

            SqlConnection connect;
            connect = new SqlConnection(asConnect);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvZlStandOld = command.ExecuteScalar();

                if (lvZlStandOld != DBNull.Value)
                {
                    if (lvZlStandOld != null)
                    {
                        ldZlStandOld = Convert.ToDecimal(lvZlStandOld);
                        ldZlVerbrauch = adZlStand - ldZlStandOld;        
                    }
                }
                else
                {
                    ldZlStandOld = 0;
                }

                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es wurde kein Verbrauch gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.getZlVerbrauch)",
                         MessageBoxButton.OK);
            }
            return ldZlVerbrauch;
        }

        // Zähler Id vom Namen des Zählers ermitteln
        internal static int getZlId(string lsZlName, string asConnect)
        {

            String lsSql = "";
            int liZlId = 0;

            lsSql = @"select id_zaehler from zaehler where zaehlernummer = '" + lsZlName.ToString().Trim() + "\'";

            SqlConnection connect;
            connect = new SqlConnection(asConnect);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvZlId = command.ExecuteScalar();

                if (lvZlId != DBNull.Value)
                {
                    if (lvZlId != null)
                    {
                        liZlId = Convert.ToInt16(lvZlId); 
                    }
                }
                else
                {
                    liZlId = 0;
                }

                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es wurde keine Zähler Id gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.getZlId)",
                         MessageBoxButton.OK);
            }
            return liZlId;

        }

        // Mehrwertsteuersatz für Zähler holen (aus ZählerId)
        internal static int getMwstSatzZaehler(int aiZlId, string asConnect)
        {
            String lsSql = "";
            int liMwstSatz = 0;

            lsSql = @"Select art_mwst.mwst from zaehler 
                        left join art_mwst on zaehler.id_mwst_art = art_mwst.Id_mwst_art
                      where id_zaehler = " + aiZlId.ToString();

            SqlConnection connect;
            connect = new SqlConnection(asConnect);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvMwstSatz = command.ExecuteScalar();

                if (lvMwstSatz != DBNull.Value)
                {
                    if (lvMwstSatz != null)
                    {
                        liMwstSatz = Convert.ToInt16(lvMwstSatz);   
                    }
                }
                else
                {
                    liMwstSatz = 0;
                }
                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es wurde keine Zähler Id gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.getMwstSatzZaehler)",
                         MessageBoxButton.OK);
            }
            return liMwstSatz;
        }

        // Für die bedingte Weiterleitung
        // Hier wird die Auswahl der Objektteile vorbereitet
        // Die Objektteile (Wohnungen) werden in die Tabelle
        // objekt_mix_parts geschrieben
        internal static int makeChoose(int aiObjekt, int aiTimeLineId , string asConnect)
        {
            int liOk = 0;
            int liRowGet = 0;
            int liRows = 0;

            // Hat die Tabelle objekt_mix_parts einen Eintrag für diese Timeline ID?
            liRows = Timeline.getInfoFromParts(asConnect, aiTimeLineId);

            if (liRows == 0)            // Kein Eintrag vorhanden, Datensatz wird angelegt
            {
                liRowGet = Timeline.copyParts(asConnect, aiObjekt, aiTimeLineId);
                liOk = 1;

            }
            if (liRows > 0)         // Es existiert ein Eintrag der Timeline ID > editieren
            {
                liOk = 2;
            }

            return liOk;
        }

        // Kopieren der Daten eines Objektes in die Tabelle objekt_mix_parts
        private static int copyParts(string asConnect, int aiObjekt, int aiTimeLineId)
        {
            String lsSql = "";
            String lsWhereAdd = "";
            int liObj = 0;

            
            lsSql = @"insert into objekt_mix_parts (Id_objekt_teil,id_objekt,flaeche_anteil,bez,geschoss,lage)
                            select Id_objekt_teil,id_objekt,flaeche_anteil,bez,geschoss,lage from objekt_teil";
            lsWhereAdd = " where objekt_teil.id_objekt = " + aiObjekt.ToString() + " ";
            lsSql = lsSql + lsWhereAdd;

            SqlConnection connect;
            connect = new SqlConnection(asConnect);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvRows = command.ExecuteScalar();

                if (lvRows != DBNull.Value)
                {
                    if (lvRows != null)
                    {
                        liObj = Convert.ToInt16(lvRows);    
                    }
                }
                else
                {
                    liObj = 0;
                }
                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es konnten keine Parts kopiert werden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.copyParts)",
                         MessageBoxButton.OK);
            }
            return liObj;
        }

        // Prüfen: Ist die Tabelle objekt_mix_parts leer für diese Timeline ID
        private static int getInfoFromParts(string asConnect, int aiTimeLineId)
        {
            String lsSql = "";
            String lsWhereAdd = "";
            int liRows = 0;

            lsSql = @"Select Count(*) from objekt_mix_parts";
            lsWhereAdd = " where id_timeline = " + aiTimeLineId.ToString() + " ";

            lsSql = lsSql + lsWhereAdd;

            SqlConnection connect;
            connect = new SqlConnection(asConnect);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvRows = command.ExecuteScalar();

                if (lvRows != DBNull.Value)
                {
                    if (lvRows != null)
                    {
                        liRows = Convert.ToInt16(lvRows);    
                    }
                }
                else
                {
                    liRows = 0;
                }
                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es wurde keine Parts gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.getInfoFromParts)",
                         MessageBoxButton.OK);
            }
            return liRows;
        }

        // Verteilungsinformationen für die Nebenkostenabrechnung ermitteln
        // aiId Rechnung ist die extern Timeline ID ACHTUNG!!
        private static object getVerteilungsInfo(string asConnectString, int aiIdRechnung, int aiArtVerteilungId, 
            int aiObjektId, int aiTObjektId, int aiMieterId, 
            string asDatVon, string asDatBis, int aiIdExternTimelineZaehlerstand, int aiDetailGrad)
        {
            string lsVertInfo = "";
            string lsVerteilung = "";
            decimal ldGesamtflaecheObjekt = 0;
            decimal ldFlaecheTObjekt = 0;
            decimal ldProzentAnteil = 0;
            decimal ldAnzPersonenGesamt = 0;
            decimal ldAnzPersonen = 0;
            DateTime ldtVon = DateTime.MinValue;
            DateTime ldtBis = DateTime.MinValue;
            int liObjektId = 0;
            
            lsVerteilung = getVerteilung(asConnectString, aiArtVerteilungId);

            // Flächenanteil rechnen
            if (lsVerteilung == "fl")
            {
                // Gesamtfläche aus Tabelle Objekt holen
                if (aiMieterId > 0 || aiTObjektId > 0 || aiObjektId > 0)
                {
                    ldGesamtflaecheObjekt = getObjektflaeche(aiObjektId, aiTObjektId, aiMieterId, asConnectString);
                    if (aiTObjektId > 0 || aiObjektId > 0 || aiMieterId > 0)
                    {
                        if (aiTObjektId > 0 || aiMieterId > 0)
                        {
                            ldFlaecheTObjekt = getTObjektflaeche(aiTObjektId, aiMieterId, asConnectString);
                            if (ldGesamtflaecheObjekt > 0)
                            {
                                lsVertInfo = @"Gesamtfläche Objekt: " + ldGesamtflaecheObjekt.ToString("0.##") + "m² / " +
                                              "Mietfläche: " + ldFlaecheTObjekt.ToString("0.##") + "m²"; // \n" +
                                              // "Faktor: " + (ldFlaecheTObjekt / ldGesamtflaecheObjekt).ToString("0.##");
                            }                                                                        
                        }
                        else
                        {
                            lsVertInfo = "";
                        }
                    }
                }
                else
                {
                    lsVertInfo = "";
                }
            }
            // Prozentanteil rechnen
            if (lsVerteilung == "pz")
            {
                if (aiMieterId > 0 || aiTObjektId > 0)
                {
                    ldProzentAnteil = getTObjektAnteil(aiTObjektId, aiMieterId, asConnectString);
                    lsVertInfo = ldProzentAnteil.ToString();                    
                }
                else
                {
                    lsVertInfo = "";
                }
            }

            // Personenanzahl für den aktuellen Monat berechnen
            if (lsVerteilung == "ps")
            {
                ldAnzPersonen = getAktPersonen(aiObjektId, aiTObjektId, aiMieterId, asDatVon, asDatBis, 2, asConnectString);
                ldAnzPersonenGesamt = getAktPersonen(aiObjektId, aiTObjektId, aiMieterId, asDatVon, asDatBis, 1, asConnectString);
                if (ldAnzPersonen > 0)
                {
                    lsVertInfo = @"Personen gesamt: " + ldAnzPersonenGesamt.ToString() + " / " +
                                   "Personen Mietfläche: " + ldAnzPersonen.ToString(); // + "\n" +
                                   // "Faktor: " + (ldAnzPersonenGesamt / ldAnzPersonen).ToString("0.##");
                }
            }

            // Direkte Verteilung 1:1 weiterleiten  
            if (lsVerteilung == "di")
            {
                lsVertInfo = "lt. Rechnung";
            }

            // Nix wird verteilt                    
            if (lsVerteilung == "nl")
            {
                lsVertInfo = "";        // Verteilung Keine Ulf!
            }

            // Zähler 
            if (lsVerteilung == "zl")
            {
                // Zählerwerte und Kosten ermitteln
                lsVertInfo = getVerteilungsInfoZaehler(aiIdExternTimelineZaehlerstand, asConnectString);
            }

            // Fläche Auswahl für den Report Nebenkosten
            // Die Gesamtfläche für die Auswahl wird ermittelt
            if (lsVerteilung == "fa")
            {
                // Gesamtfläche der ausgewählten Wohnungen aus Tabelle Objekt_mix_parts holen
                liObjektId = getIdObj(aiMieterId,asConnectString,1);
                if (liObjektId > 0)
                {
                    int liArt = 0;
                    // Gesamtfläche der Auswahl = 0 oder Gesamtfläche = 1
                    liArt = getObjektflaecheAuswFlag(liObjektId, asConnectString);
                    ldGesamtflaecheObjekt = getObjektflaecheAuswahl(liObjektId, aiIdRechnung, asConnectString,liArt);
                    if (aiTObjektId > 0 || aiMieterId > 0)
                    {
                        ldFlaecheTObjekt = getTObjektflaeche(aiTObjektId, aiMieterId, asConnectString);
                        if (ldGesamtflaecheObjekt > 0)
                        {
                            switch (liArt)
                            {
                                case 0:
                                    lsVertInfo = @"Berechnete Gesamtfläche: " + ldGesamtflaecheObjekt.ToString("0.##") + "m² / " +
                                        "Mietfläche: " + ldFlaecheTObjekt.ToString("0.##") + "m² ";
                                        // "Faktor: " + (ldFlaecheTObjekt / ldGesamtflaecheObjekt).ToString("0.##");
                                    // Infos der Beteiligten Wohnungen an der Auswahl holen
                                    // aiIdRechnung ist hier die extern timeline ID ACHTUNG!
                                    // Das hier nur in Detaillierten Abrechnung drucken
                                    if (aiDetailGrad == 2)
                                    {
                                        lsVertInfo = lsVertInfo + getObjekteAuswahl(aiIdRechnung, asConnectString);                                        
                                    }
                                    break;
                                case 1:
                                    lsVertInfo = @"Gesamtfläche: " + ldGesamtflaecheObjekt.ToString("0.##") + "m² / " +
                                        "Mietfläche: " + ldFlaecheTObjekt.ToString("0.##") + "m²"; 
                                    break;
                                default:
                                    lsVertInfo = @"Berechnete Gesamtfläche: " + ldGesamtflaecheObjekt.ToString("0.##") + "m² / " +
                                        "Mietfläche: " + ldFlaecheTObjekt.ToString("0.##") + "m²"; 
                                    break;
                            }


                        }
                    }
                    else
                    {
                        lsVertInfo = "";
                    }

                }
                else
                {
                    lsVertInfo = "";
                }

            }
            return lsVertInfo;
        }

        // Informationen über Auswahlmietflächen zusmmenstellen
        private static string getObjekteAuswahl(int aiTimelineId, string asConnectString)
        {
            string lsSql = "";
            string lsInfo = "Beteiligte Mietflächen";
            string lsBez = "";
            string lsGeschoss = "";
            string lsLage = "";
            int liOk = 0;

            // objekt_mix_parts
            lsSql = getSql(25, aiTimelineId, "", "", 0);
            liOk = fetchData(lsSql, "", 25, asConnectString);

            // schleife durch objekt_mix_parts > tableParts
            for (int i = 0; i < tableParts.Rows.Count; i++)
            {
                if (tableParts.Rows[i].ItemArray.GetValue(4) != DBNull.Value)
                    lsBez = tableParts.Rows[i].ItemArray.GetValue(4).ToString().Trim();
                if (tableParts.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                    lsGeschoss = tableParts.Rows[i].ItemArray.GetValue(10).ToString().Trim();
                if (tableParts.Rows[i].ItemArray.GetValue(11) != DBNull.Value)
                    lsLage = tableParts.Rows[i].ItemArray.GetValue(11).ToString().Trim();
                lsInfo = lsInfo + "\nBez: " + lsBez + "\nGeschoss: " + lsGeschoss + "\nLage: " + lsLage ;
            }
            return lsInfo;
        }

        // Berechnung der Fläche für die Auswahl 0 = gewählte Objekte 1 = Gesamtfläche
        private static int getObjektflaecheAuswFlag(int liObjekt, string asConnect)
        {
            int liFlag = 0;
            string lsSql = "";

            lsSql = "Select ges_fl_behalten from objekt_mix_parts where id_objekt = " + liObjekt.ToString();

            SqlConnection connect;
            connect = new SqlConnection(asConnect);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvGetFlag = command.ExecuteScalar();

                if (lvGetFlag != null && liObjekt > 0)
                {
                    int.TryParse(lvGetFlag.ToString(), out liFlag);
                }
                else
                {
                    liFlag = 0;
                }
                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es wurden kein Flag Gesamtfläche gefunden\n" +
                        "Objekt Id und Getflag:" + liObjekt.ToString() +"/" + liFlag.ToString()  + "\n",
                        "Achtung (Timeline.getObjektFlaecheAuswFlag)",
                         MessageBoxButton.OK);
            }
            return liFlag;
        }

        // Rechnungsnummer oder Rechnungstext aus RechnungesId holen 1= RgNr 2= RgText
        private static string getRgInfo(int aiIdExternTimeline, string asConnect, int aiArt)
        {
            string lsRgInfo = "";
            string lsSql = "";

            switch (aiArt)
	        {
                case 1:
                    lsSql = "Select rg_nr from rechnungen where id_extern_timeline = " + aiIdExternTimeline.ToString();
                    break;
                case 2:
                    lsSql = "Select text from rechnungen where id_extern_timeline = " + aiIdExternTimeline.ToString();
                    break;
                case 3:
                    lsSql = "Select datum_rechnung from rechnungen where id_extern_timeline = " + aiIdExternTimeline.ToString();
                    break;
                case 4:
                    lsSql = "Select id_objekt_teil from rechnungen where id_extern_timeline = " + aiIdExternTimeline.ToString();
                    break;
		        default:
                    break;
	        }

            SqlConnection connect;
            connect = new SqlConnection(asConnect);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvGetRgNr = command.ExecuteScalar();

                if (lvGetRgNr != null )
                {
                    lsRgInfo = lvGetRgNr.ToString();
                }
                else
                {
                    lsRgInfo = "";
                }
                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es wurden kein Flag Gesamtfläche gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.getRgNr)",
                         MessageBoxButton.OK);
            }
            return lsRgInfo;
        }

        // Zusammenstellen vomn Zählerinfos für die Nebenkostenabrechnung
        private static string getVerteilungsInfoZaehler(int aiIdExternTimelineZaehlerstand, string asConnectString)
        {
            string lsSql = "";
            string lsInfo = "";
            string lsZlNummer = "";
            string lsZlOrt = "";
            string lsEinheit = "";
            decimal ldVerbrauch = 0;
            decimal ldZlStand = 0;
            decimal ldKostenNetto = 0;
            decimal ldKostenBrutto = 0;
            decimal ldEinheitNetto = 0;
            decimal ldEinheitBrutto = 0;
            DateTime ldtAblesung = DateTime.MinValue;
            int liOk = 0;

            lsSql = getSql(24,aiIdExternTimelineZaehlerstand, "", "",0);
            liOk = fetchData(lsSql, "", 24, asConnectString);

            if (tableZlInfo.Rows.Count > 0)
            {
                // Es kann nur eine geben Row = 0
                if (tableZlInfo.Rows[0].ItemArray.GetValue(3) != DBNull.Value)      // Zählerstand
                    ldZlStand = (decimal)tableZlInfo.Rows[0].ItemArray.GetValue(3);
                if (tableZlInfo.Rows[0].ItemArray.GetValue(4) != DBNull.Value)      // Datum Ablesung
                    ldtAblesung = (DateTime)tableZlInfo.Rows[0].ItemArray.GetValue(4);
                if (tableZlInfo.Rows[0].ItemArray.GetValue(5) != DBNull.Value)      // Verbrauch
                    ldVerbrauch = (decimal)tableZlInfo.Rows[0].ItemArray.GetValue(5);
                if (tableZlInfo.Rows[0].ItemArray.GetValue(6) != DBNull.Value)      // Einheit Netto
                    ldEinheitNetto = (decimal)tableZlInfo.Rows[0].ItemArray.GetValue(6);
                if (tableZlInfo.Rows[0].ItemArray.GetValue(7) != DBNull.Value)      // Einheit Brutto
                    ldEinheitBrutto = (decimal)tableZlInfo.Rows[0].ItemArray.GetValue(7);
                if (tableZlInfo.Rows[0].ItemArray.GetValue(12) != DBNull.Value)      // Zählernummer
                    lsZlNummer = tableZlInfo.Rows[0].ItemArray.GetValue(12).ToString();
                if (tableZlInfo.Rows[0].ItemArray.GetValue(13) != DBNull.Value)      // Zählerort
                    lsZlOrt = tableZlInfo.Rows[0].ItemArray.GetValue(13).ToString();
                if (tableZlInfo.Rows[0].ItemArray.GetValue(14) != DBNull.Value)      // Einheit Bezeichnung
                    lsEinheit = tableZlInfo.Rows[0].ItemArray.GetValue(14).ToString();
                ldKostenNetto = ldEinheitNetto * ldVerbrauch;
                ldKostenBrutto = ldEinheitBrutto * ldVerbrauch;
            }

            lsInfo = @"Zählernummer: " + lsZlNummer + "\n" +
                      "Ort: " + lsZlOrt + "\n" +
                      "Datum der Ablesung: " + ldtAblesung.ToString("dd.MM.yyyy") + "\n" +
                      "Verbrauch: " + ldVerbrauch.ToString("0,##") + " " + lsEinheit;
                      // "Preis pro Einheit Netto: " + ldEinheitNetto.ToString("0.##") + "€\n" +
                      // "Preis pro Einheit Brutto: " + ldEinheitBrutto.ToString("0.##") + "€\n" +

            return lsInfo;
        }

        // Tabelle objekt_mix_parts leer machen
        // In der Tabelle wird nichts gelöscht      // TODO ?   Ulf!
        internal static void deleteParts(string asConnect)
        {
            string lsSql = "";

            lsSql = @"Delete from objekt_mix_parts";

            SqlConnection connect;
            connect = new SqlConnection(asConnect);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvRows = command.ExecuteScalar();
                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es konnten keine Parts gelöscht\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.deleteParts)",
                         MessageBoxButton.OK);
            }
        }

        // Informationen über Vertragsbeginn und Ende mit der Mieter id
        // Art 1 = Vertragsbeginn
        // Art 2 = Vertragsende
        private static DateTime getVertragInfo(int aiArt, DateTime adtMonat, int aiMieter, string asConnectString)
        {
            DateTime ldtVertrag = DateTime.MinValue;
            string lsSql = "";

            switch (aiArt)
            {
                case 1:
                    lsSql = @"Select datum_von from vertrag where vertrag.id_mieter = " + aiMieter.ToString() +
                             " and Month(vertrag.datum_von) = Month(Convert(DateTime," + "\'" + adtMonat + "',104))" +
                             " and Year(vertrag.datum_von) = Year(Convert(DateTime," + "\'" + adtMonat + "',104))";
                    break;
                case 2:
                    lsSql = @"Select datum_bis from vertrag where vertrag.id_mieter = " + aiMieter.ToString() +
                            " and Month(vertrag.datum_bis) = Month(Convert(DateTime," + "\'" + adtMonat + "',104))" +
                            " and Year(vertrag.datum_bis) = Year(Convert(DateTime," + "\'" + adtMonat + "',104))";
                    break;
                default:
                    break;
            }

            SqlConnection connect;
            connect = new SqlConnection(asConnectString);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvGetVertragsDatum = command.ExecuteScalar();

                if (lvGetVertragsDatum != null)
                {
                    DateTime.TryParse(lvGetVertragsDatum.ToString(),out ldtVertrag);
                }
                else
                {
                    ldtVertrag = DateTime.MinValue;
                }
                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es wurden kein Datum aus Vertrag  gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.getVertragInfo)",
                         MessageBoxButton.OK);
            }

            return ldtVertrag;
        }

        // Vertragsinfos vom Mieter art 1 = Teilbjekt
        private static int getVertragInfoFromMieter(int liIdMieter, string asConnect, int aiArt)
        {
            string lsSql = "";
            int liInfo = 0;

            switch (aiArt)
            {
                case 1:
                    lsSql = @"Select id_objekt_teil from vertrag where vertrag.id_mieter = " + liIdMieter.ToString();
                    break;
                  default:
                    break;
            }

            SqlConnection connect;
            connect = new SqlConnection(asConnect);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvGetInfo = command.ExecuteScalar();

                if (lvGetInfo != null)
                {
                    Int32.TryParse(lvGetInfo.ToString(),out liInfo);
                }
                else
                {
                    liInfo = 0;
                }
                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es wurden keine Info aus Vertrag  gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.getVertragInfoFromMieter)",
                         MessageBoxButton.OK);
            }

            return liInfo;
        }

        // Rechnungsnummer für Anschreiben aus dem Pool besorgen
        private static int getRgNrFromPool(string asConnect)
        {
            string lsSql = "select id_rg_nr from rgnr Where flag_besetzt != 1 Order by id_rg_nr";
            int liInfo = 0;



            SqlConnection connect;
            connect = new SqlConnection(asConnect);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvGetInfo = command.ExecuteScalar();

                if (lvGetInfo != null)
                {
                    Int32.TryParse(lvGetInfo.ToString(), out liInfo);
                }
                else
                {
                    liInfo = 0;
                }
                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es wurden keine Rechnungsnummer aus dem Pool  gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.getRgNrFromPool)",
                         MessageBoxButton.OK);
            }

            return liInfo;
        }

        // Rechnungsnummer aus dem Pool als besetzt kennzeichnen
        private static int setRgNrFromPool(int liIdRgNr, string asConnect)
        {
            string lsSql = "";
            int liOk = 0;

            lsSql = @"Update rgnr Set rgnr.flag_besetzt = 1 where rgnr.id_rg_nr = " + liIdRgNr.ToString();

            SqlConnection connect;
            connect = new SqlConnection(asConnect);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvRows = command.ExecuteScalar();
                liOk = 1;
                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es konnte keine Rechnungsnummer auf besetzt gesetzt werden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.setRgNrFromPool)",
                         MessageBoxButton.OK);
            }
            return liOk;
        }

        // Die ID der Rechnungsnummer Anschreiben in Timeline einsetzen
        private static int setRgNrToTml(int aiIdRgNr, string asSqlRgNr, string asConnect)
        {
            string lsSql = "";
            int liOk = 0;

            lsSql = @"Update timeline Set timeline.id_rg_nr = " + aiIdRgNr.ToString() + asSqlRgNr;

            SqlConnection connect;
            connect = new SqlConnection(asConnect);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvRows = command.ExecuteScalar();
                liOk = 1;
                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es konnte keine Rechnungsnummer auf besetzt gesetzt werden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (Timeline.setRgNrFromPool)",
                         MessageBoxButton.OK);
            }
            return liOk;
        }

    }
}