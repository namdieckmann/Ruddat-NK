using System;
using System.Collections;
using System.Collections.Generic;
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
using Microsoft.Reporting.WinForms;
using MySql.Data.MySqlClient;

namespace Ruddat_NK
{
    /// <summary>
    /// Interaktionslogik für WndEmpShow.xaml
    /// </summary>
    public partial class WndRep : Window
    {
        public String gsConnect;
        public String gsFileName;
        public String gsUserName;
        public String gsReportName = "";
        public String gsPath;
        public String gsSql;
        public string gsSqlWhere = "";
        public int giHeaderId;
        public int giDb;

        private MainWindow mainWindow;
        // ConnectString übernehmen
        public string psConnect { get; set; }

        public WndRep(MainWindow mainWindow)
        {
            // TODO: Complete member initialization
            this.mainWindow = mainWindow;
            InitializeComponent();
        }

        // Aus Delegates
        // Welche Datenbank wird verwendet 1=MsSql 2=Sqlite
        public void getDb(int aiDb)
        {
            giDb = aiDb;    // 1= MsSql 2 = MySql
            String lsPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string lsSqlDirekt = "";
            string lsSqlContent = "";
            string lsSqlContent2 = "";
            string lsSqlHeader = "";
            string lsSqlFadr = "";
            string lsSqlZahlungen = "";
            string lsSqlRgNr = "";
            string lsSqlSum = "";
            string lsDatVon = "";
            string lsDatBis = "";
            Int32 liRows = 0;

            gsPath = lsPath;

            // ConnectString global
            gsConnect = this.mainWindow.psConnect;

            // SqlSelect Headerdaten holen
            lsSqlHeader = getSql("", 1, 0);
            // SqlSelect Firmenadresse holen
            lsSqlFadr = getSql("", 2, 0);

            // Sql-Statement holen
            lsSqlDirekt = DbReadSql(lsPath, 1);         // Art 1 = Erstes SQL Statement Rechungen,Zahlungen, Kosten Direkt Tabelle Timeline                
            if (gsReportName == "kosten" || gsReportName == "kostendetail" || gsReportName == "anschreiben")
            {
                lsSqlContent = DbReadSql(lsPath, 2);        // Art 2 = Hauptinhalt der Abrechnung  Tabelle x_abr_content
                lsSqlContent2 = DbReadSql(lsPath, 7);       // Art 7 = Hauptinhalt nur ObjektKosten 
                lsSqlZahlungen = DbReadSql(lsPath, 3);      // Art 3 = Zahlungen SQL Statement aus XML holen
                lsSqlRgNr = DbReadSql(lsPath, 8);           // Art 8 = SQL für Timeline Rechnungsnummern schreiben an fill_content übergeben
                lsSqlSum = DbReadSql(lsPath, 4);            // Art 4 = Summendarstellung Kosten Zahlungen
                lsDatVon = DbReadVal(lsPath, 1);            // Art 1 = Datum Von lesen
                lsDatBis = DbReadVal(lsPath, 2);            // Art 2 = Datum bis lesen
            }
            // Report füllen
            liRows = fetchData(lsSqlDirekt, lsSqlHeader, lsSqlFadr, lsSqlZahlungen, lsSqlSum,
                lsSqlContent, lsSqlContent2, gsReportName, lsDatVon, lsDatBis, lsSqlRgNr, aiDb);
        }

        // Sql Scripts erstellen
        private string getSql(string asArt, int aiArt, int aiInfo)
        {
            string lsSql = "";

            switch (aiArt)
            {
                case 1:         // HeaderInformationen
                    lsSql = @"Select x_abr_info.id_filiale,
								x_abr_info.id_objekt as obj,
		                        x_abr_info.id_objekt_teil as objt,
		                        x_abr_info.id_mieter as mieter,
		                        x_abr_info.abr_dat_von as dvon,
		                        x_abr_info.abr_dat_bis as dbis,
		                        vertrag.datum_von as vvon,
		                        vertrag.datum_bis as vbis,
		                        vertrag.anzahl_personen as anzpers,
								adressen.adresse as madr,
		                        adressen.ort as mort,
								adressen.plz as mplz,
								art_adresse.bez as aadr,
								objekt.bez as obez,
                                objekt.flaeche_gesamt as flg,
								objekt_teil.bez as otbez,
                                objekt_teil.flaeche_anteil as fl,
                                objekt_teil.prozent_anteil as pa,
		                        mieter.bez as mbez,
                                mieter.netto,
								filiale.name as fname,
                                objekt_teil.geschoss as otges,
                                objekt_teil.lage as otlage,
                                adressen.vorname as mVorname,
                                adressen.name as mName,
                                adressen.anrede as mAnrede,
                                adressen.firma as mFirma
		                        from x_abr_info
		                        left join mieter on x_abr_info.id_mieter = mieter.id_mieter
		                        left Join vertrag on mieter.Id_Mieter = vertrag.id_mieter
		                        left join adressen on mieter.Id_mieter = adressen.Id_mieter
		                        left join art_adresse on art_adresse.id_art_adresse = adressen.id_art_adresse
								left join objekt on x_abr_info.id_objekt = objekt.Id_objekt
								left join objekt_teil on x_abr_info.id_objekt_teil = objekt_teil.Id_objekt_teil
								left join filiale on x_abr_info.Id_filiale = filiale.Id_Filiale
                                Where adressen.aktiv = 1";
                    break;
                case 2:         // Adresse Filiale
                    lsSql = @"select x_abr_info.id_filiale,
				                    adressen.adresse as fadr,
				                    adressen.plz as fplz,
				                    adressen.ort as fort,
				                    adressen.tel as ftel,
									filiale.name as finame,
                                    adressen.firma as ffiname,
                                    adressen.anrede as fanrede,
                                    adressen.vorname fvorname,
                                    adressen.name as fname,
                                    adressen.mail as fmail,
                                    adressen.mobil as fmobil,
                                    adressen.homepage as fpage
		                        from x_abr_info
								left join filiale on x_abr_info.id_filiale = filiale.Id_Filiale
				                left join adressen on x_abr_info.Id_filiale = adressen.id_filiale";
                    break;
                case 3:         // Abrechnung aus x_abr_content für Darstellung
                    lsSql = @"Select x_abr_content.id_ksa,          
                                x_abr_content.betrag_rg_netto as rgn,
                                x_abr_content.betrag_rg_brutto as rgb,
                                x_abr_content.betrag_netto_obj as obn,
                                x_abr_content.betrag_brutto_obj as obb,
                                x_abr_content.id_art_verteilung as vert,
                                x_abr_content.wtl_aus_objekt as wtlobj,
                                x_abr_content.wtl_aus_objteil as wtlobjt,
                                x_abr_content.betrag_netto_objt as otn,
                                x_abr_content.betrag_brutto_objt as otb,
                                x_abr_content.betrag_netto as nt,
                                x_abr_content.betrag_brutto as bt,
                                art_kostenart.bez as kabez,
                                art_kostenart.sort,
								art_verteilung.bez as vbez,
                                x_abr_content.verteilung,
                                x_abr_content.rg_nr,
                                x_abr_content.rg_txt,
                                x_abr_content.rg_dat,
                                rgnr.rgnr as rg_nr_ansch
                            from x_abr_content
                            left join art_kostenart on x_abr_content.id_ksa = art_kostenart.id_ksa
							left join art_verteilung on x_abr_content.id_art_verteilung = art_verteilung.Id_verteilung
                            left join rgnr on x_abr_content.id_rg_nr = rgnr.id_rg_nr
							order by art_kostenart.sort";
                    break;
             }
            return lsSql;
        }

        // Daten aus der Datenbank holen und zeigen 
        private Int32 fetchData(string asSql, string asSqlHeader, string asSqlFadr, string asSqlZahlungen, string asSqlSumme,
            string asSqlContent, string asSqlContent2, string asReportName, string asDatVon, string asDatBis, string asSqlRgNr, int aiDb)
        {
            int liRows = 0;
            int liOk = 0;

            DataTable tableRep = new DataTable();           // Grid
            DataTable tableHeader = new DataTable();        // Grid
            DataTable tableFadr = new DataTable();          // Grid
            DataTable tableZahlungen = new DataTable();
            DataTable tableSumme = new DataTable();
            DataTable tableContent = new DataTable();
            DataTable tableContentShow = new DataTable();

            string lsSqlContentShow = "";   // Darstellung der Abrechnungsdaten

            if (asSql.Length > 0)
            {
                switch (aiDb)
                {
                    case 1:
                        try
                        {
                            SqlConnection connect;
                            connect = new SqlConnection(gsConnect);
                            // Db open
                            connect.Open();

                            // Report
                            SqlCommand command = new SqlCommand(asSql, connect);
                            // Create a SqlDataReader
                            SqlDataReader queryCommandReader = command.ExecuteReader();
                            // Create a DataTable object to hold all the data returned by the query.
                            tableRep.Load(queryCommandReader);
                            liRows = tableRep.Rows.Count;

                            // Header für Report
                            SqlCommand command2 = new SqlCommand(asSqlHeader, connect);
                            // Create a SqlDataReader
                            SqlDataReader queryCommandReader2 = command2.ExecuteReader();
                            // Create a DataTable object to hold all the data returned by the query.
                            tableHeader.Load(queryCommandReader2);

                            // Firmenadresse für Report
                            SqlCommand command3 = new SqlCommand(asSqlFadr, connect);
                            // Create a SqlDataReader
                            SqlDataReader queryCommandReader3 = command3.ExecuteReader();
                            // Create a DataTable object to hold all the data returned by the query.
                            tableFadr.Load(queryCommandReader3);

                            if (asSqlZahlungen.Length > 0)
                            {
                                // DataSet für Zahlungen
                                SqlCommand command4 = new SqlCommand(asSqlZahlungen, connect);
                                // Create a SqlDataReader
                                SqlDataReader queryCommandReader4 = command4.ExecuteReader();
                                // Create a DataTable object to hold all the data returned by the query.
                                tableZahlungen.Load(queryCommandReader4);
                            }
                            if (asSqlSumme.Length > 0)
                            {
                                // DataSet für Zahlungen
                                SqlCommand command5 = new SqlCommand(asSqlSumme, connect);
                                // Create a SqlDataReader
                                SqlDataReader queryCommandReader5 = command5.ExecuteReader();
                                // Create a DataTable object to hold all the data returned by the query.
                                tableSumme.Load(queryCommandReader5);
                            }

                            if (asSqlContent.Length > 0)
                            {
                                // DataSet für Inhalt Abrechnungen aus x_abr_content
                                SqlCommand command6 = new SqlCommand(asSqlContent, connect);
                                // Create a SqlDataReader
                                SqlDataReader queryCommandReader6 = command6.ExecuteReader();
                                // Create a DataTable object to hold all the data returned by the query.
                                tableContent.Load(queryCommandReader6);
                            }

                            liRows = 1;
                            if (liRows > 0)
                            {
                                if (asReportName == "rechnungen")
                                {
                                    this.Title = "Report Rechnungen";
                                    // Report befüllen
                                    RepView.Reset();
                                    ReportDataSource rds = new ReportDataSource("DataSet1", tableRep);
                                    ReportDataSource rdsHd = new ReportDataSource("DataSetHeader", tableHeader);
                                    ReportDataSource rdsFa = new ReportDataSource("DataSetFadr", tableFadr);
                                    RepView.LocalReport.DataSources.Add(rds);
                                    RepView.LocalReport.DataSources.Add(rdsHd);
                                    RepView.LocalReport.DataSources.Add(rdsFa);
                                    RepView.LocalReport.ReportEmbeddedResource = "Ruddat_NK.ReportRechnungen.rdlc";
                                    // RepView.
                                    RepView.RefreshReport();
                                }
                                if (asReportName == "zahlungen")
                                {
                                    this.Title = "Report Zahlungen";
                                    // Report befüllen
                                    RepView.Reset();
                                    ReportDataSource rds = new ReportDataSource("DataSet1", tableRep);
                                    ReportDataSource rdsHd = new ReportDataSource("DataSetHeader", tableHeader);
                                    ReportDataSource rdsFa = new ReportDataSource("DataSetFadr", tableFadr);
                                    RepView.LocalReport.DataSources.Add(rds);
                                    RepView.LocalReport.DataSources.Add(rdsHd);
                                    RepView.LocalReport.DataSources.Add(rdsFa);
                                    RepView.LocalReport.ReportEmbeddedResource = "Ruddat_NK.ReportZahlungen.rdlc";
                                    RepView.RefreshReport();
                                }
                                if (asReportName == "kosten")  // Nebenkostenabrecnung
                                {
                                    // Die Tabelle x_abr_content muss befüllt werden
                                    liOk = Timeline.fill_content(asSql, asSqlContent, asSqlContent2, asDatVon, asDatBis, gsConnect, "", 0);
                                    // Dann die Tabelle laden 
                                    // Hauptcontent für Abrechnung holen
                                    lsSqlContentShow = getSql("", 3, 0);

                                    if (asSqlContent.Length > 0)
                                    {
                                        // DataSet für Inhalt Abrechnungen aus x_abr_content
                                        SqlCommand command7 = new SqlCommand(lsSqlContentShow, connect);
                                        // Create a SqlDataReader
                                        SqlDataReader queryCommandReader7 = command7.ExecuteReader();
                                        // Create a DataTable object to hold all the data returned by the query.
                                        tableContentShow.Load(queryCommandReader7);

                                        this.Title = "Report Kosten";
                                        // Report befüllen
                                        RepView.Reset();
                                        ReportDataSource rds = new ReportDataSource("DataSet1", tableRep);
                                        ReportDataSource rdsHd = new ReportDataSource("DataSet2", tableHeader);
                                        ReportDataSource rdsFa = new ReportDataSource("DataSet3", tableFadr);
                                        ReportDataSource rdsZlg = new ReportDataSource("DataSet4", tableZahlungen);   // Im Report Dataset Zahlungen verwenden
                                        ReportDataSource rdsSum = new ReportDataSource("DataSet5", tableSumme);       // Im Report Dataset Zahlungen verwenden
                                        ReportDataSource rdsCon = new ReportDataSource("DataSet6", tableContentShow); // Content
                                        RepView.LocalReport.DataSources.Add(rds);
                                        RepView.LocalReport.DataSources.Add(rdsHd);
                                        RepView.LocalReport.DataSources.Add(rdsFa);
                                        RepView.LocalReport.DataSources.Add(rdsZlg);
                                        RepView.LocalReport.DataSources.Add(rdsSum);
                                        RepView.LocalReport.DataSources.Add(rdsCon);
                                        RepView.LocalReport.ReportEmbeddedResource = "Ruddat_NK.ReportAbrechnung.rdlc";
                                        RepView.RefreshReport();
                                    }
                                }
                                if (asReportName == "kostendetail")  // Nebenkostenabrecnung detailliert
                                {
                                    // Die Tabelle x_abr_content muss befüllt werden
                                    liOk = Timeline.fill_content(asSql, asSqlContent, asSqlContent2, asDatVon, asDatBis, gsConnect, "", 0);
                                    // Dann die Tabelle laden 
                                    // Hauptcontent für Abrechnung holen
                                    lsSqlContentShow = getSql("", 3, 0);

                                    if (asSqlContent.Length > 0)
                                    {
                                        // DataSet für Inhalt Abrechnungen aus x_abr_content
                                        SqlCommand command7 = new SqlCommand(lsSqlContentShow, connect);
                                        // Create a SqlDataReader
                                        SqlDataReader queryCommandReader7 = command7.ExecuteReader();
                                        // Create a DataTable object to hold all the data returned by the query.
                                        tableContentShow.Load(queryCommandReader7);

                                        this.Title = "Report Kosten";
                                        // Report befüllen
                                        RepView.Reset();
                                        ReportDataSource rds = new ReportDataSource("DataSet1", tableRep);
                                        ReportDataSource rdsHd = new ReportDataSource("DataSet2", tableHeader);
                                        ReportDataSource rdsFa = new ReportDataSource("DataSet3", tableFadr);
                                        ReportDataSource rdsZlg = new ReportDataSource("DataSet4", tableZahlungen);   // Im Report Dataset Zahlungen verwenden
                                        ReportDataSource rdsSum = new ReportDataSource("DataSet5", tableSumme);       // Im Report Dataset Zahlungen verwenden
                                        ReportDataSource rdsCon = new ReportDataSource("DataSet6", tableContentShow); // Content
                                        RepView.LocalReport.DataSources.Add(rds);
                                        RepView.LocalReport.DataSources.Add(rdsHd);
                                        RepView.LocalReport.DataSources.Add(rdsFa);
                                        RepView.LocalReport.DataSources.Add(rdsZlg);
                                        RepView.LocalReport.DataSources.Add(rdsSum);
                                        RepView.LocalReport.DataSources.Add(rdsCon);
                                        RepView.LocalReport.ReportEmbeddedResource = "Ruddat_NK.ReportAbrechnungdetailliert.rdlc";
                                        RepView.RefreshReport();
                                    }
                                }
                                if (asReportName == "anschreiben")  // Anschreiben
                                {

                                    // Die Tabelle x_abr_content muss befüllt werden
                                    liOk = Timeline.fill_content(asSql, asSqlContent, asSqlContent2, asDatVon, asDatBis, gsConnect, asSqlRgNr, 1);
                                    // Dann die Tabelle laden 
                                    // Hauptcontent für Abrechnung holen
                                    lsSqlContentShow = getSql("", 3, 0);

                                    if (asSqlContent.Length > 0)
                                    {
                                        // DataSet für Inhalt Abrechnungen aus x_abr_content
                                        SqlCommand command7 = new SqlCommand(lsSqlContentShow, connect);
                                        // Create a SqlDataReader
                                        SqlDataReader queryCommandReader7 = command7.ExecuteReader();
                                        // Create a DataTable object to hold all the data returned by the query.
                                        tableContentShow.Load(queryCommandReader7);

                                        this.Title = "Report Kosten";
                                        // Report befüllen
                                        RepView.Reset();
                                        ReportDataSource rds = new ReportDataSource("DataSet1", tableRep);
                                        ReportDataSource rdsHd = new ReportDataSource("DataSet2", tableHeader);
                                        ReportDataSource rdsFa = new ReportDataSource("DataSet3", tableFadr);
                                        ReportDataSource rdsZlg = new ReportDataSource("DataSet4", tableZahlungen);   // Im Report Dataset Zahlungen verwenden
                                        ReportDataSource rdsSum = new ReportDataSource("DataSet5", tableSumme);       // Im Report Dataset Zahlungen verwenden
                                        ReportDataSource rdsCon = new ReportDataSource("DataSet6", tableContentShow); // Content
                                        RepView.LocalReport.DataSources.Add(rds);
                                        RepView.LocalReport.DataSources.Add(rdsHd);
                                        RepView.LocalReport.DataSources.Add(rdsFa);
                                        RepView.LocalReport.DataSources.Add(rdsZlg);
                                        RepView.LocalReport.DataSources.Add(rdsSum);
                                        RepView.LocalReport.DataSources.Add(rdsCon);
                                        RepView.LocalReport.ReportEmbeddedResource = "Ruddat_NK.ReportAnschreiben.rdlc";
                                        RepView.RefreshReport();
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("Es wurde kein Objekt angewählt oder es sind keine Daten vorhanden", "Keine Daten");
                            }
                        }
                        catch
                        {
                            // Die Anwendung anhalten
                            MessageBox.Show("Verarbeitungsfehler Error WndRep.F01\n" +
                                    "Achtung");
                        }
                        break;
                    case 2:
                        try
                        {
                            MySqlConnection connect;
                            connect = new MySqlConnection(gsConnect);
                            // Db open
                            connect.Open();

                            // Report
                            MySqlCommand command = new MySqlCommand(asSql, connect);
                            // Create a MySqlDataReader
                            MySqlDataReader queryCommandReader = command.ExecuteReader();
                            // Create a DataTable object to hold all the data returned by the query.
                            tableRep.Load(queryCommandReader);
                            liRows = tableRep.Rows.Count;

                            // Header für Report
                            MySqlCommand command2 = new MySqlCommand(asSqlHeader, connect);
                            // Create a MySqlDataReader
                            MySqlDataReader queryCommandReader2 = command2.ExecuteReader();
                            // Create a DataTable object to hold all the data returned by the query.
                            tableHeader.Load(queryCommandReader2);

                            // Firmenadresse für Report
                            MySqlCommand command3 = new MySqlCommand(asSqlFadr, connect);
                            // Create a MySqlDataReader
                            MySqlDataReader queryCommandReader3 = command3.ExecuteReader();
                            // Create a DataTable object to hold all the data returned by the query.
                            tableFadr.Load(queryCommandReader3);

                            if (asSqlZahlungen.Length > 0)
                            {
                                // DataSet für Zahlungen
                                MySqlCommand command4 = new MySqlCommand(asSqlZahlungen, connect);
                                // Create a MySqlDataReader
                                MySqlDataReader queryCommandReader4 = command4.ExecuteReader();
                                // Create a DataTable object to hold all the data returned by the query.
                                tableZahlungen.Load(queryCommandReader4);
                            }
                            if (asSqlSumme.Length > 0)
                            {
                                // DataSet für Zahlungen
                                MySqlCommand command5 = new MySqlCommand(asSqlSumme, connect);
                                // Create a MySqlDataReader
                                MySqlDataReader queryCommandReader5 = command5.ExecuteReader();
                                // Create a DataTable object to hold all the data returned by the query.
                                tableSumme.Load(queryCommandReader5);
                            }

                            if (asSqlContent.Length > 0)
                            {
                                // DataSet für Inhalt Abrechnungen aus x_abr_content
                                MySqlCommand command6 = new MySqlCommand(asSqlContent, connect);
                                // Create a MySqlDataReader
                                MySqlDataReader queryCommandReader6 = command6.ExecuteReader();
                                // Create a DataTable object to hold all the data returned by the query.
                                tableContent.Load(queryCommandReader6);
                            }

                            liRows = 1;
                            if (liRows > 0)
                            {
                                if (asReportName == "rechnungen")
                                {
                                    this.Title = "Report Rechnungen";
                                    // Report befüllen
                                    RepView.Reset();
                                    ReportDataSource rds = new ReportDataSource("DataSet1", tableRep);
                                    ReportDataSource rdsHd = new ReportDataSource("DataSetHeader", tableHeader);
                                    ReportDataSource rdsFa = new ReportDataSource("DataSetFadr", tableFadr);
                                    RepView.LocalReport.DataSources.Add(rds);
                                    RepView.LocalReport.DataSources.Add(rdsHd);
                                    RepView.LocalReport.DataSources.Add(rdsFa);
                                    RepView.LocalReport.ReportEmbeddedResource = "Ruddat_NK.ReportRechnungen.rdlc";
                                    // RepView.
                                    RepView.RefreshReport();
                                }
                                if (asReportName == "zahlungen")
                                {
                                    this.Title = "Report Zahlungen";
                                    // Report befüllen
                                    RepView.Reset();
                                    ReportDataSource rds = new ReportDataSource("DataSet1", tableRep);
                                    ReportDataSource rdsHd = new ReportDataSource("DataSetHeader", tableHeader);
                                    ReportDataSource rdsFa = new ReportDataSource("DataSetFadr", tableFadr);
                                    RepView.LocalReport.DataSources.Add(rds);
                                    RepView.LocalReport.DataSources.Add(rdsHd);
                                    RepView.LocalReport.DataSources.Add(rdsFa);
                                    RepView.LocalReport.ReportEmbeddedResource = "Ruddat_NK.ReportZahlungen.rdlc";
                                    RepView.RefreshReport();
                                }
                                if (asReportName == "kosten")  // Nebenkostenabrecnung
                                {
                                    // Die Tabelle x_abr_content muss befüllt werden
                                    liOk = Timeline.fill_content(asSql, asSqlContent, asSqlContent2, asDatVon, asDatBis, gsConnect, "", 0);
                                    // Dann die Tabelle laden 
                                    // Hauptcontent für Abrechnung holen
                                    lsSqlContentShow = getSql("", 3, 0);

                                    if (asSqlContent.Length > 0)
                                    {
                                        // DataSet für Inhalt Abrechnungen aus x_abr_content
                                        MySqlCommand command7 = new MySqlCommand(lsSqlContentShow, connect);
                                        // Create a SqlDataReader
                                        MySqlDataReader queryCommandReader7 = command7.ExecuteReader();
                                        // Create a DataTable object to hold all the data returned by the query.
                                        tableContentShow.Load(queryCommandReader7);

                                        this.Title = "Report Kosten";
                                        // Report befüllen
                                        RepView.Reset();
                                        ReportDataSource rds = new ReportDataSource("DataSet1", tableRep);
                                        ReportDataSource rdsHd = new ReportDataSource("DataSet2", tableHeader);
                                        ReportDataSource rdsFa = new ReportDataSource("DataSet3", tableFadr);
                                        ReportDataSource rdsZlg = new ReportDataSource("DataSet4", tableZahlungen);   // Im Report Dataset Zahlungen verwenden
                                        ReportDataSource rdsSum = new ReportDataSource("DataSet5", tableSumme);       // Im Report Dataset Zahlungen verwenden
                                        ReportDataSource rdsCon = new ReportDataSource("DataSet6", tableContentShow); // Content
                                        RepView.LocalReport.DataSources.Add(rds);
                                        RepView.LocalReport.DataSources.Add(rdsHd);
                                        RepView.LocalReport.DataSources.Add(rdsFa);
                                        RepView.LocalReport.DataSources.Add(rdsZlg);
                                        RepView.LocalReport.DataSources.Add(rdsSum);
                                        RepView.LocalReport.DataSources.Add(rdsCon);
                                        RepView.LocalReport.ReportEmbeddedResource = "Ruddat_NK.ReportAbrechnung.rdlc";
                                        RepView.RefreshReport();
                                    }
                                }
                                if (asReportName == "kostendetail")  // Nebenkostenabrecnung detailliert
                                {
                                    // Die Tabelle x_abr_content muss befüllt werden
                                    liOk = Timeline.fill_content(asSql, asSqlContent, asSqlContent2, asDatVon, asDatBis, gsConnect, "", 0);
                                    // Dann die Tabelle laden 
                                    // Hauptcontent für Abrechnung holen
                                    lsSqlContentShow = getSql("", 3, 0);

                                    if (asSqlContent.Length > 0)
                                    {
                                        // DataSet für Inhalt Abrechnungen aus x_abr_content
                                        MySqlCommand command7 = new MySqlCommand(lsSqlContentShow, connect);
                                        // Create a SqlDataReader
                                        MySqlDataReader queryCommandReader7 = command7.ExecuteReader();
                                        // Create a DataTable object to hold all the data returned by the query.
                                        tableContentShow.Load(queryCommandReader7);

                                        this.Title = "Report Kosten";
                                        // Report befüllen
                                        RepView.Reset();
                                        ReportDataSource rds = new ReportDataSource("DataSet1", tableRep);
                                        ReportDataSource rdsHd = new ReportDataSource("DataSet2", tableHeader);
                                        ReportDataSource rdsFa = new ReportDataSource("DataSet3", tableFadr);
                                        ReportDataSource rdsZlg = new ReportDataSource("DataSet4", tableZahlungen);   // Im Report Dataset Zahlungen verwenden
                                        ReportDataSource rdsSum = new ReportDataSource("DataSet5", tableSumme);       // Im Report Dataset Zahlungen verwenden
                                        ReportDataSource rdsCon = new ReportDataSource("DataSet6", tableContentShow); // Content
                                        RepView.LocalReport.DataSources.Add(rds);
                                        RepView.LocalReport.DataSources.Add(rdsHd);
                                        RepView.LocalReport.DataSources.Add(rdsFa);
                                        RepView.LocalReport.DataSources.Add(rdsZlg);
                                        RepView.LocalReport.DataSources.Add(rdsSum);
                                        RepView.LocalReport.DataSources.Add(rdsCon);
                                        RepView.LocalReport.ReportEmbeddedResource = "Ruddat_NK.ReportAbrechnungdetailliert.rdlc";
                                        RepView.RefreshReport();
                                    }
                                }
                                if (asReportName == "anschreiben")  // Anschreiben
                                {

                                    // Die Tabelle x_abr_content muss befüllt werden
                                    liOk = Timeline.fill_content(asSql, asSqlContent, asSqlContent2, asDatVon, asDatBis, gsConnect, asSqlRgNr, 1);
                                    // Dann die Tabelle laden 
                                    // Hauptcontent für Abrechnung holen
                                    lsSqlContentShow = getSql("", 3, 0);

                                    if (asSqlContent.Length > 0)
                                    {
                                        // DataSet für Inhalt Abrechnungen aus x_abr_content
                                        MySqlCommand command7 = new MySqlCommand(lsSqlContentShow, connect);
                                        // Create a SqlDataReader
                                        MySqlDataReader queryCommandReader7 = command7.ExecuteReader();
                                        // Create a DataTable object to hold all the data returned by the query.
                                        tableContentShow.Load(queryCommandReader7);

                                        this.Title = "Report Kosten";
                                        // Report befüllen
                                        RepView.Reset();
                                        ReportDataSource rds = new ReportDataSource("DataSet1", tableRep);
                                        ReportDataSource rdsHd = new ReportDataSource("DataSet2", tableHeader);
                                        ReportDataSource rdsFa = new ReportDataSource("DataSet3", tableFadr);
                                        ReportDataSource rdsZlg = new ReportDataSource("DataSet4", tableZahlungen);   // Im Report Dataset Zahlungen verwenden
                                        ReportDataSource rdsSum = new ReportDataSource("DataSet5", tableSumme);       // Im Report Dataset Zahlungen verwenden
                                        ReportDataSource rdsCon = new ReportDataSource("DataSet6", tableContentShow); // Content
                                        RepView.LocalReport.DataSources.Add(rds);
                                        RepView.LocalReport.DataSources.Add(rdsHd);
                                        RepView.LocalReport.DataSources.Add(rdsFa);
                                        RepView.LocalReport.DataSources.Add(rdsZlg);
                                        RepView.LocalReport.DataSources.Add(rdsSum);
                                        RepView.LocalReport.DataSources.Add(rdsCon);
                                        RepView.LocalReport.ReportEmbeddedResource = "Ruddat_NK.ReportAnschreiben.rdlc";
                                        RepView.RefreshReport();
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("Es wurde kein Objekt angewählt oder es sind keine Daten vorhanden", "Keine Daten");
                            }
                        }
                        catch
                        {
                            // Die Anwendung anhalten
                            MessageBox.Show("Verarbeitungsfehler Error WndRep.F01\n" +
                                    "Achtung");
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                MessageBox.Show("Es wurde kein Objekt angewählt", "Eingabe fehlt");
            }
            return liRows;
        }
        
        // Sql Statement und die Reportart unter TeilSqlArt (Kosten, Abrechnung, Zähler)aus der XML Datei lesen
        private string DbReadSql(string asPath, int aiArt)
        {
            string lsSqlWhere = "";
            String PDataPath = asPath + "\\Ruddat\\Nebenkosten";
            String PDataPathFile = "";

            if (File.Exists(PDataPath + "ruddat_sql.xml"))
            {
                PDataPathFile = PDataPath + "ruddat_sql.xml";

                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(PDataPathFile);

                switch (aiArt)
                {
                    case 1: 
                        // Letztes SQl Statement    Direktes Statement Rechnungen Zahlungen oder direkte Kosten
                        XmlNode xmlmarker1 = xmldoc.SelectSingleNode("/Root/LastSqlDirekt");
                        if (xmlmarker1.InnerText != null)
                        {
                            lsSqlWhere = xmlmarker1.InnerText;
                        }
                        break;
                    case 2:
                        // Letztes SQl Statement 2  Content Nebenkostenabrechung
                        XmlNode xmlmarker2 = xmldoc.SelectSingleNode("/Root/LastSqlContent");
                        if (xmlmarker2 != null)
                        {
                            lsSqlWhere = xmlmarker2.InnerText; 
                        }
                        break;
                    case 3:
                        // Letztes SQl Statement 3  Zahlungen für Kostenverteilung
                        XmlNode xmlmarker3 = xmldoc.SelectSingleNode("/Root/LastSqlZahlungen");
                        if (xmlmarker3 != null)
                        {
                            lsSqlWhere = xmlmarker3.InnerText;
                        }
                        break;
                    case 4:
                        // Letztes SQl Statement 4  Zahlungen für Kostenverteilung
                        XmlNode xmlmarker4 = xmldoc.SelectSingleNode("/Root/LastSqlSumme");
                        if (xmlmarker4 != null)
                        {
                            lsSqlWhere = xmlmarker4.InnerText;
                        }
                        break;
                    case 5:
                        // Letztes SQl Statement 5  Zahlungen für Kostenverteilung
                        XmlNode xmlmarker5 = xmldoc.SelectSingleNode("/Root/LastSqlContSumObj");
                        if (xmlmarker5 != null)
                        {
                            lsSqlWhere = xmlmarker5.InnerText;
                        }
                        break;
                    case 6:
                        // Letztes SQl Statement 6  Zahlungen für Kostenverteilung
                        XmlNode xmlmarker6 = xmldoc.SelectSingleNode("/Root/LastSqlContSumObjt");
                        if (xmlmarker6 != null)
                        {
                            lsSqlWhere = xmlmarker6.InnerText;
                        }
                        break;
                    case 7:
                        // Letztes SQl Statement 7  ObjektKosten Zähler für Kostenverteilung
                        XmlNode xmlmarker7 = xmldoc.SelectSingleNode("/Root/LastSqlContent2");
                        if (xmlmarker7 != null)
                        {
                            lsSqlWhere = xmlmarker7.InnerText;
                        }
                        break;
                    case 8:
                        // Letztes SQl Statement 8  Rechnungsnummer für Anschreiben
                        XmlNode xmlmarker8 = xmldoc.SelectSingleNode("/Root/LastSqlRgNr");
                        if (xmlmarker8 != null)
                        {
                            lsSqlWhere = xmlmarker8.InnerText;
                        }
                        break;

                    default:
                        break;
                }

                // Welcher Report wurde gwählt
                XmlNode xmlmarker = xmldoc.SelectSingleNode("/Root/Report");
                gsReportName = xmlmarker.InnerText;
            }
            return (lsSqlWhere);
        }


        // Datum aus XML Datei lesen
        private string DbReadVal(string asPath, int aiArt)
        {
            string lsDatum = "";
            String PDataPath = asPath + "\\Ruddat\\Nebenkosten";
            String PDataPathFile = "";

            if (File.Exists(PDataPath + "ruddat_val.xml"))
            {
                PDataPathFile = PDataPath + "ruddat_val.xml";

                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(PDataPathFile);

                switch (aiArt)
                {
                    case 1:
                        // Letztes SQl Statement    Direktes Statement Rechnungen Zahlungen oder direkte Kosten
                        XmlNode xmlmarker1 = xmldoc.SelectSingleNode("/Root/DatumVon");
                        if (xmlmarker1.InnerText != null)
                        {
                            lsDatum = xmlmarker1.InnerText;
                        }
                        break;
                    case 2:
                        // Letztes SQl Statement 2  Content Nebenkostenabrechung
                        XmlNode xmlmarker2 = xmldoc.SelectSingleNode("/Root/DatumBis");
                        if (xmlmarker2 != null)
                        {
                            lsDatum = xmlmarker2.InnerText;
                        }
                        break;
                    default:
                        break;
                }
            }
            return (lsDatum);
        }

        // Ausgang
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}