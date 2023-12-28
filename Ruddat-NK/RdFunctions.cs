using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Xml;
using MySql.Data.MySqlClient;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace Ruddat_NK
{
    public class Timeline
    {
        static string lsSql = "";

        // Datensätze Rechnungen
        static DataTable tableRechnungen;          // Rechnungen
        static DataTable tableRechnungenTimeline;      // Rechnungen für TimelineCreate
        // static DataTable tableTwo;
        static DataTable tableNewTimeline;
        static DataTable tableTimeLineSet;
        static DataTable tableObjektParts;
        static DataTable tableTimelineGet;
        static DataTable tableTaxGet;
        static DataTable tableTimeLineGet;
        static DataTable tableZlg;
        static DataTable tableZlgNew;
        static DataTable tableTml;
        static DataTable tableRgId;
        static DataTable tableConSumObj;
        static DataTable tableConSumObjT;
        static DataTable tableCnt;
        static DataTable tableCntNew;
        static DataTable tableZlInfo;
        // static DataTable tableObjTeil;       // Objektteile
        static DataTable tableParts;            // objekt_mix_parts
        static DataTable tableTmlCheckRgNr;     // Hier checken, ob schon eine Rechnungsnmmerfür das Anschreiben drin ist
        static DataTable tableTimeline;         // Timeline
        static DataTable tableTimeline1;        // Kosten des Objektes darstellen 
        static DataTable tableContent;          // Content
        static SqlDataAdapter sda;
        static SqlDataAdapter sdb;
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

        // Definieren eines Delegates
        public delegate void NachrichtenEventHandler(string nachricht);

        // Definieren eines Events
        public event NachrichtenEventHandler NachrichtGesendet;

        // Methode, die das Event auslöst
        public void SendeNachricht(string nachricht)
        {
            NachrichtGesendet?.Invoke(nachricht);
        }

        // ----------------------------------------------------------------------------------------------
        // Bisher höchste Id für Timeline ermitteln
        public static int getTimelineId(string asConnect, int asArt, int aiDb)
        {
            Int32 liGetLastTempId = 0;

            lsSql = getSql(26, asArt, "", "", 0);
            liGetLastTempId = Timeline.fetchData(lsSql, "", 26, asConnect, aiDb);
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
        // Zählerstände
        // Flag = 21 > ändern
        // Flag = 22 > löschen
        public static void editTimeline(int liTimelineId, int liFlagAdd, string asConnect, int aiDb)
        {
            string lsSql = "";
            int liRows = 0;
            int liOk = 0;

            switch (liFlagAdd)
            {
                case 1:
                    // Rechnungen Daten holen mit id extern timeline
                    lsSql = Timeline.getSql(1, liTimelineId, "", "", 0);     // Rechnungen
                    liRows = Timeline.fetchData(lsSql, "", 1, asConnect, aiDb);  // TableOne
                    break;
                case 2:
                    // Rechnung Timeline löschen
                    liOk = Timeline.TimelineDelete(liTimelineId, "R", asConnect, aiDb);
                    break;
                case 11:
                    // Zahlungen Daten holen mit id extern timeline
                    lsSql = Timeline.getSql(12, liTimelineId, "", "", 0);
                    // Sql, Art = 11 
                    liRows = Timeline.fetchData(lsSql, "", 11, asConnect, aiDb);
                    break;
                case 12:
                    // Zahlungen Timeline löschen 
                    liOk = Timeline.TimelineDelete(liTimelineId, "A", asConnect, aiDb);
                    break;
                case 13:
                    // Zahlungen importieren. Nur anderes SQL Statement, sonst wie Case 11
                    lsSql = Timeline.getSql(13, liTimelineId, "", "", 0);
                    // Sql, Art = 11 
                    liRows = Timeline.fetchData(lsSql, "", 11, asConnect, aiDb);
                    break;
                case 21:
                    // Zählerstände Daten holen mit id extern timeline
                    lsSql = Timeline.getSql(21, liTimelineId, "", "", 0);
                    // Sql, Art = 21 
                    liRows = Timeline.fetchData(lsSql, "", 21, asConnect, aiDb);
                    break;
                case 22:
                    // Zählerstände Timeline löschen
                    liOk = Timeline.TimelineDelete(liTimelineId, "Z", asConnect, aiDb);
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

            switch (piArt)
            {
                case 1:
                    // Rechnungen mit definierter id_extern_timeline
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
                    break;
                case 200:
                    // Timeline löschen Rechnung
                    lsWhereAdd = piId.ToString() + " ";

                    lsSql = @"delete from timeline
					        where id_rechnung = " + lsWhereAdd;
                    break;
                case 201:
                    // Timeline löschen Zahlung
                    lsWhereAdd = piId.ToString() + " ";

                    lsSql = @"delete from timeline
					        where id_vorauszahlung = " + lsWhereAdd;
                    break;
                case 202:
                    // Timeline löschen Zählerstand
                    lsWhereAdd = piId.ToString() + " ";

                    lsSql = @"delete from timeline
					        where id_zaehlerstand = " + lsWhereAdd;
                    break;
                case 3:
                    // Timeline neu erzeugen in ps2 steht, welches Feld beschrieben werden soll
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
                    break;
                case 31:
                    // Timeline neu erzeugen in ps2 steht, welches Feld beschrieben werden soll
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
                            where " + ps2 + " = " + lsWhereAdd;
                    break;
                case 4:
                    // TimelineRelations sollen geschrieben werden
                    // Hier auf Grundlage des Objektes
                    // Beschrieben werden die Kosten für Objektteile
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
                    break;
                case 50:                // Rechnungen
                    // TimelineRelations sollen geschrieben werden
                    // Hier auf Grundlage des ObjektTeils
                    // Beschrieben werden die Kosten für Mieter
                    lsWhereAdd = " id_rechnung = " + piId.ToString() + " ";
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
                                where " + lsWhereAdd + " and " + lsWhereAdd2 + "order by id_objekt_teil, dt_monat";
                    break;
                case 51:            // Zahlungen
                    // TimelineRelations sollen geschrieben werden
                    // Hier auf Grundlage des ObjektTeils
                    // Beschrieben werden die Kosten für Mieter
                    lsWhereAdd = " id_vorauszahlung = " + piId.ToString() + " ";
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
                                where " + lsWhereAdd + " and " + lsWhereAdd2 + "order by dt_monat";
                    break;
                case 52:        // Zähler
                    // TimelineRelations sollen geschrieben werden
                    // Hier auf Grundlage des ObjektTeils
                    // Beschrieben werden die Kosten für Mieter
                    lsWhereAdd = " id_zaehlerstand = " + piId.ToString() + " ";
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
                                where " + lsWhereAdd + " and " + lsWhereAdd2 + "order by dt_monat";
                    break;
                case 6:
                    // für die TimelineRelation Objektteile holen
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
                    break;
                case 7:
                    lsWhereAdd = "id_mieter = " + piId.ToString() + " ";
                    lsSql = @"select id_mieter,
                                id_vertrag,
                                bez
                            from mieter
                            where " + lsWhereAdd;
                    break;
                case 8:
                    lsWhereAdd = "Id_mwst_art = " + piId.ToString() + " ";
                    lsSql = @"select Id_mwst_art,
                                 bez,
                                 mwst
                            from art_mwst
                            where " + lsWhereAdd;
                    break;
                case 9:
                    // MwstSatz holen Bezeichnung ist bekannt Bsp. "normal"
                    lsWhereAdd = "bez = " + " \'" + ps2 + "\' ";
                    lsSql = @"select Id_mwst_art,
                                 bez,
                                 mwst
                            from art_mwst
                            where " + lsWhereAdd;
                    break;
                case 11:
                    // Zahlungen
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
                    break;
                case 12:
                    // Zahlungen mit definierter Timeline
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
                    break;
                case 13:
                    // Zahlungen aus automatischem Import. Alle mit flag_timeline = 1 und der übergebenen Import ID
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
                    break;
                case 21:
                    // Zählerstände mit definierter Timeline
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
                    break;
                case 24:
                    // Zählerinfo für Report Nebenkosten holen
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
                    break;
                case 25:
                    // Zusammenstellungen der gewählten Wohnungen für den Report Nebenkosten
                    lsSql = @"select Id_obj_mix_parts,id_objekt_mix,id_objekt,id_objekt_teil,bez,sel,flaeche_anteil,    
                                id_timeline,ges_fl_behalten,erklaerung,geschoss,lage
                                    from objekt_mix_parts";
                    lsWhereAdd = " where sel > 0 and id_timeline = " + piId.ToString() + " ";
                    // lsWhereAdd2 = " and id_objekt = " + piId2.ToString() + " ";
                    lsSql = lsSql + lsWhereAdd + lsWhereAdd2;
                    break;
                case 26:
                    // Max Ids ermitteln
                    switch (piId)
                    {
                        case 1:
                            lsSql = "Select max(id_extern_timeline) from rechnungen";
                            break;
                        case 2:
                            lsSql = "Select max(id_extern_timeline) from zahlungen";
                            break;
                        case 3:
                            lsSql = "Select max(id_extern_timeline) from zaehlerstaende";
                            break;
                        default:
                            break;
                    }
                    break;
                case 27:
                    lsSql = "Select id_objekt_teil from objekt_mix_parts where sel = 1 and id_objekt_teil = " + piId.ToString();
                    break;
                case 28:
                    switch (piId2)
                    {
                        case 1:
                            // Weiterleitung an Objektteil
                            lsSql = @"Select art_kostenart.wtl_obj_teil from timeline 
                                join art_kostenart on timeline.id_ksa = art_kostenart.id_ksa
                                Where timeline.id_rechnung = " + piId.ToString();
                            break;
                        // Weiterleitung an Mieter
                        case 2:
                            lsSql = @"Select art_kostenart.wtl_mieter from timeline 
                                join art_kostenart on timeline.id_ksa = art_kostenart.id_ksa
                                Where timeline.id_rechnung = " + piId.ToString();
                            break;
                        default:
                            break;
                    }
                    break;
                case 29:
                    lsSql = @"select mieter.Id_mieter as mid
                            from objekt_teil
                        join objekt on objekt_teil.id_objekt = objekt.Id_objekt
                        Join filiale on filiale.id_filiale = objekt.Id_filiale
                        join mieter on mieter.id_filiale = filiale.Id_Filiale
                            where mieter.leerstand = 1 and objekt_teil.Id_objekt_teil = " + piId.ToString();
                    break;
                case 291:
                    lsSql = @"select mieter.Id_mieter as mid
                            from objekt_teil
                        join objekt on objekt_teil.id_objekt = objekt.Id_objekt
                        Join filiale on filiale.id_filiale = objekt.Id_filiale
                        join mieter on mieter.id_filiale = filiale.Id_Filiale
                            where mieter.leerstand = 1 and objekt.Id_objekt = " + piId.ToString();
                    break;
                case 30:        // Kostenstellenart Zähler
                    switch (piId)
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
                    break;
                case 32:        // Die VerteilungsId aus Rechnungen ermitteln
                    lsSql = @"Select id_verteilung From rechnungen Where id_extern_timeline = " + piId.ToString();
                    break;
                case 33:        // Verteilungs ID aus art_verteilung ermitteln
                    lsSql = @"Select id_verteilung From art_verteilung Where kb = '" + ps2.ToString() + "'";
                    break;
                case 34:        // Aus den Verträgen die Teilobjekt ID anhand der Mieter ID ermitteln
                    lsSql = @"Select id_objekt_teil From vertrag Where id_mieter = " + piId.ToString();
                    break;
                case 35:       // Die Objekt ID aus den Vertragsdaten ermitteln aus der Mieter Id = 1 oder der Teilobjekt ID = 2
                    switch (piId2)
                    {
                        case 1:
                            lsSql = @"Select id_objekt From vertrag Where id_mieter = " + piId.ToString();
                            break;
                        case 2:
                            lsSql = @"Select id_objekt From vertrag Where id_objekt_teil = " + piId.ToString();
                            break;
                        default:
                            break;
                    }
                    break;
                case 36:        // Report löschen
                    lsSql = "delete from x_abr_content;";
                    break;
                case 37:        // Zähler Id
                    lsSql = @"select id_zaehler from zaehler where zaehlernummer = '" + ps2.Trim() + "\'";
                    break;
                case 38:        // Mwst Satz Zähler
                    lsSql = @"Select art_mwst.mwst from zaehler 
                        left join art_mwst on zaehler.id_mwst_art = art_mwst.Id_mwst_art
                      where id_zaehler = " + piId.ToString();
                    break;
                case 39:
                    lsSql = @"insert into objekt_mix_parts (Id_objekt_teil,id_objekt,flaeche_anteil,bez,geschoss,lage)
                            select Id_objekt_teil,id_objekt,flaeche_anteil,bez,geschoss,lage from objekt_teil";
                    lsWhereAdd = " where objekt_teil.id_objekt = " + piId.ToString() + " ";
                    lsSql = lsSql + lsWhereAdd;
                    break;
                case 40:
                    lsSql = @"Select Count(*) from objekt_mix_parts";
                    lsWhereAdd = " where id_timeline = " + piId.ToString() + " ";
                    lsSql = lsSql + lsWhereAdd;
                    break;
                case 41:
                    lsSql = "Select ges_fl_behalten from objekt_mix_parts where id_objekt = " + piId.ToString();
                    break;
                case 42:
                    lsSql = @"Delete from objekt_mix_parts";
                    break;
                case 43:
                    lsSql = @"Select id_objekt_teil from vertrag where vertrag.id_mieter = " + piId.ToString();
                    break;
                case 44:
                    lsSql = @"select id_rg_nr from rgnr Where flag_besetzt != 1 Order by id_rg_nr";
                    break;
                case 45:
                    lsSql = @"Update rgnr Set rgnr.flag_besetzt = 1 where rgnr.id_rg_nr = " + piId.ToString();
                    break;
                case 46:
                    lsSql = @"Update timeline Set timeline.id_rg_nr = " + piId.ToString() + ps2;
                    break;
                case 47:
                    lsSql = @"Select Id_verteilung from art_verteilung Where kb = '" + ps2 + "' ";
                    break;
                case 48:
                    lsSql = @"Select id_mandant,sel from mandanten Where sel = 1 ";
                    break;
                case 49:
                    lsSql = @"Select id_filiale From filiale Where id_mandant = " + piId.ToString(); ;
                    break;
                default:
                    break;
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
                        lsWhereAdd = " Where timeline.id_rechnung = " + piId.ToString() + " and timeline.id_objekt > 0 ";                           // Objekte
                        break;
                    case 15:
                        lsWhereAdd = @" Where timeline.id_rechnung = " + piId.ToString() + " and timeline.id_objekt_teil = " + piId2.ToString()
                                        + " And timeline.id_mieter = 0 ";                                                                           // Teilobjekte
                        break;
                    case 16:
                        lsWhereAdd = " Where timeline.id_zaehlerstand = " + piId.ToString() + " and timeline.id_objekt > 0 ";                       // Objekte
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

            return lsSql;
        }

        // Daten aus der Db holen
        public static Int32 fetchData(string psSql, string psSql2, int piArt, string asConnect, int aiDb)
        {
            DateTime ldtStart = DateTime.MinValue;
            DateTime ldtEnd = DateTime.MinValue;
            DateTime ldtMonat = DateTime.MinValue;
            DateTime ldtVertrag = DateTime.MinValue;

            int liExternId;
            int liOk = 0;

            decimal[] ladBetraege = new decimal[12];

            Int32 liReturn = 0;

            // Datenbankwahl 1=MsSql 2= Mysql
            switch (aiDb)
            {
                case 1:             //-------------------------MsSql
                    try
                    {
                        SqlConnection connect;
                        connect = new SqlConnection(asConnect);
                        SqlCommand command = new SqlCommand(psSql, connect);
                        connect.Open();

                        switch (piArt)
                        {
                            case 1:     // Rechnungen > Timeline erzeugen bearbeiten
                                tableRechnungen = new DataTable();         // Rechnung 
                                sda = new SqlDataAdapter(command);
                                sda.Fill(tableRechnungen);
                                liOk = MakeAfterFetch(piArt, 1, 0, 0, asConnect, aiDb);
                                break;
                            case 2:     // Timeline löschen
                                SqlDataReader queryCommandReader = command.ExecuteReader();
                                break;
                            case 3:     // Rechnungen + Timeline Create
                                tableRechnungenTimeline = new DataTable();         // Rechnungen
                                SqlCommand command3 = new SqlCommand(psSql2, connect);
                                sdb = new SqlDataAdapter(command3);
                                sdb.Fill(tableRechnungenTimeline);
                                // Externe ID aus der Rechnung ermitteln 
                                liExternId = MakeAfterFetch(piArt, 1, 0, 0, asConnect, aiDb);

                                // Timeline neue Datensätze erzeugen
                                tableNewTimeline = new DataTable();
                                SqlCommand command31 = new SqlCommand(psSql, connect);
                                sdc = new SqlDataAdapter(command31);
                                sdc.Fill(tableNewTimeline);
                                liExternId = MakeAfterFetch(piArt, 2, liExternId, 0, asConnect, aiDb);

                                break;
                            case 4:     // Rechnungen Timeline Create Relations Objektteile schreiben
                                // tableFive beiinhaltet die Objektteile zu einem gewählten Objekt
                                SqlCommand command6 = new SqlCommand(psSql2, connect);
                                tableObjektParts = new DataTable();
                                sde = new SqlDataAdapter(command6);
                                sde.Fill(tableObjektParts);
                                // tableFive ist jetzt mit allen Objektteilen zum Objekt gefüllt

                                // tableSix: Holen der Timeline
                                SqlCommand command5 = new SqlCommand(psSql, connect);
                                tableTimelineGet = new DataTable();
                                sdf = new SqlDataAdapter(command5);
                                sdf.Fill(tableTimelineGet);

                                // tableFour Timeline schreiben
                                SqlCommand command7 = new SqlCommand(psSql, connect);
                                tableTimeLineSet = new DataTable();
                                sdc = new SqlDataAdapter(command7);
                                sdc.Fill(tableTimeLineSet);
                                liOk = MakeAfterFetch(piArt, 0, 0, 0, asConnect, aiDb);
                                break;
                            case 5:     // Rechnungen Timeline Create Relations Mieter schreiben
                                // Vorhandene Timeline einlesen
                                SqlCommand command9 = new SqlCommand(psSql, connect);
                                tableTimeLineGet = new DataTable();
                                sdh = new SqlDataAdapter(command9);
                                sdh.Fill(tableTimeLineGet);

                                // Timeline neue Datensätze erzeugen
                                SqlCommand command8 = new SqlCommand(psSql, connect);
                                tableNewTimeline = new DataTable();
                                sdc = new SqlDataAdapter(command8);
                                sdc.Fill(tableNewTimeline);
                                // Schleife durch Timeline
                                liOk = MakeAfterFetch(piArt, 0, 0, 0, asConnect, aiDb);
                                break;
                            case 8:     // Mwst Satz holen
                                sdg = new SqlDataAdapter(command);
                                tableTaxGet = new DataTable();
                                sdg.Fill(tableTaxGet);
                                liReturn = MakeAfterFetch(piArt, 0, 0, 0, asConnect, aiDb);
                                break;
                            case 11:    // Zahlungen > Timeline erzeugen bearbeiten
                                tableZlg = new DataTable();         // Zahlungen
                                sdZlg = new SqlDataAdapter(command);
                                sdZlg.Fill(tableZlg);
                                liOk = MakeAfterFetch(piArt, 0, 0, 0, asConnect, aiDb);
                                break;
                            case 13:        // Zahlungen Timeline neu erzeugen
                                tableZlgNew = new DataTable();         // Zahlungen
                                SqlCommand command13 = new SqlCommand(psSql2, connect);
                                sdZlgNew = new SqlDataAdapter(command13);
                                sdZlgNew.Fill(tableZlgNew);
                                liExternId = MakeAfterFetch(piArt, 1, 0, 0, asConnect, aiDb);

                                // Timeline neue Datensätze erzeugen
                                SqlCommand command131 = new SqlCommand(psSql, connect);
                                tableTml = new DataTable();
                                sdTml = new SqlDataAdapter(command131);
                                sdTml.Fill(tableTml);
                                liOk = MakeAfterFetch(piArt, 2, 0, 0, asConnect, aiDb);
                                break;
                            case 14:        // Summen aus Objekt für Report Content
                                tableConSumObj = new DataTable();
                                sdConSumObj = new SqlDataAdapter(command);
                                sdConSumObj.Fill(tableConSumObj);
                                break;
                            case 15:        // Summen aus ObjektTeil für Report Content
                                tableConSumObjT = new DataTable();
                                sdConSumObjT = new SqlDataAdapter(command);
                                sdConSumObjT.Fill(tableConSumObjT);
                                break;
                            case 16:        // Die Rechnungs Id aus der Timeline ermitteln
                                tableRgId = new DataTable();
                                sdRgId = new SqlDataAdapter(command);
                                sdRgId.Fill(tableRgId);
                                liOk = MakeAfterFetch(piArt, 0, 0, 0, asConnect, aiDb);
                                break;
                            case 21:                               // Zählerstände
                                tableCnt = new DataTable();
                                sdCnt = new SqlDataAdapter(command);
                                sdCnt.Fill(tableCnt);
                                liOk = MakeAfterFetch(piArt, 0, 0, 0, asConnect, aiDb);
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
                                liOk = MakeAfterFetch(piArt, 0, 0, 0, asConnect, aiDb);
                                break;
                            case 24:            // Zählerinformationen für Report Nebenkostenabrechnungen
                                tableZlInfo = new DataTable();
                                sdZlInfo = new SqlDataAdapter(command);
                                sdZlInfo.Fill(tableZlInfo);
                                break;
                            case 25:            // Zählerinformationen für Report Nebenkostenabrechnungen
                                tableParts = new DataTable();
                                sdParts = new SqlDataAdapter(command);
                                sdParts.Fill(tableParts);
                                break;
                            case 26:            // Bisher höchste ID ermitteln
                                var lvGetId = command.ExecuteScalar();
                                if (lvGetId != null)
                                {
                                    Int32.TryParse(lvGetId.ToString(), out liReturn);
                                }
                                else
                                {
                                    liReturn = 0;
                                }
                                break;
                            case 27:        // Check Rechnungsnummer
                                tableTmlCheckRgNr = new DataTable();
                                SqlCommand command27 = new SqlCommand(psSql, connect);
                                // Create a SqlDataReader
                                SqlDataReader queryCommandReader27 = command27.ExecuteReader();
                                tableTmlCheckRgNr.Load(queryCommandReader27);
                                break;
                            case 28:
                                // Erste Tabelle Timeline holen
                                tableTimeline = new DataTable();
                                SqlCommand command28 = new SqlCommand(psSql, connect);
                                SqlDataReader queryCommandReader28 = command28.ExecuteReader();
                                tableTimeline.Load(queryCommandReader28);
                                break;
                            case 29:
                                // Zweite Tabelle Timeline ObjektKostendarstellung 
                                tableTimeline1 = new DataTable();
                                SqlCommand command29 = new SqlCommand(psSql, connect);
                                SqlDataReader queryCommandReader29 = command29.ExecuteReader();
                                tableTimeline1.Load(queryCommandReader29);
                                break;
                            case 30:
                                // ReportContent füllen
                                tableContent = new DataTable();
                                SqlCommand command30 = new SqlCommand(psSql, connect);
                                adp = new SqlDataAdapter(command);
                                SqlDataReader queryCommandReader30 = command30.ExecuteReader();
                                tableContent.Load(queryCommandReader30);
                                break;
                            case 31:
                                // ReportContent Ab in die Datenbank
                                SqlCommandBuilder commandBuilder31 = new SqlCommandBuilder(adp);
                                adp.Update(tableContent);
                                break;
                            case 32:
                                // Timeline
                                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(sdTml);
                                sdTml.Update(tableTml);
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
                        MessageBox.Show("Verarbeitungsfehler ERROR fetchdata RdFunctions MsSql \n piArt = " + piArt.ToString(),
                                "Achtung");
                    }
                    break;
                case 2:
                    try
                    {
                        MySqlConnection connect;
                        connect = new MySqlConnection(asConnect);
                        MySqlCommand command = new MySqlCommand(psSql, connect);
                        connect.Open();

                        switch (piArt)
                        {
                            case 1:     // Rechnungen > Timeline erzeugen bearbeiten
                                tableRechnungen = new DataTable();         // Rechnung 
                                mysda = new MySqlDataAdapter(command);
                                mysda.Fill(tableRechnungen);
                                liOk = MakeAfterFetch(piArt, 1, 0, 0, asConnect, aiDb);
                                break;

                            case 2:     // Timeline löschen
                                MySqlDataReader queryCommandReader = command.ExecuteReader();
                                break;
                            case 3:     // Rechnungen Timeline Create
                                tableRechnungenTimeline = new DataTable();         // Rechnungen
                                MySqlCommand command3 = new MySqlCommand(psSql2, connect);
                                mysdb = new MySqlDataAdapter(command3);
                                mysdb.Fill(tableRechnungenTimeline);
                                // Externe ID aus der Rechnung ermitteln 
                                liExternId = MakeAfterFetch(piArt, 1, 0, 0, asConnect, aiDb);

                                // Timeline neue Datensätze erzeugen
                                tableNewTimeline = new DataTable();
                                MySqlCommand command31 = new MySqlCommand(psSql, connect);
                                mysdc = new MySqlDataAdapter(command31);
                                mysdc.Fill(tableNewTimeline);
                                liExternId = MakeAfterFetch(piArt, 2, liExternId, 0, asConnect, aiDb);
                                break;
                            case 4:     // Rechnungen Timeline Create Relations Objektteile schreiben
                                // tableFive beiinhaltet die Objektteile zu einem gewählten Objekt
                                MySqlCommand command6 = new MySqlCommand(psSql2, connect);
                                tableObjektParts = new DataTable();
                                mysde = new MySqlDataAdapter(command6);
                                mysde.Fill(tableObjektParts);
                                // tableFive ist jetzt mit allen Objektteilen zum Objekt gefüllt

                                // tableSix: Holen der Timeline
                                MySqlCommand command5 = new MySqlCommand(psSql, connect);
                                tableTimelineGet = new DataTable();
                                mysdf = new MySqlDataAdapter(command5);
                                mysdf.Fill(tableTimelineGet);

                                // tableFour Timeline schreiben
                                MySqlCommand command7 = new MySqlCommand(psSql, connect);
                                tableTimeLineSet = new DataTable();
                                mysdc = new MySqlDataAdapter(command7);
                                mysdc.Fill(tableTimeLineSet);

                                liOk = MakeAfterFetch(piArt, 0, 0, 0, asConnect, aiDb);
                                break;
                            case 5:     // Rechnungen Timeline Create Relations Mieter schreiben
                                // Vorhandene Timeline einlesen
                                MySqlCommand command9 = new MySqlCommand(psSql, connect);
                                tableTimeLineGet = new DataTable();
                                mysdh = new MySqlDataAdapter(command9);
                                mysdh.Fill(tableTimeLineGet);

                                // Timeline neue Datensätze erzeugen
                                MySqlCommand command8 = new MySqlCommand(psSql, connect);
                                tableNewTimeline = new DataTable();
                                mysdc = new MySqlDataAdapter(command8);
                                mysdc.Fill(tableNewTimeline);
                                // Schleife durch Timeline
                                liOk = MakeAfterFetch(piArt, 0, 0, 0, asConnect, aiDb);
                                break;
                            case 8:     // Mwst Satz holen
                                mysdg = new MySqlDataAdapter(command);
                                tableTaxGet = new DataTable();
                                mysdg.Fill(tableTaxGet);
                                liReturn = MakeAfterFetch(piArt, 0, 0, 0, asConnect, aiDb);
                                break;
                            case 11:    // Zahlungen > Timeline erzeugen bearbeiten
                                tableZlg = new DataTable();         // Zahlungen
                                mysdZlg = new MySqlDataAdapter(command);
                                mysdZlg.Fill(tableZlg);
                                liOk = MakeAfterFetch(piArt, 0, 0, 0, asConnect, aiDb);
                                break;
                            case 13:        // Zahlungen Timeline neu erzeugen
                                tableZlgNew = new DataTable();         // Zahlungen
                                MySqlCommand command13 = new MySqlCommand(psSql2, connect);
                                mysdZlgNew = new MySqlDataAdapter(command13);
                                mysdZlgNew.Fill(tableZlgNew);
                                liExternId = MakeAfterFetch(piArt, 1, 0, 0, asConnect, aiDb);

                                // Timeline neue Datensätze erzeugen
                                MySqlCommand command131 = new MySqlCommand(psSql, connect);
                                tableTml = new DataTable();
                                mysdTml = new MySqlDataAdapter(command131);
                                mysdTml.Fill(tableTml);
                                liOk = MakeAfterFetch(piArt, 2, 0, 0, asConnect, aiDb);
                                break;
                            case 14:        // Summen aus Objekt für Report Content
                                tableConSumObj = new DataTable();
                                mysdConSumObj = new MySqlDataAdapter(command);
                                mysdConSumObj.Fill(tableConSumObj);
                                break;
                            case 15:        // Summen aus ObjektTeil für Report Content
                                tableConSumObjT = new DataTable();
                                mysdConSumObjT = new MySqlDataAdapter(command);
                                mysdConSumObjT.Fill(tableConSumObjT);
                                break;
                            case 16:        // Die Rechnungs Id aus der Timeline ermitteln
                                tableRgId = new DataTable();
                                mysdRgId = new MySqlDataAdapter(command);
                                mysdRgId.Fill(tableRgId);
                                liOk = MakeAfterFetch(piArt, 0, 0, 0, asConnect, aiDb);
                                break;
                            case 21:                               // Zählerstände
                                tableCnt = new DataTable();
                                mysdCnt = new MySqlDataAdapter(command);
                                mysdCnt.Fill(tableCnt);
                                liOk = MakeAfterFetch(piArt, 0, 0, 0, asConnect, aiDb);
                                break;
                            case 23:        // Zählerstände Timeline Create
                                tableCntNew = new DataTable();         // Zahlungen
                                MySqlCommand command23 = new MySqlCommand(psSql2, connect);
                                mysdCntNew = new MySqlDataAdapter(command23);
                                mysdCntNew.Fill(tableCntNew);
                                // Timeline neue Datensätze erzeugen
                                MySqlCommand command231 = new MySqlCommand(psSql, connect);
                                tableTml = new DataTable();
                                mysdTml = new MySqlDataAdapter(command231);
                                mysdTml.Fill(tableTml);
                                liOk = MakeAfterFetch(piArt, 0, 0, 0, asConnect, aiDb);
                                break;
                            case 24:            // Zählerinformationen für Report Nebenkostenabrechnungen
                                tableZlInfo = new DataTable();
                                mysdZlInfo = new MySqlDataAdapter(command);
                                mysdZlInfo.Fill(tableZlInfo);
                                break;
                            case 25:            // Zählerinformationen für Report Nebenkostenabrechnungen
                                tableParts = new DataTable();
                                mysdParts = new MySqlDataAdapter(command);
                                mysdParts.Fill(tableParts);
                                break;
                            case 26:            // ID ermitteln Allgemein
                                var lvGetId = command.ExecuteScalar();
                                if (lvGetId != null)
                                {
                                    Int32.TryParse(lvGetId.ToString(), out liReturn);
                                }
                                else
                                {
                                    liReturn = 0;
                                }
                                break;
                            case 27:    // Hier checken, ob schon eine Rechnungsnmmerfür das Anschreiben drin ist
                                tableTmlCheckRgNr = new DataTable();
                                MySqlCommand command271 = new MySqlCommand(psSql, connect);
                                // Create a SqlDataReader
                                MySqlDataReader queryCommandReader271 = command271.ExecuteReader();
                                tableTmlCheckRgNr.Load(queryCommandReader271);
                                break;
                            case 28:
                                // Erste Tabelle Timeline holen
                                tableTimeline = new DataTable();
                                MySqlCommand command281 = new MySqlCommand(psSql, connect);
                                // Create a SqlDataReader
                                MySqlDataReader queryCommandReader281 = command281.ExecuteReader();
                                // Create a DataTable object to hold all the data returned by the query.
                                tableTimeline.Load(queryCommandReader281);
                                break;
                            case 29:
                                // Zweite Tabelle Timeline ObjektKostendarstellung (Zähler)
                                tableTimeline1 = new DataTable();
                                MySqlCommand command291 = new MySqlCommand(psSql, connect);
                                MySqlDataReader queryCommandReader291 = command291.ExecuteReader();
                                tableTimeline1.Load(queryCommandReader291);
                                break;
                            case 30:
                                // ReportContent füllen
                                tableContent = new DataTable();
                                MySqlCommand command301 = new MySqlCommand(psSql, connect);
                                myadp = new MySqlDataAdapter(command301);
                                MySqlDataReader queryCommandReader301 = command301.ExecuteReader();
                                tableContent.Load(queryCommandReader301);
                                break;
                            case 31:
                                // ReportContent Ab in die Datenbank
                                MySqlCommandBuilder commandBuilder31 = new MySqlCommandBuilder(myadp);
                                myadp.Update(tableContent);
                                break;
                            case 32:
                                // Timeline update
                                MySqlCommandBuilder commandBuilder32 = new MySqlCommandBuilder(mysdTml);
                                mysdTml.Update(tableTml);
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
                        MessageBox.Show("Verarbeitungsfehler ERROR fetchdata RdFunctions MySql \n piArt = " + piArt.ToString(),
                                "Achtung");
                    }
                    break;
                default:
                    break;
            }
            return (liReturn);
        }

        // Daten aus der Db holen hier nur Dezimalwerte
        public static decimal fetchDataDecimal(string psSql, string psSql2, int piArt, string asConnectString, int aiDb)
        {
            DateTime ldtStart = DateTime.MinValue;
            DateTime ldtEnd = DateTime.MinValue;
            DateTime ldtMonat = DateTime.MinValue;
            DateTime ldtVertrag = DateTime.MinValue;

            decimal[] ladBetraege = new decimal[12];
            decimal ldReturn = 0;

            // Datenbankwahl 1=MsSql 2= Mysql
            switch (aiDb)
            {
                case 1:             //-------------------------MsSql
                    try
                    {
                        SqlConnection connect;
                        connect = new SqlConnection(asConnectString);
                        SqlCommand command = new SqlCommand(psSql, connect);
                        connect.Open();

                        if (piArt == 1)     // Dezimalwert ermitteln Allgemein
                        {
                            var lvGetId = command.ExecuteScalar();

                            if (lvGetId != null)
                            {
                                decimal.TryParse(lvGetId.ToString(), out ldReturn);
                            }
                            else
                            {
                                ldReturn = 0;
                            }
                        }
                        // db close
                        connect.Close();
                    }

                    catch
                    {
                        // Die Anwendung anhalten 
                        MessageBox.Show("Verarbeitungsfehler ERROR fetchdataDecimal RdFunctions \n piArt = " + piArt.ToString(),
                                "Achtung");
                    }
                    break;
                case 2:
                    try
                    {
                        MySqlConnection connect;
                        connect = new MySqlConnection(asConnectString);
                        MySqlCommand command = new MySqlCommand(psSql, connect);
                        connect.Open();

                        if (piArt == 1)     // Dezimalwert ermitteln Allgemein
                        {
                            var lvGetId = command.ExecuteScalar();
                            if (lvGetId != null)
                            {
                                decimal.TryParse(lvGetId.ToString(), out ldReturn);
                            }
                            else
                            {
                                ldReturn = 0;
                            }
                        }
                        // db close
                        connect.Close();
                    }
                    catch
                    {
                        // Die Anwendung anhalten 
                        MessageBox.Show("Verarbeitungsfehler ERROR fetchdataDecimal RdFunctions \n piArt = " + piArt.ToString(),
                                "Achtung");
                    }
                    break;
                default:
                    break;
            }
            return (ldReturn);
        }

        // Daten aus der Db holen hier nur Strings
        public static string fetchDataString(string psSql, string psSql2, int piArt, string asConnectString, int aiDb)
        {
            DateTime ldtStart = DateTime.MinValue;
            DateTime ldtEnd = DateTime.MinValue;
            DateTime ldtMonat = DateTime.MinValue;
            DateTime ldtVertrag = DateTime.MinValue;

            decimal[] ladBetraege = new decimal[12];
            string lsReturn = "";

            // Datenbankwahl 1=MsSql 2= Mysql
            switch (aiDb)
            {
                case 1:             //-------------------------MsSql
                    try
                    {
                        SqlConnection connect;
                        connect = new SqlConnection(asConnectString);
                        SqlCommand command = new SqlCommand(psSql, connect);
                        connect.Open();

                        if (piArt == 1)     // Dezimalwert ermitteln Allgemein
                        {
                            var lvGetId = command.ExecuteScalar();

                            if (lvGetId != null)
                            {
                                lsReturn = lvGetId.ToString().Trim();
                            }
                            else
                            {
                                lsReturn = "";
                            }
                        }
                        // db close
                        connect.Close();
                    }

                    catch
                    {
                        // Die Anwendung anhalten 
                        MessageBox.Show("Verarbeitungsfehler ERROR fetchdataString RdFunctions \n piArt = " + piArt.ToString(),
                                "Achtung");
                    }
                    break;
                case 2:
                    try
                    {
                        MySqlConnection connect;
                        connect = new MySqlConnection(asConnectString);
                        MySqlCommand command = new MySqlCommand(psSql, connect);
                        connect.Open();

                        if (piArt == 1)     // Dezimalwert ermitteln Allgemein
                        {
                            var lvGetId = command.ExecuteScalar();
                            if (lvGetId != null)
                            {
                                lsReturn = lvGetId.ToString().Trim();
                            }
                            else
                            {
                                lsReturn = "";
                            }
                        }
                        // db close
                        connect.Close();
                    }
                    catch
                    {
                        // Die Anwendung anhalten 
                        MessageBox.Show("Verarbeitungsfehler ERROR fetchdataString RdFunctions \n piArt = " + piArt.ToString(),
                                "Achtung");
                    }
                    break;
                default:
                    break;
            }
            return (lsReturn);
        }


        // Daten aus der Db holen hier nur Strings
        public static DateTime fetchDataDate(string psSql, string psSql2, int piArt, string asConnectString, int aiDb)
        {
            DateTime ldtStart = DateTime.MinValue;
            DateTime ldtEnd = DateTime.MinValue;
            DateTime ldtMonat = DateTime.MinValue;
            DateTime ldtVertrag = DateTime.MinValue;

            decimal[] ladBetraege = new decimal[12];
            DateTime ldtReturn = DateTime.MinValue;

            // Datenbankwahl 1=MsSql 2= Mysql
            switch (aiDb)
            {
                case 1:             //-------------------------MsSql
                    try
                    {
                        SqlConnection connect;
                        connect = new SqlConnection(asConnectString);
                        SqlCommand command = new SqlCommand(psSql, connect);
                        connect.Open();

                        if (piArt == 1)     // Dezimalwert ermitteln Allgemein
                        {
                            var lvGetId = command.ExecuteScalar();

                            if (lvGetId != null)
                            {
                                DateTime.TryParse(lvGetId.ToString(), out ldtReturn);
                            }
                            else
                            {
                                ldtReturn = DateTime.MinValue;
                            }
                        }
                        // db close
                        connect.Close();
                    }

                    catch
                    {
                        // Die Anwendung anhalten 
                        MessageBox.Show("Verarbeitungsfehler ERROR fetchdatadate RdFunctions \n piArt = " + piArt.ToString(),
                                "Achtung");
                    }
                    break;
                case 2:
                    try
                    {
                        MySqlConnection connect;
                        connect = new MySqlConnection(asConnectString);
                        MySqlCommand command = new MySqlCommand(psSql, connect);
                        connect.Open();

                        if (piArt == 1)     // Dezimalwert ermitteln Allgemein
                        {
                            var lvGetId = command.ExecuteScalar();

                            if (lvGetId != null)
                            {
                                DateTime.TryParse(lvGetId.ToString(), out ldtReturn);
                            }
                            else
                            {
                                ldtReturn = DateTime.MinValue;
                            }
                        }
                        // db close
                        connect.Close();
                    }
                    catch
                    {
                        // Die Anwendung anhalten 
                        MessageBox.Show("Verarbeitungsfehler ERROR fetchdataDate RdFunctions \n piArt = " + piArt.ToString(),
                                "Achtung");
                    }
                    break;
                default:
                    break;
            }
            return (ldtReturn);
        }

        // Einige Commandbuilder wurden hier vereint
        private static void MakeCommand(int aiDb, int aiArt)
        {

            switch (aiDb)   // 1= MsSql 2= Mysql
            {
                case 1:
                    switch (aiArt)
                    {
                        case 1:
                            SqlCommandBuilder commandBuilder11 = new SqlCommandBuilder(sdc);
                            sdc.Update(tableNewTimeline);
                            break;
                        case 2:
                            SqlCommandBuilder commandBuilder12 = new SqlCommandBuilder(sdc);
                            sdc.Update(tableTimeLineSet);
                            break;
                        case 3:
                            SqlCommandBuilder commandBuilder13 = new SqlCommandBuilder(sdc);
                            sdc.Update(tableNewTimeline);
                            break;
                        case 4:
                            SqlCommandBuilder commandBuilder14 = new SqlCommandBuilder(sdTml);
                            sdTml.Update(tableTml);
                            break;
                        default:
                            break;
                    }
                    break;
                case 2:
                    switch (aiArt)
                    {
                        case 1:
                            MySqlCommandBuilder commandBuilder21 = new MySqlCommandBuilder(mysdc);
                            mysdc.Update(tableNewTimeline);
                            break;
                        case 2:
                            MySqlCommandBuilder commandBuilder22 = new MySqlCommandBuilder(mysdc);
                            mysdc.Update(tableTimeLineSet);
                            break;
                        case 3:
                            MySqlCommandBuilder commandBuilder23 = new MySqlCommandBuilder(mysdc);
                            mysdc.Update(tableNewTimeline);
                            break;
                        case 4:
                            MySqlCommandBuilder commandBuilder24 = new MySqlCommandBuilder(mysdTml);
                            mysdTml.Update(tableTml);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        // Datenbankaktionen nach fetchdata
        public static int MakeAfterFetch(int aiArt, int aiTeil, int ai1, int ai2, string asConnect, int aiDb)
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
            // int liDaysInMonth = 0; // Tage im Monat aus Vertrag
            int liSave = 1;  // Freigabe
            int liArtRelation = 0;      // 1= Rechnung, 2=Zahlung, 3=Zähler

            decimal ldBetragNetto = 0;
            decimal ldBetragSollNetto = 0;
            decimal ldBetragBrutto = 0;
            decimal ldBetragSollBrutto = 0;
            decimal ldGesamtflaeche = 0;
            decimal ldZs = 0;            // Zählerstand
            decimal ldVerbrauch = 0;    // Zähler Verbrauch
            decimal[] ladBetraege = new decimal[12];

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
            int LiReturn = 0;

            // TODO Idee für Tasks
            // Get a reference to the MainWindow
            //MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

            //// Check if the MainWindow exists
            //if (mainWindow != null)
            //{
            //    // Use the Dispatcher to update the progress bar value on the UI thread
            //    mainWindow.Dispatcher.Invoke(() =>
            //    {
            //        // Start the progress bar
            //        mainWindow.ProgressBar.IsIndeterminate = true;
            //    });
            //}

            switch (aiArt)
            {
                case 1:
                    // Externe ID aus der Rechnung ermitteln 
                    for (int i = 0; tableRechnungen.Rows.Count > i; i++)
                    {
                        if (tableRechnungen.Rows[i].ItemArray.GetValue(14) != DBNull.Value)
                        {
                            liExternId = (int)tableRechnungen.Rows[i].ItemArray.GetValue(14);
                            // Timeline löschen
                            liOk = TimelineDelete(liExternId, "R", asConnect, aiDb);

                            // Objekt
                            if (tableRechnungen.Rows[i].ItemArray.GetValue(8) != DBNull.Value)
                                if ((int)tableRechnungen.Rows[i].ItemArray.GetValue(8) > 0)
                                {
                                    liObjekt = (int)tableRechnungen.Rows[i].ItemArray.GetValue(8);
                                    // Timeline neu erzeugen Objekte aus Rechnungen
                                    liOk = TimelineCreate(liExternId, "id_rechnung", asConnect, aiDb);

                                    // Weiterleitung an ObjektTeil aus der Kostenart ermitteln
                                    // 1 = Weiterleitung an Teilobjekt
                                    if (getWtl(1, liExternId, asConnect, aiDb) == 1)
                                    {
                                        liObjektTeil = 0;
                                        liArtRelation = 1;
                                        // Timeline neu erzeugen für Relationen
                                        liOk = TimelineCreateRelations(liExternId, liObjekt, liObjektTeil, liMieter, liArtRelation, asConnect, aiDb);

                                        // 2 = Weiterleitung an Mieter
                                        if (getWtl(2, liExternId, asConnect, aiDb) == 1)
                                        {
                                            liObjekt = 0;
                                            liObjektTeil = 1;   // Auslöser für das Weiterleiten an Mieter
                                            liArtRelation = 1;
                                            // Timeline neu erzeugen für Relationen
                                            liOk = TimelineCreateRelations(liExternId, liObjekt, liObjektTeil, liMieter, liArtRelation, asConnect, aiDb);
                                        }
                                    }
                                }

                            // ObjektTeil
                            if (tableRechnungen.Rows[i].ItemArray.GetValue(9) != DBNull.Value)
                                if ((int)tableRechnungen.Rows[i].ItemArray.GetValue(9) > 0)
                                {
                                    liObjektTeil = (int)tableRechnungen.Rows[i].ItemArray.GetValue(9);
                                    // Timeline neu erzeugen Objektteile aus Rechnungen
                                    liOk = TimelineCreate(liExternId, "id_rechnung", asConnect, aiDb);
                                    // Weiterleitung an ObjektTeil aus der Kostenart ermitteln
                                    // 2 = Weiterleitung an Mieter
                                    if (getWtl(2, liExternId, asConnect, aiDb) == 1)
                                    {
                                        liArtRelation = 1;
                                        // Timeline neu erzeugen für Relationen
                                        liOk = TimelineCreateRelations(liExternId, liObjekt, liObjektTeil, liMieter, liArtRelation, asConnect, aiDb);
                                    }
                                }

                            // Mieter
                            if (tableRechnungen.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                                if ((int)tableRechnungen.Rows[i].ItemArray.GetValue(10) > 0)
                                {
                                    liMieter = (int)tableRechnungen.Rows[i].ItemArray.GetValue(10);
                                    // Timeline neu erzeugen Mieter aus Rechnungen
                                    // TODO hier Kontrolle einbauen, ob Mietvertrag gültig ist
                                    liOk = TimelineCreate(liExternId, "id_rechnung", asConnect, aiDb);
                                }
                        }
                        else
                        {
                            MessageBox.Show("Verarbeitungsfehler ERROR fetchdata fetchdata RdFunctions 0001\n piArt = " + aiArt.ToString(),
                                        "Achtung");
                            break;
                        }
                    }
                    break;
                case 3:
                    switch (aiTeil)
                    {
                        case 1:
                            for (int i = 0; tableRechnungen.Rows.Count > i; i++)
                            {
                                if (tableRechnungen.Rows[i].ItemArray.GetValue(14) != DBNull.Value)
                                {
                                    liExternId = (int)tableRechnungen.Rows[i].ItemArray.GetValue(14);
                                    LiReturn = liExternId;
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
                            for (int i = 0; tableRechnungen.Rows.Count > i; i++)
                            {
                                if (tableRechnungen.Rows[i].ItemArray.GetValue(14) != DBNull.Value)
                                {
                                    liExternId = (int)tableRechnungen.Rows[i].ItemArray.GetValue(14);
                                    if (tableRechnungen.Rows[i].ItemArray.GetValue(8) != DBNull.Value)
                                        liObjekt = (int)tableRechnungen.Rows[i].ItemArray.GetValue(8);
                                    if (tableRechnungen.Rows[i].ItemArray.GetValue(9) != DBNull.Value)
                                        liObjektTeil = (int)tableRechnungen.Rows[i].ItemArray.GetValue(9);
                                    if (tableRechnungen.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                                        liMieter = (int)tableRechnungen.Rows[i].ItemArray.GetValue(10);
                                    if (tableRechnungen.Rows[i].ItemArray.GetValue(5) != DBNull.Value)
                                        ldBetragNetto = (decimal)tableRechnungen.Rows[i].ItemArray.GetValue(5);
                                    if (tableRechnungen.Rows[i].ItemArray.GetValue(6) != DBNull.Value)
                                        ldBetragBrutto = (decimal)tableRechnungen.Rows[i].ItemArray.GetValue(6);
                                    if (tableRechnungen.Rows[i].ItemArray.GetValue(3) != DBNull.Value)
                                        ldtStart = (DateTime)tableRechnungen.Rows[i].ItemArray.GetValue(3);
                                    if (tableRechnungen.Rows[i].ItemArray.GetValue(4) != DBNull.Value)
                                        ldtEnd = (DateTime)tableRechnungen.Rows[i].ItemArray.GetValue(4);
                                    if (tableRechnungen.Rows[i].ItemArray.GetValue(1) != DBNull.Value)
                                        liKsa = (int)tableRechnungen.Rows[i].ItemArray.GetValue(1);

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
                                        DataRow dr = tableNewTimeline.NewRow();

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
                                        dr[14] = 0;
                                        dr[15] = 0;
                                        tableNewTimeline.Rows.Add(dr);
                                        // + Monat 
                                        ldtMonat = ldtMonat.AddMonths(1);
                                        // + Zähler
                                        zl++;

                                    } while (zl <= liMonths);

                                    // und alles ab in die Datenbank
                                    MakeCommand(aiDb, 1);
                                }
                                else
                                {
                                    MessageBox.Show("Verarbeitungsfehler ERROR fetchdata fetchdata RdFunctions 0003\n piArt = " + aiArt.ToString(),
                                                "Achtung");
                                    break;
                                }
                            }
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

                    //// Thread Test
                    //// Thread für Progress definieren
                    //Thread thread = new Thread(new ThreadStart(ThreadMethod));
                    //thread.SetApartmentState(ApartmentState.STA);

                    //// Dispatcher dispatcher = Application.Current.Dispatcher;

                    //void ThreadMethod()
                    //{
                    //    WndProgress frmProgress = new WndProgress();
                    //    var dispatcher = frmProgress.Dispatcher.Invoke(() => frmProgress.PBar);
                    //    frmProgress.Dispatcher.Invoke(() => dispatcher.Value = 0);
                    //    frmProgress.ShowDialog();
                    //}
                    //// Progressfenster 
                    //thread.Start();

                    //Dispatcher.Invoke(() =>
                    //{
                    //    //    // Führen Sie die Aktion auf dem Hintergrundthread aus
                    //    //    // ...
                    //});


                    tableTimeLineSet.Rows.Clear();     // Timeline leeren

                    // Timeline
                    for (int i = 0; tableTimelineGet.Rows.Count > i; i++)
                    {
                        
                        if (tableTimelineGet.Rows[i].ItemArray.GetValue(1) != DBNull.Value)
                        {
                            liRechnungId = (int)tableTimelineGet.Rows[i].ItemArray.GetValue(1);
                            if (tableTimelineGet.Rows[i].ItemArray.GetValue(4) != DBNull.Value)
                                liObjekt = (int)tableTimelineGet.Rows[i].ItemArray.GetValue(4);
                            if (tableTimelineGet.Rows[i].ItemArray.GetValue(5) != DBNull.Value)
                                liObjektTeil = (int)tableTimelineGet.Rows[i].ItemArray.GetValue(5);
                            if (tableTimelineGet.Rows[i].ItemArray.GetValue(6) != DBNull.Value)
                                liMieter = (int)tableTimelineGet.Rows[i].ItemArray.GetValue(6);
                            if (tableTimelineGet.Rows[i].ItemArray.GetValue(7) != DBNull.Value)
                                liKsa = (int)tableTimelineGet.Rows[i].ItemArray.GetValue(7);
                            if (tableTimelineGet.Rows[i].ItemArray.GetValue(8) != DBNull.Value)
                                ldBetragNetto = (decimal)tableTimelineGet.Rows[i].ItemArray.GetValue(8);
                            if (tableTimelineGet.Rows[i].ItemArray.GetValue(9) != DBNull.Value)
                                ldBetragSollNetto = (decimal)tableTimelineGet.Rows[i].ItemArray.GetValue(9);
                            if (tableTimelineGet.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                                ldBetragBrutto = (decimal)tableTimelineGet.Rows[i].ItemArray.GetValue(10);
                            if (tableTimelineGet.Rows[i].ItemArray.GetValue(11) != DBNull.Value)
                                ldBetragSollBrutto = (decimal)tableTimelineGet.Rows[i].ItemArray.GetValue(11);
                            if (tableTimelineGet.Rows[i].ItemArray.GetValue(12) != DBNull.Value)
                                ldZs = (decimal)tableTimelineGet.Rows[i].ItemArray.GetValue(12);
                            if (tableTimelineGet.Rows[i].ItemArray.GetValue(13) != DBNull.Value)
                                ldtMonat = (DateTime)tableTimelineGet.Rows[i].ItemArray.GetValue(13);
                            if (tableTimelineGet.Rows[i].ItemArray.GetValue(17) != DBNull.Value)
                                liImportId = (int)tableTimelineGet.Rows[i].ItemArray.GetValue(17);

                            // Ermitteln der VerteilungsId aus Tabelle rechnungen
                            // Achtung nbüschen gepfuscht liRechnungId ist die externTimeline Id
                            liVerteilungId = getVerteilungsId(asConnect, liRechnungId, aiDb);

                            // Ermitteln, wie verteilt werden soll aus der Tabelle art_verteilung
                            lsVerteilung = getVerteilung(asConnect, liVerteilungId, aiDb);

                            // Alle Objektteile zu dem Objekt
                            for (int ii = 0; tableObjektParts.Rows.Count > ii; ii++)
                            {
                                // Timeline schreiben
                                DataRow dr = tableTimeLineSet.NewRow();

                                dr[1] = liRechnungId;
                                // dr[4] = liObjekt; nicht eintragen
                                if (tableObjektParts.Rows[ii].ItemArray.GetValue(0) != DBNull.Value)
                                {
                                    dr[5] = (int)tableObjektParts.Rows[ii].ItemArray.GetValue(0);   // id ObjektTeil
                                    liObjektTeil = (int)tableObjektParts.Rows[ii].ItemArray.GetValue(0);
                                    dr[6] = liMieter;
                                    dr[7] = liKsa;

                                    switch (lsVerteilung)
                                    {
                                        case "fl":
                                            if (tableObjektParts.Rows[ii].ItemArray.GetValue(6) != DBNull.Value)
                                            {
                                                if ((decimal)tableObjektParts.Rows[ii].ItemArray.GetValue(6) > 0)
                                                {
                                                    // Gesamtfläche aus Tabelle Objekt holen
                                                    if (liObjekt > 0)
                                                    {
                                                        ldGesamtflaeche = getObjektflaeche(liObjekt, 0, 0, asConnect, aiDb);
                                                        dr[8] = ldBetragNetto / (ldGesamtflaeche / (decimal)tableObjektParts.Rows[ii].ItemArray.GetValue(6));          // Netto    
                                                        dr[10] = ldBetragBrutto / (ldGesamtflaeche / (decimal)tableObjektParts.Rows[ii].ItemArray.GetValue(6));         // Brutto
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
                                            if (tableObjektParts.Rows[ii].ItemArray.GetValue(7) != DBNull.Value)
                                            {
                                                if ((decimal)tableObjektParts.Rows[ii].ItemArray.GetValue(7) > 0)
                                                {
                                                    dr[8] = (ldBetragNetto / 100) * (decimal)tableObjektParts.Rows[ii].ItemArray.GetValue(7);           // Netto    
                                                    dr[10] = (ldBetragBrutto / 100) * (decimal)tableObjektParts.Rows[ii].ItemArray.GetValue(7);         // Brutto                                                		 
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

                                            if (tableObjektParts.Rows[ii].ItemArray.GetValue(8) != DBNull.Value)
                                            {
                                                if ((int)tableObjektParts.Rows[ii].ItemArray.GetValue(8) > 0)
                                                {
                                                    // Anzahl der Personen in einem Objekt ermitteln
                                                    // Aktive Verträge
                                                    liAnzPersonenObj = Convert.ToInt32(getAktPersonen(liObjekt, 0, 0, ldtMonat.ToString(), ldtMonat.ToString(), 0, asConnect, aiDb));
                                                    // Anzahl der Personen in einem Objektteil ermitteln
                                                    liAnzPersonenObt = Convert.ToInt32(getAktPersonen(0, liObjektTeil, 0, ldtMonat.ToString(), ldtMonat.ToString(), 0, asConnect, aiDb));

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
                                            if ((decimal)tableObjektParts.Rows[ii].ItemArray.GetValue(6) > 0)
                                            {
                                                // Gesamtfläche der ausgewählten Wohnungen aus Tabelle Objekt_mix_parts holen
                                                if (liObjekt > 0)
                                                {
                                                    int liArt = 0;
                                                    // Gesamtfläche der Auswahl = 0 oder Gesamtfläche = 1
                                                    liArt = getObjektflaecheAuswFlag(liObjekt, asConnect, aiDb);
                                                    ldGesamtflaeche = getObjektflaecheAuswahl(liObjekt, liRechnungId, asConnect, liArt, aiDb);  // RechnungsId ist Timeline ID
                                                    if (getObjektTeilAuswahl((int)tableObjektParts.Rows[ii].ItemArray.GetValue(0), asConnect, aiDb) > 0)
                                                    {
                                                        // decimal ldtest = ldBetragNetto / (ldGesamtflaeche / (decimal)tableFive.Rows[ii].ItemArray.GetValue(6)); 
                                                        dr[8] = ldBetragNetto / (ldGesamtflaeche / (decimal)tableObjektParts.Rows[ii].ItemArray.GetValue(6));          // Netto    
                                                        dr[10] = ldBetragBrutto / (ldGesamtflaeche / (decimal)tableObjektParts.Rows[ii].ItemArray.GetValue(6));         // Brutto                                                                                                                                                    
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
                                    tableTimeLineSet.Rows.Add(dr);
                                }
                                liSave = 1;
                                // und alle TimelineEinträge ab in die Datenbank
                                MakeCommand(aiDb, 2);
                                tableTimeLineSet.Rows.Clear();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Verarbeitungsfehler ERROR fetchdata fetchdata RdFunctions 0004\n piArt = " + aiArt.ToString(),
                                        "Achtung");
                            break;
                        }
                    }


                    //Application.Current.Dispatcher.Invoke(() =>
                    //{

                    //});

                    break;
                case 5:         // Mieter schreiben
                    // Schleife durch Timeline
                    // Jeder Datensatz muss hier einen Datensatz für den Mieter erzeugen
                    tableNewTimeline.Rows.Clear();    // TimeLine leeren

                    for (int i = 0; tableTimeLineGet.Rows.Count > i; i++)
                    {
                        liSave = 1;
                        if (tableTimeLineGet.Rows[i].ItemArray.GetValue(1) != DBNull.Value || tableTimeLineGet.Rows[i].ItemArray.GetValue(2) != DBNull.Value || tableTimeLineGet.Rows[i].ItemArray.GetValue(3) != DBNull.Value)
                        {
                            // Rechnung
                            if (tableTimeLineGet.Rows[i].ItemArray.GetValue(1) != DBNull.Value)
                            {
                                liRechnungId = (int)tableTimeLineGet.Rows[i].ItemArray.GetValue(1);
                            }
                            // Zahlung
                            if (tableTimeLineGet.Rows[i].ItemArray.GetValue(2) != DBNull.Value)
                            {
                                liZahlungId = (int)tableTimeLineGet.Rows[i].ItemArray.GetValue(2);
                            }
                            // Zählerstand
                            if (tableTimeLineGet.Rows[i].ItemArray.GetValue(3) != DBNull.Value)
                            {
                                liZaehlerstandId = (int)tableTimeLineGet.Rows[i].ItemArray.GetValue(3);
                            }

                            if (tableTimeLineGet.Rows[i].ItemArray.GetValue(4) != DBNull.Value)
                                liObjekt = (int)tableTimeLineGet.Rows[i].ItemArray.GetValue(4);
                            if (tableTimeLineGet.Rows[i].ItemArray.GetValue(5) != DBNull.Value)
                                liObjektTeil = (int)tableTimeLineGet.Rows[i].ItemArray.GetValue(5);
                            if (tableTimeLineGet.Rows[i].ItemArray.GetValue(7) != DBNull.Value)
                                liKsa = (int)tableTimeLineGet.Rows[i].ItemArray.GetValue(7);
                            if (tableTimeLineGet.Rows[i].ItemArray.GetValue(8) != DBNull.Value)
                                ldBetragNetto = (decimal)tableTimeLineGet.Rows[i].ItemArray.GetValue(8);
                            if (tableTimeLineGet.Rows[i].ItemArray.GetValue(9) != DBNull.Value)
                                ldBetragSollNetto = (decimal)tableTimeLineGet.Rows[i].ItemArray.GetValue(9);
                            if (tableTimeLineGet.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                                ldBetragBrutto = (decimal)tableTimeLineGet.Rows[i].ItemArray.GetValue(10);
                            if (tableTimeLineGet.Rows[i].ItemArray.GetValue(11) != DBNull.Value)
                                ldBetragSollBrutto = (decimal)tableTimeLineGet.Rows[i].ItemArray.GetValue(11);
                            if (tableTimeLineGet.Rows[i].ItemArray.GetValue(12) != DBNull.Value)
                                ldZs = (decimal)tableTimeLineGet.Rows[i].ItemArray.GetValue(12);
                            if (tableTimeLineGet.Rows[i].ItemArray.GetValue(13) != DBNull.Value)
                                ldtMonat = (DateTime)tableTimeLineGet.Rows[i].ItemArray.GetValue(13);
                            if (tableTimeLineGet.Rows[i].ItemArray.GetValue(17) != DBNull.Value)
                                liImportId = (int)tableTimeLineGet.Rows[i].ItemArray.GetValue(17);

                            DataRow dr = tableNewTimeline.NewRow();
                            dr[1] = liRechnungId;
                            dr[2] = liZahlungId;
                            dr[3] = liZaehlerstandId;
                            // dr[4] = liObjekt; nicht eintragen
                            // dr[5] = liObjektTeil; 

                            //if (liObjektTeil == 97)
                            //{
                            //    int liTest = liObjektTeil;
                            //}

                            // Aktuellen Mieter ermitteln
                            liMieter = getAktMieter(liObjektTeil, ldtMonat, asConnect, aiDb);

                            if (liMieter == 107 && liObjektTeil == 97)
                            {
                                int liTest = liMieter;
                            }

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
                                liMieter = getMieterLeerstand(liObjektTeil, asConnect, aiDb);
                                if (liMieter > 0)
                                {
                                    dr[6] = liMieter;       // Mieter Leerstand existiert und wird genutzt
                                }
                                dr[16] = liObjektTeil;         // Auf Leerstand wird die TeilObjekt ID geschrieben Feld 16
                            }
                            dr[7] = liKsa;

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
                                tableNewTimeline.Rows.Add(dr);            // Timeline                                     
                            }
                            liSave = 1;
                            // und alle TimelineEinträge ab in die Datenbank
                            MakeCommand(aiDb, 3);
                            tableNewTimeline.Rows.Clear();
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
                    if (tableTaxGet.Rows.Count > 0)
                    {
                        if (tableTaxGet.Rows[0].ItemArray.GetValue(2) != DBNull.Value)
                        {
                            // Hier wird liRows ausnahmsweise mit dem Mwst-Satz belegt
                            decimal ldMwst = (decimal)tableTaxGet.Rows[0].ItemArray.GetValue(2);
                            LiReturn = (int)ldMwst;
                        }
                    }
                    break;
                case 11:
                    // Externe ID aus der Zahlung ermitteln 
                    for (int i = 0; tableZlg.Rows.Count > i; i++)
                    {
                        if (tableZlg.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                        {
                            liExternId = (int)tableZlg.Rows[i].ItemArray.GetValue(10);
                            // Timeline löschen
                            liOk = TimelineDelete(liExternId, "A", asConnect, aiDb);

                            // Objekt
                            if (tableZlg.Rows[i].ItemArray.GetValue(2) != DBNull.Value)
                                if ((int)tableZlg.Rows[i].ItemArray.GetValue(2) > 0)
                                {
                                    liObjekt = (int)tableZlg.Rows[i].ItemArray.GetValue(2);
                                    // Timeline neu erzeugen Objekte aus Rechnungen
                                    liOk = TimelineCreate(liExternId, "id_vorauszahlung", asConnect, aiDb);
                                }
                            // ObjektTeil
                            if (tableZlg.Rows[i].ItemArray.GetValue(3) != DBNull.Value)
                                if ((int)tableZlg.Rows[i].ItemArray.GetValue(3) > 0)
                                {
                                    liObjektTeil = (int)tableZlg.Rows[i].ItemArray.GetValue(3);
                                    ldtMonat = Convert.ToDateTime(tableZlg.Rows[i].ItemArray.GetValue(4));
                                    // Timeline neu erzeugen Objektteile aus Rechnungen
                                    liOk = TimelineCreate(liExternId, "id_vorauszahlung", asConnect, aiDb);

                                    // Weiterleitung an aktiven Mieter
                                    liMieter = 0;
                                    liMieter = getAktMieter(liObjektTeil, ldtMonat, asConnect, aiDb);

                                    if (liMieter > 0)
                                    {
                                        liArtRelation = 2;
                                        // Timeline neu erzeugen für Relationen
                                        liOk = TimelineCreateRelations(liExternId, liObjekt, liObjektTeil, liMieter, liArtRelation, asConnect, aiDb);
                                    }
                                }

                            // Mieter
                            if (tableZlg.Rows[i].ItemArray.GetValue(1) != DBNull.Value)
                                if ((int)tableZlg.Rows[i].ItemArray.GetValue(1) > 0)
                                {
                                    liMieter = (int)tableZlg.Rows[i].ItemArray.GetValue(1);
                                    // Timeline neu erzeugen Mieter aus Zahlungen
                                    // TODO ACHTUNG hier Kontrolle einbauen, ob Mietvertrag gültig ist
                                    liOk = TimelineCreate(liExternId, "id_vorauszahlung", asConnect, aiDb);
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
                            for (int i = 0; tableZlgNew.Rows.Count > i; i++)
                            {
                                if (tableZlgNew.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                                {
                                    liExternId = (int)tableZlgNew.Rows[i].ItemArray.GetValue(10);
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
                                        dr[9] = ldBetragSollNetto * -1;
                                        dr[10] = ldBetragBrutto * -1;
                                        dr[11] = ldBetragSollBrutto * -1;
                                        dr[12] = ldZs;                          // Zählerstand
                                        dr[13] = ldtStart;
                                        dr[14] = 0;
                                        dr[15] = 0;
                                        dr[17] = liImportId;

                                        tableTml.Rows.Add(dr);
                                        // + Monat 
                                        ldtMonat = ldtMonat.AddMonths(1);
                                        // + Zähler
                                        zl++;

                                    } while (zl <= liMonths);

                                    // und alles ab in die Datenbank
                                    liOk = fetchData("", "", 32, asConnect, aiDb);
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
                case 21:
                    // Externe ID aus der Zählerstand ermitteln 
                    for (int i = 0; tableCnt.Rows.Count > i; i++)
                    {
                        if (tableCnt.Rows[i].ItemArray.GetValue(8) != DBNull.Value)
                        {
                            liExternId = (int)tableCnt.Rows[i].ItemArray.GetValue(8);
                            // Timeline löschen
                            liOk = TimelineDelete(liExternId, "Z", asConnect, aiDb);

                            // Objekt
                            if (tableCnt.Rows[i].ItemArray.GetValue(9) != DBNull.Value)
                                if ((int)tableCnt.Rows[i].ItemArray.GetValue(9) > 0)
                                {
                                    liObjekt = (int)tableCnt.Rows[i].ItemArray.GetValue(9);
                                    // Timeline neu erzeugen Objekte aus Zählerständen
                                    liOk = TimelineCreate(liExternId, "id_zaehlerstand", asConnect, aiDb);
                                }

                            // ObjektTeil
                            if (tableCnt.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                                if ((int)tableCnt.Rows[i].ItemArray.GetValue(10) > 0)
                                {
                                    liObjektTeil = (int)tableCnt.Rows[i].ItemArray.GetValue(10);
                                    ldtMonat = Convert.ToDateTime(tableCnt.Rows[i].ItemArray.GetValue(4));
                                    // Timeline neu erzeugen Objektteile aus Zählerständen
                                    liOk = TimelineCreate(liExternId, "id_zaehlerstand", asConnect, aiDb);

                                    // Weiterleitung an aktiven Mieter
                                    liMieter = getAktMieter(liObjektTeil, ldtMonat, asConnect, aiDb);

                                    if (liMieter > 0)
                                    {
                                        liArtRelation = 3;
                                        // Timeline neu erzeugen für Relationen
                                        liOk = TimelineCreateRelations(liExternId, liObjekt, liObjektTeil, liMieter, liArtRelation, asConnect, aiDb);
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
                            dr[14] = 0;
                            dr[15] = 0;
                            // dr[17] = 99; für Testzwecke, um Zählerdaten wiederzufinden

                            tableTml.Rows.Add(dr);

                            // und alles ab in die Datenbank
                            MakeCommand(aiDb, 4);
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
                //TODO Vorrauszahlung
            }

            // Das ist ein Zählerstand
            if (liZlgOrRg == 3)
            {
                //TODO Zählerstand
            }

            return ldBetraege;
        }

        // Timeline für Relationen erzeugen
        private static int TimelineCreateRelations(int liExternId, int liObjekt, int liObjektTeil, int liMieter, int aiArt, string asConnect, int aiDb)
        {
            int liOk = 0;
            string lsSql = "";
            string lsSql2 = "";

            // Dann werden die Kosten verteilt:
            // Nach Objektteil nur nach Quadratmetern oder Anteilig
            // Nach Mieter auch nach Personenzahl

            if (liObjekt > 0)                       // Timeline Objektteil schreiben
            {
                // in Timeline Objektteil werden alle Monate nach dem Verteilungsschlüssel geschrieben
                lsSql2 = Timeline.getSql(6, liObjekt, "", "", 0);       // Objektteile holen
                lsSql = Timeline.getSql(4, liExternId, liObjekt.ToString(), "", 0);
                liOk = Timeline.fetchData(lsSql, lsSql2, 4, asConnect, aiDb);
            }

            else if (liObjektTeil > 0)
            {
                // In Timeline Mieter werden alle umlagefähigen Kosten auf den 
                // zu dem TimeLineMonat wohnenden Mieter geschrieben
                switch (aiArt)
                {
                    case 1:         // Rechnung 
                        lsSql = Timeline.getSql(50, liExternId, liObjektTeil.ToString(), "", 0);
                        break;
                    case 2:         // Zahlung
                        lsSql = Timeline.getSql(51, liExternId, liObjektTeil.ToString(), "", 0);
                        break;
                    case 3:         //Zähler
                        lsSql = Timeline.getSql(52, liExternId, liObjektTeil.ToString(), "", 0);
                        break;
                    default:
                        break;
                }

                liOk = Timeline.fetchData(lsSql, "", 5, asConnect, aiDb);
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
        private static int TimelineCreate(int liExternId, string asField, string asConnect, int aiDb)
        {
            int liOk = 0;
            string lsSql = "";
            string lsSql2 = "";

            if (asField == "id_rechnung") // Rechnung
            {
                lsSql = Timeline.getSql(31, liExternId, asField, "", 0);               // Timeline
                lsSql2 = Timeline.getSql(1, liExternId, asField, "", 0);               // Rechnung
                liOk = Timeline.fetchData(lsSql, lsSql2, 3, asConnect, aiDb);
            }

            if (asField == "id_vorauszahlung") // Vorrauszahlung                                     
            {
                lsSql = Timeline.getSql(31, liExternId, asField, "", 0);               // Timeline
                lsSql2 = Timeline.getSql(12, liExternId, asField, "", 0);              // Zahlung mit extern Timeline Id
                liOk = Timeline.fetchData(lsSql, lsSql2, 13, asConnect, aiDb);
            }

            if (asField == "id_zaehlerstand") // Zähler
            {
                lsSql = Timeline.getSql(31, liExternId, asField, "", 0);               // Timeline
                lsSql2 = Timeline.getSql(21, liExternId, asField, "", 0);              // Zählerstande mit extern Timeline Id
                liOk = Timeline.fetchData(lsSql, lsSql2, 23, asConnect, aiDb);
            }

            return liOk;
        }

        // Alle Datensätze der Timeline ID zunächst löschen
        private static int TimelineDelete(int liExternId, string asArt, string asConnect, int aiDb)
        {
            int liOk = 0;
            string lsSql = "";

            // SqlStatement für Timeline löschen
            switch (asArt)
            {
                case "R":   // Rechnung
                    lsSql = Timeline.getSql(200, liExternId, "", "", 0);
                    break;
                case "A":   // Zahlung
                    lsSql = Timeline.getSql(201, liExternId, "", "", 0);
                    break;
                case "Z":   // Zählerstand
                    lsSql = Timeline.getSql(202, liExternId, "", "", 0);
                    break;
                default:
                    break;
            }
            liOk = Timeline.fetchData(lsSql, "", 2, asConnect, aiDb);

            // Info: hier werden auch alle Datensätze evtl untergeordneter Rubriken 
            // anteilige Kosten von Objektteilen und Mietern gelöscht,
            // weil alle datensätze betr. der Extern Id gelöscht werden
            return liOk;
        }

        // Mehrwertsteuersatz holen, Bezeichnung bez ist bekannt
        public static int getMwstFromBez(string lsBez, string asConnect, int aiDb)
        {
            String lsSql = "";
            int liMwstSatz = 0;

            lsSql = Timeline.getSql(9, 0, lsBez, "", 0);
            // fetchdata gibt hier den MwstSatz zurück
            liMwstSatz = Timeline.fetchData(lsSql, "", 8, asConnect, aiDb);

            return liMwstSatz;
        }

        // Mehrwertsteuersatz holen, Art ist bekannt
        public static int getMwstSatz(int liMwstArt, string asConnectString, int aiDb)
        {
            String lsSql = "";
            int liMwstSatz = 0;

            lsSql = Timeline.getSql(8, liMwstArt, "", "", 0);
            // fetchdata gibt hier den MwstSatz zurück
            liMwstSatz = Timeline.fetchData(lsSql, "", 8, asConnectString, aiDb);

            return liMwstSatz;
        }

        // Gesamtfläche eines Objektes holen
        private static decimal getObjektflaeche(int aiObjekt, int aiTObjekt, int aiMieterId, string asConnect, int aiDb)
        {
            int liObjTeilId = 0;
            int liObjId = 0;
            decimal ldGesamtflaeche = 0;
            String lsSql = "";

            // Mieter ID vorhanden
            if (aiMieterId > 0)
            {
                liObjTeilId = getIdObjTeil(aiMieterId, asConnect, aiDb);
                liObjId = getIdObj(liObjTeilId, asConnect, 2, aiDb);
                lsSql = "Select flaeche_gesamt from objekt where id_objekt = " + liObjId.ToString();
            }
            // TeilObjekt ID vorhanden
            if (aiTObjekt > 0)
            {
                liObjId = getIdObj(liObjTeilId, asConnect, 2, aiDb);
                lsSql = "Select flaeche_gesamt from objekt where id_objekt = " + liObjId.ToString();
            }
            // Objekt ID vorhanden
            if (aiObjekt > 0)
            {
                lsSql = "Select flaeche_gesamt from objekt where id_objekt = " + aiObjekt.ToString();
            }

            // Daten holen
            ldGesamtflaeche = fetchDataDecimal(lsSql, "", 1, asConnect, aiDb);

            return ldGesamtflaeche;
        }

        // Fläche eines TeilObjektes holen
        private static decimal getTObjektflaeche(int aiTObjekt, int aiMieterId, string asConnect, int aiDb)
        {
            int liObjTeilId = 0;
            decimal ldFlaeche = 0;
            string lsSql = "";

            // Mieter ID vorhanden
            if (aiMieterId > 0)
            {
                liObjTeilId = getIdObjTeil(aiMieterId, asConnect, aiDb);
                lsSql = "Select flaeche_anteil from objekt_teil where id_objekt_teil = " + liObjTeilId.ToString();
            }
            // TeilObjekt ID vorhanden
            if (aiTObjekt > 0)
            {
                lsSql = "Select flaeche_anteil from objekt_teil where id_objekt_teil = " + aiTObjekt.ToString();
            }

            // Daten holen
            ldFlaeche = fetchDataDecimal(lsSql, "", 1, asConnect, aiDb);

            return ldFlaeche;
        }

        // Fläche eines TeilObjektes holen
        private static decimal getTObjektAnteil(int aiTObjekt, int aiMieterId, string asConnect, int aiDb)
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
                liObjTeilId = getIdObjTeil(aiMieterId, asConnect, aiDb);
                lsSql = "Select prozent_anteil from objekt_teil where id_objekt_teil = " + liObjTeilId.ToString();
            }

            // Daten holen
            ldAnteil = fetchDataDecimal(lsSql, "", 1, asConnect, aiDb);

            return ldAnteil;
        }


        // Die Gesamtfläche der Objektauswahl aus objekt_part_mix ermitteln
        // Art 1 ist die Gesamtgrundfläche der gewählten Wohnungen
        // Art 2 ist die Gesamtfläche des Objektes
        private static decimal getObjektflaecheAuswahl(int liObjekt, int aiTimelineId, string asConnect, int aiArt, int aiDb)
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

            // Daten holen
            ldGesamtflaeche = fetchDataDecimal(lsSql, "", 1, asConnect, aiDb);

            return ldGesamtflaeche;
        }

        // Es wird geprüft ob das Objektteil in der Auswahl enthalten ist
        private static int getObjektTeilAuswahl(int aiObjektTeil, string asConnect, int aiDb)
        {
            int liObjektTeil = 0;

            String lsSql = getSql(27, aiObjektTeil, "", "", 0);
            liObjektTeil = fetchData(lsSql, "", 26, asConnect, aiDb);

            return liObjektTeil;
        }


        // Ist eine Weitergabe der Kosten in art_kostenart eingetragen
        // 1 = Weiterleitung
        private static int getWtl(int p, int liExternId, string asConnect, int aiDb)
        {
            int liWtl = 0;
            string lsSql = "";

            lsSql = getSql(28, liExternId, "", "", p);
            liWtl = fetchData(lsSql, "", 26, asConnect, aiDb);

            return liWtl;
        }

        // Hier wird der aktuelle Mieter für den gegebenen Monat der Timeline ermittelt
        public static int getAktMieter(int aiObjektTeil, DateTime adtMonat, string asConnect, int aiDb)
        {
            String lsSql = "";
            Int32 liMieterId = 0;

            // adtMonat umbauen soll immer den ersten des Monats zeigen
            adtMonat = adtMonat.AddDays(-(adtMonat.Day - 1));

            lsSql = RdQueries.GetSqlSelect(41, aiObjektTeil, "", "", "", adtMonat, DateTime.MinValue, 0, asConnect, aiDb);
            liMieterId = fetchData(lsSql, "", 26, asConnect, aiDb);

            return liMieterId;
        }

        // Den Mieter für Leerstand ermitteln
        // Aus ObjektTeil
        // Für Rechnungen zur Timeline, die nicht auf einen aktiven Mietvertrag gebucht werden können
        public static int getMieterLeerstand(int aiObjektTeil, string asConnect, int aiDb)
        {
            String lsSql = "";
            int liMieterId = 0;

            if (aiObjektTeil > 0)
            {
                lsSql = getSql(29, aiObjektTeil, "", "", 0);
                liMieterId = fetchData(lsSql, "", 26, asConnect, aiDb);
            }
            return liMieterId;
        }

        // Den Mieter für Leerstand ermitteln
        // Aus Objekt
        // Für Rechnungen zur Timeline, die nicht auf einen aktiven Mietvertrag gebucht werden können
        public static int getMieterLeerstandObjekt(int aiObjekt, string asConnect, int aiDb)
        {
            String lsSql = "";
            int liMieterId = 0;

            if (aiObjekt > 0)
            {
                lsSql = getSql(291, aiObjekt, "", "", 0);
                liMieterId = fetchData(lsSql, "", 26, asConnect, aiDb);
            }
            return liMieterId;
        }

        // Ermitteln der Anzahl der aktuell wohnenden Personen in einem Objekt, Objektteil
        // Gesucht wird nach aktiven Verträgen in einem Objekt, Objektteil
        // Wird benötigt, um eine Kostenaufteilung nach Personen zu machen
        // Das Flag soll die fehlenden Informationen holen 0 = nix; 1 = ObjektId; 2 = TeilobjektId
        private static decimal getAktPersonen(int aiObjekt, int aiObjektTeil, int aiMieterId, string asDatVon, string asDatBis, int aiFlag, string asConnect, int aiDb)
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
                liObjId = getIdObj(aiMieterId, asConnect, 1, aiDb);
                lsSql = lsSql = @"Select sum(vertrag.anzahl_personen) from vertrag where vertrag.vertrag_aktiv = 1 And vertrag.id_objekt = " + liObjId.ToString();
            }

            // TeilObjekt ID aus Mieter Id holen
            if (aiFlag == 2)
            {
                liObTId = getIdObjTeil(aiMieterId, asConnect, aiDb);
                lsSql = @"Select sum(vertrag.anzahl_personen) from vertrag where vertrag.vertrag_aktiv = 1 And vertrag.id_objekt_teil = " + liObTId.ToString();
            }

            lsSqlAdd = " And vertrag.datum_von <= Convert(DateTime," + "\'" + asDatVon + "',104) "
                                 + "And vertrag.datum_bis >= Convert(DateTime," + "\'" + asDatBis + "',104)";

            lsSql = lsSql + lsSqlAdd;

            // Daten holen
            ldAnzahlPersonen = fetchDataDecimal(lsSql, "", 1, asConnect, aiDb);

            return ldAnzahlPersonen;
        }

        // Die Nebenkosten ID in der Tabelle art_KostenArt ermitteln
        // Art 1 = Zahlung Nebenkosten
        // Art 2 = Zählerstände
        public static int getKsaId(int aiArt, String asConnect, int aiDb)
        {
            int liKsaId = 0;
            String lsSql = "";

            lsSql = getSql(30, aiArt, "", "", 0);
            liKsaId = fetchData(lsSql, "", 26, asConnect, aiDb);

            return liKsaId;
        }

        // Den Verteilungskurzstring aus der Tabelle art_verteilung ermitteln
        public static string getVerteilung(String asConnect, int aiVerteilungId, int aiDb)
        {
            string lsVerteilung = "";
            String lsSql = "";

            lsSql = @"Select kb From art_verteilung Where id_verteilung = " + aiVerteilungId.ToString();
            lsVerteilung = fetchDataString(lsSql, "", 1, asConnect, aiDb);

            return lsVerteilung;
        }

        // Die VerteilungsId aus Rechnungen ermitteln
        private static int getVerteilungsId(string asConnect, int aiTimelineId, int aiDb)
        {
            int liVerteilungId = 0;
            String lsSql = "";

            lsSql = getSql(32, aiTimelineId, "", "", 0);
            liVerteilungId = fetchData(lsSql, "", 26, asConnect, aiDb);

            return liVerteilungId;
        }

        // Verteilungs ID aus art_verteilung ermitteln
        private static int getIdArtVerteilung(string asBez, string asConnect, int aiDb)
        {
            int liVerteilungId = 0;
            String lsSql = "";

            lsSql = getSql(33, 0, asBez, "", 0);
            liVerteilungId = fetchData(lsSql, "", 26, asConnect, aiDb);

            return liVerteilungId;
        }


        // Den Verteilungskurzstring aus der Tabelle art_verteilung ermitteln
        public static string getVerteilungFromString(String asConnect, string asVerteilung, int aiDb)
        {
            string lsVerteilung = "";
            String lsSql = "";

            lsSql = @"Select kb From art_verteilung Where bez = '" + asVerteilung.ToString().Trim() + " '";
            lsVerteilung = fetchDataString(lsSql, "", 1, asConnect, aiDb);

            return lsVerteilung;
        }

        // Und den Sql Zusatz für Reports in eine xml-Datei speichern
        public static void saveLastSql(string asSqlKostenDirekt, string asSqlContent, string asSqlContSumObj, string asSqlConSumObjt,
            string asSqlZahlungen, string asSqlZahlungenSumme,
            string asSqlPersonen, string asSqlZaehler, string asSqlLeerstaende, string asReport, string asSqlRgNr)
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

                if (asSqlZahlungen.Length > 0)
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

                if (asSqlZaehler.Length > 0)
                {
                    xmlwriter.WriteStartElement("LastSqlContent2");
                    xmlwriter.WriteString(asSqlZaehler);     // Darstellung nur ObjektKosten Zähler
                    xmlwriter.WriteEndElement();
                }


                if (asSqlContent.Length > 0)
                {
                    xmlwriter.WriteStartElement("LastSqlRgNr");
                    xmlwriter.WriteString(asSqlRgNr);       // Rechnungsnummer Anschreiben speichern
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
        internal static int getIdObjTeil(int aiId, string asConnect, int aiDb)
        {
            int liIdObjTeil = 0;
            String lsSql = "";

            lsSql = getSql(34, aiId, "", "", 0);
            liIdObjTeil = fetchData(lsSql, "", 26, asConnect, aiDb);

            return liIdObjTeil;
        }

        // Die Objekt ID aus den Vertragsdaten ermitteln aus der Mieter Id = 1 oder der Teilobjekt ID = 2
        internal static int getIdObj(int aiId, string asConnect, int aiArt, int aiDb)
        {
            int liIdObj = 0;
            String lsSql = "";

            lsSql = getSql(35, aiId, "", "", aiArt);
            liIdObj = fetchData(lsSql, "", 26, asConnect, aiDb);

            return liIdObj;
        }

        // Die Tabelle x_abr_content wird gefüllt
        // asSql ist die Timeline
        // asSqlContent ist die Zieltabelle. Sie zeigt das Content des Reports Nebenkostenabrechnung
        internal static int fill_content(string asSql, string asSqlContent, string asSql2, string asDatVon, string asDatBis, string asConnect, string asSqlRgNr, int aiAnschreiben, int aiDb)
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


            // Tabelle Report Content leeren
            liOk = Timeline.delContent(asConnect, aiDb);

            // Timeline einlesen
            //tableTimeline = new DataTable();
            //tableTimeline1 = new DataTable();     // Kosten des Objektes darstellen 
            //tableContent = new DataTable();       // Content

            if (aiAnschreiben == 1)
            {
                // Rechnunsnummer für Anschreiben prüfen und einsetzen
                // ist eine id_rg_nr in der Timeline vorhanden?
                liOk = fetchData(asSql, "", 27, asConnect, aiDb);

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
                    liIdRgNr = getRgNrFromPool(asConnect, aiDb);          // ID Rechnungsnummer aus dem Pool besorgen
                    if (liIdRgNr > 0)
                    {
                        liOk = setRgNrToTml(liIdRgNr, asSqlRgNr, asConnect, aiDb);       // ID Rechnungsnummer in Timeline einsetzen
                        if (liOk == 1)
                        {
                            liOk = setRgNrFromPool(liIdRgNr, asConnect, aiDb);    // Die Rechnungsnummer als besetzt kennzeichnen 
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
            if (asSql.Length > 2)
            {
                liOk = fetchData(asSql, "", 28, asConnect, aiDb);
            }
            // Zweite Tabelle Timeline ObjektKostendarstellung
            if (asSql2.Length > 2)
            {
                liOk = fetchData(asSql2, "", 29, asConnect, aiDb);
            }
            // Dritte Tabelle x_abr_content
            if (asSqlContent.Length > 2)
            {
                liOk = fetchData(asSqlContent, "", 30, asConnect, aiDb);
            }

            // Schleife durch Timeline asSql und erstmal stumpf an Tabelle Content übertragen
            // Achtung rows.count -1, weil i bei 0 anfängt
            if (tableTimeline != null)
            {
                for (int i = 0; i < tableTimeline.Rows.Count; i++)
                {
                    DataRow dr = tableContent.NewRow();

                    if (tableTimeline.Rows[i].ItemArray.GetValue(6) != DBNull.Value)
                    {
                        dr[2] = Convert.ToInt16(tableTimeline.Rows[i].ItemArray.GetValue(6).ToString());        //  Id Extern TimeLine
                        liIdExternTimeline = Convert.ToInt16(tableTimeline.Rows[i].ItemArray.GetValue(6).ToString());

                        dr[27] = getRgInfo(liIdExternTimeline, asConnect, 1, aiDb).Trim();                                   // Rechnungsnummer
                        dr[28] = getRgInfo(liIdExternTimeline, asConnect, 2, aiDb).Trim();                                   // Rechnungstext
                        string lsd;
                        lsd = getRgInfo(liIdExternTimeline, asConnect, 3, aiDb);
                        if (lsd.Length > 0)
                        {
                            dr[29] = lsd;
                        }
                    }
                    if (tableTimeline.Rows[i].ItemArray.GetValue(7) != DBNull.Value)
                    {
                        if (Convert.ToInt16(tableTimeline.Rows[i].ItemArray.GetValue(7).ToString()) > 0)
                        {
                            dr[3] = Convert.ToInt16(tableTimeline.Rows[i].ItemArray.GetValue(7).ToString());            //  Id Vorrauszahlung
                        }
                    }
                    if (tableTimeline.Rows[i].ItemArray.GetValue(18) != DBNull.Value)                                   // Id Zählerstand
                    {
                        if (Convert.ToInt16(tableTimeline.Rows[i].ItemArray.GetValue(18).ToString()) > 0)
                        {
                            dr[4] = Convert.ToInt16(tableTimeline.Rows[i].ItemArray.GetValue(18).ToString());           // Id Zählerstand
                            liIdZaehlerstand = Convert.ToInt16(tableTimeline.Rows[i].ItemArray.GetValue(18).ToString());
                            // dr[13] = Convert.ToDecimal(tableTimeline.Rows[i].ItemArray.GetValue(12).ToString());         // Zählerstand wird hier nicht genutzt auf null prüfen
                        }
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
                        liIdArtVerteilung = Timeline.getIdArtVerteilung("zl", asConnect, aiDb);
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
                        dr[26] = Timeline.getVerteilungsInfo(asConnect, liIdExternTimeline, liIdArtVerteilung, liIdObj, liIdObjt, liIdMieter, asDatVon, asDatBis, liIdExternTimelineZaehlerstand, 1, aiDb);
                    }

                    // Rechnung aus Objekt oder Teilobjekt
                    if (liIdExternTimeline > 0)
                    {
                        // Objektsummen holen
                        lsSql = getSql(14, liIdExternTimeline, "", "", 0);
                        liOk = Timeline.fetchData(lsSql, "", 14, asConnect, aiDb);

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
                            liIdObjt = getVertragInfoFromMieter(liIdMieter, asConnect, 1, aiDb);
                        }
                        lsSql = Timeline.getSql(15, liIdExternTimeline, "", "", liIdObjt);
                        liOk = Timeline.fetchData(lsSql, "", 15, asConnect, aiDb);

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
                        lsSql = getSql(16, liIdZaehlerstand, "", "", 0);
                        liOk = Timeline.fetchData(lsSql, "", 14, asConnect, aiDb);

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
                            liIdObjt = getVertragInfoFromMieter(liIdMieter, asConnect, 1, aiDb);
                        }
                        lsSql = getSql(17, liIdZaehlerstand, "", "", 0);
                        liOk = Timeline.fetchData(lsSql, "", 15, asConnect, aiDb);

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
            }



            // Zweiter Teil, nur ObjektKosten darstellen (im Moment nur Zähler)
            // Schleife durch Timeline1 asSql2 und erstmal stumpf an Tabelle Content übertragen
            // Achtung rows.count -1, weil i bei 0 anfäng
            if (tableTimeline1 != null)
            {
                for (int i = 0; i < tableTimeline1.Rows.Count; i++)
                {
                    DataRow dr = tableContent.NewRow();

                    if (tableTimeline1.Rows[i].ItemArray.GetValue(6) != DBNull.Value)
                    {
                        dr[2] = Convert.ToInt16(tableTimeline1.Rows[i].ItemArray.GetValue(6).ToString());        //  Id Rechnung
                        liIdExternTimeline = Convert.ToInt16(tableTimeline1.Rows[i].ItemArray.GetValue(6).ToString());
                        dr[27] = getRgInfo(liIdExternTimeline, asConnect, 1, aiDb).Trim();                                   // Rechnungesnummer
                        dr[28] = getRgInfo(liIdExternTimeline, asConnect, 2, aiDb).Trim();                                   // Rechnungstext

                        if (tableTimeline1.Rows[i].ItemArray.GetValue(7) != DBNull.Value)
                        {
                            dr[3] = Convert.ToInt16(tableTimeline1.Rows[i].ItemArray.GetValue(7).ToString());        //  Id Vorrauszahlung
                        }
                        if (tableTimeline1.Rows[i].ItemArray.GetValue(18) != DBNull.Value)                            // Id Zählerstand
                        {
                            dr[4] = Convert.ToInt16(tableTimeline1.Rows[i].ItemArray.GetValue(18).ToString());         // Id Zählerstand
                            liIdZaehlerstand = Convert.ToInt16(tableTimeline1.Rows[i].ItemArray.GetValue(18).ToString());
                            // dr[13] = Convert.ToDecimal(tableTimeline1.Rows[i].ItemArray.GetValue(12).ToString());   // Zählerstand wird hier nicht genutzt auf null prüfen
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
                            liIdArtVerteilung = Timeline.getIdArtVerteilung("zl", asConnect, aiDb);
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
                            dr[26] = Timeline.getVerteilungsInfo(asConnect, liIdExternTimeline, liIdArtVerteilung, liIdObj, liIdObjt, liIdMieter, asDatVon, asDatBis, liIdExternTimelineZaehlerstand, 1, aiDb);
                        }

                        // Rechnung aus Objekt oder Teilobjekt
                        if (liIdExternTimeline > 0)
                        {
                            // Objektsummen holen
                            lsSql = getSql(14, liIdExternTimeline, "", "", 0);
                            liOk = Timeline.fetchData(lsSql, "", 14, asConnect, aiDb);

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
                            liOk = Timeline.fetchData(lsSql, "", 14, asConnect, aiDb);

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
            }


            // Ab in die Datenbank
            liOk = fetchData("", "", 31, asConnect, aiDb);
            // ist es eine Mieter ID in Timeline, dann die Summen aus Teilobjekt und Objekt einsetzen
            // Ist es eine Teilobjekt ID, dann die Summen aus Objekt einsetzen
            return (liOk);
        }

        // ReportTabelle vor Gebrauch löschen
        private static int delContent(string asConnect, int aiDb)
        {
            int liOk = 0;
            // kann schonmal gelöscht werden
            lsSql = getSql(36, 0, "", "", 0);
            liOk = fetchData(lsSql, "", 26, asConnect, aiDb);
            return (liOk);
        }

        // Die Rechnungs ID aus dem SqlStatement ermitteln
        internal static int getRechnungsId(string asSqlTimeline, string asConnect, int aiDb)
        {
            int liIdRechnung = 0;

            liIdRechnung = fetchData(asSqlTimeline, "", 16, asConnect, aiDb);

            return (liIdRechnung);
        }

        // Den Verbrauch aus dem Zählerstand ermitteln
        internal static decimal getZlVerbrauch(decimal adZlStand, int aiZlId, string asConnect, int aiFlagNew, int aiDb)
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

            // Daten holen
            ldZlStandOld = fetchDataDecimal(lsSql, "", 1, asConnect, aiDb);
            // Differenz
            ldZlVerbrauch = adZlStand - ldZlStandOld;

            return ldZlVerbrauch;
        }

        // Zähler Id vom Namen des Zählers ermitteln
        internal static int getZlId(string lsZlName, string asConnect, int aiDb)
        {
            String lsSql = "";
            int liZlId = 0;

            lsSql = getSql(37, 0, lsZlName, "", 0);
            liZlId = fetchData(lsSql, "", 26, asConnect, aiDb);

            return liZlId;
        }

        // Mehrwertsteuersatz für Zähler holen (aus ZählerId)
        internal static int getMwstSatzZaehler(int aiZlId, string asConnect, int aiDb)
        {
            String lsSql = "";
            int liMwstSatz = 0;

            lsSql = getSql(38, aiZlId, "", "", 0);
            liMwstSatz = fetchData(lsSql, "", 26, asConnect, aiDb);

            return liMwstSatz;
        }

        // Für die bedingte Weiterleitung
        // Hier wird die Auswahl der Objektteile vorbereitet
        // Die Objektteile (Wohnungen) werden in die Tabelle
        // objekt_mix_parts geschrieben
        internal static int makeChoose(int aiObjekt, int aiTimeLineId, string asConnect, int aiDb)
        {
            int liOk = 0;
            int liRowGet = 0;
            int liRows = 0;

            // Hat die Tabelle objekt_mix_parts einen Eintrag für diese Timeline ID?
            liRows = Timeline.getInfoFromParts(asConnect, aiTimeLineId, aiDb);

            if (liRows == 0)            // Kein Eintrag vorhanden, Datensatz wird angelegt
            {
                liRowGet = Timeline.copyParts(asConnect, aiObjekt, aiTimeLineId, aiDb);
                liOk = 1;

            }
            if (liRows > 0)         // Es existiert ein Eintrag der Timeline ID > editieren
            {
                liOk = 2;
            }
            return liOk;
        }

        // Kopieren der Daten eines Objektes in die Tabelle objekt_mix_parts
        private static int copyParts(string asConnect, int aiObjekt, int aiTimeLineId, int aiDb)
        {
            String lsSql = "";
            int liObj = 0;

            lsSql = getSql(39, aiObjekt, "", "", 0);
            liObj = fetchData(lsSql, "", 26, asConnect, aiDb);

            return liObj;
        }

        // Prüfen: Ist die Tabelle objekt_mix_parts leer für diese Timeline ID
        private static int getInfoFromParts(string asConnect, int aiTimeLineId, int aiDb)
        {
            String lsSql = "";
            int liRows = 0;

            lsSql = getSql(40, aiTimeLineId, "", "", 0);
            liRows = fetchData(lsSql, "", 26, asConnect, aiDb);

            return liRows;
        }

        // Verteilungsinformationen für die Nebenkostenabrechnung ermitteln
        // aiId Rechnung ist die Rechnungs Id aus extern Timeline ID ACHTUNG!!
        private static object getVerteilungsInfo(string asConnect, int aiIdRechnung, int aiArtVerteilungId,
            int aiObjektId, int aiTObjektId, int aiMieterId,
            string asDatVon, string asDatBis, int aiIdExternTimelineZaehlerstand, int aiDetailGrad, int aiDb)
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

            lsVerteilung = getVerteilung(asConnect, aiArtVerteilungId, aiDb);

            // Flächenanteil rechnen
            if (lsVerteilung == "fl")
            {
                // Gesamtfläche aus Tabelle Objekt holen
                if (aiMieterId > 0 || aiTObjektId > 0 || aiObjektId > 0)
                {
                    ldGesamtflaecheObjekt = getObjektflaeche(aiObjektId, aiTObjektId, aiMieterId, asConnect, aiDb);
                    if (aiTObjektId > 0 || aiObjektId > 0 || aiMieterId > 0)
                    {
                        if (aiTObjektId > 0 || aiMieterId > 0)
                        {
                            ldFlaecheTObjekt = getTObjektflaeche(aiTObjektId, aiMieterId, asConnect, aiDb);
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
                    ldProzentAnteil = getTObjektAnteil(aiTObjektId, aiMieterId, asConnect, aiDb);
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
                ldAnzPersonen = getAktPersonen(aiObjektId, aiTObjektId, aiMieterId, asDatVon, asDatBis, 2, asConnect, aiDb);
                ldAnzPersonenGesamt = getAktPersonen(aiObjektId, aiTObjektId, aiMieterId, asDatVon, asDatBis, 1, asConnect, aiDb);
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
                // lsVertInfo = "lt. Rechnung";
                lsVertInfo = "Aus Rg.Nr: \n" + getRgInfo(aiIdRechnung, asConnect, 1, aiDb);
            }

            // Nix wird verteilt                    
            if (lsVerteilung == "nl")
            {
                lsVertInfo = "";
            }

            // Zähler 
            if (lsVerteilung == "zl")
            {
                // Zählerwerte und Kosten ermitteln
                lsVertInfo = getVerteilungsInfoZaehler(aiIdExternTimelineZaehlerstand, asConnect, aiDb);
            }

            // Fläche Auswahl für den Report Nebenkosten
            // Die Gesamtfläche für die Auswahl wird ermittelt
            if (lsVerteilung == "fa")
            {
                // Gesamtfläche der ausgewählten Wohnungen aus Tabelle Objekt_mix_parts holen
                liObjektId = getIdObj(aiMieterId, asConnect, 1, aiDb);
                if (liObjektId > 0)
                {
                    int liArt = 0;
                    // Gesamtfläche der Auswahl = 0 oder Gesamtfläche = 1
                    liArt = getObjektflaecheAuswFlag(liObjektId, asConnect, aiDb);
                    ldGesamtflaecheObjekt = getObjektflaecheAuswahl(liObjektId, aiIdRechnung, asConnect, liArt, aiDb);
                    if (aiTObjektId > 0 || aiMieterId > 0)
                    {
                        ldFlaecheTObjekt = getTObjektflaeche(aiTObjektId, aiMieterId, asConnect, aiDb);
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
                                        lsVertInfo = lsVertInfo + getObjekteAuswahl(aiIdRechnung, asConnect, aiDb);
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
        private static string getObjekteAuswahl(int aiTimelineId, string asConnectString, int aiDb)
        {
            string lsSql = "";
            string lsInfo = "Beteiligte Mietflächen";
            string lsBez = "";
            string lsGeschoss = "";
            string lsLage = "";
            int liOk = 0;

            // objekt_mix_parts
            lsSql = getSql(25, aiTimelineId, "", "", 0);
            liOk = fetchData(lsSql, "", 25, asConnectString, aiDb);

            // schleife durch objekt_mix_parts > tableParts
            for (int i = 0; i < tableParts.Rows.Count; i++)
            {
                if (tableParts.Rows[i].ItemArray.GetValue(4) != DBNull.Value)
                    lsBez = tableParts.Rows[i].ItemArray.GetValue(4).ToString().Trim();
                if (tableParts.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                    lsGeschoss = tableParts.Rows[i].ItemArray.GetValue(10).ToString().Trim();
                if (tableParts.Rows[i].ItemArray.GetValue(11) != DBNull.Value)
                    lsLage = tableParts.Rows[i].ItemArray.GetValue(11).ToString().Trim();
                lsInfo = lsInfo + "\nBez: " + lsBez + "\nGeschoss: " + lsGeschoss + "\nLage: " + lsLage;
            }
            return lsInfo;
        }

        // Berechnung der Fläche für die Auswahl 0 = gewählte Objekte 1 = Gesamtfläche
        private static int getObjektflaecheAuswFlag(int liObjekt, string asConnect, int aiDb)
        {
            int liFlag = 0;
            string lsSql = "";

            lsSql = getSql(41, liObjekt, "", "", 0);
            liFlag = fetchData(lsSql, "", 26, asConnect, aiDb);

            return liFlag;
        }

        // Rechnungsnummer oder Rechnungstext aus RechnungesId holen 1= RgNr 2= RgText
        private static string getRgInfo(int aiIdExternTimeline, string asConnect, int aiArt, int aiDb)
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

            lsRgInfo = fetchDataString(lsSql, "", 1, asConnect, aiDb);

            return lsRgInfo;
        }

        // Zusammenstellen vomn Zählerinfos für die Nebenkostenabrechnung
        private static string getVerteilungsInfoZaehler(int aiIdExternTimelineZaehlerstand, string asConnectString, int aiDb)
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

            lsSql = getSql(24, aiIdExternTimelineZaehlerstand, "", "", 0);
            liOk = fetchData(lsSql, "", 24, asConnectString, aiDb);

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
        internal static void deleteParts(string asConnect, int aiDb)
        {
            string lsSql = "";
            int liOk = 0;

            lsSql = getSql(42, 0, "", "", 0);
            liOk = fetchData(lsSql, "", 26, asConnect, aiDb);
        }

        // Informationen über Vertragsbeginn und Ende mit der Mieter id
        // Art 1 = Vertragsbeginn
        // Art 2 = Vertragsende
        private static DateTime getVertragInfo(int aiArt, DateTime adtMonat, int aiMieter, string asConnect, int aiDb)
        {
            DateTime ldtVertrag = DateTime.MinValue;
            string lsSql = "";

            lsSql = RdQueries.GetSqlSelect(42, aiMieter, "", "", "", adtMonat, DateTime.MinValue, aiArt, asConnect, aiDb);
            // Daten holen
            ldtVertrag = fetchDataDate(lsSql, "", 1, asConnect, aiDb);

            return ldtVertrag;
        }

        // Vertragsinfos vom Mieter art 1 = Teilbjekt
        private static int getVertragInfoFromMieter(int liIdMieter, string asConnect, int aiArt, int aiDb)
        {
            string lsSql = "";
            int liInfo = 0;

            lsSql = getSql(43, liIdMieter, "", "", 0);
            liInfo = fetchData(lsSql, "", 26, asConnect, aiDb);

            return liInfo;
        }

        // Rechnungsnummer für Anschreiben aus dem Pool besorgen
        private static int getRgNrFromPool(string asConnect, int aiDb)
        {
            string lsSql = "";
            int liInfo = 0;

            lsSql = getSql(44, 0, "", "", 0);
            liInfo = fetchData(lsSql, "", 26, asConnect, aiDb);

            return liInfo;
        }

        // Rechnungsnummer aus dem Pool als besetzt kennzeichnen
        private static int setRgNrFromPool(int liIdRgNr, string asConnect, int aiDb)
        {
            string lsSql = "";
            int liOk = 0;

            lsSql = getSql(45, liIdRgNr, "", "", 0);
            liOk = fetchData(lsSql, "", 26, asConnect, aiDb);

            return liOk;
        }

        // Die ID der Rechnungsnummer Anschreiben in Timeline einsetzen
        private static int setRgNrToTml(int aiIdRgNr, string asSqlRgNr, string asConnect, int aiDb)
        {
            string lsSql = "";
            int liOk = 0;

            lsSql = getSql(46, aiIdRgNr, asSqlRgNr, "", 0);
            liOk = fetchData(lsSql, "", 26, asConnect, aiDb);

            return liOk;
        }

        // Aus dem String der Bezeichnung die VerteilungsId holen
        internal static int getVertId(string asBez, string asConnect, int aiDb)
        {
            int liId;

            lsSql = getSql(47, 0, asBez, "", 0);
            liId = fetchData(lsSql, "", 26, asConnect, aiDb);

            return (liId);
        }

        // ermitteln des des aktuellen Mandanten
        internal static int getMandantId(string asConnect, int aiDb)
        {
            int liId;

            lsSql = getSql(48, 0, "", "", 0);
            liId = fetchData(lsSql, "", 26, asConnect, aiDb);

            return (liId);
        }

        // Id der Filaile aus der Mandanten Id ermitteln
        internal static int getFilialeId(int aiMandantId, string asConnect, int aiDb)
        {
            int liId;

            lsSql = getSql(49, aiMandantId, "", "", 0);
            liId = fetchData(lsSql, "", 26, asConnect, aiDb);

            return (liId);
        }

        // Ermitteln des Start und Endedatum eines Jahres
        internal static DateTime GetYear(DateTime adtYear, int aiArt)
        {
            int liYear = adtYear.Year;
            switch (aiArt)
            {
                case 1:
                    adtYear = new DateTime(liYear, 1, 1);
                    break;
                case 2:
                    adtYear = new DateTime(liYear, 12, 31, 23, 59, 59);
                    break;
                default:
                    break;
            }
            return adtYear;
        }
    }

}