using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruddat_NK
{
    internal class RdQueriesFunctions
    {
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
                                    id_rechnung_source,
                                    flag_editable
                            FROM rechnungen
					         WHERE id_extern_timeline = " + lsWhereAdd +
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
                case 205:
                    // Timeline löschen Source Rechnungsnummer
                    lsWhereAdd = piId.ToString() + " ";

                    lsSql = @"delete FROM timeline
					         WHERE id_rg_nr = " + lsWhereAdd;
                    break;
                case 206:
                    lsWhereAdd = piId.ToString() + " ";

                    lsSql = @"delete FROM timeline
					         WHERE id_mieter = " + lsWhereAdd;
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
                            FROM timeline ";
                           //   WHERE " + ps2 + " = " + lsWhereAdd;
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
                case 28:        // Weiterleitung Info holen
                    switch (piId2)
                    {
                        case 1:
                            // Weiterleitung an Objektteil
                            lsSql = "SELECT wtl_obj_teil FROM dbo.art_kostenart WHERE Id_ksa =" + piId.ToString();
                            break;
                            // Weiterleitung an Mieter
                        case 2:
                            lsSql = "SELECT wtl_mieter FROM dbo.art_kostenart WHERE Id_ksa =" + piId.ToString();
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
                case 32:        // Die VerteilungsId aus der übergeordneten Rechnungen ermitteln
                    lsSql = @"SELECT id_verteilung FROM rechnungen  WHERE id_rechnungen = " + piId.ToString();
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
                case 53:         // ProzentAnteil aus Teilobjekt
                    lsSql = @"SELECT prozent_anteil FROM objekt_teil WHERE Id_objekt_teil = " + piId.ToString();
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
    }
}
