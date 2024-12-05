using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
// using System.Windows.Forms;
using System.Xml;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Windows.Forms.LinkLabel;


namespace Ruddat_NK
{
    // Todo verhindern, dass Objekte usw. gelöscht weden

    public partial class MainWindow : Window
    {
        // Global
        private string gsPath = "";                 // DataPath des xml
        String gsItemHeader = "";           // Gewähltes Item aus dem Treeview
        private string gsConnect = "";
        private int giMandantId = 0;                  // Mandant        
        private int giFiliale = 0;                  // Angewählte Firma (Aus xml Konfig, den letzten Wert holen)
        private int GiObjektId = 0;                   // Objekt global
        private int GiObjektTeilId = 0;               // Objektteil global
        private int GiMieterId = 0;                   // Mieter global
        private int giDelId = 0;                    // Rechnungsdatensatz löschen
        private int giDelZlId = 0;                  // Zahlungsdatensatz löschen
        private int giDelZlWertId = 0;              // Zählerwert löschen
        private int giZlId = 0;                     // Zähler Id
        private int GiRechnungId = 0;               // TimelineId für löschen
        private int GiFlagTimeline = 0;             // Flag TimeLinebearbeitung
        private int giIndex = 0;                    // Index > Objekt, Teil oder Mieter 1,2,3
        private int giMwstSatz = 99;                // Mwst Satz ! Null > 0 gibs ja
        private int giMwstSatzZl = 99;              // Für Zähler
        private int giDb = 2;                       // Datenbank 1 = MsqSql 2= Mysql
        private DateTime gdtZahlung = DateTime.MinValue; // Zahlungsdatum aus Datepicker DataGrid Zahlungen
        // private DateTime gdtFrom = DateTime.MinValue;
        // private DateTime gdtTo = DateTime.MinValue;
        private DateTime gdtYear = DateTime.MinValue;

        //Todo PB
        // private readonly DispatcherTimer timer;
        // private int currentValue = 0;

        // Daten
        DataTable TblRechnungen;
        DataTable TblTmlDetail;
        DataTable TblFilialen;
        DataTable TblObjTeilObj;
        DataTable TblZlgKostenart;
        DataTable TblRgMwst;
        DataTable TblTmlSum;
        DataTable TblZahlungen;
        DataTable TblVerteilung;
        DataTable TblAbrechnungInfo;
        DataTable TblLeerstand;
        DataTable TblZlWerte;
        DataTable TblZlNummern;
        SqlDataAdapter SdRechnungen;
        SqlDataAdapter SdTmlDetail;
        SqlDataAdapter SdFilialen;
        SqlDataAdapter sdd;
        SqlDataAdapter AdZlgKostenart;
        SqlDataAdapter SdMwstRechnungen;
        SqlDataAdapter SdTmlSummen;
        SqlDataAdapter SdZahlungen;
        SqlDataAdapter SdVerteilung;
        SqlDataAdapter SdAbrInfo;
        SqlDataAdapter SdLeerstand;
        SqlDataAdapter SdZlWerte;
        SqlDataAdapter SdZlNummern;
        MySqlDataAdapter MySdRechnungen;
        MySqlDataAdapter MySdTmlDetail;
        MySqlDataAdapter MySdFilialen;
        MySqlDataAdapter MySdObjTeilObj;
        MySqlDataAdapter MySdZlgKostArt;
        MySqlDataAdapter MySdRgMwst;
        MySqlDataAdapter MySdTmlSum;
        MySqlDataAdapter MySdZahlungen;
        MySqlDataAdapter MySdVerteilung;
        MySqlDataAdapter MySdAbrInfo;
        MySqlDataAdapter MySdLeerstand;
        MySqlDataAdapter MySdZlWert;
        MySqlDataAdapter MySdZlNummer;

        // Datenübergabe an WndChooseSet
        private delegate void delPassData(int giTimelineId);
        // Übergabe an Reports und Stammdaten
        private delegate void DelPassDataSql(string Sql);
        private delegate void DelPassDataArt(int Art);
        private delegate void DelPassConnect(string Connect);
        private delegate void DelPassShowArt(int show);
        private delegate void DelPassDb(int giDb);

        public MainWindow()
        {
            int liRows = 0;
            String lsSql = "";
            String UPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string lsConnect = "";
            DateTime ldtWtStart = DateTime.MinValue;
            DateTime ldtWtEnd = DateTime.MinValue;
            DateTime ldtFrom = DateTime.MinValue;
            DateTime ldtYear = DateTime.MinValue;
            DateTime ldtTo = DateTime.Today;
            gsPath = UPath;                         // Pfad der Konfigurationsdatei global verfügbar machen
            
            InitializeComponent();

            // DatenbankConnect
            lsConnect = DbConnect(UPath);

            // Menüpunkte
            mnImpRg.IsEnabled = false;

            // Kalender erstmal aus
            clFrom.IsEnabled = false;
            clTo.IsEnabled = false;
            clYear.IsEnabled = false;
            // restliche Checkboxen erstmal aus
            cbObj.IsEnabled = false;
            cbObjTeil.IsEnabled = false;
            cbName.IsEnabled = false;
            // save + del + add Button Rechnungen aus
            btnRgSave.IsEnabled = false;
            btnRgDel.IsEnabled = false;
            btnRgAdd.IsEnabled = false;
            // save + del + add Zufügen Button Zahlungen aus
            btnZlSave.IsEnabled = false;
            btnZlDel.IsEnabled = false;
            btnZlAdd.IsEnabled = false;

            // Radiobutton Aktive Mieter setzen
            rbAktEmps.IsChecked = true;

            // Aktiven Mandanten ermitteln
            giMandantId = Timeline.GetMandantId(lsConnect);

            // Daten für Listbox Filiale holen
            lsSql = RdQueries.GetSqlSelect(1, giMandantId, "", "", "", DateTime.MinValue, DateTime.MinValue, giFiliale, lsConnect, giDb);
            liRows = FetchData(lsSql, 1, giDb, lsConnect);

            // Daten für Treeview holen
            lsSql = RdQueries.GetSqlSelect(2, giFiliale, "", "", "", DateTime.Today, DateTime.Today, giFiliale, lsConnect, giDb);
            liRows = FetchData(lsSql, 2, giDb, lsConnect);

            // Standard ist Jahr -1
            ldtYear = DateTime.Now.AddYears(-2);
            gdtYear = ldtYear;

            ldtFrom = Timeline.GetYear(ldtYear, 1);
            ldtTo = Timeline.GetYear(ldtYear, 2);

            tbDateFrom.Text = ldtFrom.ToString("dd-MM-yyyy HH:mm");
            tbDateTo.Text = ldtTo.ToString("dd-MM-yyyy HH:mm");

            // clFrom.DisplayDate = ldtFrom;
            clFrom.SelectedDate = ldtFrom;
            clFrom.DisplayDate = ldtFrom;
            // gdtFrom = ldtFrom;

            // clTo.DisplayDate = ldtTo;
            clTo.SelectedDate = ldtTo;
            clTo.DisplayDate = ldtTo;
            // gdtTo = ldtTo;

            // Abrechnungsjahr zeigen
            clYear.SelectedDate = ldtYear;
            clYear.DisplayDate = ldtYear;

            Mouse.OverrideCursor = null;
        }

        // Verbindung zur Datenbank
        private string DbConnect(string p)
        {
            string SqlConnectionString = "";
            string MySqlConnectionString = "";
            String PDataPath = p + "\\Ruddat\\Nebenkosten\\";
            String PDataPathFile = "";
            String Server, DbName, Timeout;
            string lsConnect = "";

            // Daten aus xml-Datei lesen
            // Hier wird zweckentfremdet auch einen voreingestellte Gruppenwahl 
            // in die entsprechenden globalen Variablen eingelesen
            // C:\Dokumente und Einstellungen\swbdiec\Lokale Einstellungen\Anwendungsdaten
            if (File.Exists(PDataPath + "ruddat_nk_config.xml"))
            {
                PDataPathFile = PDataPath + "ruddat_nk_config.xml";

                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(PDataPathFile);
                // Datenbankverbindung
                XmlNode xmlmarker = xmldoc.SelectSingleNode("/Konfiguration/Datenbankverbindung/Server");
                Server = xmlmarker.InnerText;
                xmlmarker = xmldoc.SelectSingleNode("/Konfiguration/Datenbankverbindung/Datenbankname");
                DbName = xmlmarker.InnerText;
                //xmlmarker = xmldoc.SelectSingleNode("/Konfiguration/Datenbankverbindung/Trust");
                //Trust = xmlmarker.InnerText;
                xmlmarker = xmldoc.SelectSingleNode("/Konfiguration/Datenbankverbindung/Timeout");
                Timeout = xmlmarker.InnerText;

                // Datenbankconnect zusammenbauen
                switch (giDb)
                {
                    case 1:
                        // SqlConnectionString = Server + DbName + Trust + Timeout;
                        SqlConnectionString = Server + DbName + Timeout;
                        break;
                    case 2:
                        MySqlConnectionString = Server + DbName + Timeout;
                        break;
                    default:
                        break;
                }
                //MessageBox.Show("SqlConnectionString \n" + SqlConnectionString + "\n" +
                //                    "PDataPathFile\n" + PDataPathFile + "\n" +
                //                    "PDataPath\n" + PDataPath,
                //                    "Verbindungsinformationen",
                //                    MessageBoxButton.OK);
            }
            else
            {
                // XML-Datei erzeugen, wenn sie nicht existiert
                try
                {
                    // Verzeichnis anlegen
                    System.IO.Directory.CreateDirectory(PDataPath);

                    XmlTextWriter xmlwriter = new XmlTextWriter(PDataPath + "ruddat_nk_config.xml", null)
                    {
                        Formatting = Formatting.Indented
                    };
                    xmlwriter.WriteStartDocument();
                    xmlwriter.WriteStartElement("Konfiguration");
                    xmlwriter.WriteStartElement("Datenbankverbindung");
                    xmlwriter.WriteStartElement("Server");
                    xmlwriter.WriteString("Data Source=217.160.33.71;PORT=3306;USERID=namdi;PASSWORD=7V7ADTqWqQPCf9Sge4PT;");
                    xmlwriter.WriteEndElement();
                    xmlwriter.WriteStartElement("Datenbankname");
                    xmlwriter.WriteString("database=dbo; ");
                    xmlwriter.WriteEndElement();
                    //xmlwriter.WriteStartElement("Trust");
                    //xmlwriter.WriteString("Integrated Security=True;");
                    //xmlwriter.WriteEndElement();
                    xmlwriter.WriteStartElement("Timeout");
                    xmlwriter.WriteString("Connect Timeout=20 ");
                    xmlwriter.WriteEndElement();
                    xmlwriter.WriteEndElement();
                    xmlwriter.Close();

                    MessageBox.Show("Es wurde eine Standardkonfiguration erzeugt.\n" +
                                    "Die Serververbindung muss noch überprüft werden\n" +
                                    "Die Datei heißt:\n" + PDataPath + "ruddat_nk_config.xml\n",
                                    "Achtung",
                                    MessageBoxButton.OK);
                }
                catch
                {
                    MessageBox.Show("Konfigurationsdatei konnte nicht erzeugt werden", "Achtung",
                                    MessageBoxButton.OK);
                }
            }

            switch (giDb)
            {
                case 1:
                    // Für Testzwewcke Firma lokale Db
                    // SqlConnectionString = "Data Source=(LocalDB)\\v11.0;AttachDbFilename=C:\\Users\\Ulf Dieckmann\\AppData\\Local\\Ruddat\\Nebenkosten\\rdnk.mdf;Integrated Security=True;Connect Timeout=5";
                    // Für Testzwecke Notebook lokale Db
                    // SqlConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\udiec\\AppData\\Local\\Ruddat\\Nebenkosten\\rdnk.mdf;Integrated Security=True;Connect Timeout=5";
                    // Für Testzwecke Server Firma
                    // SqlConnectionString = "Data Source=(LocalDB)\\v11.0;AttachDbFilename=G:\\Software\\Ruddat-Nebenkosten\\DbOne\\rdnk.mdf;Integrated Security=True;Connect Timeout=20";
                    // MessageBox.Show("Lokale Datenbank MsSql Express wird verwendet", "Achtung! ", MessageBoxButton.OK);
                    break;
                case 2:
                    // Lokal MySql 
                    MySqlConnectionString = @"server=localhost;userid=rdnk;password=r1d8n9k4!;database=dbo";
                    // MessageBox.Show("Lokales Login");
                    // Ionos Server 
                    // MySqlConnectionString = @"Data Source=217.160.33.71;PORT=3306;USERID=namdi;PASSWORD=7V7ADTqWqQPCf9Sge4PT;database=dbo;Connect Timeout = 60";
                    // MessageBox.Show("Ionos Datenbank MySql wird verwendet", "Achtung! ", MessageBoxButton.OK);
                    break;
                default:
                    break;
            }
            //Globaler ConnectString
            switch (giDb)
            {
                case 1:
                    lsConnect = SqlConnectionString;
                    gsConnect = SqlConnectionString;
                    giDb = 1;
                    break;
                case 2:
                    lsConnect = MySqlConnectionString;
                    gsConnect = MySqlConnectionString;
                    giDb = 2;
                    break;
                default:
                    break;
            }
            return (lsConnect);
        }

        // Daten aus der Db holen
        private Int32 FetchData(string psSql, int piArt, int aiDb, string asConnect)
        {
            Int32 liRows = 0;
            string lsObjektBez = "", lsObjektTeilBez = "";
            string lsObjektBezS = "";

            try
            {
                MySqlConnection con;
                con = new MySqlConnection(asConnect);
                MySqlCommand com = new MySqlCommand(psSql, con);
                // Db open
                con.Open();

                // Daten für Filiale holen
                if (piArt == 1)
                {
                    TblFilialen = new DataTable();   // Filialen
                    MySdFilialen = new MySqlDataAdapter(com);
                    MySdFilialen.Fill(TblFilialen);
                    lbFiliale.ItemsSource = TblFilialen.DefaultView;
                }

                // Daten für Objekte und Teilobjekte holen ab ins Treeview
                // Für aktive Verträge
                if (piArt == 2)
                {
                    TblObjTeilObj = new DataTable();    // Objekte Teilobjekte
                    MySdObjTeilObj = new MySqlDataAdapter(com);
                    MySdObjTeilObj.Fill(TblObjTeilObj);

                    if (TblObjTeilObj.Rows.Count > 0)
                    {
                        int i = 0;
                        tvMain.Items.Clear();

                        //  Eine Schleife durch die Tabelle, um das Treview zu befüllen
                        for (i = 0; i < TblObjTeilObj.Rows.Count; i++)
                        {
                            lsObjektBez = TblObjTeilObj.Rows[i].ItemArray.GetValue(4).ToString().Trim() + ":" + TblObjTeilObj.Rows[i].ItemArray.GetValue(0).ToString().Trim();
                            lsObjektTeilBez = TblObjTeilObj.Rows[i].ItemArray.GetValue(1).ToString();

                            TreeViewItem root = new TreeViewItem
                            {
                                Header = lsObjektBez
                            };

                            // Nur, wenn ein neues Objekt und Teilobjekt in der Liste steht
                            if (lsObjektBez != lsObjektBezS)
                            {
                                tvMain.Items.Add(root);
                                lsObjektBezS = lsObjektBez;
                            }

                            PopulateTree(i, root, TblObjTeilObj);

                            i++;
                        }
                    }
                    else
                    {
                        tvMain.Items.Clear();
                    }
                }

                // Die Id aus Objekt holen
                if (piArt == 3)
                {
                    TblObjTeilObj = new DataTable();    // Objekte Teilobjekte
                    MySdObjTeilObj = new MySqlDataAdapter(com);
                    MySdObjTeilObj.Fill(TblObjTeilObj);
                    if (TblObjTeilObj.Rows.Count > 0)
                    {
                        liRows = Convert.ToInt16(TblObjTeilObj.Rows[0].ItemArray.GetValue(5).ToString());
                    }
                }
                // Die Id aus Teilobjekt holen
                if (piArt == 4)
                {
                    TblObjTeilObj = new DataTable();    // Objekte Teilobjekte
                    MySdObjTeilObj = new MySqlDataAdapter(com);
                    MySdObjTeilObj.Fill(TblObjTeilObj);
                    if (TblObjTeilObj.Rows.Count > 0)
                    {
                        liRows = Convert.ToInt16(TblObjTeilObj.Rows[0].ItemArray.GetValue(6).ToString());
                    }
                }
                // Die Id aus Mieter holen
                if (piArt == 5)
                {
                    TblObjTeilObj = new DataTable();    // Objekte Teilobjekte
                    MySdObjTeilObj = new MySqlDataAdapter(com);
                    MySdObjTeilObj.Fill(TblObjTeilObj);
                    if (TblObjTeilObj.Rows.Count > 0)
                    {
                        liRows = Convert.ToInt16(TblObjTeilObj.Rows[0].ItemArray.GetValue(7).ToString());
                    }
                }
                // DataGrid Timline Summen
                if (piArt == 8)
                {
                    TblTmlSum = new DataTable();   // Timeline Summen 
                    MySdTmlSum = new MySqlDataAdapter(com);
                    MySdTmlSum.Fill(TblTmlSum);
                    DgrCost.ItemsSource = TblTmlSum.DefaultView;
                    liRows = DgrCost.Items.Count;
                }
                // Datagrid für Rechnungen
                if (piArt == 9)
                {
                    TblRechnungen = new DataTable();     // Rechnungen
                    MySdRechnungen = new MySqlDataAdapter(com);
                    MySdRechnungen.Fill(TblRechnungen);
                    DgrRechnungen.ItemsSource = TblRechnungen.DefaultView;
                    liRows = DgrRechnungen.Items.Count;
                }
                // ListBox Filiale befüllen
                if (piArt == 10)
                {
                    TblFilialen = new DataTable();
                    MySdFilialen = new MySqlDataAdapter(com);
                    MySdFilialen.Fill(TblFilialen);
                    lbFiliale.ItemsSource = TblFilialen.DefaultView;
                }
                // Combobox Kostenart in Rechnungen
                if (piArt == 11)
                {
                    TblZlgKostenart = new DataTable();    // Kostenart
                    MySdZlgKostArt = new MySqlDataAdapter(com);
                    MySdZlgKostArt.Fill(TblZlgKostenart);
                    kostenart.ItemsSource = TblZlgKostenart.DefaultView;
                }
                // Combobox mwst in Rechnungen
                if (piArt == 12)
                {
                    TblRgMwst = new DataTable();     // mwst
                    MySdRgMwst = new MySqlDataAdapter(com);
                    MySdRgMwst.Fill(TblRgMwst);
                    mwst.ItemsSource = TblRgMwst.DefaultView;                // Rechnungen
                }
                // DataGrid Timline Detail
                if (piArt == 13)
                {
                    TblTmlDetail = new DataTable();     // Timeline
                    MySdTmlDetail = new MySqlDataAdapter(com);
                    MySdTmlDetail.Fill(TblTmlDetail);
                    DgrCostDetail.ItemsSource = TblTmlDetail.DefaultView;
                    liRows = DgrCostDetail.Items.Count;
                }
                // DataGrid Zahlungen
                if (piArt == 14)
                {
                    TblZahlungen = new DataTable();     // Zahlungen
                    MySdZahlungen = new MySqlDataAdapter(com);
                    MySdZahlungen.Fill(TblZahlungen);
                    DgrZahlungen.ItemsSource = TblZahlungen.DefaultView;
                    liRows = DgrZahlungen.Items.Count;
                }
                // DataGrid Leerstand Detail
                if (piArt == 19)
                {
                    TblLeerstand = new DataTable();     // Timeline
                    MySdLeerstand = new MySqlDataAdapter(com);
                    MySdLeerstand.Fill(TblLeerstand);
                    DgrLeerDetail.ItemsSource = TblLeerstand.DefaultView;
                    liRows = DgrLeerDetail.Items.Count;
                }
                // Combobox Kostenart in Zahlungen
                if (piArt == 15)
                {
                    TblZlgKostenart = new DataTable();    // Kostenart
                    MySdZlgKostArt = new MySqlDataAdapter(com);
                    MySdZlgKostArt.Fill(TblZlgKostenart);
                    kostenartZlg.ItemsSource = TblZlgKostenart.DefaultView;
                }
                // Combobox Verteilung in Rechnungen und Zähler
                if (piArt == 16)
                {
                    TblVerteilung = new DataTable();    // Verteilung Rechnungen
                    MySdVerteilung = new MySqlDataAdapter(com);
                    MySdVerteilung.Fill(TblVerteilung);
                    kostenvert.ItemsSource = TblVerteilung.DefaultView;
                    kostenvertZl.ItemsSource = TblVerteilung.DefaultView;
                }
                // Tabelle Infos für Abrechnung
                if (piArt == 17)
                {
                    TblAbrechnungInfo = new DataTable();    // Abrechnung
                    MySdAbrInfo = new MySqlDataAdapter(com);
                    MySdAbrInfo.Fill(TblAbrechnungInfo);
                }
                // Tabelle Leerstände
                if (piArt == 18)
                {
                    TblLeerstand = new DataTable();    // Leerstand
                    MySdLeerstand = new MySqlDataAdapter(com);
                    MySdLeerstand.Fill(TblLeerstand);
                    DgrLeer.ItemsSource = TblLeerstand.DefaultView;
                }
                // Tabelle Zählerwerte
                if (piArt == 21)
                {
                    TblZlWerte = new DataTable();    // Zählerwert
                    MySdZlWert = new MySqlDataAdapter(com);
                    MySdZlWert.Fill(TblZlWerte);
                    DgrCounters.ItemsSource = TblZlWerte.DefaultView;
                }
                // Combobox Zählernummern
                if (piArt == 22)
                {
                    TblZlNummern = new DataTable();    // Kostenart
                    MySdZlNummer = new MySqlDataAdapter(com);
                    MySdZlNummer.Fill(TblZlNummern);
                    zlNummer.ItemsSource = TblZlNummern.DefaultView;
                    zleh.ItemsSource = TblZlNummern.DefaultView;
                    zlmw.ItemsSource = TblZlNummern.DefaultView;
                }
                if (piArt == 35)
                {
                    // MySqlDataAdapter mysda = new MySqlDataAdapter(com);
                    MySqlCommandBuilder commandBuilder23 = new MySqlCommandBuilder(MySdRechnungen);
                    MySdRechnungen.Update(TblRechnungen);
                }
                // Rechnung löschen
                if (piArt == 36)
                {
                    // Rechnungen löschen
                    MySqlDataReader queryCommandReader36 = com.ExecuteReader();
                }
                if (piArt == 37)    // Zahlung 
                {
                    // MySqlDataAdapter mysdZlg = new MySqlDataAdapter(com);
                    MySqlCommandBuilder commandBuilder37 = new MySqlCommandBuilder(MySdZahlungen);
                    MySdZahlungen.Update(TblZahlungen);
                }
                if (piArt == 38)
                {
                    // Zahlung löschen
                    MySqlDataReader queryCommandReader = com.ExecuteReader();
                }
                if (piArt == 39)
                {
                    // Zählerstände
                    MySqlCommandBuilder commandBuilder39 = new MySqlCommandBuilder(MySdZlWert);
                    MySdZlWert.Update(TblZlWerte);
                }
                if (piArt == 40)
                {
                    // Zählerstände löschen
                    MySqlDataReader queryCommandReader40 = com.ExecuteReader();
                }
                // db close
                con.Close();
            }
            catch (MySqlException)
            {
                // Die Anwendung anhalten 
                MessageBox.Show("Verarbeitungsfehler ERROR fetchdata main MySQL \n piArt = " + piArt.ToString(),
                            "Achtung");
                throw;
            }



            return (liRows);     // oder Ausnahmsweise die gefundene ID bei art 3-5
        }

        // Teilobjekte Children für TreeView
        public void PopulateTree(int i, TreeViewItem pNode, DataTable dt)
        {
            string lsObjektTeilBez = "";
            string lsObjektTeilBezS = "";
            string lsObjektBez = "";
            string lsObjektBezGet = dt.Rows[i].ItemArray.GetValue(0).ToString();
            // int liVertragAktiv = 0;

            for (int ii = 0; ii < dt.Rows.Count; ii++)
            {
                lsObjektTeilBez = dt.Rows[ii].ItemArray.GetValue(1).ToString();
                lsObjektBez = dt.Rows[ii].ItemArray.GetValue(0).ToString();

                //if (dt.Rows[ii].ItemArray.GetValue(8) != DBNull.Value)
                //    liVertragAktiv = (int)dt.Rows[ii].ItemArray.GetValue(8);

                // && liVertragAktiv == 1

                if (lsObjektBezGet == lsObjektBez)
                {
                    if (lsObjektTeilBez != lsObjektTeilBezS)
                    {
                        TreeViewItem cChild = new TreeViewItem
                        {
                            Header = lsObjektTeilBez
                        };
                        pNode.Items.Add(cChild);
                        lsObjektTeilBezS = lsObjektTeilBez;
                        PopulateTree2(ii, cChild, dt);
                    }
                }
            }
        }

        // Mieter Children für TreeView
        public void PopulateTree2(int i, TreeViewItem pNode, DataTable dt)
        {
            string lsMieter = "";
            // string lsMieterS = "";
            string lsObjektTeilBez = "";
            string lsObjektTeilBezGet = dt.Rows[i].ItemArray.GetValue(1).ToString();
            int liObjTeil = 0;
            int liMieterId = 0;
            int liVertragAktiv = 0;
            DateTime ldtVon = DateTime.Today;

            for (int ii = i; ii < dt.Rows.Count; ii++)
            {
                lsMieter = "Kein Mieter";
                // liVertragAktiv = 0;
                lsObjektTeilBez = "";
                if (dt.Rows[ii].ItemArray.GetValue(1) != DBNull.Value)
                    lsObjektTeilBez = dt.Rows[ii].ItemArray.GetValue(1).ToString();
                if (dt.Rows[ii].ItemArray.GetValue(6) != DBNull.Value)
                    liObjTeil = (int)dt.Rows[ii].ItemArray.GetValue(6);
                if (dt.Rows[ii].ItemArray.GetValue(7) != DBNull.Value)
                    liMieterId = (int)dt.Rows[ii].ItemArray.GetValue(7);
                if (dt.Rows[ii].ItemArray.GetValue(8) != DBNull.Value)
                    liVertragAktiv = (int)dt.Rows[ii].ItemArray.GetValue(8);
                if (dt.Rows[ii].ItemArray.GetValue(2) != DBNull.Value)
                    lsMieter = dt.Rows[ii].ItemArray.GetValue(2).ToString();
                if (rbAktEmps.IsChecked == true)    // nur aktuelle Mieter
                {
                    if (liMieterId != 0 && liVertragAktiv == 1)
                    {
                        if (lsObjektTeilBezGet == lsObjektTeilBez)
                        {
                            TreeViewItem cChild = new TreeViewItem
                            {
                                Header = lsMieter
                            };
                            pNode.Items.Add(cChild);

                            lsObjektTeilBezGet = lsObjektTeilBez;
                        }
                    }
                }

                if (rbAllEmps.IsChecked == true)    // Alle Mieter
                {
                    if (lsObjektTeilBezGet == lsObjektTeilBez)
                    {
                        TreeViewItem cChild = new TreeViewItem
                        {
                            Header = lsMieter
                        };
                        pNode.Items.Add(cChild);

                        lsObjektTeilBezGet = lsObjektTeilBez;
                    }
                }
            }
        }

        // Firma gewechselt
        private void lbFiliale_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int liFiliale = 0;
            int liRows = 0;
            String lsSql = "";
            string lsConnect = "";
            DateTime ldtFrom = DateTime.MinValue;
            DateTime ldtTo = DateTime.MaxValue;
            DateTime ldtFromZaehler = DateTime.MinValue;
            lsConnect = gsConnect;


            if (lbFiliale.SelectedValue != null)
            {
                liFiliale = Convert.ToInt16(lbFiliale.SelectedValue.ToString());
                giFiliale = liFiliale;
            }

            // Start und EndeDatum angegeben
            if (clFrom.SelectedDate != null && clTo.SelectedDate != null)
            {
                ldtFrom = clFrom.SelectedDate.Value;
                ldtTo = clTo.SelectedDate.Value;
                ldtFromZaehler = ldtFrom.AddYears(-1);          //Zähler sollen ein Jahr Vergangenheit zeigen
            }

            if (liFiliale > 0)
            {
                // Treeview befüllen 
                lsSql = RdQueries.GetSqlSelect(2, liFiliale, "", "", "", DateTime.Today, DateTime.Today, giFiliale, gsConnect, giDb);

                // Daten holen 
                liRows = FetchData(lsSql, 2, giDb, lsConnect);                          // Aufruf Art 2 ist Treeview befüllen   

                // Tabelle Leerstand befüllen
                lsSql = RdQueries.GetSqlSelect(211, liFiliale, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
                liRows = FetchData(lsSql, 18, giDb, lsConnect);
            }
        }

        // Programmende
        private void mnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Kalender öffnen
        private void cbCal_Checked(object sender, RoutedEventArgs e)
        {
            clFrom.IsEnabled = true;
            clTo.IsEnabled = true;
            cbCal.Content = "Kalender angewählt";
        }

        // Kalender sperren und Rücksetzen
        private void cbCal_Unchecked(object sender, RoutedEventArgs e)
        {
            DateTime ldtYear = DateTime.MinValue;
            DateTime ldtFrom = DateTime.MinValue;
            DateTime ldtTo = DateTime.MinValue;

            cbCal.Content = "Kalender anwählen";
            clFrom.IsEnabled = false;
            clTo.IsEnabled = false;
            clFrom.SelectedDate = null;
            clTo.SelectedDate = null;

            ldtYear = gdtYear;

            ldtFrom = Timeline.GetYear(ldtYear, 1);
            ldtTo = Timeline.GetYear(ldtYear, 2);

            tbDateFrom.Text = ldtFrom.ToString("dd-MM-yyyy HH:mm");
            tbDateTo.Text = ldtTo.ToString("dd-MM-yyyy HH:mm");

            // clFrom.DisplayDate = ldtFrom;
            clFrom.SelectedDate = ldtFrom;
            clFrom.DisplayDate = ldtFrom;
            // gdtFrom = ldtFrom;

            // clTo.DisplayDate = ldtTo;
            clTo.SelectedDate = ldtTo;
            clTo.DisplayDate = ldtTo;
            // gdtTo = ldtTo;

            // Abrechnungsjahr zeigen
            clYear.SelectedDate = ldtYear;
            clYear.DisplayDate = ldtYear;

            tbDateTo.Text = ldtTo.ToString("dd-MM-yyyy HH:mm");
        }

        // Abrechnungsjahr ein
        private void CbYear_Checked(object sender, RoutedEventArgs e)
        {
            clYear.IsEnabled = true;
        }
        // Abrechnungsjahr aus
        private void CbYear_Unchecked(object sender, RoutedEventArgs e)
        {
            clYear.IsEnabled = false;
        }

        // Datum gewählt Kalender From
        private void clFrom_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime ldtFrom = DateTime.MinValue;
            DateTime ldtTo = DateTime.MinValue;
            DateTime ldtDummy = DateTime.MinValue;
            String lsDateFrom = "";
            int liOk = 0;

            if (clFrom.SelectedDate.HasValue)
            {
                ldtFrom = clFrom.SelectedDate.Value;
                lsDateFrom = ldtFrom.ToString("dd-MM-yyyy HH:mm");
                tbDateFrom.Text = lsDateFrom;
            }

            // Alle DataGrids aktualisieren
            liOk = updateAllDataGrids(0);
        }

        // Datum gewählt Kalender to 
        private void clTo_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime ldtFrom = DateTime.MinValue;
            DateTime ldtTo = DateTime.MinValue;
            DateTime ldtDummy = DateTime.MinValue;
            String lsDateFrom = "";
            String lsDateTo = "";
            int liOk = 0;

            if (clTo.SelectedDate.HasValue && clFrom.SelectedDate.HasValue)
            {
                ldtFrom = clFrom.SelectedDate.Value;
                lsDateFrom = ldtFrom.ToString("dd-MM-yyyy HH:mm");
                tbDateFrom.Text = lsDateFrom;

                ldtTo = clTo.SelectedDate.Value;
                lsDateTo = ldtTo.ToString("dd-MM-yyyy HH:mm");
                tbDateTo.Text = lsDateTo;

                // Alle DataGrids aktualisieren
                liOk = updateAllDataGrids(0);
            }
        }

        // Alle Daten aktualisieren, wenn z.B. ein anderes Datum gewählt wurde
        // Art 1 = mit Filiale 
        // Art 2 = mit Treeview neu
        // Art 3 = SQL Statement für Rechnungen holen
        // Art 4 = SQL Statement für Zahlungen holen holen
        // Art 5 = geplant für die Komplette Abrechnung... mal sehen
        private int updateAllDataGrids(int asArt)
        {
            int liOk = 0;
            int liId = 0;
            int liRows = 0;
            int liIndex = 0;
            int liObjektIdTmp = 0;

            string lsTmp = "";
            string lsSql = "";
            string lsSqlZahlungen = "";
            string lsSqlSumme = "";
            string lsSqlRechnungen = "";
            string lsSqlZaehlerstd = "";        // Todo Wird noch für Report Zählerstände benötigt
            string lsSqlTimeline = "";
            string lsSqlTimeline2 = "";
            string lsSqlTimeline3 = "";         // Für das Einsetzen der Rg Nummer in die Timeline
            string lsSqlHeader = "";
            string lsSqlAbrContent = "";
            string lsSqlRgNrAnschreiben = "";
            string lsSqlLeerstand = "";         // Leerstand für Report
            DateTime ldtFrom = DateTime.MinValue;
            DateTime ldtTo = DateTime.MaxValue;
            DateTime ldtFromZaehler = DateTime.MinValue;

            // nur StartDatum
            if (clFrom.SelectedDate != null)
            {
                ldtFrom = clFrom.SelectedDate.Value;
            }

            // Start und EndeDatum angegeben
            if (clFrom.SelectedDate != null && clTo.SelectedDate != null)
            {
                ldtFrom = clFrom.SelectedDate.Value;
                ldtTo = clTo.SelectedDate.Value;
                ldtFromZaehler = ldtFrom.AddYears(-1);          //Zähler sollen ein Jahr Vergangenheit zeigen
            }

            if (asArt == 1)
            {
                // Daten für die Anwahl der Firma nur nach Filialänderungen durchführen
                // Datum ist egal
                // Daten für listbox Filiale holen
                giMandantId = Timeline.GetMandantId(gsConnect);
                lsSql = RdQueries.GetSqlSelect(1, giMandantId, "", "", "", DateTime.MinValue, DateTime.MinValue, giFiliale, gsConnect, giDb);
                // Daten holen für Listbox Filiale
                liRows = FetchData(lsSql, 1, giDb, gsConnect);
                // Daten für Treeview holen
                lsSql = RdQueries.GetSqlSelect(2, giFiliale, "", "", "", DateTime.Today, DateTime.Today, giFiliale, gsConnect, giDb);
                liRows = FetchData(lsSql, 2, giDb, gsConnect);
            }
            //  Änderung: Anwahl nur aktive Mieter zeigen
            if (asArt == 11)
            {
                // Daten für Treeview holen
                lsSql = RdQueries.GetSqlSelect(2, giFiliale, "", "", "", DateTime.Today, DateTime.Today, giFiliale, gsConnect, giDb);
                liRows = FetchData(lsSql, 2, giDb, gsConnect);
                giIndex = 0;        // Index auf 0 setzen, da ja nix angwählte ist
            }

            // Timeline Detail leeren
            DgrCostDetail.ItemsSource = null;

            // Index aus dem Treeview vorerst nur global
            liIndex = giIndex;

            // Buttons Rechnung, Zahlung und Zähler zufügen öffnen
            if (liIndex >= 0)
            {
                btnRgAdd.IsEnabled = true;
                btnZlAdd.IsEnabled = true;
                btnCntAdd.IsEnabled = true;
            }

            // ID Unabhängige Daten 
            // Combobox Mwst in Rechnungen befüllen Art = 11
            lsSql = RdQueries.GetSqlSelect(12, 0, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
            liRows = FetchData(lsSql, 12, giDb, gsConnect);
            // Combobox Kostenverteilung in Rechnungen befüllen Art = 16
            lsSql = RdQueries.GetSqlSelect(16, 0, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
            liRows = FetchData(lsSql, 16, giDb, gsConnect);
            // Combobox Kostenart in Zahlungen befüllen Art = 11/15 Objekt Kennung 4
            lsSql = RdQueries.GetSqlSelect(11, 4, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
            liRows = FetchData(lsSql, 15, giDb, gsConnect);

            // Die Ebene der TreeViewanwahl
            switch (liIndex)
            {
                case 1:     // Objekt
                    cbObj.IsChecked = true;
                    cbObjTeil.IsChecked = false;
                    cbName.IsChecked = false;
                    // Objekt in Tab Rechungen anzeigen
                    lsTmp = gsItemHeader;
                    // Aus lsTmp wieder den rechten Teil extrahieren Bsp Bremen/Obernstraße steht im Treeview
                    string[] words = lsTmp.Split(':');
                    // In Rechnungen
                    tbObjekt.Text = words[1];
                    tbObjektTeil.Text = "";
                    tbMieter.Text = "";
                    // In Zahlungen
                    tbZlObjekt.Text = words[1];
                    tbZlObjektTeil.Text = "";
                    tbZlMieter.Text = "";
                    // In Zählerständen
                    tbCntObjekt.Text = words[1];
                    tbCntObjektTeil.Text = "";
                    tbCntMieter.Text = "";

                    // Die Objekt ID ermitteln
                    lsSql = RdQueries.GetSqlSelect(3, giFiliale, words[1], "1", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
                    liId = FetchData(lsSql, 3, giDb, gsConnect);

                    // Combobox Kostenart in rechnungen befüllen Art = 11 Objekt Kennung 1
                    lsSql = RdQueries.GetSqlSelect(11, liIndex, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
                    liRows = FetchData(lsSql, 11, giDb, gsConnect);

                    // Combobox Zählernummern und Mwst in Zähler
                    lsSql = RdQueries.GetSqlSelect(22, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
                    liRows = FetchData(lsSql, 22, giDb, gsConnect);

                    // TimeLine holen für Objekte
                    lsSql = RdQueries.GetSqlSelect(5, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
                    liRows = FetchData(lsSql, 8, giDb, gsConnect);
                    lsSqlTimeline = RdQueries.GetSqlSelect(105, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);    // Report

                    // Rechnungen zeigen  Art 8 = Rechungen zeigen für Objekte Datum aktiv
                    lsSql = RdQueries.GetSqlSelect(8, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
                    liRows = FetchData(lsSql, 9, giDb, gsConnect);
                    lsSqlRechnungen = RdQueries.GetSqlSelect(108, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);  // Report

                    // Zahlungen zeigen Art 14 Zahlungen für Objekte
                    lsSql = RdQueries.GetSqlSelect(24, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
                    liRows = FetchData(lsSql, 14, giDb, gsConnect);
                    lsSqlZahlungen = RdQueries.GetSqlSelect(124, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);   // Report

                    // Zählerstände zeigen Art 34 Objekte
                    lsSql = RdQueries.GetSqlSelect(34, liId, "", "", "", ldtFromZaehler, ldtTo, giFiliale, gsConnect, giDb);
                    liRows = FetchData(lsSql, 21, giDb, gsConnect);
                    // Report  Zählerstände
                    lsSqlZaehlerstd = RdQueries.GetSqlSelect(134, liId, "", "", "", ldtFromZaehler, ldtTo,giFiliale,gsConnect, giDb);   // Report

                    // Tabelle Leerstand befüllen
                    DgrLeerDetail.ItemsSource = null;
                    lsSql = RdQueries.GetSqlSelect(212, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
                    liRows = FetchData(lsSql, 18, giDb, gsConnect);
                    lsSqlLeerstand = RdQueries.GetSqlSelect(222, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);     // für Report

                    // Db Header für Report befüllen für Objekte x_abr_info
                    lsSqlHeader = RdQueries.GetSqlSelect(201, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
                    liRows = FetchData(lsSqlHeader, 17, giDb, gsConnect);

                    // Global Objekt Id
                    GiObjektId = liId;
                    GiObjektTeilId = 0;
                    GiMieterId = 0;

                    break;
                case 2:     // ObjektTeil
                    cbObj.IsChecked = true;
                    cbObjTeil.IsChecked = true;
                    cbName.IsChecked = false;
                    // Objekt-Teil in Tab Rechungen anzeigen
                    lsTmp = gsItemHeader;
                    // In Rechnungen
                    tbObjekt.Text = "";
                    tbObjektTeil.Text = lsTmp;
                    tbMieter.Text = "";
                    // In Zahlungen
                    tbZlObjekt.Text = "";
                    tbZlObjektTeil.Text = lsTmp;
                    tbZlMieter.Text = "";
                    // In Zählerständen
                    tbCntObjekt.Text = "";
                    tbCntObjektTeil.Text = lsTmp;
                    tbCntMieter.Text = "";

                    // Combobox Kostenart in rechnungen befüllen Art = 11 ObjektTeil Kennung 2
                    lsSql = RdQueries.GetSqlSelect(11, liIndex, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
                    liRows = FetchData(lsSql, 11, giDb, gsConnect);

                    // Combobox Zählernummern und mwst in Zähler
                    lsSql = RdQueries.GetSqlSelect(2222, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
                    liRows = FetchData(lsSql, 22, giDb, gsConnect);

                    // Die TeilObjekt ID ermitteln
                    lsSql = RdQueries.GetSqlSelect(3, giFiliale, gsItemHeader, "2", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
                    liId = FetchData(lsSql, 4, giDb, gsConnect);

                    // Untergordnete Rechungen und Timline für Mieter erzeugen (alte erstmal löschen)
                    Timeline.EditRechung(0, 0, liId, 3, gsConnect);

                    // TimeLine holen für ObjektTeile
                    lsSql = RdQueries.GetSqlSelect(6, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
                    liRows = FetchData(lsSql, 8, giDb, gsConnect);
                    lsSqlTimeline = RdQueries.GetSqlSelect(106, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);      // Report
                    lsSqlTimeline2 = RdQueries.GetSqlSelect(116, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);     // Darstellung der ObjektKosten in der NKA
                    lsSqlTimeline3 = RdQueries.GetSqlSelect(140, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);     // Für das Einsetzen der Rechnungsnummer in die Timeline

                    // Rechnungen zeigen  Art 9 = Rechungen zeigen für Teilobjekte Datum aktiv
                    lsSql = RdQueries.GetSqlSelect(9, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
                    liRows = FetchData(lsSql, 9, giDb, gsConnect);
                    lsSqlRechnungen = RdQueries.GetSqlSelect(109, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);    // Report

                    // Zahlungen zeigen Art 15 Zahlungen für ObjektTeile
                    lsSql = RdQueries.GetSqlSelect(25, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
                    liRows = FetchData(lsSql, 14, giDb, gsConnect);
                    lsSqlZahlungen = RdQueries.GetSqlSelect(125, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);     // Report

                    // Zählerstände zeigen Art 35 ObjektTeile
                    lsSql = RdQueries.GetSqlSelect(35, liId, "", "", "", ldtFromZaehler, ldtTo, giFiliale, gsConnect, giDb);
                    liRows = FetchData(lsSql, 21, giDb, gsConnect);
                    // Report Zählerstände
                    lsSqlZaehlerstd = RdQueries.GetSqlSelect(135, liId, "", "", "", ldtFromZaehler, ldtTo,giFiliale,gsConnect, giDb);   // Report

                    // Db Header für Report befüllen für ObjektTeile x_abr_info
                    lsSqlHeader = RdQueries.GetSqlSelect(202, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
                    liRows = FetchData(lsSqlHeader, 17, giDb, gsConnect);

                    // Tabelle Leerstand befüllen
                    DgrLeerDetail.ItemsSource = null;
                    lsSql = RdQueries.GetSqlSelect(213, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
                    liRows = FetchData(lsSql, 18, giDb, gsConnect);
                    // Detaillierter Leerstand
                    lsSqlLeerstand = RdQueries.GetSqlSelect(223, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);     // für Report

                    // Global TeilObjekt Id
                    GiObjektId = 0;
                    GiObjektTeilId = liId;
                    GiMieterId = 0;

                    break;
                case 3:         // Mieter
                    cbObj.IsChecked = true;
                    cbObjTeil.IsChecked = true;
                    cbName.IsChecked = true;
                    // Mieter in Tab Rechungen anzeigen
                    lsTmp = gsItemHeader;
                    // In Rechnungen
                    tbMieter.Text = lsTmp;
                    // In Zahlungen
                    tbZlMieter.Text = lsTmp;
                    // In Zählerständen
                    tbCntMieter.Text = lsTmp;

                    // Combobox Kostenart in rechnungen befüllen Art = 11
                    lsSql = RdQueries.GetSqlSelect(11, liIndex, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
                    liRows = FetchData(lsSql, 11, giDb, gsConnect);

                    // Die Mieter ID ermitteln
                    lsSql = RdQueries.GetSqlSelect(3, giFiliale, gsItemHeader, "3", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
                    liId = FetchData(lsSql, 5, giDb, gsConnect);

                    // Die Objekt Id für die Darstellung der ObjektKosten besorgen
                    liObjektIdTmp = Timeline.GetIdObj(liId, gsConnect, 1);

                    // TimeLine holen für Mieter
                    lsSql = RdQueries.GetSqlSelect(7, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
                    liRows = FetchData(lsSql, 8, giDb, gsConnect);
                    lsSqlTimeline = RdQueries.GetSqlSelect(107, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);               // Report Nebenkosten Hauptteil
                    lsSqlTimeline2 = RdQueries.GetSqlSelect(116, liObjektIdTmp, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);     // Darstellung der ObjektKosten in der NKA
                    lsSqlTimeline3 = RdQueries.GetSqlSelect(140, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);              // Für das Einsetzen der Rechnungsnummer in die Timeline

                    // Rechnungen zeigen  Art 10 = Rechungen zeigen für Mieter Datum aktiv
                    lsSql = RdQueries.GetSqlSelect(10, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
                    liRows = FetchData(lsSql, 9, giDb, gsConnect);
                    lsSqlRechnungen = RdQueries.GetSqlSelect(110, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);  // Report

                    // Zahlungen zeigen Art 13 Zahlungen für Mieter
                    lsSql = RdQueries.GetSqlSelect(23, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
                    liRows = FetchData(lsSql, 14, giDb, gsConnect);
                    lsSqlZahlungen = RdQueries.GetSqlSelect(123, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);     // Report
                    lsSqlSumme = RdQueries.GetSqlSelect(115, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);         // Report Summendarstellung Zahlbetrag

                    // Tabelle Leerstand nicht befüllen, sondern leeren.
                    // Für Mieter gibt es keinen Leerstand
                    DgrLeer.ItemsSource = null;
                    DgrLeerDetail.ItemsSource = null;
                    lsSqlLeerstand = "";

                    // Zählerstände gibts nicht für Mieter
                    DgrCounters.ItemsSource = null;

                    // Db Header für Report befüllen für Mieter x_abr_info
                    lsSqlHeader = RdQueries.GetSqlSelect(203, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);        // Header
                    liRows = FetchData(lsSqlHeader, 17, giDb, gsConnect);

                    // Global Mieter Id
                    GiObjektId = 0;
                    GiObjektTeilId = 0;
                    GiMieterId = liId;
                    break;
                default:
                    break;
            }

            // hier die Where Klausel vom Sql-Statement für Reports speichern
            switch (asArt)
            {
                case 3:
                    // Rechnungen
                    Timeline.SaveLastSql(lsSqlRechnungen, "", "", "", "", "", "", "", "", "rechnungen", "");
                    break;
                case 4:
                    // Zahlungen
                    Timeline.SaveLastSql(lsSqlZahlungen, "", "", "", "", "", "", "", "", "zahlungen", "");
                    break;
                case 5:
                    // Nebenkostenabrechnung 
                    // SqlStatement für die Zieltabelle x_abr_content erzeugen Abrechnung
                    // Das Befüllen der Tabelle erfolgt In FillContent in Funktionen
                    lsSqlAbrContent = RdQueries.GetSqlSelect(300, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);      // Abrechnung Content x_abr_content
                    // Abrechnungen (Kosten,Kostenverteilung,Kostenverteilung Summen,Zahlungen Summe,Personen,Zähler,Art)
                    if (liIndex == 3)       // Nebenkosten Mieter
                    {
                        Timeline.SaveLastSql(lsSqlTimeline, lsSqlAbrContent, "",
                                "", lsSqlZahlungen, lsSqlSumme, "", lsSqlTimeline2, "", "kosten", "");                  // direkte Kosten Mieter 
                    }
                    if (liIndex == 2)       // Nebenkosten Teilobjekt
                    {
                        Timeline.SaveLastSql(lsSqlTimeline, lsSqlAbrContent, "", 
                                "", lsSqlZahlungen, lsSqlSumme, "", lsSqlTimeline2, "", "kostenteilobjekt", "");       // direkte Kosten Teilobjekt
                    }

                    Timeline.saveLastVal(ldtFrom, ldtTo, "Datum");                          // Übergabe des Datumsbereiches 
                    break;
                case 6:
                    // Anschreiben
                    // SqlStatement für die Zieltabelle x_abr_content erzeugen Abrechnung
                    // Das Befüllen der Tabelle erfolgt dann in WndRep
                    lsSqlAbrContent = RdQueries.GetSqlSelect(300, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);      // Abrechnung Content x_abr_content
                    lsSqlRgNrAnschreiben = RdQueries.GetSqlSelect(140, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb); // Speichern der Rechnungsnummer Anschreiben
                    // Abrechnungen (Kosten,Kostenverteilung,Kostenverteilung Summen,Zahlungen Summe,Personen,Zähler,Art, Rechnungsnummer Anschreiben)
                    Timeline.SaveLastSql(lsSqlTimeline, lsSqlAbrContent, "",
                            "", lsSqlZahlungen, lsSqlSumme, "", lsSqlTimeline2, "", "anschreiben", lsSqlRgNrAnschreiben);  // direkte Kosten
                    Timeline.saveLastVal(ldtFrom, ldtTo, "Datum");                          // Übergabe des Datumsbereiches 
                    break;
                case 7:
                    // Nebenkostenabrechnung detailliert 
                    // SqlStatement für die Zieltabelle x_abr_content erzeugen Abrechnung
                    // Das Befüllen der Tabelle erfolgt dann in WndRep
                    lsSqlAbrContent = RdQueries.GetSqlSelect(300, liId, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);      // Abrechnung Content x_abr_content
                    // Abrechnungen (Kosten,Kostenverteilung,Kostenverteilung Summen,Zahlungen Summe,Personen,Zähler,Art)
                    Timeline.SaveLastSql(lsSqlTimeline, lsSqlAbrContent, "",
                            "", lsSqlZahlungen, lsSqlSumme, "", lsSqlTimeline2, "", "kostendetail", "");       // direkte Kosten detailliert
                    Timeline.saveLastVal(ldtFrom, ldtTo, "Datum");                                         // Übergabe des Datumsbereiches 
                    break;
                case 8:
                    // Zählerstände
                    Timeline.SaveLastSql(lsSqlZaehlerstd, "", "", "", "", "", "", "", "", "zaehler", "");
                    break;
                case 9:
                    // Leerstände
                    Timeline.SaveLastSql(lsSqlLeerstand, "", "", "", "", "", "", "", "", "leerstand", "");
                    break;
                default:
                    break;
            }
            return (liOk);
        }

        // Treeview: Ein anderes Item wurde gewählt
        private void tvMain_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var Tree = sender as TreeView;
            int index = 0;

            DateTime ldtFrom = DateTime.MinValue;
            DateTime ldtTo = DateTime.MinValue;

            // Löschen und Save Button aus
            // Rechnung
            btnRgDel.IsEnabled = false;
            btnRgSave.IsEnabled = false;
            // Zahlung
            btnZlDel.IsEnabled = false;
            btnZlSave.IsEnabled = false;
            // Zähler
            btnCntDel.IsEnabled = false;
            btnCntSave.IsEnabled = false;

            // Button Texte Rücksetzen
            btnRgSave.Content = "Speichern";
            btnRgDel.Content = "Löschen";
            btnZlSave.Content = "Speichern";
            btnZlDel.Content = "Löschen";
            btnCntSave.Content = "Speichern";
            btnCntDel.Content = "Löschen";

            // Details Kosten Grid leeren
            if (Tree.Items.Count >= 0)
            {
                var tree = sender as TreeView;

                if (tree.SelectedValue != null)
                {
                    index++;
                    TreeViewItem item = tree.SelectedItem as TreeViewItem;
                    ItemsControl parent = ItemsControl.ItemsControlFromItemContainer(item);
                    tbNameSearch.Text = item.Header.ToString();
                    while (parent != null && parent.GetType() == typeof(TreeViewItem))
                    {
                        index++;
                        parent = ItemsControl.ItemsControlFromItemContainer(parent);
                    }

                    // gibt es gewählte Kalender, dann hier Daten einsetzen
                    if (cbCal.IsChecked == true)
                    {
                        // nur StartDatum
                        if (clFrom.SelectedDate != null)
                        {
                            if (clFrom.SelectedDate.Value > DateTime.MinValue)
                            {
                                ldtFrom = clFrom.SelectedDate.Value;
                            }
                        }

                        // Start und EndeDatum angegeben
                        if (clFrom.SelectedDate != null && clTo.SelectedDate != null)
                        {
                            if (clFrom.SelectedDate.Value > DateTime.MinValue && clTo.SelectedDate.Value > DateTime.MinValue)
                            {
                                ldtFrom = clFrom.SelectedDate.Value;
                                ldtTo = clTo.SelectedDate.Value;
                            }
                        }

                    }
                    else
                    {
                        // Todo Ulf Testweise ausgeschaltet 221201
                        //// Startdatum ist Jahresbeginn
                        //int liYear = DateTime.Now.Year - 1;
                        //string lsStart = (liYear.ToString()) + "-01-01";
                        //string lsEnd = (liYear.ToString()) + "-12-31";
                        //DateTime ldtStart = DateTime.Parse(lsStart);                 // Jahresanfang VorJahr
                        //DateTime ldtEnd = DateTime.Parse(lsEnd);
                    }

                    // Der Index wird nochmal bei TimeLine Details benötigt
                    giIndex = index;
                    gsItemHeader = item.Header.ToString().Trim();

                    if (gsItemHeader != "Kein Mieter")
                    {
                        updateAllDataGrids(0);      // alle grids aktualisieren
                    }
                    else
                    {
                        updateAllDataGrids(11);     // Treview zurücksetzen ohne Auswahl
                    }
                }
             }
        }

        // Rechnungen DataGrid 
        private void DgrRechnungen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // delete Button auf
            btnRgDel.IsEnabled = true;
        }

        // Rechnungen DataGrid Zeile Zugefügt oder bearbeitet
        private void DgrRechnungen_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            btnRgSave.IsEnabled = true;
        }

        // Rechnungen Button Save 
        private void btnRgSave_Click(object sender, RoutedEventArgs e)
        {
            string lsSql = "";
            int liOk = 0;
            int LiObkjektId = 0;    

            // aktualsiert Rechnungen TblRechnungen
            FetchData("", 35, giDb, gsConnect);

            LiObkjektId = int.Parse(TblRechnungen.Rows[DgrRechnungen.SelectedIndex][8].ToString());

            // Timeline bearbeiten    giFlagTimeline 1 = Rechnungen
            Timeline.EditRechung(GiRechnungId, LiObkjektId, 0, GiFlagTimeline, gsConnect);

            // Die IDs und Flags zurücksetzen
            GiRechnungId = 0;
            giMwstSatz = 99;

            // save Button Rechnungen wieder aus
            btnRgSave.IsEnabled = false;
            btnRgAdd.IsEnabled = true;
            updateAllDataGrids(0);
        }

        // Rechnungen Beginn Eingabe
        private void DgrRechnungen_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {

            // gewählten Datensatz ermitteln
            int LiRgId = 0;
            int LiSel = DgrRechnungen.SelectedIndex;

            if (LiSel >= 0)
            {
                DataRow dr = TblRechnungen.Rows[LiSel];
                if (dr[14] != DBNull.Value)
                {
                    LiRgId = Int32.Parse(dr[14].ToString());                // RechnungsId holen
                }
                GiRechnungId = LiRgId;
                GiFlagTimeline = 1;                                         // 1 = Rechnung bearbeiten
            }

            // Save Button auf
            btnRgSave.IsEnabled = true;
        }

        // Rechnungen Button zufügen
        private void btnRgAdd_Click(object sender, RoutedEventArgs e)
        {

            // Temporäre ID Rechnungen ermitteln Art
            int liTmpId = Timeline.getTmpId(gsConnect, 1) + 1;
            GiRechnungId = liTmpId;

            DataRow dr = TblRechnungen.NewRow();
            dr[8] = GiObjektId;
            dr[9] = GiObjektTeilId;
            dr[10] = GiMieterId;
            dr[14] = liTmpId;
            dr[15] = 1;                 // Flag für Bearbeitung erzeugen

            TblRechnungen.Rows.Add(dr);

            btnRgAdd.IsEnabled = false;
        }

        // Rechnungen Button löschen
        private void btnRgDel_Click(object sender, RoutedEventArgs e)
        {
            int LiSel = DgrRechnungen.SelectedIndex;
            int LiDelId = 0;
            int LiOk = 0;
            string LsSql = string.Empty;    

            if (LiSel >= 0)
            {
                MessageBoxResult result = MessageBox.Show("Soll die Rechnung wirklich gelöscht werden?", "Rechnungen", MessageBoxButton.YesNo, MessageBoxImage.Question);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        DataRow dr = TblRechnungen.Rows[LiSel];
                        LiDelId = (int)(dr[0]);                // Id des zu löschenden Datensatzes

                        if (LiDelId >= 0)
                        {
                            TblRechnungen.Rows.Remove(dr);

                            LsSql = RdQueries.GetSqlSelect(36, LiDelId, "", "", "", DateTime.MinValue, DateTime.MinValue, giFiliale, gsConnect, giDb);
                            FetchData(LsSql, 36, giDb, gsConnect);

                            // Erzeugte Untergeordnete Rechnungen löschen
                            // Alle mit der Id der Hauptrechnung in id_rechnung_source
                            Timeline.DeleteRechnung(LiDelId, "R", gsConnect);
                            // Delete Timeline mit der Rechnungs id
                            Timeline.DeleteTimeline(LiDelId, "R", gsConnect);

                            // delete Button zu
                            btnRgDel.IsEnabled = false;
                        }
                        break;
               
                }
            }
        }

        // Zahlung Save
        private void btnZlSave_Click(object sender, RoutedEventArgs e)
        {
            int liOk = 0;
            int liRows = 0;
            // int liNkId = 0;
            int liTimelineId = 0;
            string lsSql = "";

            // Datenverbindung
            liOk = FetchData(lsSql, 37, giDb, gsConnect);

            // Timeline bearbeiten Art 11 = Zahlungen ändern
            int liFlagTimeline = 11;
            // Timeline.editTimeline(giTimelineId, giFlagTimeline, gsConnect, giDb);
            liRows = TblZahlungen.Rows.Count;

            if (liRows > 0)
            {

                for (int i = 0; i < liRows; i++)           // Ende bei 12 Monate
                {
                    if (TblZahlungen.Rows[i][0] == DBNull.Value)        // Id ist noch leer
                    {
                        Int32.TryParse(TblZahlungen.Rows[i][10].ToString(), out liTimelineId);       // Timeline Id holen

                        Timeline.EditRechung(liTimelineId, 0,liFlagTimeline, 0, gsConnect);   // Timeline aktualisieren
                    }
                }
            }

            // Update der Daten
            liOk = updateAllDataGrids(0);

            // Die IDs und Flags zurücksetzen
            giDelZlId = 0;
            GiRechnungId = 0;

            // save Button Zahlungen wieder aus
            btnZlSave.IsEnabled = false;
            btnZlSave.Content = "Speichern";
            btnZlAdd.IsEnabled = true;
        }

        // Zahlung Zufügen
        private void btnZlAdd_Click(object sender, RoutedEventArgs e)
        {
            int liTimelineId = 0;
            int liNkId = 0;
            int liRows = TblZahlungen.Rows.Count;
            DateTime ldtZlg = DateTime.MinValue;

            // ID für Timeline ermitteln Art 2 = Zahlungs ID
            liTimelineId = Timeline.getTmpId(gsConnect, 2) + 1;

            // Kostenart ID ermitteln Art 1 = Nebenkostenzahlungen
            liNkId = Timeline.GetKsaId(1, gsConnect);

            DataRow dr = TblZahlungen.NewRow();
            dr[2] = GiObjektId;
            dr[3] = GiObjektTeilId;
            dr[1] = GiMieterId;
            dr[10] = liTimelineId;      // ID für Timeline
            dr[11] = 1;                 // Flag für Timelinebearbeitung erzeugen
            dr[12] = liNkId;            // Kostenart Nebenkosten

            // Datum vorbelegen erst ab dem 2 ten Datensatz
            // Der neueste ist immer der oberste 0
            if (liRows > 0 && TblZahlungen.Rows[0][4] != DBNull.Value)
            {
                ldtZlg = Convert.ToDateTime(TblZahlungen.Rows[0][4]);
                dr[4] = ldtZlg.AddMonths(1);       // Ende Datum

                if (TblZahlungen.Rows[0][6] != DBNull.Value)   // Netto
                {
                    dr[6] = TblZahlungen.Rows[0][6];
                }

                if (TblZahlungen.Rows[0][7] != DBNull.Value)   // Brutto
                {
                    dr[7] = TblZahlungen.Rows[0][7];
                }

                GiRechnungId = liTimelineId;
                GiFlagTimeline = 11;                                         // 11 = Zahlung bearbeiten

                btnZlSave.IsEnabled = true;
            }

            TblZahlungen.Rows.Add(dr);
            btnZlAdd.IsEnabled = false;
        }

        // Hier sollen Zahlungen automatisch erzeugt werden
        private void DgrZahlungen_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int liTimelineId = 0;
            int liNkId = 0;
            int liRows = TblZahlungen.Rows.Count;
            DateTime ldtZlg = DateTime.MinValue;

            // Datum vorbelegen erst ab dem 2 ten Datensatz
            // Der neueste ist immer der oberste 0
            if (liRows > 0 && TblZahlungen.Rows[0][4] != DBNull.Value && DgrZahlungen.SelectedIndex != 0)
            {
                // Kostenart ID ermitteln Art 1 = Nebenkostenzahlungen
                liNkId = Timeline.GetKsaId(1, gsConnect);

                // ID für Timeline ermitteln Art 2 = Zahlungs ID
                liTimelineId = Timeline.getTmpId(gsConnect, 2) + 1;

                // Monat der vorhandenen Zahlung
                ldtZlg = Convert.ToDateTime(TblZahlungen.Rows[0][4]);

                for (int i = liRows; i < 12; i++)           // Ende bei 12 Monate
                {
                    DataRow dr = TblZahlungen.NewRow();
                    dr[2] = GiObjektId;
                    dr[3] = GiObjektTeilId;
                    dr[1] = GiMieterId;
                    dr[10] = liTimelineId;      // ID für Timeline
                    dr[11] = 1;                 // Flag für Timelinebearbeitung erzeugen
                    dr[12] = liNkId;            // Kostenart Nebenkosten
                    dr[4] = ldtZlg.AddMonths(i);       // Datum

                    if (TblZahlungen.Rows[0][6] != DBNull.Value)   // Netto
                    {
                        dr[6] = TblZahlungen.Rows[0][6];
                    }

                    if (TblZahlungen.Rows[0][7] != DBNull.Value)   // Brutto
                    {
                        dr[7] = TblZahlungen.Rows[0][7];
                    }

                    TblZahlungen.Rows.Add(dr);

                    liTimelineId++;
                }

                GiRechnungId = liTimelineId;
                GiFlagTimeline = 11;                                         // 11 = Zahlung bearbeiten
                btnZlSave.IsEnabled = true;
            }

        }

        // Zahlung löschen
        private void btnZlDel_Click(object sender, RoutedEventArgs e)
        {
            int liTimelineId = 0;

            GiFlagTimeline = 12;                // 12 = Zahlung löschen

            // Durch alle zum Löschen gewählten Datensätze
            if (DgrZahlungen.SelectedItems.Count > 0)
            {
                for (int i = 0; i < DgrZahlungen.SelectedItems.Count; i++)
                {

                    System.Data.DataRowView selectedFile = (System.Data.DataRowView)DgrZahlungen.SelectedItems[i];

                    giDelZlId = (int)selectedFile.Row.ItemArray[0];
                    liTimelineId = (int)selectedFile.Row.ItemArray[10];          // TimeLine ID holen                    

                    // Timeline bearbeiten Art 12 = Zahlungen löschen
                    Timeline.EditRechung(liTimelineId, 0, GiFlagTimeline, 0, gsConnect);

                    // Delete Kommando muss extra erzeugt werden
                    // Gibt es eine Datensatz ID zum Löschen
                    if (giDelZlId > 0)
                    {
                        string lsSql = RdQueries.GetSqlSelect(38, giDelZlId, "", "", "", DateTime.MinValue, DateTime.MinValue, giFiliale, gsConnect, giDb);
                        int liOk = FetchData(lsSql, 38, giDb, gsConnect);
                    }
                }
            }

            // Update der Daten
            int liOk1 = updateAllDataGrids(0);
        }

        // Falls Zahlung angewählt ist, mit einem Click wegnehmen
        private void DgrZahlungen_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Falls ein Datensatz angewählt ist, Anwahl wegnehmen
            if (DgrZahlungen.SelectedIndex >= 0)
            {
                DgrZahlungen.SelectedIndex = -1;
            }
        }

        // Zählerstand löschen
        private void btnCntDel_Click(object sender, RoutedEventArgs e)
        {
            int liTimelineId = 0;
            int liTest = 1;

            int liSel = DgrCounters.SelectedIndex;
            if (liSel >= 0)
            {

                DataRow dr = TblZlWerte.Rows[liSel];
                giDelZlWertId = (int)(dr[0]);                // Id des zu löschenden Datensatzes


                if (dr[7] != DBNull.Value || liTest == 1)
                {
                    liTimelineId = (int)dr[7];          // TimeLine ID holen                    
                    GiRechnungId = liTimelineId;
                    TblZlWerte.Rows.Remove(dr);

                    btnCntSave.Content = "wirklich löschen?";
                    btnCntSave.IsEnabled = true;
                    btnCntAdd.IsEnabled = false;

                    GiFlagTimeline = 22;                 // Zählerwert löschen
                    // delete Button zu
                    btnCntDel.IsEnabled = false;
                }
            }
        }

        // Zählerstand zufügen
        private void btnCntAdd_Click(object sender, RoutedEventArgs e)
        {
            int liTmpId = 0;
            int liKsaId = 0;

            // Temporäre ID ermitteln Art
            liTmpId = Timeline.getTmpId(gsConnect, 3) + 1;
            // KostenstellenartId Zähler ermitteln
            liKsaId = Timeline.GetKsaId(2, gsConnect);

            // Nur wenn das Grid DgrCounters erzeugt wurde
            // Zählerstand ermöglichen
            if (DgrCounters.ItemsSource != null)
            {
                DataRow dr = TblZlWerte.NewRow();

                TblZlWerte.Rows.Add(dr);
                dr[7] = liTmpId;       // ID für Timeline
                dr[8] = GiObjektId;           // Objekt
                dr[9] = GiObjektTeilId;       // Teilobjekt
                dr[11] = liKsaId;           // Kostenstellenart einsetzen

                btnCntAdd.IsEnabled = false;
            }
            else
            {
                MessageBox.Show("Kein Zähler auf dieser Ebene vorhanden", "Achtung");
            }
        }

        // Zählerstand speichern
        private void btnCntSave_Click(object sender, RoutedEventArgs e)
        {
            int liOk = 0;
            string lsSql = "";

            // Update
            liOk = FetchData("", 39, giDb, gsConnect);

            // Timeline bearbeiten Art 21 = Zähler   
            Timeline.EditRechung(GiRechnungId, 0, GiFlagTimeline, 0, gsConnect);

            // Delete Kommando muss extra erzeugt werden
            // Gibt es eine Datensatz ID zum Löschen (button btnCntDel)
            if (giDelZlWertId > 0)
            {
                // Den Zählerstand löschen
                lsSql = RdQueries.GetSqlSelect(40, giDelZlWertId, "", "", "", DateTime.MinValue, DateTime.MinValue, giFiliale, gsConnect, giDb);
                liOk = FetchData(lsSql, 40, giDb, gsConnect);

            }
            // Update der Daten
            liOk = updateAllDataGrids(0);

            // Die IDs und Flags zurücksetzen
            giDelZlWertId = 0;
            giZlId = 0;                 // globale Zähler Id
            GiRechnungId = 0;
            giMwstSatzZl = 99;

            // Save Button Zähler wieder aus
            btnCntSave.IsEnabled = false;
            btnCntSave.Content = "Speichern";
            btnCntAdd.IsEnabled = true;
        }

        // Rechnungen Netto und Brutto Umrechnungen
        // und für die bedingte Verteilung von Flächen das Auswahlformular öffnen
        private void DgrRechnungen_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            int liMwstSatz = 99;
            int liMwstArt = 0;
            int liOk = 0;
            string lsNetto = "";
            string lsBrutto = "";
            string lsMwstSatz = "";
            string lsArtVerteilung = "";
            string lsArtVertKurz = "";
            decimal ldNetto = 0;
            decimal ldBrutto = 0;

            // gewählten Datensatz ermitteln
            int liSel = DgrRechnungen.SelectedIndex;

            if (liSel >= 0)
            {
                int x = e.Column.DisplayIndex;
                int y = e.Row.GetIndex();

                if (x == 1)     // Art der Verteilung
                {
                    lsArtVerteilung = getCurrentCellValue((ComboBox)e.EditingElement);
                    // Verteilung Kurzzeichen ermitteln
                    lsArtVertKurz = Timeline.GetVerteilungFromString(gsConnect, lsArtVerteilung, giDb);
                    // Wurde eine Bedingte Verteilung gewählt? Auswahlformular öffnen?
                    if (lsArtVertKurz == "fa")
                    {
                        // Objekt Mix neu anlegen mit Objekt ID und 
                        liOk = Timeline.MakeChoose(GiObjektId, GiRechnungId, gsConnect, giDb);
                        // Objekt Mix Parts auswählen
                        WndChooseSet frmChooseSet = new WndChooseSet(this);
                        // Welche Datenbank
                        delPassData delegt4 = new delPassData(frmChooseSet.getDb);
                        delegt4(giDb);
                        // Übergabe der TimeLine ID an das Auswahlfenster
                        delPassData delegt = new delPassData(frmChooseSet.getTimelineId);
                        delegt(GiRechnungId);
                        // Übergabe der Objekt ID
                        delPassData delegt2 = new delPassData(frmChooseSet.getObjektId);
                        delegt2(GiObjektId);
                        // Übergabe, ob Datensatz existiert oder wurde neu angelegt 1,2
                        delPassData delegt3 = new delPassData(frmChooseSet.getArt);
                        delegt3(liOk);

                        frmChooseSet.ShowDialog();
                    }
                }

                if (x == 7)     // MwstFeld in globale Variable AUSNAHMSWEISE
                {

                    lsMwstSatz = getCurrentCellValue((ComboBox)e.EditingElement);
                    if (lsMwstSatz == "")
                    {
                        lsMwstSatz = "0";
                    }
                    liMwstSatz = Convert.ToInt16(lsMwstSatz);
                    giMwstSatz = liMwstSatz;

                }

                if (x == 8)     // NettoPreis !! Achtung: Der Displayindex ist die Darstellung im 
                                // DGR und nicht die Itemliste
                {
                    // Hier wird die Zelle des DataGrid ausgelesen, oder bei NewRow der Wert aus der globalen Variablen geholt
                    if (liMwstSatz == 99 && ((DgrRechnungen.Items[liSel] as DataRowView).Row.ItemArray[7] != DBNull.Value))
                    {
                        liMwstArt = Int32.Parse((DgrRechnungen.Items[liSel] as DataRowView).Row.ItemArray[7].ToString()); // Art Mehrwertsteuer
                        liMwstSatz = Timeline.GetMwstSatz(liMwstArt, gsConnect, giDb);
                    }
                    else
                    {
                        liMwstSatz = giMwstSatz;
                        liMwstSatz = giMwstSatz;
                        if (liMwstSatz == 99)
                        {
                            liMwstSatz = 0;
                        }
                    }

                    // Element holen
                    TextBox t1 = e.EditingElement as TextBox;
                    lsNetto = t1.Text.ToString();

                    if (lsNetto.Length > 0 && lsNetto.Substring(lsNetto.Length - 1, 1) == "€")                             
                    {
                        lsNetto = lsNetto.Substring(0, lsNetto.Length - 2);                     // Das Eurozeichen muss raus
                    }
                    if (lsNetto.Length > 0)
                    {
                        ldNetto = Convert.ToDecimal(lsNetto);
                        ldBrutto = ldNetto + (ldNetto / 100) * liMwstSatz;                      // Netto
                        if (ldNetto > 0)
                        {
                            DataRowView oDataRowView = DgrRechnungen.SelectedItem as DataRowView;
                            oDataRowView.Row[6] = ldBrutto;                                     // Bruttowert schreiben

                            // Todo Nettower in DataGrid schreiben
                        }
                    }

                }
                if (x == 9)     // Brutto
                {
                    // Hier wird die Zelle des DataGrid ausgelesen, oder bei NewRow der Wert aus der globalen Variablen geholt
                    if (liMwstSatz == 99 && ((DgrRechnungen.Items[liSel] as DataRowView).Row.ItemArray[7] != DBNull.Value))
                    {
                        liMwstArt = Int32.Parse((DgrRechnungen.Items[liSel] as DataRowView).Row.ItemArray[7].ToString()); // Art Mehrwertsteuer                            
                        liMwstSatz = Timeline.GetMwstSatz(liMwstArt, gsConnect, giDb);
                    }
                    else
                    {
                        liMwstSatz = giMwstSatz;
                        if (liMwstSatz == 99)
                        {
                            liMwstSatz = 0;
                        }
                    }

                    // Element holen
                    TextBox t2 = e.EditingElement as TextBox;
                    lsBrutto = t2.Text.ToString();

                    if (lsBrutto.Length > 0 && lsBrutto.Substring(lsBrutto.Length - 1, 1) == "€")
                    {
                        lsBrutto = lsBrutto.Substring(0, lsBrutto.Length - 2);                  // Das Eurozeichen muss raus                            
                    }
                    if (lsBrutto.Length > 0)
                    {
                        ldBrutto = Convert.ToDecimal(lsBrutto);
                        ldNetto = (ldBrutto / (100 + liMwstSatz)) * 100;                        // Nettobetrag
                        if (ldBrutto > 0)
                        {
                            DataRowView oDataRowView = DgrRechnungen.SelectedItem as DataRowView;
                            oDataRowView.Row[5] = ldNetto;                                      // Nettowert schreiben
                        }
                    }
                }
                //}
            }
        }

        // Kosten In der Summendarstellung der Timeline wurde eine Detaildarstellung angewählt
        private void DgrCost_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int liExternId = 0;
            int liSel = DgrCost.SelectedIndex;
            int liOk = 0;
            String lsSql = "";

            int liYear = DateTime.Now.Year - 1;
            string lsStart = (liYear.ToString()) + "-01-01";
            string lsEnd = (liYear.ToString()) + "-12-31";
            DateTime ldtFrom = DateTime.Parse(lsStart);                 // Jahresanfang VorJahr
            DateTime ldtTo = DateTime.Parse(lsEnd);

            String lsDateFrom = "";
            String lsDateTo = "";
            String lsIdObj = "";
            string lsIdObjTeil = "";

            if (liSel >= 0)
            {

                // Start und Endedatum wurden gewählt
                if (clTo.SelectedDate.HasValue && clFrom.SelectedDate.HasValue)
                {
                    ldtFrom = clFrom.SelectedDate.Value;
                    lsDateFrom = ldtFrom.ToString("dd-MM-yyyy HH:mm");
                    tbDateFrom.Text = lsDateFrom;

                    ldtTo = clTo.SelectedDate.Value;
                    // Enddatum bis 23:59:59
                    //ldtTo = ldtTo.AddHours(23);
                    //ldtTo = ldtTo.AddMinutes(59);
                    //ldtTo = ldtTo.AddSeconds(59);
                    lsDateTo = ldtTo.ToString("dd-MM-yyyy HH:mm");
                    tbDateTo.Text = lsDateTo;

                }
                // nur das Startdatum wurde gewählt; EndeDatum ist heutiger Tag
                else if (clTo.SelectedDate.HasValue)
                {
                    ldtFrom = clFrom.SelectedDate.Value;
                    lsDateFrom = ldtFrom.ToString("dd-MM-yyyy HH:mm");
                    tbDateFrom.Text = lsDateFrom;
                    ldtTo = DateTime.Today;
                }

                switch (giIndex)
                {
                    case 1:
                        lsIdObj = GiObjektId.ToString();
                        break;
                    case 2:
                        lsIdObj = GiObjektTeilId.ToString();
                        break;
                    case 3:
                        lsIdObj = GiMieterId.ToString();
                        break;
                    default:
                        break;
                }

                DataRowView rowview = DgrCost.SelectedItem as DataRowView;
                // Es ist eine Rechnung gewählt
                if (rowview.Row[5] != DBNull.Value)
                {
                    liExternId = Int32.Parse(rowview.Row[5].ToString());
                    if (liExternId > 0)
                    {
                        // Daten für Details zeigen
                        lsSql = RdQueries.GetSqlSelect(130, liExternId, giIndex.ToString(), lsIdObj, lsIdObjTeil, ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
                        liOk = FetchData(lsSql, 13, giDb, gsConnect);
                    }
                }
                // Es ist eine Zahlung gewählt
                if (rowview.Row[6] != DBNull.Value)
                {
                    liExternId = Int32.Parse(rowview.Row[6].ToString());
                    if (liExternId > 0)
                    {
                        // Daten für Deatils zeigen
                        lsSql = RdQueries.GetSqlSelect(131, liExternId, giIndex.ToString(), lsIdObj, lsIdObjTeil, ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
                        liOk = FetchData(lsSql, 13, giDb, gsConnect);
                    }
                }
                // Es ist ein Zaehlerstand gewählt
                if (rowview.Row[9] != DBNull.Value)
                {
                    liExternId = Int32.Parse(rowview.Row[9].ToString());
                    if (liExternId > 0)
                    {
                        // Daten für Deatils zeigen
                        lsSql = RdQueries.GetSqlSelect(132, liExternId, giIndex.ToString(), lsIdObj, lsIdObjTeil, ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
                        liOk = FetchData(lsSql, 13, giDb, gsConnect);
                    }
                }
            }
        }

        // Kleine Hilfsfunktion, um Zellwerte in Text zu verwandeln
        private string getCurrentCellValue(ComboBox txtCurCell)
        {
            return txtCurCell.Text;
        }

        // Übergabe des ConnectStrings an andere Fenster
        public string psConnect
        {
            get { return gsConnect; }
        }

        // Zahlungen gewählt
        private void DgrZahlungen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnZlDel.IsEnabled = true;
        }

        // Zahlungen wurden editiert
        private void DgrZahlungen_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            int liMwstSatz = 99;
            int liObjTeilId = 0;
            string lsNetto = "";
            string lsBrutto = "";
            decimal ldNetto = 0;
            decimal ldBrutto = 0;
            DateTime ldtVon = DateTime.MinValue;

            // gewählten Datensatz ermitteln
            int liSel = DgrZahlungen.SelectedIndex;

            if (liSel >= 0)
            {
                int x = e.Column.DisplayIndex;
                int y = e.Row.GetIndex();

                // hier nochmal schnell die Mieter ID eintragen, wenn ein Teilobjekt 
                // gewählt wurde. Das Teilobjekt gibt den Wert an den derzeit
                // gültigen Mieter weiter
                if (x == 1 && TblZahlungen.Rows[liSel][3] != DBNull.Value)        // Teilobjekt ID ist vorhanden
                {
                    if ((int)TblZahlungen.Rows[liSel].ItemArray.GetValue(3) >= 0)
                    {
                        liObjTeilId = (int)TblZahlungen.Rows[liSel].ItemArray.GetValue(3);
                    }
                }

                if (x == 2)
                // NettoPreis !! Achtung: Der Displayindex ist die Darstellung im 
                // DGR und nicht die Itemliste
                {
                    // MwstSatz holen
                    liMwstSatz = Timeline.GetMwstFromBez("normal", gsConnect, giDb);
                    // Element holen
                    TextBox t1 = e.EditingElement as TextBox;
                    lsNetto = t1.Text.ToString();
                    if (lsNetto.Length > 0 && lsNetto.Substring(lsNetto.Length - 1, 1) == "€")                             // Das Eurozeichen muss raus
                    {
                        lsNetto = lsNetto.Substring(0, lsNetto.Length - 2);
                    }
                    if (lsNetto.Length > 0)
                    {
                        ldNetto = Convert.ToDecimal(lsNetto);
                        ldBrutto = ldNetto;                     // + ((ldNetto / 100) * liMwstSatz);                          // Bruttobetrag = Netto
                        DataRowView oDataRowView = DgrZahlungen.SelectedItem as DataRowView;
                        oDataRowView.Row[7] = ldBrutto;                                      
                    }
                }
                if (x == 3)     // Brutto
                {
                    // Hier wird die Zelle des DataGrid ausgelesen, oder bei NewRow der Wert aus der globalen Variablen geholt
                    // MwstSatz holen
                    liMwstSatz = Timeline.GetMwstFromBez("normal", gsConnect, giDb);
                    // Element holen
                    TextBox t2 = e.EditingElement as TextBox;
                    lsBrutto = t2.Text.ToString();
                    if (lsBrutto.Length > 0 && lsBrutto.Substring(lsBrutto.Length - 1, 1) == "€")
                    {
                        lsBrutto = lsBrutto.Substring(0, lsBrutto.Length - 2);                   // Das Eurozeichen muss raus                            
                    }
                    if (lsBrutto.Length > 0)
                    {
                        ldBrutto = Convert.ToDecimal(lsBrutto);
                        ldNetto = ldBrutto;                       // (ldBrutto / (100 + liMwstSatz)) * 100;                            // Nettobetrag= Brutto
                        DataRowView oDataRowView = DgrZahlungen.SelectedItem as DataRowView;
                        oDataRowView.Row[6] = ldNetto;                                      // Nettowert schreiben
                    }
                }

                if (x == 4)     // Netto Soll !! Achtung: Der Displayindex ist die Darstellung im 
                // DGR und nicht die Itemliste
                {
                    // MwstSatz holen
                    liMwstSatz = Timeline.GetMwstFromBez("normal", gsConnect, giDb);
                    // Element holen
                    TextBox t1 = e.EditingElement as TextBox;
                    lsNetto = t1.Text.ToString();
                    if (lsNetto.Length > 0 && lsNetto.Substring(lsNetto.Length - 1, 1) == "€")                             // Das Eurozeichen muss raus
                    {
                        lsNetto = lsNetto.Substring(0, lsNetto.Length - 2);
                    }
                    if (lsNetto.Length > 0)
                    {
                        ldNetto = Convert.ToDecimal(lsNetto);
                        ldBrutto = ldNetto + (ldNetto / 100) * liMwstSatz;                          // Bruttobetrag
                        DataRowView oDataRowView = DgrZahlungen.SelectedItem as DataRowView;
                        oDataRowView.Row[9] = ldBrutto;                                
                    }

                }
                if (x == 5)     // Brutto Soll
                {
                    // Hier wird die Zelle des DataGrid ausgelesen, oder bei NewRow der Wert aus der globalen Variablen geholt
                    // MwstSatz holen
                    liMwstSatz = Timeline.GetMwstFromBez("normal", gsConnect, giDb);
                    // Element holen
                    TextBox t2 = e.EditingElement as TextBox;
                    lsBrutto = t2.Text.ToString();
                    if (lsBrutto.Length > 0 && lsBrutto.Substring(lsBrutto.Length - 1, 1) == "€")
                    {
                        lsBrutto = lsBrutto.Substring(0, lsBrutto.Length - 2);                   // Das Eurozeichen muss raus                            
                    }
                    if (lsBrutto.Length > 0)
                    {
                        ldBrutto = Convert.ToDecimal(lsBrutto);
                        ldNetto = (ldBrutto / (100 + liMwstSatz)) * 100;                            // Nettobetrag
                        DataRowView oDataRowView = DgrZahlungen.SelectedItem as DataRowView;
                        oDataRowView.Row[8] = ldNetto;
                    }
                }
            }
        }

        // Zahlungseingabe begonnen
        private void DgrZahlungen_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            // gewählten Datensatz ermitteln
            int liTimelineId = 0;

            int liSel = DgrZahlungen.SelectedIndex;
            if (liSel >= 0)
            {
                DataRow dr = TblZahlungen.Rows[liSel];
                if (dr[10] != DBNull.Value)
                {
                    liTimelineId = Int32.Parse(dr[10].ToString());           // TimeLine ID holen
                }
                GiRechnungId = liTimelineId;
                GiFlagTimeline = 11;                                         // 11 = Zahlung bearbeiten

            }
            // Button Save auf
            btnZlSave.IsEnabled = true;
        }

        // Zählerstände Selection Changed
        private void DgrCounters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnCntDel.IsEnabled = true;
        }

        // Zählerstände Zeile editiert
        private void DgrCounters_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {

        }

        // Zählerstände Beginn Edit
        private void DgrCounters_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            // gewählten Datensatz ermitteln
            int liTimelineId = 0;

            int liSel = DgrCounters.SelectedIndex;

            if (liSel >= 0)
            {
                DataRow dr = TblZlWerte.Rows[liSel];
                if (dr[8] != DBNull.Value)
                {
                    liTimelineId = Int32.Parse(dr[7].ToString());                  // TimeLine ID holen
                    GiRechnungId = liTimelineId;
                    GiFlagTimeline = 21;                                           // 21 = Zähler bearbeiten
                }
            }
            // Button Save auf
            btnCntSave.IsEnabled = true;
        }

        // Zählerstände Zelle Editiert
        private void DgrCounters_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            int liMwstSatz = 99;
            int liZlId = 0;
            int liFlagNew = 0;
            string lsNetto = "";
            string lsBrutto = "";
            string lsZlStand = "";
            string lsZlName = "";
            decimal ldNetto = 0;
            decimal ldBrutto = 0;
            decimal ldZlStand = 0;
            decimal ldVerbrauch = 0;
            DateTime ldtVon = DateTime.MinValue;

            // gewählten Datensatz ermitteln
            int liSel = DgrCounters.SelectedIndex;

            if (liSel >= 0)
            {
                int x = e.Column.DisplayIndex;
                int y = e.Row.GetIndex();

                if (x == 0)       // Gewählter Zähler Id ermitteln
                {
                    lsZlName = getCurrentCellValue((ComboBox)e.EditingElement);
                    liZlId = Timeline.GetZlId(lsZlName, gsConnect, giDb);
                    // Das Feld Zähler Id befüllen
                    TblZlWerte.Rows[liSel][10] = liZlId;

                    giZlId = liZlId;
                }

                if (x == 3)     // Zählerstand wurde eingegeben
                {
                    TextBox t2 = e.EditingElement as TextBox;
                    lsZlStand = t2.Text.ToString();
                    if (lsZlStand.Length > 0)
                    {
                        ldZlStand = Convert.ToDecimal(lsZlStand);
                        if (TblZlWerte.Rows[liSel][10] != DBNull.Value)                // Zähler Id aus DataGrid
                        {
                            liZlId = Convert.ToInt32(TblZlWerte.Rows[liSel][10]);      // Zähler Id  
                            liFlagNew = 0;  // Datensatz wird editiert
                        }
                        if (giZlId > 0)     // Zähler Id aus globaler Variable
                        {
                            liZlId = giZlId;
                            liFlagNew = 1;  // Neuer Datensatz
                        }

                        if (liZlId > 0)
                        {
                            ldVerbrauch = Timeline.GetZlVerbrauch(ldZlStand, liZlId, gsConnect, liFlagNew, giDb);
                            DataRowView oDataRowView = DgrCounters.SelectedItem as DataRowView;
                            oDataRowView.Row[3] = ldVerbrauch;
                        }
                    }
                }

                // x == 5 ist die Einheit

                if (x == 6)     // NettoPreis !! Achtung: Der Displayindex ist die Darstellung im 
                // DGR und nicht die Itemliste
                {
                    // MwstSatz holen
                    if (TblZlWerte.Rows[liSel][10] == DBNull.Value && giZlId >= 0)
                    {
                        liMwstSatz = Timeline.GetMwstSatzZaehler(giZlId, gsConnect, giDb);
                    }
                    else
                    {
                        liMwstSatz = Timeline.GetMwstSatzZaehler(Convert.ToInt32(TblZlWerte.Rows[liSel][10]), gsConnect, giDb);
                    }
                    // Element holen
                    TextBox t1 = e.EditingElement as TextBox;
                    lsNetto = t1.Text.ToString();
                    if (lsNetto.Length > 0 && lsNetto.Substring(lsNetto.Length - 1, 1) == "€")                             // Das Eurozeichen muss raus
                    {
                        lsNetto = lsNetto.Substring(0, lsNetto.Length - 2);
                    }
                    if (lsNetto.Length > 0)
                    {
                        ldNetto = Convert.ToDecimal(lsNetto);
                        ldBrutto = ldNetto + (ldNetto / 100) * liMwstSatz;                          // Bruttobetrag
                        DataRowView oDataRowView = DgrCounters.SelectedItem as DataRowView;
                        oDataRowView.Row[6] = ldBrutto;
                    }
                }
                if (x == 7)     // Brutto
                {
                    // MwstSatz holen
                    if (TblZlWerte.Rows[liSel][10] == DBNull.Value && giZlId >= 0)
                    {
                        liMwstSatz = Timeline.GetMwstSatzZaehler(giZlId, gsConnect, giDb);
                    }
                    else
                    {
                        liMwstSatz = Timeline.GetMwstSatzZaehler(Convert.ToInt32(TblZlWerte.Rows[liSel][10]), gsConnect, giDb);
                    }
                    // Element holen
                    TextBox t2 = e.EditingElement as TextBox;
                    lsBrutto = t2.Text.ToString();
                    if (lsBrutto.Length > 0 && lsBrutto.Substring(lsBrutto.Length - 1, 1) == "€")
                    {
                        lsBrutto = lsBrutto.Substring(0, lsBrutto.Length - 2);                   // Das Eurozeichen muss raus                            
                    }
                    if (lsBrutto.Length > 0)
                    {
                        ldBrutto = Convert.ToDecimal(lsBrutto);
                        ldNetto = (ldBrutto / (100 + liMwstSatz)) * 100;                            // Nettobetrag
                        DataRowView oDataRowView = DgrCounters.SelectedItem as DataRowView;
                        oDataRowView.Row[5] = ldNetto;
                    }
                }
            }
        }


        // DataGrid Leerstände Item gewählt
        private void DgrLeer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int liExternId = 0;
            int liMieter = 0;
            int liObjekt = 0;
            int liSel = DgrLeer.SelectedIndex;
            int liOk = 0;
            String lsSql = "";

            int liYear = DateTime.Now.Year - 1;
            string lsStart = (liYear.ToString()) + "-01-01";
            string lsEnd = (liYear.ToString()) + "-12-31";
            DateTime ldtFrom = DateTime.Parse(lsStart);                 // Jahresanfang VorJahr
            DateTime ldtTo = DateTime.Parse(lsEnd);

            String lsDateFrom = "";
            String lsDateTo = "";
            String lsIdObj = "";

            if (liSel >= 0)
            {
                // Start und Endedatum wurden gewählt
                if (clTo.SelectedDate.HasValue && clFrom.SelectedDate.HasValue)
                {
                    ldtFrom = clFrom.SelectedDate.Value;
                    lsDateFrom = ldtFrom.ToString("dd-MM-yyyy HH:mm");
                    tbDateFrom.Text = lsDateFrom;

                    ldtTo = clTo.SelectedDate.Value;
                    // Enddatum bis 23:59:59
                    ldtTo = ldtTo.AddHours(23);
                    ldtTo = ldtTo.AddMinutes(59);
                    ldtTo = ldtTo.AddSeconds(59);
                    lsDateTo = ldtTo.ToString("dd-MM-yyyy HH:mm");
                    tbDateTo.Text = lsDateTo;
                }
                // nur das Startdatum wurde gewählt; EndeDatum ist heutiger Tag
                else if (clTo.SelectedDate.HasValue)
                {
                    ldtFrom = clFrom.SelectedDate.Value;
                    lsDateFrom = ldtFrom.ToString("dd-MM-yyyy HH:mm");
                    tbDateFrom.Text = lsDateFrom;
                    ldtTo = DateTime.Today;
                }

                switch (giIndex)
                {
                    case 1:
                        // Daten für Leerstand Details zeigen
                        liObjekt = Int16.Parse(GiObjektId.ToString());
                        lsIdObj = liObjekt.ToString();
                        // Mieter Leerstand ermitteln
                        liMieter = Timeline.GetMieterLeerstandObjekt(liObjekt,gsConnect,giDb);

                        DataRowView rowview = DgrLeer.SelectedItem as DataRowView;
                        // Es ist eine Leerstand gewählt
                        if (rowview.Row[5] != DBNull.Value)
                        {
                            liExternId = Int32.Parse(rowview.Row[5].ToString());
                            if (liExternId > 0)
                            {
                                lsSql = RdQueries.GetSqlSelect(130, liExternId, "5", lsIdObj, "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
                                liOk = FetchData(lsSql, 19, giDb, gsConnect);
                            }
                        }
                        break;
                    case 2:
                        lsIdObj = GiObjektTeilId.ToString();
                        DataRowView rowview1 = DgrLeer.SelectedItem as DataRowView;
                        // Es ist eine Leerstand gewählt
                        if (rowview1.Row[5] != DBNull.Value)
                        {
                            liExternId = Int32.Parse(rowview1.Row[5].ToString());
                            if (liExternId > 0)
                            {
                                // Daten für Leerstand Details zeigen
                                lsSql = RdQueries.GetSqlSelect(130, liExternId, "4", lsIdObj, "",ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
                                liOk = FetchData(lsSql, 19, giDb, gsConnect);
                            }
                        }
                        break;
                    case 3:
                        lsIdObj = GiMieterId.ToString();
                        break;
                    default:
                        break;
                }
            }
        }

        // Zahlungen vom Datepicker wird das Datum benötigt, um nach der Eingabe den aktuellen Mieter zu ermitteln
        private void dpkZlg_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            //DateTime ldtZlg = DateTime.MinValue;

            //ldtZlg = (DateTime)e.AddedItems[0];
            //// Globale Variable für Event DgrZahlungen_CellEditEnding
            //gdtZahlung = ldtZlg;
        }

        // Das Abrechnungsjahr kann gewählt werden
        private void ClYear_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            int liRows = 0;
            string lsSql = "";

            DateTime ldtYear = DateTime.MinValue;
            DateTime ldtFrom = DateTime.MinValue;
            DateTime ldtTo = DateTime.MinValue;

            ldtYear = clYear.SelectedDate.Value;

            gdtYear = ldtYear;      // Clobal

            ldtFrom = Timeline.GetYear(ldtYear, 1);
            ldtTo = Timeline.GetYear(ldtYear, 2);

            tbDateFrom.Text = ldtFrom.ToString("dd-MM-yyyy HH:mm");
            tbDateTo.Text = ldtTo.ToString("dd-MM-yyyy HH:mm");

            // clFrom.DisplayDate = ldtFrom;
            clFrom.SelectedDate = ldtFrom;
            clFrom.DisplayDate = ldtFrom;
            // gdtFrom = ldtFrom;          // Global

            // clTo.DisplayDate = ldtTo;
            clTo.SelectedDate = ldtTo;
            clTo.DisplayDate = ldtTo;
            // gdtTo = ldtTo;              // Global

            // Calender Year aus
            clYear.IsEnabled = false;
            cbYear.IsChecked = false;

            // Treeview befüllen 
            lsSql = RdQueries.GetSqlSelect(2, giFiliale, "", "", "", DateTime.Today, DateTime.Today, giFiliale, gsConnect, giDb);

            // Daten holen 
            liRows = FetchData(lsSql, 2, giDb, gsConnect);                          // Aufruf Art 2 ist Treeview befüllen   

            // Tabelle Leerstand befüllen
            lsSql = RdQueries.GetSqlSelect(211, giFiliale, "", "", "", ldtFrom, ldtTo, giFiliale, gsConnect, giDb);
            liRows = FetchData(lsSql, 18, giDb, gsConnect);

        }

        // Todo Menü Rechnungen importieren
        private void mnImpRg_Click(object sender, RoutedEventArgs e)
        {

        }

        // Menü Zahlungen importieren
        private void mnImpZl_Click(object sender, RoutedEventArgs e)
        {
            // Import der Ascii Datei 
            WndZlgImport frmZlgImp = new WndZlgImport(this);
            DelPassDataArt delegt = new DelPassDataArt(frmZlgImp.getDb);
            delegt(giDb);
            frmZlgImp.ShowDialog();
        }

        // AUSGABEN --------------------------------------------------------------
        // Menü Ausgaben Kosten
        private void mnOutKosten_Click(object sender, RoutedEventArgs e)
        {
            // Sql Statement für die Rechnungen in XML Datei speichern
            updateAllDataGrids(3);

            WndRep frmRep = new WndRep(this);
            DelPassDataArt delegt = new DelPassDataArt(frmRep.getDb);
            delegt(giDb);
            frmRep.ShowDialog();
        }

        // Ausgabe Zahlungen
        private void mnOutZahlungen_Click(object sender, RoutedEventArgs e)
        {
            // Sql Statement für die Zahlungen in XML Datei speichern
            updateAllDataGrids(4);

            WndRep frmRep = new WndRep(this);
            DelPassDataArt delegt = new DelPassDataArt(frmRep.getDb);
            delegt(giDb);
            frmRep.ShowDialog();

        }
        // Ausgabe Abrechnung
        private void mnOutAbrechnungen_Click(object sender, RoutedEventArgs e)
        {
            // Sql Statement für die Nebenkostenabrechnung in XML Datei speichern
            updateAllDataGrids(5);

            WndRep frmRep = new WndRep(this);
            DelPassDataArt delegt = new DelPassDataArt(frmRep.getDb);
            delegt(giDb);
            frmRep.ShowDialog();
        }

        // Ausgabe des Anschreibens
        private void mnOutAnschreiben_Click(object sender, RoutedEventArgs e)
        {
            // Sql Statement für das Anschreiben in XML Datei speichern
            updateAllDataGrids(6);

            WndRep frmRep = new WndRep(this);
            DelPassDataArt delegt = new DelPassDataArt(frmRep.getDb);
            delegt(giDb);
            frmRep.ShowDialog();
        }

        // Nebenkostenabrechung detailliert
        private void mnOutAbrechnungDetail_Click(object sender, RoutedEventArgs e)
        {
            // Sql Statement für das Anschreiben in XML Datei speichern
            updateAllDataGrids(7);
            WndRep frmRep = new WndRep(this);
            DelPassDataArt delegt = new DelPassDataArt(frmRep.getDb);
            delegt(giDb);
            frmRep.ShowDialog();
        }

        // Report Zählerstände
        private void MnOutZaehler_Click(object sender, RoutedEventArgs e)
        {
            // Sql Statement für das Anschreiben in XML Datei speichern
            updateAllDataGrids(8);
            WndRep frmRep = new WndRep(this);
            DelPassDataArt delegt = new DelPassDataArt(frmRep.getDb);
            delegt(giDb);
            frmRep.ShowDialog();
        }
        // Menü Leerstände
        private void MnOutLeerstaende_Click(object sender, RoutedEventArgs e)
        {
            updateAllDataGrids(9);
            WndRep frmRep = new WndRep(this);
            DelPassDataArt delegt = new DelPassDataArt(frmRep.getDb);
            delegt(giDb);
            frmRep.ShowDialog();
        }

        // STAMMDATEN -----------------------------------------------------------
        // Menü Objekte bearbeiten
        private void mnMasterObject_Click(object sender, RoutedEventArgs e)
        {
            WndStammObjekte frmStammObjekte = new WndStammObjekte(this);
            DelPassDataArt delegt = new DelPassDataArt(frmStammObjekte.getDb);
            delegt(giDb);
            frmStammObjekte.ShowDialog();
        }

        // Menü Objektteile bearbeiten
        private void mnMasterObjPart_Click(object sender, RoutedEventArgs e)
        {
            WndStammObjTeile frmStammObjTeile = new WndStammObjTeile(this);
            DelPassDataArt delegt = new DelPassDataArt(frmStammObjTeile.getDb);
            delegt(giDb);
            frmStammObjTeile.ShowDialog();
        }

        // Menü Mieter bearbeiten
        private void mnMasterMieter_Click(object sender, RoutedEventArgs e)
        {
            WndStammMieter frmStammMieter = new WndStammMieter(this);
            DelPassDataArt delegt = new DelPassDataArt(frmStammMieter.getDb);
            delegt(giDb);
            frmStammMieter.ShowDialog();
        }

        // Menü Verträge bearbeiten
        private void mnMasterContract_Click(object sender, RoutedEventArgs e)
        {
            WndStammContract frmStammContract = new WndStammContract(this);
            DelPassDataArt delegt = new DelPassDataArt(frmStammContract.getDb);
            delegt(giDb);
            frmStammContract.ShowDialog();
        }

        // Dialog Kostenarten bearbeiten
        private void mnMasterKsa_Click(object sender, RoutedEventArgs e)
        {
            WndKsa frmKsa = new WndKsa(this);
            DelPassDataArt delegt = new DelPassDataArt(frmKsa.getDb);
            delegt(giDb);
            frmKsa.ShowDialog();
        }

        // Stammdaten Zähler
        private void mnMasterCounter_Click(object sender, RoutedEventArgs e)
        {
            WndStammZaehler frmStZl = new WndStammZaehler(this);
            DelPassDataArt delegt = new DelPassDataArt(frmStZl.getDb);
            delegt(giDb);
            frmStZl.ShowDialog();
        }

        // Dialog Mandanten

        private void MnMasterMandanten_Click(object sender, RoutedEventArgs e)
        {
            WndMandanten frmMnd = new WndMandanten(this);
            DelPassDataArt delegt = new DelPassDataArt(frmMnd.getDb);
            delegt(giDb);
            frmMnd.ShowDialog();
            // Update der Daten nach Mandantenwechsel
            updateAllDataGrids(1);
            tvMain.Items.Clear();
        }

        // Dialog Gesellschaften bearbeiten
        private void mnMasterCompany_Click(object sender, RoutedEventArgs e)
        {
            WndCompanies frmCmp = new WndCompanies(this);
            DelPassDataArt delegt = new DelPassDataArt(frmCmp.getDb);
            delegt(giDb);
            frmCmp.ShowDialog();
            // Update der Daten nach Firmenwechsel
            updateAllDataGrids(1);
            tvMain.Items.Clear();
        }

        // Menü Tracetabelle Vorauszahlungen öffnen
        private void mnInfoZahlungenTrace_Click(object sender, RoutedEventArgs e)
        {
            WndZlgTrace frmZlgTrace = new WndZlgTrace(this);
            DelPassDataArt delegt = new DelPassDataArt(frmZlgTrace.getDb);
            delegt(giDb);
            frmZlgTrace.ShowDialog();
        }

        // Menü SoftwareInfo
        private void mnInfoSoftware_Click(object sender, RoutedEventArgs e)
        {
            WndAboutBox1 frmSoftware = new WndAboutBox1();
            frmSoftware.ShowDialog();
            mnMasterMandanten.IsEnabled = true;
        }

        // Menü Eingaben Tab Kosten anwählen
        private void mnInputCost_Click(object sender, RoutedEventArgs e)
        {
            tbKosten.IsSelected = true;
        }

        // Menü Eingaben Tab Rechnungen anwählen
        private void mnInputAccount_Click(object sender, RoutedEventArgs e)
        {
            TbRechnungen.IsSelected = true;
        }

        // Menü Eingaben Tab Zahlungen anwählen
        private void mnInputPayment_Click(object sender, RoutedEventArgs e)
        {
            tbZahlungen.IsSelected = true;
        }

        // Menü Eingaben Tab Zählerstände anwählen
        private void mnInputCount_Click(object sender, RoutedEventArgs e)
        {
            tbZaehler.IsSelected = true;
        }

        // Menü Eingaben Tab Leerstände anwählen
        private void mnInputEmpty_Click(object sender, RoutedEventArgs e)
        {
            tbLeerstand.IsSelected = true;
        }

        // Pool für Rechnungsnummern bearbeiten
        private void mnInputPool_Click(object sender, RoutedEventArgs e)
        {
            WndPoolRgNr frmPoolRgNr = new WndPoolRgNr(this);

            DelPassDataArt delegt = new DelPassDataArt(frmPoolRgNr.getDb);
            delegt(giDb);

            frmPoolRgNr.ShowDialog();
        }

        // Radiobutton Aktive Mieter zeigen
        private void rbAktEmps_Checked(object sender, RoutedEventArgs e)
        {
            updateAllDataGrids(11);
        }

        // Radiobutton Alle Mieter zeigen
        private void rbAllEmps_Checked(object sender, RoutedEventArgs e)
        {
            updateAllDataGrids(11);
            // updateAllDataGrids(111);
        }
    }
}
