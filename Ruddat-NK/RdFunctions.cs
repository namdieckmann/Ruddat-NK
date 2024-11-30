using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Xml;
using MySql.Data.MySqlClient;
using System.Threading;
using System.Windows.Threading;
// using Microsoft.Office.Interop.Excel;

namespace Ruddat_NK
{
    public class Timeline
    {
        static string lsSql = "";

        // Datensätze Rechnungen
        static DataTable TblRechnungen;              // Rechnungen
        static DataTable TblRechnungenTimeline;      // Rechnungen für TimelineCreate
        static DataTable TblTimelineNew;
        static DataTable TblTimeLineSet;
        static DataTable TblObjektTeile;
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

        static MySqlDataAdapter MySdRechnungen;
        static MySqlDataAdapter MySdObjektTeile;
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
        static MySqlDataAdapter mysdParts;
        static MySqlDataAdapter myadp;

        // Delegates
        private delegate void DelPassDb(int giDb);

        // ----------------------------------------------------------------------------------------------
        // Bisher höchste Id für Timeline ermitteln
        public static int getTimelineId(string asConnect, int asArt, int aiDb)
        {
            Int32 liGetLastTempId = 0;

            lsSql = GetSql(26, asArt, "", "", 0);
            liGetLastTempId = Timeline.FetchData(lsSql, "", 26, asConnect);
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
        public static void EditRechung(int LiIdRechnung, int LiObjektId, int liFlagAdd, string asConnect)
        {
            string lsSql = "";
            string LsSql2 = "";
            int liRows = 0;
            int liOk = 0;

            switch (liFlagAdd)
            {
                case 1:
                    // Rechnungen Daten holen mit id extern timeline
                    lsSql = Timeline.GetSql(1, LiIdRechnung, "", "", 0);                // Rechnungen
                    LsSql2 = Timeline.GetSql(6, LiObjektId, "", "", 0);                 // Liste der Teilobjekte dazuholen
                    liRows = Timeline.FetchData(lsSql, LsSql2, 1, asConnect);           // TblRechnungen
                    break;
                case 2:
                    // Rechnung Timeline löschen
                    liOk = Timeline.TimelineDelete(LiIdRechnung, "R", asConnect);
                    break;
                case 11:
                    // Zahlungen Daten holen mit id extern timeline
                    lsSql = Timeline.GetSql(12, LiIdRechnung, "", "", 0);
                    // Sql, Art = 11 
                    liRows = Timeline.FetchData(lsSql, "", 11, asConnect);
                    break;
                case 12:
                    // Zahlungen Timeline löschen 
                    liOk = Timeline.TimelineDelete(LiIdRechnung, "A", asConnect);
                    break;
                case 13:
                    // Zahlungen importieren. Nur anderes SQL Statement, sonst wie Case 11
                    lsSql = Timeline.GetSql(13, LiIdRechnung, "", "", 0);
                    // Sql, Art = 11 
                    liRows = Timeline.FetchData(lsSql, "", 11, asConnect);
                    break;
                case 21:
                    // Zählerstände Daten holen mit id extern timeline
                    lsSql = Timeline.GetSql(21, LiIdRechnung, "", "", 0);
                    // Sql, Art = 21 
                    liRows = Timeline.FetchData(lsSql, "", 21, asConnect);
                    break;
                case 22:
                    // Zählerstände Timeline löschen
                    liOk = Timeline.TimelineDelete(LiIdRechnung, "Z", asConnect);
                    break;
                default:
                    break;
            }
        }

        // Sql Statements zusammenbauen
        public static string GetSql(int piArt, int piId, string ps2, string ps3, int piId2)
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

                    lsSql = @"SELECT id_rechnungen,
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
                                    id_verteilung,
                                    id_rechnung_source
                            FROM rechnungen
					         WHERE id_rechnungen = " + lsWhereAdd +
                          " ORDER BY rechnungen.datum_rechnung desc";
                    break;
                case 200:
                    // Timeline löschen Rechnung
                    lsWhereAdd = piId.ToString() + " ";

                    lsSql = @"delete FROM timeline
					         WHERE id_rechnung = " + lsWhereAdd;
                    break;
                case 201:
                    // Timeline löschen Zahlung
                    lsWhereAdd = piId.ToString() + " ";

                    lsSql = @"delete FROM timeline
					         WHERE id_vorauszahlung = " + lsWhereAdd;
                    break;
                case 202:
                    // Timeline löschen Zählerstand
                    lsWhereAdd = piId.ToString() + " ";

                    lsSql = @"delete FROM timeline
					         WHERE id_zaehlerstand = " + lsWhereAdd;
                    break;
                case 3:
                    // Timeline neu erzeugen in ps2 steht, welches Feld beschrieben werden soll
                    lsWhereAdd = piId.ToString() + " ";
                    lsSql = @"SELECT 
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
                            FROM timeline
                             WHERE " + ps2 + " = " + " \'" + lsWhereAdd + "\'";
                    break;
                case 31:
                    // Timeline neu erzeugen in ps2 steht, welches Feld beschrieben werden soll
                    lsWhereAdd = piId.ToString() + " ";
                    lsSql = @"SELECT 
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
                            FROM timeline
                             WHERE " + ps2 + " = " + lsWhereAdd;
                    break;
                case 4:
                    // TimelineRelations sollen geschrieben werden
                    // Hier auf Grundlage des Objektes
                    // Beschrieben werden die Kosten für Objektteile
                    lsWhereAdd = "id_rechnung = " + piId.ToString() + " ";
                    lsWhereAdd2 = " id_objekt = " + ps2 + " ";

                    lsSql = @"SELECT 
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
                            FROM timeline
                                 WHERE " + lsWhereAdd + " and " + lsWhereAdd2 + " ORDER BY dt_monat";
                    break;
                case 50:                // Rechnungen
                    // TimelineRelations sollen geschrieben werden
                    // Hier auf Grundlage des ObjektTeils
                    // Beschrieben werden die Kosten für Mieter
                    lsWhereAdd = " id_rechnung = " + piId.ToString() + " ";
                    lsWhereAdd2 = " id_objekt_teil > 0 "; // + ps2 + " ";

                    lsSql = @"SELECT 
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
                            FROM timeline
                                 WHERE " + lsWhereAdd + " and " + lsWhereAdd2 + "ORDER BY id_objekt_teil, dt_monat";
                    break;
                case 51:            // Zahlungen
                    // TimelineRelations sollen geschrieben werden
                    // Hier auf Grundlage des ObjektTeils
                    // Beschrieben werden die Kosten für Mieter
                    lsWhereAdd = " id_vorauszahlung = " + piId.ToString() + " ";
                    lsWhereAdd2 = " id_objekt_teil > 0 "; // + ps2 + " ";

                    lsSql = @"SELECT 
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
                            FROM timeline
                                 WHERE " + lsWhereAdd + " and " + lsWhereAdd2 + "ORDER BY dt_monat";
                    break;
                case 52:        // Zähler
                    // TimelineRelations sollen geschrieben werden
                    // Hier auf Grundlage des ObjektTeils
                    // Beschrieben werden die Kosten für Mieter
                    lsWhereAdd = " id_zaehlerstand = " + piId.ToString() + " ";
                    lsWhereAdd2 = " id_objekt_teil > 0 "; // + ps2 + " ";

                    lsSql = @"SELECT 
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
                            FROM timeline
                                 WHERE " + lsWhereAdd + " and " + lsWhereAdd2 + "ORDER BY dt_monat";
                    break;
                case 6:
                    // für die TimelineRelation Objektteile holen
                    lsWhereAdd = "id_objekt = " + piId.ToString() + " ";
                    lsSql = @"SELECT id_objekt_teil,
                                id_objekt,
                                bez,
                                geschoss,
                                lage,
                                id_adresse,
                                flaeche_anteil,
                                prozent_anteil,
                                personen_anteil_flag
                            FROM objekt_teil
                             WHERE " + lsWhereAdd;
                    break;
                case 7:
                    lsWhereAdd = "id_mieter = " + piId.ToString() + " ";
                    lsSql = @"SELECT id_mieter,
                                id_vertrag,
                                bez
                            FROM mieter
                             WHERE " + lsWhereAdd;
                    break;
                case 8:
                    lsWhereAdd = "Id_mwst_art = " + piId.ToString() + " ";
                    lsSql = @"SELECT Id_mwst_art,
                                 bez,
                                 mwst
                            FROM art_mwst
                             WHERE " + lsWhereAdd;
                    break;
                case 9:
                    // MwstSatz holen Bezeichnung ist bekannt Bsp. "normal"
                    lsWhereAdd = "bez = " + " \'" + ps2 + "\' ";
                    lsSql = @"SELECT Id_mwst_art,
                                 bez,
                                 mwst
                            FROM art_mwst
                             WHERE " + lsWhereAdd;
                    break;
                case 11:
                    // Zahlungen
                    lsWhereAdd = "id_vz = " + piId.ToString() + " ";
                    lsSql = @"SELECT id_vz,
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
                            FROM zahlungen  WHERE " + lsWhereAdd;
                    break;
                case 12:
                    // Zahlungen mit definierter Timeline
                    lsWhereAdd = "id_extern_timeline = " + piId.ToString() + " ";
                    lsSql = @"SELECT id_vz,
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
                            FROM zahlungen  WHERE " + lsWhereAdd;
                    break;
                case 13:
                    // Zahlungen aus automatischem Import. Alle mit flag_timeline = 1 und der übergebenen Import ID
                    lsWhereAdd = "id_import = " + piId.ToString() + " ";
                    lsSql = @"SELECT id_vz,
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
                            FROM zahlungen  WHERE flag_timeline = 1 and " + lsWhereAdd;
                    break;
                case 21:
                    // Zählerstände mit definierter Timeline
                    lsWhereAdd = "id_extern_timeline = " + piId.ToString() + " ";
                    lsSql = @"SELECT Id_zs,               
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
                        FROM zaehlerstaende  WHERE " + lsWhereAdd;
                    break;
                case 24:
                    // Zählerinfo für Report Nebenkosten holen
                    lsWhereAdd = "  WHERE id_extern_timeline = " + piId.ToString() + " ";
                    lsSql = @"SELECT Id_zs,               
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
                        FROM zaehlerstaende
                        left join zaehler on zaehler.Id_zaehler = zaehlerstaende.id_zaehler
                        left join art_einheit on zaehler.id_einheit = art_einheit.Id_einheit "
                        + lsWhereAdd;
                    break;
                case 25:
                    // Zusammenstellungen der gewählten Wohnungen für den Report Nebenkosten
                    lsSql = @"SELECT Id_obj_mix_parts,id_objekt_mix,id_objekt,id_objekt_teil,bez,sel,flaeche_anteil,    
                                id_timeline,ges_fl_behalten,erklaerung,geschoss,lage
                                    FROM objekt_mix_parts";
                    lsWhereAdd = "  WHERE sel > 0 and id_timeline = " + piId.ToString() + " ";
                    // lsWhereAdd2 = " and id_objekt = " + piId2.ToString() + " ";
                    lsSql = lsSql + lsWhereAdd + lsWhereAdd2;
                    break;
                case 26:
                    // Max Ids ermitteln
                    switch (piId)
                    {
                        case 1:
                            lsSql = "SELECT max(id_extern_timeline) FROM rechnungen";
                            break;
                        case 2:
                            lsSql = "SELECT max(id_extern_timeline) FROM zahlungen";
                            break;
                        case 3:
                            lsSql = "SELECT max(id_extern_timeline) FROM zaehlerstaende";
                            break;
                        default:
                            break;
                    }
                    break;
                case 27:
                    lsSql = "SELECT id_objekt_teil FROM objekt_mix_parts  WHERE sel = 1 and id_objekt_teil = " + piId.ToString();
                    break;
                case 28:
                    switch (piId2)
                    {
                        case 1:
                            // Weiterleitung an Objektteil
                            lsSql = "SELECT wtl_obj_teil FROM dbo.art_kostenart WHERE Id_ksa =" + piId.ToString();
                            break;
                        // Weiterleitung an Mieter
                        case 2:
                            lsSql = @"SELECT art_kostenart.wtl_mieter FROM timeline 
                                join art_kostenart on timeline.id_ksa = art_kostenart.id_ksa
                                 WHERE timeline.id_rechnung = " + piId.ToString();
                            break;
                        default:
                            break;
                    }
                    break;
                case 29:
                    lsSql = @"SELECT mieter.Id_mieter as mid
                            FROM objekt_teil
                        join objekt on objekt_teil.id_objekt = objekt.Id_objekt
                        Join filiale on filiale.id_filiale = objekt.Id_filiale
                        join mieter on mieter.id_filiale = filiale.Id_Filiale
                             WHERE mieter.leerstand = 1 and objekt_teil.Id_objekt_teil = " + piId.ToString();
                    break;
                case 291:
                    lsSql = @"SELECT mieter.Id_mieter as mid
                            FROM objekt_teil
                        join objekt on objekt_teil.id_objekt = objekt.Id_objekt
                        Join filiale on filiale.id_filiale = objekt.Id_filiale
                        join mieter on mieter.id_filiale = filiale.Id_Filiale
                             WHERE mieter.leerstand = 1 and objekt.Id_objekt = " + piId.ToString();
                    break;
                case 30:        // Kostenstellenart Zähler
                    switch (piId)
                    {
                        case 1:
                            lsSql = @"SELECT id_ksa FROM art_kostenart  WHERE ksa_zahlung = 1 ORDER BY sort;";
                            break;
                        case 2:
                            lsSql = @"SELECT id_ksa FROM art_kostenart  WHERE ksa_zaehler = 1 ORDER BY sort;";
                            break;
                        default:
                            break;
                    }
                    break;
                case 32:        // Die VerteilungsId aus Rechnungen ermitteln
                    lsSql = @"SELECT id_verteilung FROM rechnungen  WHERE id_extern_timeline = " + piId.ToString();
                    break;
                case 33:        // Verteilungs ID aus art_verteilung ermitteln
                    lsSql = @"SELECT id_verteilung FROM art_verteilung  WHERE kb = '" + ps2.ToString() + "'";
                    break;
                case 34:        // Aus den Verträgen die Teilobjekt ID anhand der Mieter ID ermitteln
                    lsSql = @"SELECT id_objekt_teil FROM vertrag  WHERE id_mieter = " + piId.ToString();
                    break;
                case 35:       // Die Objekt ID aus den Vertragsdaten ermitteln aus der Mieter Id = 1 oder der Teilobjekt ID = 2
                    switch (piId2)
                    {
                        case 1:
                            lsSql = @"SELECT id_objekt FROM vertrag  WHERE id_mieter = " + piId.ToString();
                            break;
                        case 2:
                            lsSql = @"SELECT id_objekt FROM vertrag  WHERE id_objekt_teil = " + piId.ToString();
                            break;
                        default:
                            break;
                    }
                    break;
                case 36:        // Report löschen
                    lsSql = "delete FROM x_abr_content;";
                    break;
                case 37:        // Zähler Id
                    lsSql = @"SELECT id_zaehler FROM zaehler  WHERE zaehlernummer = '" + ps2.Trim() + "\'";
                    break;
                case 38:        // Mwst Satz Zähler
                    lsSql = @"SELECT art_mwst.mwst FROM zaehler 
                        left join art_mwst on zaehler.id_mwst_art = art_mwst.Id_mwst_art
                       WHERE id_zaehler = " + piId.ToString();
                    break;
                case 39:
                    lsSql = @"insert into objekt_mix_parts (Id_objekt_teil,id_objekt,flaeche_anteil,bez,geschoss,lage)
                            select Id_objekt_teil,id_objekt,flaeche_anteil,bez,geschoss,lage FROM objekt_teil";
                    lsWhereAdd = "  WHERE objekt_teil.id_objekt = " + piId.ToString() + " ";
                    lsSql = lsSql + lsWhereAdd;
                    break;
                case 40:
                    lsSql = @"SELECT Count(*) FROM objekt_mix_parts";
                    lsWhereAdd = "  WHERE id_timeline = " + piId.ToString() + " ";
                    lsSql = lsSql + lsWhereAdd;
                    break;
                case 41:
                    lsSql = "SELECT ges_fl_behalten FROM objekt_mix_parts  WHERE id_objekt = " + piId.ToString();
                    break;
                case 42:
                    lsSql = @"Delete FROM objekt_mix_parts";
                    break;
                case 43:
                    lsSql = @"SELECT id_objekt_teil FROM vertrag  WHERE vertrag.id_mieter = " + piId.ToString();
                    break;
                case 44:
                    lsSql = @"SELECT id_rg_nr FROM rgnr  WHERE flag_besetzt != 1 ORDER BY rgnr";
                    break;
                case 45:
                    lsSql = @"Update rgnr Set rgnr.flag_besetzt = 1  WHERE rgnr.id_rg_nr = " + piId.ToString();
                    break;
                case 46:
                    lsSql = @"Update timeline Set timeline.id_rg_nr = " + piId.ToString() + ps2;
                    break;
                case 47:
                    lsSql = @"SELECT Id_verteilung FROM art_verteilung  WHERE kb = '" + ps2 + "' ";
                    break;
                case 48:
                    lsSql = @"SELECT id_mandant,sel FROM mandanten  WHERE sel = 1 ";
                    break;
                case 49:
                    lsSql = @"SELECT id_filiale FROM filiale  WHERE id_mandant = " + piId.ToString();
                    break;
                case 150:        // Unterrechnungen löschen
                    lsSql = @"Delete FROM rechnungen  WHERE id_rechnung_source = " + piId.ToString();
                    break;
                default:
                    break;
            }

            // Aus der Rechnungs ID die untergeordneten Summen der Timeline ermitteln
            if (piArt == 14 || piArt == 15 || piArt == 16 || piArt == 17)
            {
                lsSql = @"SELECT Sum(timeline.betrag_netto) as betrag_netto,
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
                        FROM timeline
						Left Join rechnungen on rechnungen.id_extern_timeline = timeline.id_rechnung
						Right Join art_kostenart on timeline.id_ksa = art_kostenart.id_ksa";
                lsGroup = @" Group by timeline.id_rechnung,timeline.id_vorauszahlung,timeline.id_objekt,
							timeline.id_objekt_teil,timeline.id_mieter,rechnungen.Rg_nr,art_kostenart.bez,
							rechnungen.betrag_netto,rechnungen.betrag_brutto,art_kostenart.sort,timeline.wtl_aus_objekt,
                            timeline.wtl_aus_objteil,rechnungen.datum_rechnung,rechnungen.firma,timeline.id_ksa,
                            rechnungen.id_verteilung,timeline.id_zaehlerstand ";
                lsOrder = " ORDER BY art_kostenart.sort ";

                switch (piArt)
                {
                    case 14:
                        lsWhereAdd = "  WHERE timeline.id_rechnung = " + piId.ToString() + " and timeline.id_objekt > 0 ";                           // Objekte
                        break;
                    case 15:
                        lsWhereAdd = @"  WHERE timeline.id_rechnung = " + piId.ToString() + " and timeline.id_objekt_teil = " + piId2.ToString()
                                        + " And timeline.id_mieter = 0 ";                                                                           // Teilobjekte
                        break;
                    case 16:
                        lsWhereAdd = "  WHERE timeline.id_zaehlerstand = " + piId.ToString() + " and timeline.id_objekt > 0 ";                       // Objekte
                        break;
                    case 17:
                        lsWhereAdd = "  WHERE timeline.id_zaehlerstand = " + piId.ToString() + " and timeline.id_objekt_teil = " + piId2.ToString();    // Teilobjekte
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
        public static Int32 FetchData(string psSql, string psSql2, int piArt, string asConnect)
        {
            DateTime ldtStart = DateTime.MinValue;
            DateTime ldtEnd = DateTime.MinValue;
            DateTime ldtMonat = DateTime.MinValue;
            DateTime ldtVertrag = DateTime.MinValue;

            int liExternId;
            int liOk = 0;

            decimal[] ladBetraege = new decimal[12];

            Int32 liReturn = 0;

            try
            {
                MySqlConnection connect;
                connect = new MySqlConnection(asConnect);
                MySqlCommand command = new MySqlCommand(psSql, connect);
                MySqlCommand command2 = new MySqlCommand(psSql2, connect); 
                connect.Open();

                switch (piArt)
                {
                    case 1:     // Rechnungen > Timeline erzeugen bearbeiten
                        TblRechnungen = new DataTable();         // Rechnung 
                        MySdRechnungen = new MySqlDataAdapter(command);
                        TblObjektTeile = new DataTable();
                        MySdObjektTeile = new MySqlDataAdapter(command2);

                        liOk = Afterfetch.MakeAfterFetch(piArt, 1, 0, 0, asConnect, 
                            MySdRechnungen, TblRechnungen,
                            MySdObjektTeile, TblObjektTeile);

                        break;
                    case 2:     // Datensatz löschen
                        MySqlDataReader queryCommandReader = command.ExecuteReader();
                        break;
                    case 3:     // Rechnungen Timeline Create
                        TblRechnungenTimeline = new DataTable();         // Rechnungen
                        MySqlCommand command3 = new MySqlCommand(psSql2, connect);
                        MySdRechnungen = new MySqlDataAdapter(command3);
                        MySdRechnungen.Fill(TblRechnungenTimeline);
                        // Externe ID aus der Rechnung ermitteln 
                        liExternId = Afterfetch.MakeAfterFetch(piArt, 1, 0, 0, asConnect, MySdRechnungen, TblRechnungen, null, null);

                        // Timeline neue Datensätze erzeugen
                        TblTimelineNew = new DataTable();
                        MySqlCommand command31 = new MySqlCommand(psSql, connect);
                        mysdc = new MySqlDataAdapter(command31);
                        mysdc.Fill(TblTimelineNew);
                        // liExternId = Afterfetch.MakeAfterFetch(piArt, 2, liExternId, 0, asConnect, aiDb);
                        break;
                    case 4:     // Rechnungen Timeline Create Relations Objektteile schreiben
                        // tableFive beiinhaltet die Objektteile zu einem gewählten Objekt
                        MySqlCommand command6 = new MySqlCommand(psSql2, connect);
                        TblObjektTeile = new DataTable();
                        mysde = new MySqlDataAdapter(command6);
                        mysde.Fill(TblObjektTeile);
                        // tableFive ist jetzt mit allen Objektteilen zum Objekt gefüllt

                        // tableSix: Holen der Timeline
                        MySqlCommand command5 = new MySqlCommand(psSql, connect);
                        TblTimelineGet = new DataTable();
                        mysdf = new MySqlDataAdapter(command5);
                        mysdf.Fill(TblTimelineGet);

                        // tableFour Timeline schreiben
                        MySqlCommand command7 = new MySqlCommand(psSql, connect);
                        TblTimeLineSet = new DataTable();
                        mysdc = new MySqlDataAdapter(command7);
                        mysdc.Fill(TblTimeLineSet);

                        // liOk = Afterfetch.MakeAfterFetch(piArt, 0, 0, 0, asConnect, aiDb);
                        break;
                    case 5:     // Rechnungen Timeline Create Relations Mieter schreiben
                        // Vorhandene Timeline einlesen
                        MySqlCommand command9 = new MySqlCommand(psSql, connect);
                        TblTimeLineGet = new DataTable();
                        mysdh = new MySqlDataAdapter(command9);
                        mysdh.Fill(TblTimeLineGet);

                        // Timeline neue Datensätze erzeugen
                        MySqlCommand command8 = new MySqlCommand(psSql, connect);
                        TblTimelineNew = new DataTable();
                        mysdc = new MySqlDataAdapter(command8);
                        mysdc.Fill(TblTimelineNew);
                        // Schleife durch Timeline
                        // liOk = Afterfetch.MakeAfterFetch(piArt, 0, 0, 0, asConnect, aiDb);
                        break;
                    case 8:     // Mwst Satz holen
                        mysdg = new MySqlDataAdapter(command);
                        TblTaxGet = new DataTable();
                        mysdg.Fill(TblTaxGet);
                        // liReturn = Afterfetch.MakeAfterFetch(piArt, 0, 0, 0, asConnect, aiDb);
                        break;
                    case 11:    // Zahlungen > Timeline erzeugen bearbeiten
                        TblZlg = new DataTable();         // Zahlungen
                        mysdZlg = new MySqlDataAdapter(command);
                        mysdZlg.Fill(TblZlg);
                        // liOk = Afterfetch.MakeAfterFetch(piArt, 0, 0, 0, asConnect, aiDb);
                        break;
                    case 13:        // Zahlungen Timeline neu erzeugen
                        TblZlgNew = new DataTable();         // Zahlungen
                        MySqlCommand command13 = new MySqlCommand(psSql2, connect);
                        mysdZlgNew = new MySqlDataAdapter(command13);
                        mysdZlgNew.Fill(TblZlgNew);
                        // // liExternId = Afterfetch.MakeAfterFetch(piArt, 1, 0, 0, asConnect, aiDb);

                        // Timeline neue Datensätze erzeugen
                        MySqlCommand command131 = new MySqlCommand(psSql, connect);
                        TblTml = new DataTable();
                        mysdTml = new MySqlDataAdapter(command131);
                        mysdTml.Fill(TblTml);
                        // liOk = Afterfetch.MakeAfterFetch(piArt, 2, 0, 0, asConnect, aiDb);
                        break;
                    case 14:        // Summen aus Objekt für Report Content
                        TblConSumObj = new DataTable();
                        mysdConSumObj = new MySqlDataAdapter(command);
                        mysdConSumObj.Fill(TblConSumObj);
                        break;
                    case 15:        // Summen aus ObjektTeil für Report Content
                        TblConSumObjT = new DataTable();
                        mysdConSumObjT = new MySqlDataAdapter(command);
                        mysdConSumObjT.Fill(TblConSumObjT);
                        break;
                    case 16:        // Die Rechnungs Id aus der Timeline ermitteln
                        TblRgId = new DataTable();
                        mysdRgId = new MySqlDataAdapter(command);
                        mysdRgId.Fill(TblRgId);
                        // liOk = Afterfetch.MakeAfterFetch(piArt, 0, 0, 0, asConnect, aiDb);
                        break;
                    case 21:                               // Zählerstände
                        TblCnt = new DataTable();
                        mysdCnt = new MySqlDataAdapter(command);
                        mysdCnt.Fill(TblCnt);
                        // liOk = Afterfetch.MakeAfterFetch(piArt, 0, 0, 0, asConnect, aiDb);
                        break;
                    case 23:        // Zählerstände Timeline Create
                        TblCntNew = new DataTable();         // Zahlungen
                        MySqlCommand command23 = new MySqlCommand(psSql2, connect);
                        mysdCntNew = new MySqlDataAdapter(command23);
                        mysdCntNew.Fill(TblCntNew);
                        // Timeline neue Datensätze erzeugen
                        MySqlCommand command231 = new MySqlCommand(psSql, connect);
                        TblTml = new DataTable();
                        mysdTml = new MySqlDataAdapter(command231);
                        mysdTml.Fill(TblTml);
                        // liOk = Afterfetch.MakeAfterFetch(piArt, 0, 0, 0, asConnect, aiDb, sda );
                        break;
                    case 24:            // Zählerinformationen für Report Nebenkostenabrechnungen
                        TblZlInfo = new DataTable();
                        mysdZlInfo = new MySqlDataAdapter(command);
                        mysdZlInfo.Fill(TblZlInfo);
                        break;
                    case 25:            // Zählerinformationen für Report Nebenkostenabrechnungen
                        TblParts = new DataTable();
                        mysdParts = new MySqlDataAdapter(command);
                        mysdParts.Fill(TblParts);
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
                        TblTmlCheckRgNr = new DataTable();
                        MySqlCommand command271 = new MySqlCommand(psSql, connect);
                        // Create a SqlDataReader
                        MySqlDataReader queryCommandReader271 = command271.ExecuteReader();
                        TblTmlCheckRgNr.Load(queryCommandReader271);
                        break;
                    case 28:
                        // Erste Tabelle Timeline holen
                        TblTimeline = new DataTable();
                        MySqlCommand command281 = new MySqlCommand(psSql, connect);
                        // Create a SqlDataReader
                        MySqlDataReader queryCommandReader281 = command281.ExecuteReader();
                        // Create a DataTable object to hold all the data returned by the query.
                        TblTimeline.Load(queryCommandReader281);
                        break;
                    case 29:
                        // Zweite Tabelle Timeline ObjektKostendarstellung (Zähler)
                        TblTimelineObjKst = new DataTable();
                        MySqlCommand command291 = new MySqlCommand(psSql, connect);
                        MySqlDataReader queryCommandReader291 = command291.ExecuteReader();
                        TblTimelineObjKst.Load(queryCommandReader291);
                        break;
                    case 30:
                        // ReportContent füllen
                        TblContent = new DataTable();
                        MySqlCommand command301 = new MySqlCommand(psSql, connect);
                        myadp = new MySqlDataAdapter(command301);
                        MySqlDataReader queryCommandReader301 = command301.ExecuteReader();
                        TblContent.Load(queryCommandReader301);
                        break;
                    case 31:
                        // ReportContent Ab in die Datenbank
                        MySqlCommandBuilder commandBuilder31 = new MySqlCommandBuilder(myadp);
                        myadp.Update(TblContent);
                        break;
                    case 32:
                        // Timeline update
                        MySqlCommandBuilder commandBuilder32 = new MySqlCommandBuilder(mysdTml);
                        mysdTml.Update(TblTml);
                        break;
                    case 33:
                        // Rechnungen update
                        MySqlCommandBuilder commandBuilder33 = new MySqlCommandBuilder(MySdRechnungen);
                        MySdRechnungen.Update(TblRechnungen);
                        break;
                    case 34:
                        var lvId = command.ExecuteScalar();
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
            return (liReturn);
        }

        // Daten aus der Db holen hier nur Dezimalwerte
        public static decimal FetchDataDecimal(string psSql, string psSql2, int piArt, string asConnectString)
        {
            DateTime ldtStart = DateTime.MinValue;
            DateTime ldtEnd = DateTime.MinValue;
            DateTime ldtMonat = DateTime.MinValue;
            DateTime ldtVertrag = DateTime.MinValue;

            decimal[] ladBetraege = new decimal[12];
            decimal ldReturn = 0;

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
            return (ldReturn);
        }

        // Daten aus der Db holen hier nur Strings
        public static string fetchDataString(string psSql, string psSql2, int piArt, string asConnectString)
        {
            DateTime ldtStart = DateTime.MinValue;
            DateTime ldtEnd = DateTime.MinValue;
            DateTime ldtMonat = DateTime.MinValue;
            DateTime ldtVertrag = DateTime.MinValue;

            decimal[] ladBetraege = new decimal[12];
            string lsReturn = "";

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

            return (lsReturn);
        }

        // Daten aus der Db holen hier nur Datum
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
        public static void MakeCommand(int aiArt)
        {
            switch (aiArt)
            {
                case 1:
                    MySqlCommandBuilder commandBuilder21 = new MySqlCommandBuilder(mysdc);
                    mysdc.Update(TblTimelineNew);
                    break;
                case 2:
                    MySqlCommandBuilder commandBuilder22 = new MySqlCommandBuilder(mysdc);
                    mysdc.Update(TblTimeLineSet);
                    break;
                case 3:
                    MySqlCommandBuilder commandBuilder23 = new MySqlCommandBuilder(mysdc);
                    mysdc.Update(TblTimelineNew);
                    break;
                case 4:
                    MySqlCommandBuilder commandBuilder24 = new MySqlCommandBuilder(mysdTml);
                    mysdTml.Update(TblTml);
                    break;
                default:
                    break;
            }
        }



        // Berechnen der monatlichen Beträge für die Timeline
        public static decimal[] GetBetraege(int liMonths, int liDaysStart, int liDaysEnd,
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
        public static int TimelineCreateRelations(int liExternId, int liObjekt, int liObjektTeil, int liMieter, int aiArt, string asConnect, int aiDb)
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
                lsSql2 = Timeline.GetSql(6, liObjekt, "", "", 0);       // Objektteile holen
                lsSql = Timeline.GetSql(4, liExternId, liObjekt.ToString(), "", 0);
                liOk = Timeline.FetchData(lsSql, lsSql2, 4, asConnect);
            }

            else if (liObjektTeil > 0)
            {
                // In Timeline Mieter werden alle umlagefähigen Kosten auf den 
                // zu dem TimeLineMonat wohnenden Mieter geschrieben
                switch (aiArt)
                {
                    case 1:         // Rechnung 
                        lsSql = Timeline.GetSql(50, liExternId, liObjektTeil.ToString(), "", 0);
                        break;
                    case 2:         // Zahlung
                        lsSql = Timeline.GetSql(51, liExternId, liObjektTeil.ToString(), "", 0);
                        break;
                    case 3:         //Zähler
                        lsSql = Timeline.GetSql(52, liExternId, liObjektTeil.ToString(), "", 0);
                        break;
                    default:
                        break;
                }

                liOk = Timeline.FetchData(lsSql, "", 5, asConnect);
            }

            return liOk;
        }

        // Anzahl der Tage bis Monatsende
        public static int GetDaysEnd(DateTime ldtEnd)
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
        public static int GetDaysStart(DateTime ldtStart)
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
        public static int GetMonths(DateTime ldtStart, DateTime ldtEnd)
        {
            int liMonths = 0;

            liMonths = ((ldtEnd.Year - ldtStart.Year) * 12) + ldtEnd.Month - ldtStart.Month + 1;

            return liMonths;
        }

        // Timeline neu erzeugen
        public static int TimelineCreate(int liExternId, string asField, string asConnect, int aiDb)
        {
            int liOk = 0;
            string lsSql = "";
            string lsSql2 = "";

            if (asField == "id_rechnung") // Rechnung
            {
                lsSql = Timeline.GetSql(31, liExternId, asField, "", 0);               // Timeline
                lsSql2 = Timeline.GetSql(1, liExternId, asField, "", 0);               // Rechnung
                liOk = Timeline.FetchData(lsSql, lsSql2, 3, asConnect);
            }

            if (asField == "id_vorauszahlung") // Vorrauszahlung                                     
            {
                lsSql = Timeline.GetSql(31, liExternId, asField, "", 0);               // Timeline
                lsSql2 = Timeline.GetSql(12, liExternId, asField, "", 0);              // Zahlung mit extern Timeline Id
                liOk = Timeline.FetchData(lsSql, lsSql2, 13, asConnect);
            }

            if (asField == "id_zaehlerstand") // Zähler
            {
                lsSql = Timeline.GetSql(31, liExternId, asField, "", 0);               // Timeline
                lsSql2 = Timeline.GetSql(21, liExternId, asField, "", 0);              // Zählerstande mit extern Timeline Id
                liOk = Timeline.FetchData(lsSql, lsSql2, 23, asConnect);
            }

            return liOk;
        }

        // Alle Datensätze der Timeline ID zunächst löschen
        public static int TimelineDelete(int liExternId, string asArt, string asConnect)
        {
            int liOk = 0;
            string lsSql = "";

            // SqlStatement für Timeline löschen
            switch (asArt)
            {
                case "R":   // Rechnung
                    lsSql = Timeline.GetSql(200, liExternId, "", "", 0);
                    break;
                case "A":   // Zahlung
                    lsSql = Timeline.GetSql(201, liExternId, "", "", 0);
                    break;
                case "Z":   // Zählerstand
                    lsSql = Timeline.GetSql(202, liExternId, "", "", 0);
                    break;
                default:
                    break;
            }
            liOk = Timeline.FetchData(lsSql, "", 2, asConnect);

            // Info: hier werden auch alle Datensätze evtl untergeordneter Rubriken 
            // anteilige Kosten von Objektteilen und Mietern gelöscht,
            // weil alle datensätze betr. der Extern Id gelöscht werden
            return liOk;
        }

        // Mehrwertsteuersatz holen, Bezeichnung bez ist bekannt
        public static int GetMwstFromBez(string lsBez, string asConnect, int aiDb)
        {
            String lsSql = "";
            int liMwstSatz = 0;

            lsSql = Timeline.GetSql(9, 0, lsBez, "", 0);
            // fetchdata gibt hier den MwstSatz zurück
            liMwstSatz = Timeline.FetchData(lsSql, "", 8, asConnect);

            return liMwstSatz;
        }

        // Mehrwertsteuersatz holen, Art ist bekannt
        public static int GetMwstSatz(int liMwstArt, string asConnectString, int aiDb)
        {
            String lsSql = "";
            int liMwstSatz = 0;

            lsSql = Timeline.GetSql(8, liMwstArt, "", "", 0);
            // fetchdata gibt hier den MwstSatz zurück
            liMwstSatz = Timeline.FetchData(lsSql, "", 8, asConnectString);

            return liMwstSatz;
        }

        // Gesamtfläche eines Objektes holen
        public static decimal GetObjektflaeche(int aiObjekt, int aiTObjekt, int aiMieterId, string asConnect)
        {
            int liObjTeilId = 0;
            int liObjId = 0;
            decimal ldGesamtflaeche = 0;
            String lsSql = "";

            // Mieter ID vorhanden
            if (aiMieterId > 0)
            {
                liObjTeilId = GetIdObjTeil(aiMieterId, asConnect);
                liObjId = GetIdObj(liObjTeilId, asConnect, 2);
                lsSql = "SELECT flaeche_gesamt FROM objekt  WHERE id_objekt = " + liObjId.ToString();
            }
            // TeilObjekt ID vorhanden
            if (aiTObjekt > 0)
            {
                liObjId = GetIdObj(liObjTeilId, asConnect, 2);
                lsSql = "SELECT flaeche_gesamt FROM objekt  WHERE id_objekt = " + liObjId.ToString();
            }
            // Objekt ID vorhanden
            if (aiObjekt > 0)
            {
                lsSql = "SELECT flaeche_gesamt FROM objekt  WHERE id_objekt = " + aiObjekt.ToString();
            }

            // Daten holen
            ldGesamtflaeche = FetchDataDecimal(lsSql, "", 1, asConnect);

            return ldGesamtflaeche;
        }

        // Fläche eines TeilObjektes holen
        public static decimal GetTObjektflaeche(int aiTObjekt, int aiMieterId, string asConnect)
        {
            int liObjTeilId = 0;
            decimal ldFlaeche = 0;
            string lsSql = "";

            // Mieter ID vorhanden
            if (aiMieterId > 0)
            {
                liObjTeilId = GetIdObjTeil(aiMieterId, asConnect);
                lsSql = "SELECT flaeche_anteil FROM objekt_teil  WHERE id_objekt_teil = " + liObjTeilId.ToString();
            }
            // TeilObjekt ID vorhanden
            if (aiTObjekt > 0)
            {
                lsSql = "SELECT flaeche_anteil FROM objekt_teil  WHERE id_objekt_teil = " + aiTObjekt.ToString();
            }

            // Daten holen
            ldFlaeche = FetchDataDecimal(lsSql, "", 1, asConnect);

            return ldFlaeche;
        }

        // Fläche eines TeilObjektes holen
        public static decimal GetTObjektAnteil(int aiTObjekt, int aiMieterId, string asConnect)
        {
            decimal ldAnteil = 0;
            int liObjTeilId = 0;
            string lsSql = "";

            if (aiTObjekt > 0)
            {
                lsSql = "SELECT prozent_anteil FROM objekt_teil  WHERE id_objekt_teil = " + aiTObjekt.ToString();
            }
            if (aiMieterId > 0)
            {
                liObjTeilId = GetIdObjTeil(aiMieterId, asConnect);
                lsSql = "SELECT prozent_anteil FROM objekt_teil  WHERE id_objekt_teil = " + liObjTeilId.ToString();
            }

            // Daten holen
            ldAnteil = FetchDataDecimal(lsSql, "", 1, asConnect);

            return ldAnteil;
        }

        // Die Gesamtfläche der Objektauswahl aus objekt_part_mix ermitteln
        // Art 1 ist die Gesamtgrundfläche der gewählten Wohnungen
        // Art 2 ist die Gesamtfläche des Objektes
        public static decimal GetObjektflaecheAuswahl(int liObjekt, int aiTimelineId, string asConnect, int aiArt)
        {
            decimal ldGesamtflaeche = 0;
            string lsSql = "";

            switch (aiArt)
            {
                case 0:
                    lsSql = @"SELECT Sum(flaeche_anteil) FROM objekt_mix_parts  WHERE sel = 1 
                                and id_objekt = " + liObjekt.ToString() + " and id_timeline = " + aiTimelineId.ToString();
                    break;
                case 1:
                    lsSql = @"SELECT Sum(flaeche_anteil) FROM objekt_mix_parts  WHERE ges_fl_behalten = 1 
                                and id_objekt = " + liObjekt.ToString() + " and id_timeline = " + aiTimelineId.ToString();
                    break;
                default:
                    break;
            }

            // Daten holen
            ldGesamtflaeche = FetchDataDecimal(lsSql, "", 1, asConnect);

            return ldGesamtflaeche;
        }

        // Es wird geprüft ob das Objektteil in der Auswahl enthalten ist
        public static int GetObjektTeilAuswahl(int aiObjektTeil, string asConnect)
        {
            int liObjektTeil = 0;

            String lsSql = GetSql(27, aiObjektTeil, "", "", 0);
            liObjektTeil = FetchData(lsSql, "", 26, asConnect);

            return liObjektTeil;
        }

        // Ist eine Weitergabe der Kosten in art_kostenart eingetragen
        // 1 = Weiterleitung
        public static int GetWeiterleitung(int p, int liExternId, string asConnect)
        {
            int liWtl = 0;
            string lsSql = "";

            lsSql = GetSql(28, liExternId, "", "", p);
            liWtl = FetchData(lsSql, "", 26, asConnect);

            return liWtl;
        }

        // Hier wird der aktuelle Mieter für den gegebenen Monat der Timeline ermittelt
        public static int GetAktMieter(int aiObjektTeil, DateTime adtMonat, string asConnect, int aiDb)
        {
            String lsSql = "";
            Int32 liMieterId = 0;

            // adtMonat umbauen soll immer den ersten des Monats zeigen
            adtMonat = adtMonat.AddDays(-(adtMonat.Day - 1));

            lsSql = RdQueries.GetSqlSelect(41, aiObjektTeil, "", "", "", adtMonat, DateTime.MinValue, 0, asConnect,2);
            liMieterId = FetchData(lsSql, "", 26, asConnect);

            return liMieterId;
        }

        // Den Mieter für Leerstand ermitteln
        // Aus ObjektTeil
        // Für Rechnungen zur Timeline, die nicht auf einen aktiven Mietvertrag gebucht werden können
        public static int GetMieterLeerstand(int aiObjektTeil, string asConnect, int aiDb)
        {
            String lsSql = "";
            int liMieterId = 0;

            if (aiObjektTeil > 0)
            {
                lsSql = GetSql(29, aiObjektTeil, "", "", 0);
                liMieterId = FetchData(lsSql, "", 26, asConnect);
            }
            return liMieterId;
        }

        // Den Mieter für Leerstand ermitteln
        // Aus Objekt
        // Für Rechnungen zur Timeline, die nicht auf einen aktiven Mietvertrag gebucht werden können
        public static int GetMieterLeerstandObjekt(int aiObjekt, string asConnect, int aiDb)
        {
            String lsSql = "";
            int liMieterId = 0;

            if (aiObjekt > 0)
            {
                lsSql = GetSql(291, aiObjekt, "", "", 0);
                liMieterId = FetchData(lsSql, "", 26, asConnect);
            }
            return liMieterId;
        }

        // Ermitteln der Anzahl der aktuell wohnenden Personen in einem Objekt, Objektteil
        // Gesucht wird nach aktiven Verträgen in einem Objekt, Objektteil
        // Wird benötigt, um eine Kostenaufteilung nach Personen zu machen
        // Das Flag soll die fehlenden Informationen holen 0 = nix; 1 = ObjektId; 2 = TeilobjektId
        public static decimal GetAktPersonen(int aiObjekt, int aiObjektTeil, int aiMieterId, string asDatVon, string asDatBis, int aiFlag, string asConnect)
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
                    lsSql = @"SELECT sum(vertrag.anzahl_personen) FROM vertrag  WHERE vertrag.vertrag_aktiv = 1 And vertrag.id_objekt = " + aiObjekt.ToString();
                }
                if (aiObjektTeil > 0)
                {
                    lsSql = @"SELECT sum(vertrag.anzahl_personen) FROM vertrag  WHERE vertrag.vertrag_aktiv = 1 And vertrag.id_objekt_teil = " + aiObjektTeil.ToString();
                }
            }

            // Objekt ID aus Mieter ID holen
            if (aiFlag == 1)
            {
                liObjId = GetIdObj(aiMieterId, asConnect, 1);
                lsSql = lsSql = @"SELECT sum(vertrag.anzahl_personen) FROM vertrag  WHERE vertrag.vertrag_aktiv = 1 And vertrag.id_objekt = " + liObjId.ToString();
            }

            // TeilObjekt ID aus Mieter Id holen
            if (aiFlag == 2)
            {
                liObTId = GetIdObjTeil(aiMieterId, asConnect);
                lsSql = @"SELECT sum(vertrag.anzahl_personen) FROM vertrag  WHERE vertrag.vertrag_aktiv = 1 And vertrag.id_objekt_teil = " + liObTId.ToString();
            }

            lsSqlAdd = " And vertrag.datum_von <= Convert(DateTime," + "\'" + asDatVon + "',104) "
                                 + "And vertrag.datum_bis >= Convert(DateTime," + "\'" + asDatBis + "',104)";

            lsSql = lsSql + lsSqlAdd;

            // Daten holen
            ldAnzahlPersonen = FetchDataDecimal(lsSql, "", 1, asConnect);

            return ldAnzahlPersonen;
        }

        // Die Nebenkosten ID in der Tabelle art_KostenArt ermitteln
        // Art 1 = Zahlung Nebenkosten
        // Art 2 = Zählerstände
        public static int GetKsaId(int aiArt, String asConnect, int aiDb)
        {
            int liKsaId = 0;
            String lsSql = "";

            lsSql = GetSql(30, aiArt, "", "", 0);
            liKsaId = FetchData(lsSql, "", 26, asConnect);

            return liKsaId;
        }

        // Den Verteilungskurzstring aus der Tabelle art_verteilung ermitteln
        public static string GetVerteilung(String asConnect, int aiVerteilungId)
        {
            string lsVerteilung = "";
            String lsSql = "";

            lsSql = @"SELECT kb FROM art_verteilung  WHERE id_verteilung = " + aiVerteilungId.ToString();
            lsVerteilung = fetchDataString(lsSql, "", 1, asConnect);

            return lsVerteilung;
        }

        // Die VerteilungsId aus Rechnungen ermitteln
        public static int GetVerteilungsId(string asConnect, int aiTimelineId)
        {
            int liVerteilungId = 0;
            String lsSql = "";

            lsSql = GetSql(32, aiTimelineId, "", "", 0);
            liVerteilungId = FetchData(lsSql, "", 26, asConnect);

            return liVerteilungId;
        }

        // Verteilungs ID aus art_verteilung ermitteln
        public static int GetIdArtVerteilung(string asBez, string asConnect)
        {
            int liVerteilungId = 0;
            String lsSql = "";

            lsSql = GetSql(33, 0, asBez, "", 0);
            liVerteilungId = FetchData(lsSql, "", 26, asConnect);

            return liVerteilungId;
        }


        // Den Verteilungskurzstring aus der Tabelle art_verteilung ermitteln
        public static string GetVerteilungFromString(String asConnect, string asVerteilung, int aiDb)
        {
            string lsVerteilung = "";
            String lsSql = "";

            lsSql = @"SELECT kb FROM art_verteilung  WHERE bez = '" + asVerteilung.ToString().Trim() + " '";
            lsVerteilung = fetchDataString(lsSql, "", 1, asConnect);

            return lsVerteilung;
        }

        // Und den Sql Zusatz für Reports in eine xml-Datei speichern
        public static void SaveLastSql(string asSqlKostenDirekt, string asSqlContent, string asSqlContSumObj, string asSqlConSumObjt,
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
        internal static int GetIdObjTeil(int aiId, string asConnect)
        {
            int liIdObjTeil = 0;
            String lsSql = "";

            lsSql = GetSql(34, aiId, "", "", 0);
            liIdObjTeil = FetchData(lsSql, "", 26, asConnect);

            return liIdObjTeil;
        }

        // Die Objekt ID aus den Vertragsdaten ermitteln aus der Mieter Id = 1 oder der Teilobjekt ID = 2
        internal static int GetIdObj(int aiId, string asConnect, int aiArt)
        {
            int liIdObj = 0;
            String lsSql = "";

            lsSql = GetSql(35, aiId, "", "", aiArt);
            liIdObj = FetchData(lsSql, "", 26, asConnect);

            return liIdObj;
        }

        // Die Tabelle x_abr_content wird gefüllt
        // asSql ist die Timeline
        // asSqlContent ist die Zieltabelle. Sie zeigt das Content des Reports Nebenkostenabrechnung
        internal static int FillContent(string asSql, string asSqlContent, string asSql2, string asDatVon, string asDatBis, string asConnect, string asSqlRgNr, int aiAnschreiben)
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
            liOk = Timeline.DelContent(asConnect);

            // Timeline einlesen
            //tableTimeline = new DataTable();
            //tableTimeline1 = new DataTable();     // Kosten des Objektes darstellen 
            //tableContent = new DataTable();       // Content

            if (aiAnschreiben == 1)
            {
                // Rechnunsnummer für Anschreiben prüfen und einsetzen
                // ist eine id_rg_nr in der Timeline vorhanden?
                liOk = FetchData(asSql, "", 27, asConnect);

                if (TblTmlCheckRgNr.Rows.Count > 0)
                {
                    if (TblTmlCheckRgNr.Rows[0].ItemArray.GetValue(22) != DBNull.Value)
                    {
                        liIdRgNr = Convert.ToInt16(TblTmlCheckRgNr.Rows[0].ItemArray.GetValue(22).ToString());       //  id Rechnungsnummer für Anschreiben
                    }
                }

                // In dem Fall muss die Rechnungsnummer Anschreiben und das Besetzt-Kennzeichen in RgNr eingesetzt werden
                if (liIdRgNr == 0)
                {
                    liIdRgNr = GetRgNrFromPool(asConnect);          // ID Rechnungsnummer aus dem Pool besorgen
                    if (liIdRgNr > 0)
                    {
                        liOk = SetRgNrToTml(liIdRgNr, asSqlRgNr, asConnect);       // ID Rechnungsnummer in Timeline einsetzen
                        liOk = SetRgNrFromPool(liIdRgNr, asConnect);    // Die Rechnungsnummer als besetzt kennzeichnen 
                    }
                    else
                    {
                        // Keine Rechnungsnummer Im Pool vorhanden, bitte Eintragen
                        MessageBox.Show("Keine Rechnungsnummer im Pool vorhanden, \nbitte Eintragen");
                    }
                }
            }

            // Erste Tabelle Timeline holen
            if (asSql.Length > 2)
            {
                liOk = FetchData(asSql, "", 28, asConnect);
            }
            // Zweite Tabelle Timeline ObjektKostendarstellung
            if (asSql2.Length > 2)
            {
                liOk = FetchData(asSql2, "", 29, asConnect);
            }
            // Dritte Tabelle x_abr_content
            if (asSqlContent.Length > 2)
            {
                liOk = FetchData(asSqlContent, "", 30, asConnect);
            }

            // Schleife durch Timeline asSql und erstmal stumpf an Tabelle Content übertragen
            // Achtung rows.count -1, weil i bei 0 anfängt
            if (TblTimeline != null)
            {
                for (int i = 0; i < TblTimeline.Rows.Count; i++)
                {
                    DataRow dr = TblContent.NewRow();

                    if (TblTimeline.Rows[i].ItemArray.GetValue(6) != DBNull.Value)
                    {
                        dr[2] = Convert.ToInt16(TblTimeline.Rows[i].ItemArray.GetValue(6).ToString());        //  Id Extern TimeLine
                        liIdExternTimeline = Convert.ToInt16(TblTimeline.Rows[i].ItemArray.GetValue(6).ToString());

                        dr[27] = getRgInfo(liIdExternTimeline, asConnect, 1).Trim();                                   // Rechnungsnummer
                        dr[28] = getRgInfo(liIdExternTimeline, asConnect, 2).Trim();                                   // Rechnungstext
                        string lsd;
                        lsd = getRgInfo(liIdExternTimeline, asConnect, 3);
                        if (lsd.Length > 0)
                        {
                            dr[29] = lsd;
                        }
                    }
                    if (TblTimeline.Rows[i].ItemArray.GetValue(7) != DBNull.Value)
                    {
                        if (Convert.ToInt16(TblTimeline.Rows[i].ItemArray.GetValue(7).ToString()) > 0)
                        {
                            dr[3] = Convert.ToInt16(TblTimeline.Rows[i].ItemArray.GetValue(7).ToString());            //  Id Vorrauszahlung
                        }
                    }
                    if (TblTimeline.Rows[i].ItemArray.GetValue(18) != DBNull.Value)                                   // Id Zählerstand
                    {
                        if (Convert.ToInt16(TblTimeline.Rows[i].ItemArray.GetValue(18).ToString()) > 0)
                        {
                            dr[4] = Convert.ToInt16(TblTimeline.Rows[i].ItemArray.GetValue(18).ToString());           // Id Zählerstand
                            liIdZaehlerstand = Convert.ToInt16(TblTimeline.Rows[i].ItemArray.GetValue(18).ToString());
                            // dr[13] = Convert.ToDecimal(tableTimeline.Rows[i].ItemArray.GetValue(12).ToString());         // Zählerstand wird hier nicht genutzt auf null prüfen
                        }
                    }
                    if (TblTimeline.Rows[i].ItemArray.GetValue(8) != DBNull.Value)
                    {
                        dr[5] = Convert.ToInt16(TblTimeline.Rows[i].ItemArray.GetValue(8).ToString());        //  Id Objekt
                        liIdObj = Convert.ToInt16(TblTimeline.Rows[i].ItemArray.GetValue(8).ToString());
                    }
                    if (TblTimeline.Rows[i].ItemArray.GetValue(9) != DBNull.Value)
                    {
                        dr[6] = Convert.ToInt16(TblTimeline.Rows[i].ItemArray.GetValue(9).ToString());        //  Id Teilobjekt
                        liIdObjt = Convert.ToInt16(TblTimeline.Rows[i].ItemArray.GetValue(9).ToString());
                    }
                    if (TblTimeline.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                    {
                        dr[7] = Convert.ToInt16(TblTimeline.Rows[i].ItemArray.GetValue(10).ToString());       //  Id Mieter
                        liIdMieter = Convert.ToInt16(TblTimeline.Rows[i].ItemArray.GetValue(10).ToString());
                    }
                    if (TblTimeline.Rows[i].ItemArray.GetValue(16) != DBNull.Value)
                    {
                        dr[8] = Convert.ToInt16(TblTimeline.Rows[i].ItemArray.GetValue(16).ToString());       //  Id Kostenart
                    }
                    if (TblTimeline.Rows[i].ItemArray.GetValue(0) != DBNull.Value)
                    {
                        dr[9] = Convert.ToDecimal(TblTimeline.Rows[i].ItemArray.GetValue(0).ToString());      //  Netto
                    }
                    if (TblTimeline.Rows[i].ItemArray.GetValue(1) != DBNull.Value)
                    {
                        dr[11] = Convert.ToDecimal(TblTimeline.Rows[i].ItemArray.GetValue(1).ToString());     //  Brutto
                    }

                    if (TblTimeline.Rows[i].ItemArray.GetValue(4) != DBNull.Value)
                    {
                        dr[15] = Convert.ToInt16(TblTimeline.Rows[i].ItemArray.GetValue(4).ToString());       //  Weiterleitung Objekt
                    }
                    if (TblTimeline.Rows[i].ItemArray.GetValue(5) != DBNull.Value)
                    {
                        dr[16] = Convert.ToInt16(TblTimeline.Rows[i].ItemArray.GetValue(5).ToString());       //  Weiterleitung ObjektTeil
                    }
                    if (TblTimeline.Rows[i].ItemArray.GetValue(17) != DBNull.Value)
                    {
                        if (Convert.ToInt16(TblTimeline.Rows[i].ItemArray.GetValue(17)) > 0)
                        {
                            dr[23] = Convert.ToInt16(TblTimeline.Rows[i].ItemArray.GetValue(17).ToString());       //  Art der Verteilung REchnungen
                            liIdArtVerteilung = Convert.ToInt16(TblTimeline.Rows[i].ItemArray.GetValue(17).ToString());
                        }
                    }
                    else if (TblTimeline.Rows[i].ItemArray.GetValue(18) != DBNull.Value)                      // Art der Verteilung für Zähler ermitteln "zl"
                    {
                        liIdExternTimelineZaehlerstand = Convert.ToInt16(TblTimeline.Rows[i].ItemArray.GetValue(18));
                        liIdArtVerteilung = Timeline.GetIdArtVerteilung("zl", asConnect);
                    }
                    if (TblTimeline.Rows[i].ItemArray.GetValue(2) != DBNull.Value)
                    {
                        dr[24] = Convert.ToDecimal(TblTimeline.Rows[i].ItemArray.GetValue(2).ToString());       //  Rechnung Netto
                    }
                    if (TblTimeline.Rows[i].ItemArray.GetValue(3) != DBNull.Value)
                    {
                        dr[25] = Convert.ToDecimal(TblTimeline.Rows[i].ItemArray.GetValue(3).ToString());       //  Rechnung Brutto
                    }

                    if (TblTimeline.Rows[i].ItemArray.GetValue(22) != DBNull.Value)
                    {
                        dr[30] = Convert.ToInt16(TblTimeline.Rows[i].ItemArray.GetValue(22).ToString());       //  id Rechnungsnummer für Anschreiben
                    }

                    // Verteilungsinformationen holen
                    if (liIdArtVerteilung > 0)
                    {
                        // Verteilungsinfos ermittlen; letztes Argument ist der Detailgrad 2 = Alles TODO Ulf!!!
                        dr[26] = Timeline.GetVerteilungsInfo(asConnect, liIdExternTimeline, liIdArtVerteilung, liIdObj, liIdObjt, liIdMieter, asDatVon, asDatBis, liIdExternTimelineZaehlerstand, 1);
                    }

                    // Rechnung aus Objekt oder Teilobjekt
                    if (liIdExternTimeline > 0)
                    {
                        // Objektsummen holen
                        lsSql = GetSql(14, liIdExternTimeline, "", "", 0);
                        liOk = Timeline.FetchData(lsSql, "", 14, asConnect);

                        if (TblConSumObj.Rows.Count > 0)
                        {
                            if (TblConSumObj.Rows[0].ItemArray.GetValue(0) != DBNull.Value)
                            {
                                dr[21] = Convert.ToDecimal(TblConSumObj.Rows[0].ItemArray.GetValue(0));
                            }
                            if (TblConSumObj.Rows[0].ItemArray.GetValue(1) != DBNull.Value)
                            {
                                dr[22] = Convert.ToDecimal(TblConSumObj.Rows[0].ItemArray.GetValue(1));
                            }
                        }

                        // Teilobjekt ID aus der Mieter ID  ermitteln
                        if (liIdMieter > 0)
                        {
                            liIdObjt = getVertragInfoFromMieter(liIdMieter, asConnect, 1);
                        }
                        lsSql = Timeline.GetSql(15, liIdExternTimeline, "", "", liIdObjt);
                        liOk = Timeline.FetchData(lsSql, "", 15, asConnect);

                        if (TblConSumObjT.Rows.Count > 0 && liIdObjt > 0)
                        {
                            if (TblConSumObjT.Rows[0].ItemArray.GetValue(0) != DBNull.Value)
                            {
                                dr[19] = Convert.ToDecimal(TblConSumObjT.Rows[0].ItemArray.GetValue(0));
                            }
                            if (TblConSumObjT.Rows[0].ItemArray.GetValue(1) != DBNull.Value)
                            {
                                dr[20] = Convert.ToDecimal(TblConSumObjT.Rows[0].ItemArray.GetValue(1));
                            }
                        }
                    }

                    // Zählerstand aus Objekt oder ObjektTeil
                    if (liIdZaehlerstand > 0)
                    {
                        // Objektsummen holen
                        lsSql = GetSql(16, liIdZaehlerstand, "", "", 0);
                        liOk = Timeline.FetchData(lsSql, "", 14, asConnect);

                        if (TblConSumObj.Rows.Count > 0)
                        {
                            if (TblConSumObj.Rows[0].ItemArray.GetValue(0) != DBNull.Value)
                            {
                                dr[21] = Convert.ToDecimal(TblConSumObj.Rows[0].ItemArray.GetValue(0));
                            }
                            if (TblConSumObj.Rows[0].ItemArray.GetValue(1) != DBNull.Value)
                            {
                                dr[22] = Convert.ToDecimal(TblConSumObj.Rows[0].ItemArray.GetValue(1));
                            }
                        }

                        // TeilobjektSummen holen
                        // Teilobjekt ID aus der Mieter ID  ermitteln
                        if (liIdMieter > 0)
                        {
                            liIdObjt = getVertragInfoFromMieter(liIdMieter, asConnect, 1);
                        }
                        lsSql = GetSql(17, liIdZaehlerstand, "", "", 0);
                        liOk = Timeline.FetchData(lsSql, "", 15, asConnect);

                        if (TblConSumObjT.Rows.Count > 0)
                        {
                            if (TblConSumObjT.Rows[0].ItemArray.GetValue(0) != DBNull.Value)
                            {
                                dr[19] = Convert.ToDecimal(TblConSumObjT.Rows[0].ItemArray.GetValue(0));
                            }
                            if (TblConSumObjT.Rows[0].ItemArray.GetValue(1) != DBNull.Value)
                            {
                                dr[20] = Convert.ToDecimal(TblConSumObjT.Rows[0].ItemArray.GetValue(1));
                            }
                        }
                    }

                    TblContent.Rows.Add(dr);
                }
            }


            // Zweiter Teil, nur ObjektKosten darstellen (im Moment nur Zähler)
            // Schleife durch Timeline1 asSql2 und erstmal stumpf an Tabelle Content übertragen
            // Achtung rows.count -1, weil i bei 0 anfäng
            if (TblTimelineObjKst != null)
            {
                for (int i = 0; i < TblTimelineObjKst.Rows.Count; i++)
                {
                    DataRow dr = TblContent.NewRow();

                    if (TblTimelineObjKst.Rows[i].ItemArray.GetValue(6) != DBNull.Value)
                    {
                        dr[2] = Convert.ToInt16(TblTimelineObjKst.Rows[i].ItemArray.GetValue(6).ToString());        //  Id Rechnung
                        liIdExternTimeline = Convert.ToInt16(TblTimelineObjKst.Rows[i].ItemArray.GetValue(6).ToString());
                        dr[27] = getRgInfo(liIdExternTimeline, asConnect, 1).Trim();                                   // Rechnungesnummer
                        dr[28] = getRgInfo(liIdExternTimeline, asConnect, 2).Trim();                                   // Rechnungstext

                        if (TblTimelineObjKst.Rows[i].ItemArray.GetValue(7) != DBNull.Value)
                        {
                            dr[3] = Convert.ToInt16(TblTimelineObjKst.Rows[i].ItemArray.GetValue(7).ToString());        //  Id Vorrauszahlung
                        }
                        if (TblTimelineObjKst.Rows[i].ItemArray.GetValue(18) != DBNull.Value)                            // Id Zählerstand
                        {
                            dr[4] = Convert.ToInt16(TblTimelineObjKst.Rows[i].ItemArray.GetValue(18).ToString());         // Id Zählerstand
                            liIdZaehlerstand = Convert.ToInt16(TblTimelineObjKst.Rows[i].ItemArray.GetValue(18).ToString());
                            // dr[13] = Convert.ToDecimal(tableTimeline1.Rows[i].ItemArray.GetValue(12).ToString());   // Zählerstand wird hier nicht genutzt auf null prüfen
                        }

                        if (TblTimelineObjKst.Rows[i].ItemArray.GetValue(8) != DBNull.Value)
                        {
                            dr[5] = Convert.ToInt16(TblTimelineObjKst.Rows[i].ItemArray.GetValue(8).ToString());        //  Id Objekt
                            liIdObj = Convert.ToInt16(TblTimelineObjKst.Rows[i].ItemArray.GetValue(8).ToString());
                        }
                        if (TblTimelineObjKst.Rows[i].ItemArray.GetValue(9) != DBNull.Value)
                        {
                            dr[6] = Convert.ToInt16(TblTimelineObjKst.Rows[i].ItemArray.GetValue(9).ToString());        //  Id Teilobjekt
                            liIdObjt = Convert.ToInt16(TblTimelineObjKst.Rows[i].ItemArray.GetValue(9).ToString());
                        }
                        if (TblTimelineObjKst.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                        {
                            dr[7] = Convert.ToInt16(TblTimelineObjKst.Rows[i].ItemArray.GetValue(10).ToString());       //  Id Mieter
                            liIdMieter = Convert.ToInt16(TblTimelineObjKst.Rows[i].ItemArray.GetValue(10).ToString());
                        }
                        if (TblTimelineObjKst.Rows[i].ItemArray.GetValue(16) != DBNull.Value)
                        {
                            dr[8] = Convert.ToInt16(TblTimelineObjKst.Rows[i].ItemArray.GetValue(16).ToString());       //  Id Kostenart
                        }
                        //if (tableTimeline1.Rows[i].ItemArray.GetValue(0) != DBNull.Value)
                        //{
                        //    dr[9] = Convert.ToDecimal(tableTimeline1.Rows[i].ItemArray.GetValue(0).ToString());      //  Netto
                        //}
                        //if (tableTimeline1.Rows[i].ItemArray.GetValue(1) != DBNull.Value)
                        //{
                        //    dr[11] = Convert.ToDecimal(tableTimeline1.Rows[i].ItemArray.GetValue(1).ToString());     //  Brutto
                        //}

                        if (TblTimelineObjKst.Rows[i].ItemArray.GetValue(4) != DBNull.Value)
                        {
                            dr[15] = Convert.ToInt16(TblTimelineObjKst.Rows[i].ItemArray.GetValue(4).ToString());       //  Weiterleitung Objekt
                        }
                        if (TblTimelineObjKst.Rows[i].ItemArray.GetValue(5) != DBNull.Value)
                        {
                            dr[16] = Convert.ToInt16(TblTimelineObjKst.Rows[i].ItemArray.GetValue(5).ToString());       //  Weiterleitung ObjektTeil
                        }
                        if (TblTimelineObjKst.Rows[i].ItemArray.GetValue(17) != DBNull.Value)
                        {
                            if (Convert.ToInt16(TblTimelineObjKst.Rows[i].ItemArray.GetValue(17)) > 0)
                            {
                                dr[23] = Convert.ToInt16(TblTimelineObjKst.Rows[i].ItemArray.GetValue(17).ToString());       //  Art der Verteilung REchnungen
                                liIdArtVerteilung = Convert.ToInt16(TblTimelineObjKst.Rows[i].ItemArray.GetValue(17).ToString());
                            }
                        }
                        else if (TblTimelineObjKst.Rows[i].ItemArray.GetValue(18) != DBNull.Value)                      // Art der Verteilung für Zähler ermitteln "zl"
                        {
                            liIdExternTimelineZaehlerstand = Convert.ToInt16(TblTimelineObjKst.Rows[i].ItemArray.GetValue(18));
                            liIdArtVerteilung = Timeline.GetIdArtVerteilung("zl", asConnect);
                        }

                        //// Hier nicht zeigen
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
                            dr[26] = Timeline.GetVerteilungsInfo(asConnect, liIdExternTimeline, liIdArtVerteilung, liIdObj, liIdObjt, liIdMieter, asDatVon, asDatBis, liIdExternTimelineZaehlerstand, 1);
                        }

                        // Rechnung aus Objekt oder Teilobjekt
                        if (liIdExternTimeline > 0)
                        {
                            // Objektsummen holen
                            lsSql = GetSql(14, liIdExternTimeline, "", "", 0);
                            liOk = Timeline.FetchData(lsSql, "", 14, asConnect);

                            if (TblConSumObj.Rows.Count > 0)
                            {
                                if (TblConSumObj.Rows[0].ItemArray.GetValue(0) != DBNull.Value)
                                {
                                    dr[21] = Convert.ToDecimal(TblConSumObj.Rows[0].ItemArray.GetValue(0));
                                }
                                if (TblConSumObj.Rows[0].ItemArray.GetValue(1) != DBNull.Value)
                                {
                                    dr[22] = Convert.ToDecimal(TblConSumObj.Rows[0].ItemArray.GetValue(1));
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
                            lsSql = GetSql(16, liIdZaehlerstand, "", "", 0);
                            liOk = Timeline.FetchData(lsSql, "", 14, asConnect);

                            if (TblConSumObj.Rows.Count > 0)
                            {
                                if (TblConSumObj.Rows[0].ItemArray.GetValue(0) != DBNull.Value)
                                {
                                    dr[21] = Convert.ToDecimal(TblConSumObj.Rows[0].ItemArray.GetValue(0));
                                }
                                if (TblConSumObj.Rows[0].ItemArray.GetValue(1) != DBNull.Value)
                                {
                                    dr[22] = Convert.ToDecimal(TblConSumObj.Rows[0].ItemArray.GetValue(1));
                                }
                            }
                        }
                    }
                    TblContent.Rows.Add(dr);
                }
            }


            // Ab in die Datenbank
            liOk = FetchData("", "", 31, asConnect);
            // ist es eine Mieter ID in Timeline, dann die Summen aus Teilobjekt und Objekt einsetzen
            // Ist es eine Teilobjekt ID, dann die Summen aus Objekt einsetzen
            return (liOk);
        }

        // ReportTabelle vor Gebrauch löschen
        public static int DelContent(string asConnect)
        {
            int liOk = 0;
            // kann schonmal gelöscht werden
            lsSql = GetSql(36, 0, "", "", 0);
            liOk = FetchData(lsSql, "", 26, asConnect);
            return (liOk);
        }

        // Die Rechnungs ID aus dem SqlStatement ermitteln
        internal static int GetRechnungsId(string asSqlTimeline, string asConnect, int aiDb)
        {
            int liIdRechnung = 0;

            liIdRechnung = FetchData(asSqlTimeline, "", 16, asConnect);

            return (liIdRechnung);
        }

        // Den Verbrauch aus dem Zählerstand ermitteln
        internal static decimal GetZlVerbrauch(decimal adZlStand, int aiZlId, string asConnect, int aiFlagNew, int aiDb)
        {
            decimal ldZlStandOld = 0;
            decimal ldZlVerbrauch = 0;
            String lsSql = "";

            if (aiFlagNew == 1)
            {
                lsSql = @"SELECT zs FROM zaehlerstaende  WHERE id_zaehler = " + aiZlId.ToString() + " ORDER BY zs desc";
            }
            else
            {
                lsSql = @"SELECT zs FROM zaehlerstaende  WHERE id_zaehler = " + aiZlId.ToString() + " ORDER BY zs desc";
            }

            // Daten holen
            ldZlStandOld = FetchDataDecimal(lsSql, "", 1, asConnect);
            // Differenz
            ldZlVerbrauch = adZlStand - ldZlStandOld;

            return ldZlVerbrauch;
        }

        // Zähler Id vom Namen des Zählers ermitteln
        internal static int GetZlId(string lsZlName, string asConnect, int aiDb)
        {
            String lsSql = "";
            int liZlId = 0;

            lsSql = GetSql(37, 0, lsZlName, "", 0);
            liZlId = FetchData(lsSql, "", 26, asConnect);

            return liZlId;
        }

        // Mehrwertsteuersatz für Zähler holen (aus ZählerId)
        internal static int GetMwstSatzZaehler(int aiZlId, string asConnect, int aiDb)
        {
            String lsSql = "";
            int liMwstSatz = 0;

            lsSql = GetSql(38, aiZlId, "", "", 0);
            liMwstSatz = FetchData(lsSql, "", 26, asConnect);

            return liMwstSatz;
        }

        // Für die bedingte Weiterleitung
        // Hier wird die Auswahl der Objektteile vorbereitet
        // Die Objektteile (Wohnungen) werden in die Tabelle
        // objekt_mix_parts geschrieben
        internal static int MakeChoose(int aiObjekt, int aiTimeLineId, string asConnect, int aiDb)
        {
            int liOk = 0;
            int liRowGet = 0;
            int liRows = 0;

            // Hat die Tabelle objekt_mix_parts einen Eintrag für diese Timeline ID?
            liRows = Timeline.GetInfoFromParts(asConnect, aiTimeLineId, aiDb);

            if (liRows == 0)            // Kein Eintrag vorhanden, Datensatz wird angelegt
            {
                liRowGet = Timeline.CopyParts(asConnect, aiObjekt, aiTimeLineId, aiDb);
                liOk = 1;

            }
            if (liRows > 0)         // Es existiert ein Eintrag der Timeline ID > editieren
            {
                liOk = 2;
            }
            return liOk;
        }

        // Kopieren der Daten eines Objektes in die Tabelle objekt_mix_parts
        public static int CopyParts(string asConnect, int aiObjekt, int aiTimeLineId, int aiDb)
        {
            String lsSql = "";
            int liObj = 0;

            lsSql = GetSql(39, aiObjekt, "", "", 0);
            liObj = FetchData(lsSql, "", 26, asConnect);

            return liObj;
        }

        // Prüfen: Ist die Tabelle objekt_mix_parts leer für diese Timeline ID
        public static int GetInfoFromParts(string asConnect, int aiTimeLineId, int aiDb)
        {
            String lsSql = "";
            int liRows = 0;

            lsSql = GetSql(40, aiTimeLineId, "", "", 0);
            liRows = FetchData(lsSql, "", 26, asConnect);

            return liRows;
        }

        // Verteilungsinformationen für die Nebenkostenabrechnung ermitteln
        // aiId Rechnung ist die Rechnungs Id aus extern Timeline ID ACHTUNG!!
        public static object GetVerteilungsInfo(string asConnect, int aiIdRechnung, int aiArtVerteilungId,
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

            lsVerteilung = GetVerteilung(asConnect, aiArtVerteilungId);

            // Flächenanteil rechnen
            if (lsVerteilung == "fl")
            {
                // Gesamtfläche aus Tabelle Objekt holen
                if (aiMieterId > 0 || aiTObjektId > 0 || aiObjektId > 0)
                {
                    ldGesamtflaecheObjekt = GetObjektflaeche(aiObjektId, aiTObjektId, aiMieterId, asConnect);
                    if (aiTObjektId > 0 || aiObjektId > 0 || aiMieterId > 0)
                    {
                        if (aiTObjektId > 0 || aiMieterId > 0)
                        {
                            ldFlaecheTObjekt = GetTObjektflaeche(aiTObjektId, aiMieterId, asConnect);
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
                    ldProzentAnteil = GetTObjektAnteil(aiTObjektId, aiMieterId, asConnect);
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
                ldAnzPersonen = GetAktPersonen(aiObjektId, aiTObjektId, aiMieterId, asDatVon, asDatBis, 2, asConnect);
                ldAnzPersonenGesamt = GetAktPersonen(aiObjektId, aiTObjektId, aiMieterId, asDatVon, asDatBis, 1, asConnect);
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
                lsVertInfo = "Aus Rg.Nr: \n" + getRgInfo(aiIdRechnung, asConnect, 1);
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
                lsVertInfo = GetVerteilungsInfoZaehler(aiIdExternTimelineZaehlerstand, asConnect);
            }

            // Fläche Auswahl für den Report Nebenkosten
            // Die Gesamtfläche für die Auswahl wird ermittelt
            if (lsVerteilung == "fa")
            {
                // Gesamtfläche der ausgewählten Wohnungen aus Tabelle Objekt_mix_parts holen
                liObjektId = GetIdObj(aiMieterId, asConnect, 1);
                if (liObjektId > 0)
                {
                    int liArt = 0;
                    // Gesamtfläche der Auswahl = 0 oder Gesamtfläche = 1
                    liArt = GetObjektflaecheAuswFlag(liObjektId, asConnect);
                    ldGesamtflaecheObjekt = GetObjektflaecheAuswahl(liObjektId, aiIdRechnung, asConnect, liArt);
                    if (aiTObjektId > 0 || aiMieterId > 0)
                    {
                        ldFlaecheTObjekt = GetTObjektflaeche(aiTObjektId, aiMieterId, asConnect);
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
                                        lsVertInfo = lsVertInfo + getObjekteAuswahl(aiIdRechnung, asConnect);
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
        public static string getObjekteAuswahl(int aiTimelineId, string asConnectString)
        {
            string lsSql = "";
            string lsInfo = "Beteiligte Mietflächen";
            string lsBez = "";
            string lsGeschoss = "";
            string lsLage = "";
            int liOk = 0;

            // objekt_mix_parts
            lsSql = GetSql(25, aiTimelineId, "", "", 0);
            liOk = FetchData(lsSql, "", 25, asConnectString);

            // schleife durch objekt_mix_parts > tableParts
            for (int i = 0; i < TblParts.Rows.Count; i++)
            {
                if (TblParts.Rows[i].ItemArray.GetValue(4) != DBNull.Value)
                    lsBez = TblParts.Rows[i].ItemArray.GetValue(4).ToString().Trim();
                if (TblParts.Rows[i].ItemArray.GetValue(10) != DBNull.Value)
                    lsGeschoss = TblParts.Rows[i].ItemArray.GetValue(10).ToString().Trim();
                if (TblParts.Rows[i].ItemArray.GetValue(11) != DBNull.Value)
                    lsLage = TblParts.Rows[i].ItemArray.GetValue(11).ToString().Trim();
                lsInfo = lsInfo + "\nBez: " + lsBez + "\nGeschoss: " + lsGeschoss + "\nLage: " + lsLage;
            }
            return lsInfo;
        }

        // Berechnung der Fläche für die Auswahl 0 = gewählte Objekte 1 = Gesamtfläche
        public static int GetObjektflaecheAuswFlag(int liObjekt, string asConnect)
        {
            int liFlag = 0;
            string lsSql = "";

            lsSql = GetSql(41, liObjekt, "", "", 0);
            liFlag = FetchData(lsSql, "", 26, asConnect);

            return liFlag;
        }

        // Rechnungsnummer oder Rechnungstext aus RechnungesId holen 1= RgNr 2= RgText
        public static string getRgInfo(int aiIdExternTimeline, string asConnect, int aiArt)
        {
            string lsRgInfo = "";
            string lsSql = "";

            switch (aiArt)
            {
                case 1:
                    lsSql = "SELECT rg_nr FROM rechnungen  WHERE id_extern_timeline = " + aiIdExternTimeline.ToString();
                    break;
                case 2:
                    lsSql = "SELECT text FROM rechnungen  WHERE id_extern_timeline = " + aiIdExternTimeline.ToString();
                    break;
                case 3:
                    lsSql = "SELECT datum_rechnung FROM rechnungen  WHERE id_extern_timeline = " + aiIdExternTimeline.ToString();
                    break;
                case 4:
                    lsSql = "SELECT id_objekt_teil FROM rechnungen  WHERE id_extern_timeline = " + aiIdExternTimeline.ToString();
                    break;
                default:
                    break;
            }

            lsRgInfo = fetchDataString(lsSql, "", 1, asConnect);

            return lsRgInfo;
        }

        // Zusammenstellen vomn Zählerinfos für die Nebenkostenabrechnung
        public static string GetVerteilungsInfoZaehler(int aiIdExternTimelineZaehlerstand, string asConnectString)
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

            lsSql = GetSql(24, aiIdExternTimelineZaehlerstand, "", "", 0);
            liOk = FetchData(lsSql, "", 24, asConnectString);

            if (TblZlInfo.Rows.Count > 0)
            {
                // Es kann nur eine geben Row = 0
                if (TblZlInfo.Rows[0].ItemArray.GetValue(3) != DBNull.Value)      // Zählerstand
                    ldZlStand = (decimal)TblZlInfo.Rows[0].ItemArray.GetValue(3);
                if (TblZlInfo.Rows[0].ItemArray.GetValue(4) != DBNull.Value)      // Datum Ablesung
                    ldtAblesung = (DateTime)TblZlInfo.Rows[0].ItemArray.GetValue(4);
                if (TblZlInfo.Rows[0].ItemArray.GetValue(5) != DBNull.Value)      // Verbrauch
                    ldVerbrauch = (decimal)TblZlInfo.Rows[0].ItemArray.GetValue(5);
                if (TblZlInfo.Rows[0].ItemArray.GetValue(6) != DBNull.Value)      // Einheit Netto
                    ldEinheitNetto = (decimal)TblZlInfo.Rows[0].ItemArray.GetValue(6);
                if (TblZlInfo.Rows[0].ItemArray.GetValue(7) != DBNull.Value)      // Einheit Brutto
                    ldEinheitBrutto = (decimal)TblZlInfo.Rows[0].ItemArray.GetValue(7);
                if (TblZlInfo.Rows[0].ItemArray.GetValue(12) != DBNull.Value)      // Zählernummer
                    lsZlNummer = TblZlInfo.Rows[0].ItemArray.GetValue(12).ToString();
                if (TblZlInfo.Rows[0].ItemArray.GetValue(13) != DBNull.Value)      // Zählerort
                    lsZlOrt = TblZlInfo.Rows[0].ItemArray.GetValue(13).ToString();
                if (TblZlInfo.Rows[0].ItemArray.GetValue(14) != DBNull.Value)      // Einheit Bezeichnung
                    lsEinheit = TblZlInfo.Rows[0].ItemArray.GetValue(14).ToString();
                ldKostenNetto = ldEinheitNetto * ldVerbrauch;
                ldKostenBrutto = ldEinheitBrutto * ldVerbrauch;
            }

            lsInfo = @"Zählernummer: " + lsZlNummer + "\n" +
                      "Ort: " + lsZlOrt + "\n" +
                      "Datum der Ablesung: " + ldtAblesung.ToString("dd.MM.yyyy") + "\n" +
                      "Verbrauch: " + ldVerbrauch.ToString("0,##") + " " + lsEinheit + "\n" +
                      "Preis pro Einheit Netto: " + ldEinheitNetto.ToString("0.####") + "€\n" +
                      "Preis pro Einheit Brutto: " + ldEinheitBrutto.ToString("0.####") + "€";

            return lsInfo;
        }

        // Tabelle objekt_mix_parts leer machen
        // In der Tabelle wird nichts gelöscht      // TODO ?   Ulf!
        internal static void deleteParts(string asConnect, int aiDb)
        {
            string lsSql = "";
            int liOk = 0;

            lsSql = GetSql(42, 0, "", "", 0);
            liOk = FetchData(lsSql, "", 26, asConnect);
        }

        // Informationen über Vertragsbeginn und Ende mit der Mieter id
        // Art 1 = Vertragsbeginn
        // Art 2 = Vertragsende
        public static DateTime getVertragInfo(int aiArt, DateTime adtMonat, int aiMieter, string asConnect)
        {
            DateTime ldtVertrag = DateTime.MinValue;
            string lsSql = "";

            lsSql = RdQueries.GetSqlSelect(42, aiMieter, "", "", "", adtMonat, DateTime.MinValue, aiArt, asConnect, 2);
            // Daten holen
            ldtVertrag = fetchDataDate(lsSql, "", 1, asConnect, 2);

            return ldtVertrag;
        }

        // Vertragsinfos vom Mieter art 1 = Teilbjekt
        public static int getVertragInfoFromMieter(int liIdMieter, string asConnect, int aiArt)
        {
            string lsSql = "";
            int liInfo = 0;

            lsSql = GetSql(43, liIdMieter, "", "", 0);
            liInfo = FetchData(lsSql, "", 26, asConnect);

            return liInfo;
        }

        // Rechnungsnummer für Anschreiben aus dem Pool besorgen
        public static int GetRgNrFromPool(string asConnect)
        {
            string lsSql = "";
            int liInfo = 0;

            lsSql = GetSql(44, 0, "", "", 0);
            liInfo = FetchData(lsSql, "", 26, asConnect);

            return liInfo;
        }

        // Rechnungsnummer aus dem Pool als besetzt kennzeichnen
        public static int SetRgNrFromPool(int liIdRgNr, string asConnect)
        {
            string lsSql = "";
            int liOk = 0;

            lsSql = GetSql(45, liIdRgNr, "", "", 0);
            liOk = FetchData(lsSql, "", 26, asConnect);

            return liOk;
        }

        // Die ID der Rechnungsnummer Anschreiben in Timeline einsetzen
        public static int SetRgNrToTml(int aiIdRgNr, string asSqlRgNr, string asConnect)
        {
            string lsSql = "";
            int liOk = 0;

            lsSql = GetSql(46, aiIdRgNr, asSqlRgNr, "", 0);
            liOk = FetchData(lsSql, "", 26, asConnect);

            return liOk;
        }

        // Aus dem String der Bezeichnung die VerteilungsId holen
        internal static int GetVertId(string asBez, string asConnect)
        {
            int liId;

            lsSql = GetSql(47, 0, asBez, "", 0);
            liId = FetchData(lsSql, "", 26, asConnect);

            return (liId);
        }

        // ermitteln des des aktuellen Mandanten
        internal static int GetMandantId(string asConnect)
        {
            int liId;

            lsSql = GetSql(48, 0, "", "", 0);
            liId = FetchData(lsSql, "", 26, asConnect);

            return (liId);
        }

        // Id der Filaile aus der Mandanten Id ermitteln
        internal static int GetFilialeId(int aiMandantId, string asConnect)
        {
            int liId;

            lsSql = GetSql(49, aiMandantId, "", "", 0);
            liId = FetchData(lsSql, "", 26, asConnect);

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

        // Untergeordnete Rechnungen erzeugen
        // Zuerst nur Rechnungen für Teilobjekte
        internal static int CreateRechnungen(int AiSourceId, int AiObjektId, int AiObjektTeilId, 
                int AiMieterid, int AiArtRelation, MySqlDataAdapter ASdaRechnungen, System.Data.DataTable ATblRechnungen,
                MySqlDataAdapter ASdaTeilobjekte, System.Data.DataTable ATblTeilobjekte, string AsConnect  )
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
                LiSourceId = (int)ATblRechnungen.Rows[0].ItemArray.GetValue(14);        // rechnungs Id Quelle
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
                // Anzahl der einzutragenden Monat ermitteln
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
                // Achtung nbüschen gepfuscht liRechnungId ist die externTimeline Id
                liVerteilungId = Timeline.GetVerteilungsId(AsConnect, LiSourceId);
                // Ermitteln, wie verteilt werden soll aus der Tabelle art_verteilung
                LsVerteilung = Timeline.GetVerteilung(AsConnect, liVerteilungId);
                // Gesamtfläche aus Tabelle Objekt holen
                ldGesamtflaeche = Timeline.GetObjektflaeche(liObjekt, 0, 0, AsConnect);

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

                    // Beträge nach Art der Verteilung berechnen
                    switch (LsVerteilung)
                    {
                        case "fl":
                            if (ATblTeilobjekte.Rows[i].ItemArray.GetValue(6) != DBNull.Value)
                            {
                                if ((decimal)ATblTeilobjekte.Rows[i].ItemArray.GetValue(6) > 0) // Fläche Teilobjekt
                                {

                                    if (liObjekt > 0)
                                    {
                                        DrRechnung[5] = ldBetragNetto / (ldGesamtflaeche / (decimal)ATblTeilobjekte.Rows[i].ItemArray.GetValue(6));           // Netto    
                                        DrRechnung[6] = ldBetragBrutto / (ldGesamtflaeche / (decimal)ATblTeilobjekte.Rows[i].ItemArray.GetValue(6));         // Brutto
                                    }
                                }
                                else
                                {
                                    liSave = 0;
                                }
                            }
                            break;
                        case "pz":
                            break;
                        case "ps":
                            break;
                        case "di":
                            break;
                        case "nl":
                            break;
                        case "fa":
                            break;
                        default:
                            break;
                    }

                    if ((int)ATblTeilobjekte.Rows[i].ItemArray.GetValue(0) == 444)
                    {
                        int lit = 1;
                    }
                    ATblRechnungen.Rows.Add(DrRechnung);
                }

                // Daten in die Datenbank schreiben
                LiOk = FetchData("", "", 33, AsConnect);

                }
            else
            {
                MessageBox.Show("Verarbeitungsfehler ERROR fetchdata fetchdata RdFunctions\n CreateRechnungen = ",
                            "Achtung");
            }
            return LiOk;
        }

        // Untergeordnete Rechungen löschen
        internal static int DeleteRechnungen(int AiSourceId, string asArt, string asConnect)
        {
            string LsSql;
            int LiOk = 0;

            // Rechnungen löschen
            LsSql = GetSql(150, AiSourceId, "", "", 0);
            LiOk = FetchData(LsSql, "", 34, asConnect);

            return LiOk;
        }
    }
}