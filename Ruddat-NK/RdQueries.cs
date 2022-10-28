using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruddat_NK
{
    public class RdQueries
    {
        // Sql-Statement erstellen
        public static string GetSqlSelect(int piArt, int piId, string ps2, string ps3, DateTime adtWtStart, DateTime adtWtEnd, int aiFiliale, string asConnectString, int aiDb)
        {
            String lsSql = "";
            String lsWhereAdd = "";
            String lsWhereAdd1 = "";
            String lsWhereAdd2 = "";
            String lsWhereAdd3 = "";
            String lsWhereAdd4 = "";
            String lsField = "";
            String lsAnd = " Where ";
            String lsOrder = "";
            String lsGroup = "";
            DateTime ldtAdd = DateTime.MinValue;
            // DateTime ldtEnd = DateTime.Today;                       // Heute
            int liYear = DateTime.Now.Year - 1;
            string lsStart = (liYear.ToString()) + "-01-01";
            string lsEnd = (liYear.ToString()) + "-12-31";
            DateTime ldtStart = DateTime.Parse(lsStart);                 // Jahresanfang VorJahr
            DateTime ldtEnd = DateTime.Parse(lsEnd);
            int liOne = 0;                                          // Ein oder 2 Zeitangaben
            int liMieterId = 0;
            int liObjId = 0;
            int liObjTeilId = 0;

            if (piArt == 1)
            {
                lsSql = "Select id_filiale,name from filiale order by name";
                // Todo Erweiterung für mehrere Mandanten; Mandant in das XML schreiben
                // lsWhereAdd = " Where id_mandant = " + piId.ToString();
            }

            // Sql für Treeview komplett
            if (piArt == 2)
            {
                // Um Objekte oder Teilobjekte im Treeview zu zeigen müssen:
                // Das Objekt eine Adresse haben
                // ein Mieter eingetragen sein
                // ein Vertrag existieren

                // lsWhereAdd = " and  vertrag.vertrag_aktiv = 1";

                lsSql = @"Select    objekt.bez as obj,
				            objekt_teil.bez as objteil,
				            mieter.bez as mieter, 
				            adressen.adresse as adresse, 
				            adressen.ort as ort,
							objekt.Id_objekt,
							objekt_teil.Id_objekt_teil,
							mieter.Id_mieter,
                            vertrag.vertrag_aktiv
        		from filiale 
	                join objekt on objekt.id_filiale = filiale.id_filiale 
	                join objekt_teil on objekt_teil.id_objekt = objekt.Id_objekt
					Join Adressen on adressen.Id_objekt = objekt.Id_objekt
					left Join vertrag on vertrag.id_objekt_teil = objekt_teil.Id_objekt_teil
					left Join mieter on mieter.Id_Mieter = vertrag.id_mieter
	                    where filiale.Id_Filiale = " + piId.ToString() +
                                    lsWhereAdd + " Order by objekt.kst,objekt_teil.kst";
            }

            // Sql für Treeview Objekte und Teilobjekte
            if (piArt == 21)
            {

                lsSql = @"Select    objekt.bez as obj,
				            objekt_teil.bez as objteil,
							objekt.Id_objekt
        		from filiale 
	                join objekt on objekt.id_filiale = filiale.id_filiale 
	                join objekt_teil on objekt_teil.id_objekt = objekt.Id_objekt
	            where filiale.Id_Filiale = " + piId.ToString() +
                        "Order by id_objekt,id_objekt_teil";
            }

            // Sql für Ermitteln der ID für die Timeline
            if (piArt == 3)
            {
                switch (ps3)
                {
                    case "1":
                        lsWhereAdd = " and objekt.bez = \'" + ps2 + "\'";
                        break;
                    case "2":
                        lsWhereAdd = " and objekt_teil.bez = \'" + ps2 + "\'";
                        break;
                    case "3":
                        lsWhereAdd = " and mieter.bez = \'" + ps2 + "\'";
                        break;
                    default:
                        break;
                }

                lsWhereAdd2 = " ";
                lsWhereAdd = " " + lsWhereAdd.Trim();

                lsSql = @"Select    objekt.bez as obj, 
				            objekt_teil.bez as objteil,
				            mieter.bez as mieter, 
				            adressen.adresse as adresse, 
				            adressen.ort as ort,
							objekt.Id_objekt,
							objekt_teil.Id_objekt_teil,
							mieter.Id_mieter
        		from filiale 
	                join objekt on objekt.id_filiale = filiale.id_filiale 
	                join objekt_teil on objekt_teil.id_objekt = objekt.Id_objekt
					Join Adressen on adressen.Id_objekt = objekt.Id_objekt
					left Join vertrag on vertrag.id_objekt_teil = objekt_teil.Id_objekt_teil
					left Join mieter on mieter.Id_Mieter = vertrag.id_mieter
	                    where filiale.Id_Filiale = " + piId.ToString() +
                                    lsWhereAdd + lsWhereAdd2 + " Order by id_objekt,id_objekt_teil ";
            }

            // SQL für die Timeline Summendarstellung Objekte, TeilObjekte oder Mieter
            if (piArt == 5 || piArt == 6 || piArt == 7)
            {
                lsSql = @"Select                  
                    art_kostenart.bez as ksa_bez,
                    Sum(timeline.betrag_netto) as betrag_netto,
					Sum(timeline.betrag_brutto) as betrag_brutto,
                    Sum(timeline.betrag_soll_netto),
                    Sum(timeline.betrag_soll_brutto),
                    timeline.id_rechnung,
                    timeline.id_vorauszahlung,
                    timeline.wtl_aus_objekt,
                    timeline.wtl_aus_objteil,
                    timeline.id_zaehlerstand
                from timeline
                Right Join art_kostenart on timeline.id_ksa = art_kostenart.id_ksa";
                lsGroup = @" Group by art_kostenart.bez,art_kostenart.sort,timeline.id_rechnung,timeline.id_vorauszahlung,
                        timeline.wtl_aus_objekt,timeline.wtl_aus_objteil,timeline.id_zaehlerstand  ";
                lsOrder = " Order by art_kostenart.sort ";
                // Objekt ID
                if (piId > 0)
                {
                    switch (piArt)
                    {
                        case 5:                     // Objekt
                            lsWhereAdd1 = " Where timeline.Id_objekt = " + piId.ToString() + " ";
                            lsSql = lsSql + lsWhereAdd1;
                            lsAnd = " And ";
                            break;
                        case 6:                     // TeilObjekt
                            lsWhereAdd1 = " Where timeline.Id_objekt_teil = " + piId.ToString() + " ";
                            lsSql = lsSql + lsWhereAdd1;
                            lsAnd = " And ";
                            break;
                        case 7:                     // Mieter
                            lsWhereAdd1 = " Where timeline.Id_mieter = " + piId.ToString() + " ";
                            lsSql = lsSql + lsWhereAdd1;
                            lsAnd = " And ";
                            break;
                        case 71:                     // Leerstand Teilobjekt
                            lsWhereAdd1 = " Where timeline.leerstand = " + piId.ToString() + " ";
                            lsSql = lsSql + lsWhereAdd1;
                            lsAnd = " And ";
                            break;
                        default:
                            break;
                    }
                    // Rückgabe des ZeitQueries für TimeLine
                    lsField = "timeline.dt_monat";
                    liOne = 2;
                    lsWhereAdd2 = RdQueriesTime.GetDateQueryResult(adtWtStart, adtWtEnd, ldtStart, ldtEnd, lsField, lsAnd, liOne, aiDb);
                    lsSql = lsSql + lsWhereAdd2;
                    lsSql = lsSql + lsGroup + lsOrder;
                }
                else
                {
                    lsAnd = " Where ";
                }
            }

            // Rechnungsdarstellung für Objekte
            if (piArt == 8)
            {
                lsAnd = " And ";

                // Rückgabe des ZeitQueries für Rechnungen
                lsField = "rechnungen.datum_von";
                liOne = 1;
                lsWhereAdd2 = RdQueriesTime.GetDateQueryResult(adtWtStart, adtWtEnd, ldtStart, ldtEnd, lsField, lsAnd, liOne, aiDb);

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
					where id_objekt = " + piId.ToString() + lsWhereAdd2 +
                            " Order by rechnungen.datum_rechnung desc";
            }

            // Rechnungsdarstellung für TeilObjekte
            if (piArt == 9)
            {
                lsAnd = " And ";
                lsField = "rechnungen.datum_von";
                liOne = 1;
                lsWhereAdd2 = RdQueriesTime.GetDateQueryResult(adtWtStart, adtWtEnd, ldtStart, ldtEnd, lsField, lsAnd, liOne, aiDb);
                
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
					where id_objekt_teil = " + piId.ToString() + lsWhereAdd2 +
                            " Order by rechnungen.datum_rechnung desc";
            }

            // Rechnungsdarstellung für Mieter
            if (piArt == 10)
            {
                lsAnd = " And ";
                lsField = "rechnungen.datum_von";
                liOne = 2;
                lsWhereAdd2 = RdQueriesTime.GetDateQueryResult(adtWtStart, adtWtEnd, ldtStart, ldtEnd, lsField, lsAnd, liOne, aiDb);
                
                lsSql = @"select id_rechnungen,
                            id_ksa,
                            datum_rechnung as datum,
                            datum_von as von,
                            datum_bis as bis,
                            betrag_netto as netto,
                            betrag_brutto as brutto, 
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
					where id_mieter = " + piId.ToString() + lsWhereAdd2 +
                            " Order by rechnungen.datum_rechnung desc";
            }
            // Combobox Kostenart: Wird abhängig von der Anwahl gezeigt
            if (piArt == 11)
            {
                lsSql = " Select id_ksa,bez,wtl_obj_teil,wtl_mieter from art_kostenart ";
                switch (piId)
                {
                    case 1: // Objekt
                        lsWhereAdd = " Where ksa_objekt = 1 ";
                        break;
                    case 2: // Objektteil
                        lsWhereAdd = " Where ksa_obj_teil = 1 ";
                        break;
                    case 3: // Mieter
                        lsWhereAdd = " Where ksa_mieter = 1 ";
                        break;
                    case 4: // Zahlung
                        lsWhereAdd = " Where ksa_zahlung = 1 ";
                        break;
                    case 5: // Zähler
                        lsWhereAdd = " Where ksa_zaehler = 1 ";
                        break;
                    default:
                        break;
                }

                lsOrder = " order by bez ";
                lsSql = lsSql + lsWhereAdd + lsOrder;
            }
            // Combobox mwst
            if (piArt == 12)
            {
                lsSql = " Select id_mwst_art,mwst from art_mwst";
            }

            // SQL für die Timeline Detaildarstellung Objekte, TeilObjekte oder Mieter
            // Zufügen einer Where-Klausel für die externe TimeLine ID
            if (piArt == 13)
            {
                switch (ps2)
                {
                    case "1":       // Objekt
                        lsWhereAdd2 = " And timeline.id_objekt = " + ps3 + " ";
                        break;
                    case "2":       // Teil
                        lsWhereAdd2 = " And timeline.id_objekt_teil = " + ps3 + " ";
                        break;
                    case "3":       // Mieter
                        lsWhereAdd2 = " And timeline.id_mieter = " + ps3 + " ";
                        break;
                    case "4":
                        lsWhereAdd2 = " And timeline.leerstand = " + ps3 + " ";
                        break;
                    default:
                        lsWhereAdd2 = "";
                        break;
                }

                lsSql = @"Select                  
                    timeline.Id_timeline,
                    art_kostenart.bez as ksa_bez,
                    timeline.betrag_netto,
					timeline.betrag_brutto,
                    timeline.betrag_soll_netto,
                    timeline.betrag_soll_brutto,
                    timeline.dt_monat as monat,
                    timeline.wtl_aus_objekt,
                    timeline.wtl_aus_objteil
                from timeline
                Right Join art_kostenart on timeline.id_ksa = art_kostenart.id_ksa ";

                lsWhereAdd = " Where ( timeline.Id_rechnung = " + piId.ToString() + " or timeline.Id_vorauszahlung = " + piId.ToString() + " or timeline.Id_zaehlerstand = " + piId.ToString() + " )";
                lsOrder = " Order by art_kostenart.sort, timeline.dt_monat ";
                lsAnd = " And ";
                lsField = "timeline.dt_monat";
                liOne = 2;
                lsWhereAdd3 = RdQueriesTime.GetDateQueryResult(adtWtStart, adtWtEnd, ldtStart, ldtEnd, lsField, lsAnd, liOne, aiDb);
                lsSql = lsSql + lsWhereAdd + lsWhereAdd2 + lsWhereAdd3 + lsOrder;
            }

            // Combobox Kosten Verteilungsarten
            if (piArt == 16)
            {
                lsSql = @" Select id_verteilung
                                ,bez as b
                                ,kb 
                            from art_verteilung";
            }

            // InfoTablelle für den Druck der Abrechnungen
            if (piArt == 17)
            {
                lsSql = "Select Id_info,id_objekt,id_objekt_teil,id_mieter,abr_dat_von,abr_dat_bis,vertr_dat_von,vertr_dat_bis from x_abr_info";
            }

            // Combobox Einheiten Zähler
            if (piArt == 20)
            {
                lsSql = " Select id_einheit as id_eh ,bez,faktor from art_einheit";
            }

            // Combobox Zählernummern für Objekte und ObjektTeile
            if (piArt == 22 || piArt == 222)
            {
                switch (piArt)
                {
                    case 22:
                        lsWhereAdd = " Where zaehler.Id_objekt = " + piId.ToString() + " and zaehler.Id_objekt_teil = 0 ";
                        break;
                    case 222:
                        lsWhereAdd = " Where zaehler.Id_objekt_teil = " + piId.ToString();
                        break;
                    default:
                        break;
                }
                lsSql = @" Select id_zaehler as id_zl
                                , zaehlernummer as zn
                                , art_einheit.bez as zleh
                                , art_mwst.mwst as zlmw 
                                from zaehler
                        left join art_mwst on zaehler.id_mwst_art = art_mwst.Id_mwst_art
                        left join art_einheit on zaehler.id_einheit = art_einheit.id_einheit";
                lsSql = lsSql + lsWhereAdd;
            }

            // Zahlungsdarstellung
            if (piArt == 23 || piArt == 24 || piArt == 25)
            {
                lsAnd = " And ";
                lsField = "zahlungen.datum_von";
                liOne = 2;
                lsWhereAdd2 = RdQueriesTime.GetDateQueryResult(adtWtStart, adtWtEnd, ldtStart, ldtEnd, lsField, lsAnd, liOne, aiDb);

                if (piArt == 23)  // Zahlungen für Mieter
                {
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
                    from zahlungen
					where id_mieter = " + piId.ToString() + lsWhereAdd2;
                }
                if (piArt == 24)  // Zahlungen für Objekte
                {
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
                    from zahlungen
					where id_objekt = " + piId.ToString() + lsWhereAdd2;
                }
                if (piArt == 25)  // Zahlungen für Teilobjekte
                {
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
                    from zahlungen
					where id_objekt_teil = " + piId.ToString() + lsWhereAdd2;
                }
                lsOrder = " Order by datum_von desc ";
                lsSql = lsSql + lsOrder;
            }

            // Zählerstände für Objekte, TeilObjekte, und die UpdateTabelle
            if (piArt == 34 || piArt == 35)
            {
                lsAnd = " And ";
                lsField = "zaehlerstaende.datum_von";
                liOne = 2;
                lsWhereAdd2 = RdQueriesTime.GetDateQueryResult(adtWtStart, adtWtEnd, ldtStart, ldtEnd, lsField, lsAnd, liOne, aiDb);

                if (piArt == 34)  // Zählerstände für Objekte
                {
                    lsSql = @"select id_zs,
                        zaehlerstaende.datum_von as von,
						zaehlerstaende.zs as zs,
						zaehlerstaende.verbrauch as verb,
						zaehlerstaende.id_einheit,
                        zaehlerstaende.preis_einheit_netto as prnetto,
                        zaehlerstaende.preis_einheit_brutto as prbrutto,
                        zaehlerstaende.id_extern_timeline,
						zaehlerstaende.id_objekt,
                        zaehlerstaende.id_objekt_teil,
                        zaehlerstaende.id_zaehler,
                        zaehlerstaende.id_ksa,
                        zaehlerstaende.id_verteilung as id_verteilung_zl
				from zaehlerstaende
				where zaehlerstaende.id_objekt = " + piId.ToString() + lsWhereAdd2;
                }
                if (piArt == 35)  // Zählerstände für Teilobjekte
                {
                    lsSql = @"select id_zs,
                        zaehlerstaende.datum_von as von,
						zaehlerstaende.zs as zs,
						zaehlerstaende.verbrauch as verb,
						zaehlerstaende.id_einheit,
                        zaehlerstaende.preis_einheit_netto as prnetto,
                        zaehlerstaende.preis_einheit_brutto as prbrutto,
						zaehlerstaende.id_extern_timeline,
						zaehlerstaende.id_objekt,
                        zaehlerstaende.id_objekt_teil,
                        zaehlerstaende.id_zaehler,
                        zaehlerstaende.id_ksa,
                        zaehlerstaende.id_verteilung as id_verteilung_zl
				from zaehlerstaende
				where zaehlerstaende.id_objekt_teil = " + piId.ToString() + lsWhereAdd2;
                }
                lsOrder = " Order by datum_von desc ";
                lsSql = lsSql + lsOrder;
            }

            if (piArt == 36)
            {
                // Rechnung löschen
                lsSql = "Delete from rechnungen Where id_rechnungen = " + piId.ToString();
            }
            if (piArt == 38)
            {
                // Zahlung löschen
                lsSql = "Delete from zahlungen Where id_vz = " + piId.ToString();
            }
            if (piArt == 40)
            {
                // Zählerstände löschen
                lsSql = "Delete from zaehlerstaende Where id_zs = " + piId.ToString();
            }   // Mieter Id aus Vertrag ermitteln
            if (piArt == 41)
            {
                lsSql = @"Select id_mieter from vertrag
                        Where id_objekt_teil = " + piId.ToString() + " AND vertrag_aktiv = 1 " ;
                lsField = "vertrag.datum_von";
                lsWhereAdd2 = RdQueriesTime.GetDateQueryResultVertrag(adtWtStart, lsField, liOne, aiDb);
                lsSql = lsSql + lsWhereAdd2;
            }   // Vertragsbeginn oder -ende
            if (piArt == 42)
            {
                switch (aiFiliale)      // Filiale wird hier verwendet, um Vetragsbeginn oder -ende zu ermitteln
                {
                    case 1:
                        lsSql = @"Select datum_von from vertrag where vertrag.id_mieter = " + piId.ToString() + " AND vertrag_aktiv = 1 ";
                        break;
                    case 2:
                        lsSql = @"Select datum_bis from vertrag where vertrag.id_mieter = " + piId.ToString() + " AND vertrag_aktiv = 1 ";
                        break;
                    default:
                        break;
                }
            }

            // -----------------------------------------------------------------------------------------------------------------------------
            // ----------------------------------------------------Reports ab hier----------------------------------------------------------
            // -----------------------------------------------------------------------------------------------------------------------------
            // SQL für die Timeline Summendarstellung Objekte, TeilObjekte, Mieter, eine gezielte Rechnung (Objekt oder Teilobjekt) oder Mieter NK Zahlungen 115
            if (piArt == 105 || piArt == 106 || piArt == 107 || piArt == 115 || piArt == 116)
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
                    timeline.id_zaehlerstand,
                    art_kostenart.wtl_obj_teil,
                    art_kostenart.wtl_mieter,
                    rechnungen.text as rg_txt,
                    timeline.id_rg_nr
                from timeline
				Left Join rechnungen on rechnungen.id_extern_timeline = timeline.id_rechnung
                Left Join zaehlerstaende on zaehlerstaende.id_extern_timeline = timeline.id_zaehlerstand				
				Right Join art_kostenart on timeline.id_ksa = art_kostenart.id_ksa";
                lsGroup = @" Group by timeline.id_rechnung,timeline.id_vorauszahlung,timeline.id_objekt,
					timeline.id_objekt_teil,timeline.id_mieter,rechnungen.Rg_nr,art_kostenart.bez,
					rechnungen.betrag_netto,rechnungen.betrag_brutto,art_kostenart.sort,timeline.wtl_aus_objekt,
                    timeline.wtl_aus_objteil,rechnungen.datum_rechnung,rechnungen.firma,timeline.id_ksa,
                    rechnungen.id_verteilung,timeline.id_zaehlerstand,art_kostenart.wtl_obj_teil,
                    art_kostenart.wtl_mieter,rechnungen.text,timeline.id_rg_nr ";
                lsOrder = " Order by art_kostenart.sort ";
                // Objekt ID
                if (piId > 0)
                {
                    switch (piArt)
                    {
                        case 105:                     // Objekt
                            lsWhereAdd1 = " Where timeline.Id_objekt = " + piId.ToString() + " ";
                            lsSql = lsSql + lsWhereAdd1;
                            lsAnd = " And ";
                            lsWhereAdd4 = lsAnd + " (timeline.id_rechnung > 0 or timeline.id_zaehlerstand > 0) ";     // nur Rechnungen und Zählerstände
                            break;
                        case 106:                     // TeilObjekt
                            lsWhereAdd1 = " Where timeline.Id_objekt_teil = " + piId.ToString() + " ";
                            lsSql = lsSql + lsWhereAdd1;
                            lsAnd = " And ";
                            lsWhereAdd4 = lsAnd + " (timeline.id_rechnung > 0 or timeline.id_zaehlerstand > 0) ";     // nur Rechnungen und Zählerstände
                            break;
                        case 107:                     // Mieter
                            lsWhereAdd1 = " Where timeline.Id_mieter = " + piId.ToString() + " ";
                            lsSql = lsSql + lsWhereAdd1;
                            lsAnd = " And ";
                            lsWhereAdd4 = lsAnd + " (timeline.id_rechnung > 0 or timeline.id_zaehlerstand > 0) ";     // nur Rechnungen und Zählerstände
                            break;
                        case 116:                   // Jetzt wird es kompliziert > Objekt
                            // id der Verteilung ermitteln, dann wird kein Join benötigt
                            int liId = Timeline.getVertId("nl", asConnectString, aiDb);
                            lsWhereAdd1 = " Where timeline.Id_objekt = " + piId.ToString() + " ";                     // Nur Zählerstände für das Objekt darstellen  
                            lsSql = lsSql + lsWhereAdd1;                                                              // Es sollen nur ObjektKosten in der Nebenkostenabrechnung dargestellt werden
                            lsAnd = " And ";
                            lsWhereAdd4 = lsAnd + " (timeline.id_zaehlerstand > 0 or (timeline.id_rechnung > 0)) "
                                                    + " And (rechnungen.Id_verteilung = " + liId.ToString()
                                                    + " Or zaehlerstaende.id_verteilung = " + liId.ToString() + ") ";                       
                                                                                                // nur Rechnungen und Zählerstände und keine Verteilung
                            break;                                                              // ACHTUNG Ulf TODO wenn weitere Kosten gezeigt werden sollen, id Rechnung > 0 einfügen
                        case 115:                      // Mieter Kosten und Vorrauszahlungen für Summendarstellung
                            lsWhereAdd1 = " Where timeline.Id_mieter = " + piId.ToString() + " ";
                            lsSql = lsSql + lsWhereAdd1;
                            lsAnd = " And ";
                            break;
                        default:
                            break;
                    }
                    lsField = "timeline.dt_monat";
                    liOne = 2;
                    lsWhereAdd2 = RdQueriesTime.GetDateQueryResult(adtWtStart, adtWtEnd, ldtStart, ldtEnd, lsField, lsAnd, liOne, aiDb);
                    lsAnd = " AND ";

                    // Nur wenn Ausdruck gewünscht wird
                    lsWhereAdd3 = lsAnd + " art_kostenart.sort > 0";

                    lsSql = lsSql + lsWhereAdd2 + lsWhereAdd3 + lsWhereAdd4;
                    lsSql = lsSql + lsGroup + lsOrder;
                }
                else
                {
                    lsAnd = " Where ";
                }
            }

            // Rechnungen
            if (piArt == 108)   // Objekte
            {
                lsAnd = " And ";
                lsField = "rechnungen.datum_rechnung";
                liOne = 2;
                lsWhereAdd2 = RdQueriesTime.GetDateQueryResult(adtWtStart, adtWtEnd, ldtStart, ldtEnd, lsField, lsAnd, liOne, aiDb);

                lsSql = @"select id_rechnungen,
                        art_kostenart.bez as kbez,
                        datum_rechnung as datum,
                        datum_von as von,
                        datum_bis as bis,
                        betrag_netto netto,
                        betrag_brutto brutto,
                        art_mwst.mwst as mwst,
                        objekt.bez as objbez,
                        rg_nr,
                        firma,
                        text,
                        id_extern_timeline,
                        flag_timeline
				from rechnungen
                left join art_kostenart on rechnungen.id_ksa = art_kostenart.id_ksa
                left join art_mwst on rechnungen.id_mwst_art = art_mwst.id_mwst_art
                left join objekt on rechnungen.id_objekt = objekt.id_objekt
                left join objekt_teil on rechnungen.id_objekt_teil = objekt_teil.id_objekt_teil
                left join mieter on rechnungen.id_mieter = mieter.id_mieter
				where rechnungen.id_objekt = " + piId.ToString() + lsWhereAdd2 +
                            " Order by rechnungen.datum_rechnung desc";
            }

            if (piArt == 109)   // ObjektTeile
            {
                lsAnd = " And ";
                lsField = "rechnungen.datum_rechnung";
                liOne = 2;
                lsWhereAdd2 = RdQueriesTime.GetDateQueryResult(adtWtStart, adtWtEnd, ldtStart, ldtEnd, lsField, lsAnd, liOne, aiDb);

                lsSql = @"select id_rechnungen,
                        art_kostenart.bez as kbez,
                        datum_rechnung as datum,
                        datum_von as von,
                        datum_bis as bis,
                        betrag_netto netto,
                        betrag_brutto brutto,
                        art_mwst.mwst as mwst,
                        objekt_teil.bez as obtbez,
                        rg_nr,
                        firma,
                        text,
                        id_extern_timeline,
                        flag_timeline
				from rechnungen
                left join art_kostenart on rechnungen.id_ksa = art_kostenart.id_ksa
                left join art_mwst on rechnungen.id_mwst_art = art_mwst.id_mwst_art
                left join objekt on rechnungen.id_objekt = objekt.id_objekt
                left join objekt_teil on rechnungen.id_objekt_teil = objekt_teil.id_objekt_teil
                left join mieter on rechnungen.id_mieter = mieter.id_mieter
				where rechnungen.id_objekt_teil = " + piId.ToString() + lsWhereAdd2 +
                            " Order by rechnungen.datum_rechnung desc";
            }

            if (piArt == 110)   // Mieter
            {
                lsAnd = " And ";
                lsField = "rechnungen.datum_rechnung";
                liOne = 2;
                lsWhereAdd2 = RdQueriesTime.GetDateQueryResult(adtWtStart, adtWtEnd, ldtStart, ldtEnd, lsField, lsAnd, liOne, aiDb);

                lsSql = @"select id_rechnungen,
                        art_kostenart.bez as kbez,
                        datum_rechnung as datum,
                        datum_von as von,
                        datum_bis as bis,
                        betrag_netto netto,
                        betrag_brutto brutto,
                        art_mwst.mwst as mwst,
                        mieter.bez as mbez,
                        rg_nr,
                        firma,
                        text,
                        id_extern_timeline,
                        flag_timeline
				from rechnungen
                left join art_kostenart on rechnungen.id_ksa = art_kostenart.id_ksa
                left join art_mwst on rechnungen.id_mwst_art = art_mwst.id_mwst_art
                left join objekt on rechnungen.id_objekt = objekt.id_objekt
                left join objekt_teil on rechnungen.id_objekt_teil = objekt_teil.id_objekt_teil
                left join mieter on rechnungen.id_mieter = mieter.id_mieter
				where rechnungen.id_mieter = " + piId.ToString() + lsWhereAdd2 +
                            " Order by rechnungen.datum_rechnung desc";
            }

            // Nur Where für Reports Zahlungen
            if (piArt == 123 || piArt == 124 || piArt == 125)
            {
                lsAnd = " And ";
                lsField = "zahlungen.datum_von";
                liOne = 2;
                lsWhereAdd2 = RdQueriesTime.GetDateQueryResult(adtWtStart, adtWtEnd, ldtStart, ldtEnd, lsField, lsAnd, liOne, aiDb);

                if (piArt == 124)   // Objekte
                {
                    lsSql = @"select id_vz,
                        objekt.bez as objbez,
                        objekt_teil.bez as obtbez,
                        mieter.bez as mbez,
                        datum_von as von,
                        datum_bis as bis,
                        betrag_netto netto,
                        betrag_brutto brutto, 
                        betrag_netto_soll snetto,
                        betrag_brutto_soll sbrutto, 
                        id_extern_timeline,
                        flag_timeline,
                        art_kostenart.bez as kbez
				from zahlungen
                left join art_kostenart on zahlungen.id_ksa = art_kostenart.id_ksa
                left join objekt on zahlungen.id_objekt = objekt.id_objekt
                left join objekt_teil on zahlungen.id_objekt_teil = objekt_teil.id_objekt_teil
                left join mieter on zahlungen.id_mieter = mieter.id_mieter
				where zahlungen.id_objekt = " + piId.ToString() + lsWhereAdd2;
                }

                if (piArt == 125)   // ObjektTeile
                {
                    lsSql = @"select id_vz,
                        objekt.bez as objbez,
                        objekt_teil.bez as obtbez,
                        mieter.bez as mbez,
                        datum_von as von,
                        datum_bis as bis,
                        betrag_netto netto,
                        betrag_brutto brutto, 
                        betrag_netto_soll snetto,
                        betrag_brutto_soll sbrutto, 
                        id_extern_timeline,
                        flag_timeline,
                        art_kostenart.bez as kbez
				from zahlungen
                left join art_kostenart on zahlungen.id_ksa = art_kostenart.id_ksa
                left join objekt on zahlungen.id_objekt = objekt.id_objekt
                left join objekt_teil on zahlungen.id_objekt_teil = objekt_teil.id_objekt_teil
                left join mieter on zahlungen.id_mieter = mieter.id_mieter
				where zahlungen.id_objekt_teil = " + piId.ToString() + lsWhereAdd2;
                }

                if (piArt == 123)   // Mieter
                {
                    lsSql = @"select id_vz,
                        objekt.bez as objbez,
                        objekt_teil.bez as obtbez,
                        mieter.bez as mbez,
                        datum_von as von,
                        datum_bis as bis,
                        betrag_netto netto,
                        betrag_brutto brutto, 
                        betrag_netto_soll snetto,
                        betrag_brutto_soll sbrutto, 
                        id_extern_timeline,
                        flag_timeline,
                        art_kostenart.bez as kbez
				from zahlungen
                left join art_kostenart on zahlungen.id_ksa = art_kostenart.id_ksa
                left join objekt on zahlungen.id_objekt = objekt.id_objekt
                left join objekt_teil on zahlungen.id_objekt_teil = objekt_teil.id_objekt_teil
                left join mieter on zahlungen.id_mieter = mieter.id_mieter
				where zahlungen.id_mieter = " + piId.ToString() + lsWhereAdd2;
                }
            }

            // Nur Where für Reports Zählerstände
            if (piArt == 133 || piArt == 134 || piArt == 135)
            {
                lsAnd = " And ";
                lsField = "zaehlerstaende.datum_von";
                liOne = 2;
                lsWhereAdd2 = RdQueriesTime.GetDateQueryResult(adtWtStart, adtWtEnd, ldtStart, ldtEnd, lsField, lsAnd, liOne, aiDb);

                if (piArt == 134)   // Objekte
                {
                    lsSql = @"select id_zs,
                        zaehlerstaende.datum_von as von,
						zaehlerstaende.zs as zs,
						zaehlerstaende.verbrauch as verb,
						zaehlerstaende.id_einheit,
                        zaehlerstaende.preis_einheit_netto as prnetto,
                        zaehlerstaende.preis_einheit_brutto as prbrutto,
						objekt.bez as objbez,
                        objekt_teil.bez as obtbez,
                        zaehlerstaende.id_extern_timeline
				from zaehlerstaende
				left join zaehler on zaehler.Id_zaehler = zaehlerstaende.id_zaehler
				left join objekt on zaehler.id_objekt = objekt.id_objekt
                left join objekt_teil on zaehler.id_objekt_teil = objekt_teil.id_objekt_teil
				where zaehler.id_objekt = " + piId.ToString() + lsWhereAdd2;
                }

                if (piArt == 135)   // ObjektTeile
                {
                    lsSql = @"select id_zs,
                        zaehlerstaende.datum_von as von,
						zaehlerstaende.zs as zs,
						zaehlerstaende.verbrauch as verb,
						zaehlerstaende.id_einheit,
                        zaehlerstaende.preis_einheit_netto,
                        zaehlerstaende.preis_einheit_brutto,
						objekt.bez as objbez,
                        objekt_teil.bez as obtbez,
                        zaehlerstaende.id_extern_timeline
				from zaehlerstaende
				left join zaehler on zaehler.Id_zaehler = zaehlerstaende.id_zaehler
				left join objekt on zaehler.id_objekt = objekt.id_objekt
                left join objekt_teil on zaehler.id_objekt_teil = objekt_teil.id_objekt_teil
				where zaehler.id_objekt_teil = " + piId.ToString() + lsWhereAdd2;
                }

                if (piArt == 133)   // Zähler für Mieter gibt es nicht
                {
                    lsSql = "";
                }
            }

            // Bei Druck des Anschreibens muss die Rechnungsnummer in die Timeline eingesetzt werden
            // Also nur die Tabelle Timeline und die Where Klausel
            if (piArt == 140)
            {
                lsSql = @"timeline.id_rechnung,
				timeline.id_vorauszahlung,
				timeline.id_objekt,
				timeline.id_objekt_teil,
				timeline.id_mieter,
                timeline.id_ksa,
                timeline.id_zaehlerstand,
                timeline.id_rg_nr
            from timeline";

                lsWhereAdd1 = " Where timeline.Id_mieter = " + piId.ToString() + " ";
                // lsSql = lsSql + lsWhereAdd1; // gesamte Klausel
                lsSql = lsWhereAdd1;    // nur Where
                lsAnd = " And ";
                lsWhereAdd4 = lsAnd + " (timeline.id_rechnung > 0 or timeline.id_zaehlerstand > 0) ";     // nur Rechnungen und Zählerstände
                lsField = "timeline.dt_monat";
                liOne = 2;
                lsWhereAdd2 = RdQueriesTime.GetDateQueryResult(adtWtStart, adtWtEnd, ldtStart, ldtEnd, lsField, lsAnd, liOne, aiDb);

                lsSql = lsSql + lsWhereAdd2 + lsWhereAdd4;
                lsSql = lsSql + lsGroup + lsOrder;
            }
            else
            {
                lsAnd = " Where ";
            }
            
            //----------------------------------------------------------------------------------------------------------------
            // Den Header für Reports befüllen
            //----------------------------------------------------------------------------------------------------------------
            if (piArt == 201 || piArt == 202 || piArt == 203)
            {

                // Ddatetimes für das Sql Statement
                DateTime ldtStartTmp = DateTime.MinValue;
                DateTime ldtEndTmp = DateTime.MinValue;

                switch (piArt)
                {
                    case 201:       // Objekt ID übergeben
                        liObjId = piId;
                        ldtStartTmp = adtWtStart;
                        ldtEndTmp = ldtAdd;
                        break;
                    case 202:       // TeilObjekt ID übergeben
                        ldtStartTmp = adtWtStart;
                        ldtEndTmp = adtWtEnd;
                        liObjTeilId = piId;
                        liObjId = Timeline.getIdObj(piId, asConnectString, 2, aiDb);
                        break;
                    case 203:       // Mieter Id übergeben
                        ldtStartTmp = adtWtStart;
                        ldtEndTmp = adtWtEnd;
                        liMieterId = piId;
                        liObjTeilId = Timeline.getIdObjTeil(piId, asConnectString, aiDb);
                        liObjId = Timeline.getIdObj(piId, asConnectString, 1, aiDb);
                        break;
                    default:
                        break;
                }
                lsSql = RdQueriesTime.GetAbrInfo(aiFiliale, liObjId, liObjTeilId, liMieterId, ldtStartTmp, ldtEndTmp, piArt, aiDb);
            }

            // Leerstand 
            // SQL für die Timeline Summendarstellung Objekte, TeilObjekte oder Mieter
            // Bei Leerstand wird das Feld Filiale in der Tabelle mieter geschrieben
            if (piArt == 211 || piArt == 212 || piArt == 213)
            {
                lsSql = @"Select                  
                    art_kostenart.bez as ksa_bez,
                    Sum(timeline.betrag_netto) as betrag_netto,
					Sum(timeline.betrag_brutto) as betrag_brutto,
                    Sum(timeline.betrag_soll_netto),
                    Sum(timeline.betrag_soll_brutto),
                    timeline.id_rechnung,
                    timeline.id_vorauszahlung,
                    timeline.wtl_aus_objekt,
                    timeline.wtl_aus_objteil,
                    timeline.id_zaehlerstand                            
                from timeline
                Right Join art_kostenart on timeline.id_ksa = art_kostenart.id_ksa
                Right Join mieter on timeline.id_mieter = mieter.id_mieter
                Left Join objekt_teil on timeline.leerstand = objekt_teil.id_objekt_teil";
                lsGroup = @" Group by art_kostenart.bez,art_kostenart.sort,timeline.id_rechnung,timeline.id_vorauszahlung,
                    timeline.wtl_aus_objekt,timeline.wtl_aus_objteil,timeline.id_zaehlerstand  ";
                lsOrder = " Order by art_kostenart.sort ";
                // Objekt ID
                if (piId > 0)
                {
                    switch (piArt)
                    {
                        case 211:                     // Filiale
                            lsWhereAdd1 = " Where mieter.Id_filiale = " + piId.ToString() + " ";
                            lsWhereAdd2 = " And timeline.leerstand > 0 ";
                            lsSql = lsSql + lsWhereAdd1 + lsWhereAdd2;
                            lsAnd = " And ";
                            break;
                        case 212:                     // Objekt
                            lsWhereAdd1 = " Where objekt_teil.Id_objekt = " + piId.ToString() + " ";
                            lsSql = lsSql + lsWhereAdd1;
                            lsAnd = " And ";
                            break;
                        case 213:                     // TeilObjekt
                            lsWhereAdd1 = " Where timeline.leerstand = " + piId.ToString() + " ";
                            lsSql = lsSql + lsWhereAdd1;
                            lsAnd = " And ";
                            break;
                        default:
                            break;
                    }

                    lsField = "timeline.dt_monat";
                    liOne = 2;
                    lsWhereAdd2 = RdQueriesTime.GetDateQueryResult(adtWtStart, adtWtEnd, ldtStart, ldtEnd, lsField, lsAnd, liOne, aiDb);

                    lsSql = lsSql + lsWhereAdd2;
                    lsSql = lsSql + lsGroup + lsOrder;
                }
                else
                {
                    lsAnd = " Where ";
                }
            }

            //----------------------------------------------------------------------------------------------------------------
            // Das Content Abrechnung für Reports befüllen
            // Es wird nur eine Art benötigt
            //----------------------------------------------------------------------------------------------------------------
            if (piArt == 300)
            {
                // Ddatetimes für das Sql Statement
                DateTime ldtStartTmp = DateTime.MinValue;
                DateTime ldtEndTmp = DateTime.MinValue;

                lsAnd = " And ";
                lsField = "vorrauszahlungen.datum_von";
                liOne = 2;
                lsWhereAdd2 = RdQueriesTime.GetDateQueryResult(adtWtStart, adtWtEnd, ldtStart, ldtEnd, lsField, lsAnd, liOne, aiDb);

                lsSql = @"Select Id_abr_content,
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
                            id_import,        
                            betrag_netto_objt,
                            betrag_brutto_objt,
                            betrag_netto_obj, 
                            betrag_brutto_obj,
                            id_art_verteilung,
                            betrag_rg_netto,
                            betrag_rg_brutto,
                            verteilung,
                            rg_nr,
                            rg_txt,
                            rg_dat,
                            id_rg_nr
                from x_abr_content
                order by id_abr_content";
            }
            return lsSql;
        }
    }
}
