using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using MySql.Data.MySqlClient;

namespace Ruddat_NK
{

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        // Global
        string gsPath = "";                 // DataPath des xml
        String gsConnectString;
        String gsItemHeader = "";           // Gewähltes Item aus dem Treeview
        int giFiliale = 0;                  // Angewählte Firma (Aus xml Konfig, den letzten Wert holen)
        int giObjekt = 0;                   // Objekt global
        int giObjektTeil = 0;               // Objektteil global
        int giMieter = 0;                   // Mieter global
        int giDelId = 0;                    // Rechnungsdatensatz löschen
        int giDelZlId = 0;                  // Zahlungsdatensatz löschen
        int giDelZlWertId = 0;              // Zählerwert löschen
        int giZlId = 0;                     // Zähler Id
        int giTimelineId = 0;               // TimelineId für löschen
        int giFlagTimeline = 0;             // Flag TimeLinebearbeitung
        int giIndex = 0;                    // Index > Objekt, Teil oder Mieter 1,2,3
        int giMwstSatz = 99;                // Mwst Satz ! Null > 0 gibs ja
        int giMwstSatzZl = 99;              // Für Zähler
        int giDb = 2;                       // Datenbank 1 = MsqSql 2= Mysql
        DateTime gdtZahlung = DateTime.MinValue; // Zahlungsdatum aus Datepicker DataGrid Zahlungen

        // Daten
        DataTable tableOne;
        DataTable tableTwo;
        DataTable tableThree;
        DataTable tableFour;
        DataTable tableFive;
        DataTable tableSix;
        DataTable tableSeven;
        DataTable tableZlg;
        DataTable tableVert;
        DataTable tableAbrInfo;
        DataTable tableLeerstand;
        DataTable tableZlWert;
        DataTable tableZlNummer;
        SqlDataAdapter sda;
        SqlDataAdapter sdb;
        SqlDataAdapter sdc;
        SqlDataAdapter sdd;
        SqlDataAdapter sde;
        SqlDataAdapter sdf;
        SqlDataAdapter sdg;
        SqlDataAdapter sdZlg;
        SqlDataAdapter sdVert;
        SqlDataAdapter sdAbrInfo;
        SqlDataAdapter sdLeerstand;
        SqlDataAdapter sdZlWert;
        SqlDataAdapter sdZlNummer;
        MySqlDataAdapter mysda;
        MySqlDataAdapter mysdb;
        MySqlDataAdapter mysdc;
        MySqlDataAdapter mysdd;
        MySqlDataAdapter mysde;
        MySqlDataAdapter mysdf;
        MySqlDataAdapter mysdg;
        MySqlDataAdapter mysdZlg;
        MySqlDataAdapter mysdVert;
        MySqlDataAdapter mysdAbrInfo;
        MySqlDataAdapter mysdLeerstand;
        MySqlDataAdapter mysdZlWert;
        MySqlDataAdapter mysdZlNummer;

        // Datenübergabe an WndChooseSet
        public delegate void delPassData(int giTimelineId);

        public MainWindow()
        {
            int liRows = 0;
            int liOk = 0;
            String lsSql = "";
            String UPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            DateTime ldtWtStart = DateTime.MinValue;
            DateTime ldtWtEnd = DateTime.MinValue;
            DateTime ldtFrom = DateTime.MinValue;
            DateTime ldtTo = DateTime.Today;
            gsPath = UPath;                         // Pfad der Konfigurationsdatei global verfügbar machen

            InitializeComponent();

            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            liOk = DbConnect(UPath);

            // Menüpunkte
            mnImpRg.IsEnabled = false;

            // Kalender erstmal aus
            clFrom.IsEnabled = false;
            clTo.IsEnabled = false;
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

            // Daten für listbox Filiale holen
            lsSql = RdQueries.GetSqlSelect(1, 0, "", "", DateTime.MinValue, DateTime.MinValue,giFiliale,gsConnectString,giDb);
            // Daten holen für Listbox Filiale
            // Sql, Art
            liRows = FetchData(lsSql, 1, giDb);

            // Daten für Treeview holen
            lsSql = RdQueries.GetSqlSelect(2, giFiliale, "", "", DateTime.Today, DateTime.Today, giFiliale, gsConnectString, giDb);
            liRows = FetchData(lsSql, 2, giDb);                                                       // Todo fetchdata ist um Mysql erweitert > Funktionieren alle Sql-Statements? 

            int liYear = DateTime.Now.Year - 1;
            string dt = (liYear.ToString()) + "-01-01";
            ldtFrom = DateTime.Parse(dt);                 // Jahresanfang
            tbDateFrom.Text = ldtFrom.ToString("dd-MM-yyyy HH:mm");

            string sdte = (liYear.ToString()) + "-12-31";
            ldtTo = DateTime.Parse(sdte);
            // Enddatum bis 23:59:59
            ldtTo = ldtTo.AddHours(23);
            ldtTo = ldtTo.AddMinutes(59);
            ldtTo = ldtTo.AddSeconds(59);

            tbDateTo.Text = ldtTo.ToString("dd-MM-yyyy HH:mm");

            Mouse.OverrideCursor = null;
        }

        // Verbindung zur Datenbank
        private int DbConnect(string p)
        {
            string SqlConnectionString = "";
            string MySqlConnectionString = "";
            String PDataPath = p + "\\Ruddat\\Nebenkosten\\";
            String PDataPathFile = "";
            String Server, DbName, Trust, Timeout;
            // string lsSql = "";

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
                xmlmarker = xmldoc.SelectSingleNode("/Konfiguration/Datenbankverbindung/Trust");
                Trust = xmlmarker.InnerText;
                xmlmarker = xmldoc.SelectSingleNode("/Konfiguration/Datenbankverbindung/Timeout");
                Timeout = xmlmarker.InnerText;

                // Datenbankconnect zusammenbauen
                SqlConnectionString = Server + DbName + Trust + Timeout;

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
                    xmlwriter.WriteString("Data Source=server1\\rdnk;");
                    xmlwriter.WriteEndElement();
                    xmlwriter.WriteStartElement("Datenbankname");
                    xmlwriter.WriteString("Initial Catalog=rdnk;");
                    xmlwriter.WriteEndElement();
                    xmlwriter.WriteStartElement("Trust");
                    xmlwriter.WriteString("Integrated Security=True;");
                    xmlwriter.WriteEndElement();
                    xmlwriter.WriteStartElement("Timeout");
                    xmlwriter.WriteString("Connect Timeout=60;");
                    xmlwriter.WriteEndElement();
                    xmlwriter.WriteEndElement();
                    xmlwriter.Close();

                    // Die hier eingetragene Db-Verbindung nehmen
                    SqlConnectionString = "Data Source=server1\rdnk;Initial Catalog=rdnk;Integrated Security=True";

                    //MessageBox.Show("Es wurde eine Standardkonfiguration erzeugt.\n" +
                    //                "Die Serververbindung muss noch überprüft werden\n" +
                    //                "Die Datei heißt:\n" + PDataPath + "ruddat_nk_config.xml\n",
                    //                "Achtung",
                    //                MessageBoxButton.OK);
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
                    SqlConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\udiec\\AppData\\Local\\Ruddat\\Nebenkosten\\rdnk.mdf;Integrated Security=True;Connect Timeout=5";
                    // Für Testzwecke Server Firma
                    // SqlConnectionString = "Data Source=(LocalDB)\\v11.0;AttachDbFilename=G:\\Software\\Ruddat-Nebenkosten\\DbOne\\rdnk.mdf;Integrated Security=True;Connect Timeout=20";
                    MessageBox.Show("Lokale Datenbank MsSql Express wird verwendet", "Achtung! ", MessageBoxButton.OK);
                    break;
                case 2:
                    // MySql Testverbindung
                    MySqlConnectionString = @"server=localhost;userid=rdnk;password=r1d8n9k4!;database=dbo";
                    // MessageBox.Show("Lokale Datenbank MySql wird verwendet", "Achtung! ", MessageBoxButton.OK);
                    break;
                default:
                    break;
            }



            //Globaler ConnectString
            switch (giDb)
            {
                case 1:
                    gsConnectString = SqlConnectionString;
                    break;
                case 2:
                    gsConnectString = MySqlConnectionString;
                    break;
                default:
                    break;
            }



            return (1);
        }

        // Daten aus der Db holen
        private Int32 FetchData(string psSql, int piArt, int aiDb)
        {
            Int32 liRows = 0;
            string lsObjektBez = "", lsObjektTeilBez = "";
            string lsObjektBezS = "";

            switch (aiDb)
            {
                case 1:             // MsSql

                    try
                    {
                        SqlConnection connect;
                        connect = new SqlConnection(gsConnectString);
                        // Pass both strings to a new SqlCommand object.
                        SqlCommand command = new SqlCommand(psSql, connect);
                        // Db open
                        connect.Open();

                        // Daten für Filiale holen
                        if (piArt == 1)
                        {
                            tableThree = new DataTable();   // Filialen
                            sdc = new SqlDataAdapter(command);
                            sdc.Fill(tableThree);
                            lbFiliale.ItemsSource = tableThree.DefaultView;
                        }

                        // Daten für Objekte und Teilobjekte holen ab ins Treeview
                        // Für aktive Verträge
                        if (piArt == 2)
                        {
                            tableFour = new DataTable();    // Objekte Teilobjekte
                            sdd = new SqlDataAdapter(command);
                            sdd.Fill(tableFour);

                            if (tableFour.Rows.Count > 0)
                            {
                                int i = 0;
                                tvMain.Items.Clear();

                                //  Eine Schleife durch die Tabelle, um das Treview zu befüllen
                                foreach (DataRow dr in tableFour.Rows)
                                {
                                    lsObjektBez = tableFour.Rows[i].ItemArray.GetValue(4).ToString().Trim() + ":" + tableFour.Rows[i].ItemArray.GetValue(0).ToString().Trim();
                                    lsObjektTeilBez = tableFour.Rows[i].ItemArray.GetValue(1).ToString();

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

                                    PopulateTree(i, root, tableFour);

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
                            tableFour = new DataTable();    // Objekte Teilobjekte
                            sdd = new SqlDataAdapter(command);
                            sdd.Fill(tableFour);
                            if (tableFour.Rows.Count > 0)
                            {
                                liRows = Convert.ToInt16(tableFour.Rows[0].ItemArray.GetValue(5).ToString());
                            }
                        }

                        // Die Id aus Teilobjekt holen
                        if (piArt == 4)
                        {
                            tableFour = new DataTable();    // Objekte Teilobjekte
                            sdd = new SqlDataAdapter(command);
                            sdd.Fill(tableFour);
                            if (tableFour.Rows.Count > 0)
                            {
                                liRows = Convert.ToInt16(tableFour.Rows[0].ItemArray.GetValue(6).ToString());
                            }
                        }

                        // Die Id aus Mieter holen
                        if (piArt == 5)
                        {
                            tableFour = new DataTable();    // Objekte Teilobjekte
                            sdd = new SqlDataAdapter(command);
                            sdd.Fill(tableFour);
                            if (tableFour.Rows.Count > 0)
                            {
                                liRows = Convert.ToInt16(tableFour.Rows[0].ItemArray.GetValue(7).ToString());
                            }
                        }

                        // DataGrid Timline Summen
                        if (piArt == 8)
                        {
                            tableSeven = new DataTable();   // Timeline Summen 
                            sdg = new SqlDataAdapter(command);
                            sdg.Fill(tableSeven);
                            DgrCost.ItemsSource = tableSeven.DefaultView;
                            liRows = DgrCost.Items.Count;
                        }

                        // Datagrid für Rechnungen
                        if (piArt == 9)
                        {
                            tableOne = new DataTable();     // Rechnungen
                            sda = new SqlDataAdapter(command);
                            sda.Fill(tableOne);
                            DgrRechnungen.ItemsSource = tableOne.DefaultView;
                            liRows = DgrRechnungen.Items.Count;
                        }

                        // ListBox Filiale befüllen
                        if (piArt == 10)
                        {
                            tableThree = new DataTable();
                            sdc = new SqlDataAdapter(command);
                            sdc.Fill(tableThree);
                            lbFiliale.ItemsSource = tableThree.DefaultView;
                        }

                        // Combobox Kostenart in Rechnungen
                        if (piArt == 11)
                        {
                            tableFive = new DataTable();    // Kostenart
                            sde = new SqlDataAdapter(command);
                            sde.Fill(tableFive);
                            kostenart.ItemsSource = tableFive.DefaultView;
                        }

                        // Combobox mwst in Rechnungen
                        if (piArt == 12)
                        {
                            tableSix = new DataTable();     // mwst
                            sdf = new SqlDataAdapter(command);
                            sdf.Fill(tableSix);
                            mwst.ItemsSource = tableSix.DefaultView;                // Rechnungen
                        }

                        // DataGrid Timline Detail
                        if (piArt == 13)
                        {
                            tableTwo = new DataTable();     // Timeline
                            sdb = new SqlDataAdapter(command);
                            sdb.Fill(tableTwo);
                            DgrCostDetail.ItemsSource = tableTwo.DefaultView;
                            liRows = DgrCostDetail.Items.Count;
                        }

                        // DataGrid Zahlungen
                        if (piArt == 14)
                        {
                            tableZlg = new DataTable();     // Zahlungen
                            sdZlg = new SqlDataAdapter(command);
                            sdZlg.Fill(tableZlg);
                            DgrZahlungen.ItemsSource = tableZlg.DefaultView;
                            liRows = DgrZahlungen.Items.Count;
                        }

                        // DataGrid Leerstand Detail
                        if (piArt == 19)
                        {
                            tableLeerstand = new DataTable();     // Timeline
                            sdLeerstand = new SqlDataAdapter(command);
                            sdLeerstand.Fill(tableLeerstand);
                            DgrLeerDetail.ItemsSource = tableLeerstand.DefaultView;
                            liRows = DgrLeerDetail.Items.Count;
                        }

                        // Combobox Kostenart in Zahlungen
                        if (piArt == 15)
                        {
                            tableFive = new DataTable();    // Kostenart
                            sde = new SqlDataAdapter(command);
                            sde.Fill(tableFive);
                            kostenartZlg.ItemsSource = tableFive.DefaultView;
                        }

                        // Combobox Verteilung in Rechnungen und Zähler
                        if (piArt == 16)
                        {
                            tableVert = new DataTable();    // Verteilung Rechnungen
                            sdVert = new SqlDataAdapter(command);
                            sdVert.Fill(tableVert);
                            kostenvert.ItemsSource = tableVert.DefaultView;
                            kostenvertZl.ItemsSource = tableVert.DefaultView;
                        }


                        // Tabelle Infos für Abrechnung
                        if (piArt == 17)
                        {
                            tableAbrInfo = new DataTable();    // Abrechnung
                            sdAbrInfo = new SqlDataAdapter(command);
                            sdAbrInfo.Fill(tableAbrInfo);
                        }

                        // Tabelle Leerstände
                        if (piArt == 18)
                        {
                            tableLeerstand = new DataTable();    // Leerstand
                            sdLeerstand = new SqlDataAdapter(command);
                            sdLeerstand.Fill(tableLeerstand);
                            DgrLeer.ItemsSource = tableLeerstand.DefaultView;
                            liRows = DgrLeer.Items.Count;
                        }

                        // Tabelle Zählerwerte
                        if (piArt == 21)
                        {
                            tableZlWert = new DataTable();    // Zählerwert
                            sdZlWert = new SqlDataAdapter(command);
                            sdZlWert.Fill(tableZlWert);
                            DgrCounters.ItemsSource = tableZlWert.DefaultView;
                            liRows = DgrCounters.Items.Count;
                        }

                        // Combobox Zählernummern
                        if (piArt == 22)
                        {
                            tableZlNummer = new DataTable();    // Kostenart
                            sdZlNummer = new SqlDataAdapter(command);
                            sdZlNummer.Fill(tableZlNummer);
                            zlNummer.ItemsSource = tableZlNummer.DefaultView;
                            zleh.ItemsSource = tableZlNummer.DefaultView;
                            zlmw.ItemsSource = tableZlNummer.DefaultView;
                        }

                        // db close
                        connect.Close();
                    }
                    catch (SqlException ex)
                    {
                        for (int i = 0; i < ex.Errors.Count; i++)
                        {
                            MessageBox.Show("Index #" + i + "\n" +
                                "Error: " + ex.Errors[i].ToString() + "\n", "Achtung");
                        }
                        Console.ReadLine();

                        // Die Anwendung anhalten 
                        MessageBox.Show("Verarbeitungsfehler ERROR fetchdata main MsSQL \n piArt = " + piArt.ToString(),
                                 "Achtung");

                        throw;
                    }

                    break;
                case 2:                                     // MySql
                    try
                    {
                        MySqlConnection con;
                        con = new MySqlConnection(gsConnectString);
                        MySqlCommand com = new MySqlCommand(psSql, con);

                        // Daten für Filiale holen
                        if (piArt == 1)
                        {
                            tableThree = new DataTable();   // Filialen
                            mysdc = new MySqlDataAdapter(com);
                            mysdc.Fill(tableThree);
                            lbFiliale.ItemsSource = tableThree.DefaultView;
                        }

                        // Daten für Objekte und Teilobjekte holen ab ins Treeview
                        // Für aktive Verträge
                        if (piArt == 2)
                        {
                            tableFour = new DataTable();    // Objekte Teilobjekte
                            mysdd = new MySqlDataAdapter(com);
                            mysdd.Fill(tableFour);

                            if (tableFour.Rows.Count > 0)
                            {
                                int i = 0;
                                tvMain.Items.Clear();

                                //  Eine Schleife durch die Tabelle, um das Treview zu befüllen
                                foreach (DataRow dr in tableFour.Rows)
                                {
                                    lsObjektBez = tableFour.Rows[i].ItemArray.GetValue(4).ToString().Trim() + ":" + tableFour.Rows[i].ItemArray.GetValue(0).ToString().Trim();
                                    lsObjektTeilBez = tableFour.Rows[i].ItemArray.GetValue(1).ToString();

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

                                    PopulateTree(i, root, tableFour);

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
                            tableFour = new DataTable();    // Objekte Teilobjekte
                            mysdd = new MySqlDataAdapter(com);
                            mysdd.Fill(tableFour);
                            if (tableFour.Rows.Count > 0)
                            {
                                liRows = Convert.ToInt16(tableFour.Rows[0].ItemArray.GetValue(5).ToString());
                            }
                        }

                        // Die Id aus Teilobjekt holen
                        if (piArt == 4)
                        {
                            tableFour = new DataTable();    // Objekte Teilobjekte
                            mysdd = new MySqlDataAdapter(com);
                            mysdd.Fill(tableFour);
                            if (tableFour.Rows.Count > 0)
                            {
                                liRows = Convert.ToInt16(tableFour.Rows[0].ItemArray.GetValue(6).ToString());
                            }
                        }

                        // Die Id aus Mieter holen
                        if (piArt == 5)
                        {
                            tableFour = new DataTable();    // Objekte Teilobjekte
                            mysdd = new MySqlDataAdapter(com);
                            mysdd.Fill(tableFour);
                            if (tableFour.Rows.Count > 0)
                            {
                                liRows = Convert.ToInt16(tableFour.Rows[0].ItemArray.GetValue(7).ToString());
                            }
                        }

                        // DataGrid Timline Summen
                        if (piArt == 8)
                        {
                            tableSeven = new DataTable();   // Timeline Summen 
                            mysdg = new MySqlDataAdapter(com);
                            mysdg.Fill(tableSeven);
                            DgrCost.ItemsSource = tableSeven.DefaultView;
                            liRows = DgrCost.Items.Count;
                        }

                        // Datagrid für Rechnungen
                        if (piArt == 9)
                        {
                            tableOne = new DataTable();     // Rechnungen
                            mysda = new MySqlDataAdapter(com);
                            mysda.Fill(tableOne);
                            DgrRechnungen.ItemsSource = tableOne.DefaultView;
                            liRows = DgrRechnungen.Items.Count;
                        }

                        // ListBox Filiale befüllen
                        if (piArt == 10)
                        {
                            tableThree = new DataTable();
                            mysdc = new MySqlDataAdapter(com);
                            mysdc.Fill(tableThree);
                            lbFiliale.ItemsSource = tableThree.DefaultView;
                        }

                        // Combobox Kostenart in Rechnungen
                        if (piArt == 11)
                        {
                            tableFive = new DataTable();    // Kostenart
                            mysde = new MySqlDataAdapter(com);
                            mysde.Fill(tableFive);
                            kostenart.ItemsSource = tableFive.DefaultView;
                        }

                        // Combobox mwst in Rechnungen
                        if (piArt == 12)
                        {
                            tableSix = new DataTable();     // mwst
                            mysdf = new MySqlDataAdapter(com);
                            mysdf.Fill(tableSix);
                            mwst.ItemsSource = tableSix.DefaultView;                // Rechnungen
                        }

                        // DataGrid Timline Detail
                        if (piArt == 13)
                        {
                            tableTwo = new DataTable();     // Timeline
                            mysdb = new MySqlDataAdapter(com);
                            mysdb.Fill(tableTwo);
                            DgrCostDetail.ItemsSource = tableTwo.DefaultView;
                            liRows = DgrCostDetail.Items.Count;
                        }

                        // DataGrid Zahlungen
                        if (piArt == 14)
                        {
                            tableZlg = new DataTable();     // Zahlungen
                            mysdZlg = new MySqlDataAdapter(com);
                            mysdZlg.Fill(tableZlg);
                            DgrZahlungen.ItemsSource = tableZlg.DefaultView;
                            liRows = DgrZahlungen.Items.Count;
                        }

                        // DataGrid Leerstand Detail
                        if (piArt == 19)
                        {
                            tableLeerstand = new DataTable();     // Timeline
                            mysdLeerstand = new MySqlDataAdapter(com);
                            mysdLeerstand.Fill(tableLeerstand);
                            DgrLeerDetail.ItemsSource = tableLeerstand.DefaultView;
                            liRows = DgrLeerDetail.Items.Count;
                        }

                        // Combobox Kostenart in Zahlungen
                        if (piArt == 15)
                        {
                            tableFive = new DataTable();    // Kostenart
                            mysde = new MySqlDataAdapter(com);
                            mysde.Fill(tableFive);
                            kostenartZlg.ItemsSource = tableFive.DefaultView;
                        }

                        // Combobox Verteilung in Rechnungen und Zähler
                        if (piArt == 16)
                        {
                            tableVert = new DataTable();    // Verteilung Rechnungen
                            mysdVert = new MySqlDataAdapter(com);
                            mysdVert.Fill(tableVert);
                            kostenvert.ItemsSource = tableVert.DefaultView;
                            kostenvertZl.ItemsSource = tableVert.DefaultView;
                        }

                        // Tabelle Infos für Abrechnung
                        if (piArt == 17)
                        {
                            //TODO prüfen und einsetzen
                            //tableAbrInfo = new DataTable();    // Abrechnung
                            //mysdAbrInfo = new MySqlDataAdapter(com);
                            //mysdAbrInfo.Fill(tableAbrInfo);
                        }

                        // Tabelle Leerstände
                        if (piArt == 18)
                        {
                            tableLeerstand = new DataTable();    // Leerstand
                            mysdLeerstand = new MySqlDataAdapter(com);
                            mysdLeerstand.Fill(tableLeerstand);
                            DgrLeer.ItemsSource = tableLeerstand.DefaultView;
                        }

                        // Tabelle Zählerwerte
                        if (piArt == 21)
                        {
                            tableZlWert = new DataTable();    // Zählerwert
                            mysdZlWert = new MySqlDataAdapter(com);
                            mysdZlWert.Fill(tableZlWert);
                            DgrCounters.ItemsSource = tableZlWert.DefaultView;
                        }

                        // Combobox Zählernummern
                        if (piArt == 22)
                        {
                            tableZlNummer = new DataTable();    // Kostenart
                            mysdZlNummer = new MySqlDataAdapter(com);
                            mysdZlNummer.Fill(tableZlNummer);
                            zlNummer.ItemsSource = tableZlNummer.DefaultView;
                            zleh.ItemsSource = tableZlNummer.DefaultView;
                            zlmw.ItemsSource = tableZlNummer.DefaultView;
                        }

                        // db close
                        con.Close();
                    }
                    catch (MySqlException myex)
                    {
                        // Die Anwendung anhalten 
                        MessageBox.Show("Verarbeitungsfehler ERROR fetchdata main MySql \n piArt = " + piArt.ToString(),
                                 "Achtung");
                        throw;
                    }
                    break;
                default:
                    break;
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

                if (lsObjektBezGet == lsObjektBez )   
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

            if (lbFiliale.SelectedValue != null)
            {
                liFiliale = Convert.ToInt16(lbFiliale.SelectedValue.ToString());
                giFiliale = liFiliale;
            }

            if (liFiliale > 0)
            {
                // Treeview befüllen 
                lsSql = RdQueries.GetSqlSelect(2, liFiliale, "", "", DateTime.Today, DateTime.Today, giFiliale, gsConnectString, giDb);
                // Daten holen 
                liRows = FetchData(lsSql, 2, giDb);                          // Aufruf Art 2 ist Treeview befüllen   

                // Tabelle Leerstand befüllen
                lsSql = RdQueries.GetSqlSelect(211, liFiliale, "", "", DateTime.MinValue, DateTime.MaxValue, giFiliale, gsConnectString, giDb);
                liRows = FetchData(lsSql, 18, giDb);
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

            cbCal.Content = "Kalender anwählen";
            clFrom.IsEnabled = false;
            clTo.IsEnabled = false;
            clFrom.SelectedDate = null;
            clTo.SelectedDate = null;

            int liYear = DateTime.Now.Year - 1;
            string dt = (liYear.ToString()) + "-01-01";
            DateTime ldtFrom = DateTime.Parse(dt);                 // Jahresanfang
            tbDateFrom.Text = ldtFrom.ToString("dd-MM-yyyy HH:mm");

            string sdte = (liYear.ToString()) + "-12-31";
            DateTime ldtTo = DateTime.Parse(sdte);
            // Enddatum bis 23:59:59
            ldtTo = ldtTo.AddHours(23);
            ldtTo = ldtTo.AddMinutes(59);
            ldtTo = ldtTo.AddSeconds(59);

            tbDateTo.Text = ldtTo.ToString("dd-MM-yyyy HH:mm");
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
                // Enddatum bis 23:59:59
                ldtTo = ldtTo.AddHours(23);
                ldtTo = ldtTo.AddMinutes(59);
                ldtTo = ldtTo.AddSeconds(59);
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
            string lsSqlZaehlerstd = "";        // Wird noch für Report Zählerstände benötigt
            string lsSqlTimeline = "";
            string lsSqlTimeline2 = "";
            string lsSqlTimeline3 = "";         // Für das Einsetzen der Rg Nummer in die Timeline
            string lsSqlHeader = "";
            string lsSqlAbrContent = "";
            string lsSqlRgNrAnschreiben = "";
            DateTime ldtFrom = DateTime.MinValue;
            DateTime ldtTo = DateTime.MaxValue;

            // gibt es gewählte Kalender, dann hier Daten einsetzen
            // Sonst der Standardzeitraum
            if (cbCal.IsChecked == true)
            {
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
                }
            }
            else
            {
                // Startdatum ist Jahresbeginn
                int liYear = DateTime.Now.Year - 1;
                string lsStart = (liYear.ToString()) + "-01-01";
                string lsEnd = (liYear.ToString()) + "-12-31";
                ldtFrom = DateTime.Parse(lsStart);                    // Jahresanfang VorJahr
                ldtTo = DateTime.Parse(lsEnd);                        // Jahresende Vorjahr
            }

            if (asArt == 1)
            {
                // Daten für die Anwahl der Firma nur nach Filialänderungen durchführen
                 // Datum ist egal
                // Daten für listbox Filiale holen
                lsSql = RdQueries.GetSqlSelect(1, 0, "", "", DateTime.MinValue, DateTime.MinValue, giFiliale, gsConnectString, giDb);
                // Daten holen für Listbox Filiale
                // Sql, Art
                liRows = FetchData(lsSql, 1, giDb);
                // Daten für Treeview holen
                lsSql = RdQueries.GetSqlSelect(2, giFiliale, "", "", DateTime.Today, DateTime.Today, giFiliale, gsConnectString, giDb);
                liRows = FetchData(lsSql, 2, giDb);
            }

            //  Änderung: Anwahl nur aktive Mieter zeigen
            if (asArt == 11)
            {
                // Daten für Treeview holen
                lsSql = RdQueries.GetSqlSelect(2, giFiliale, "", "", DateTime.Today, DateTime.Today, giFiliale, gsConnectString, giDb);
                liRows = FetchData(lsSql, 2, giDb);
                giIndex = 0;        // Index auf 0 setzen, da ja nix angwählte ist
            }

            ////  Änderung: Anwahl alle Mieter zeigen (auch die ohne Vertrag)
            //if (asArt == 111)
            //{
            //    // Daten für Treeview holen
            //    lsSql = RdQueries.GetSqlSelect(2222, giFiliale, "", "", DateTime.Today, DateTime.Today);
            //    liRows = FetchData(lsSql, 2);
            //}

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
                    lsSql = RdQueries.GetSqlSelect(3, giFiliale, words[1], "1", ldtFrom, ldtTo, giFiliale, gsConnectString, giDb);
                    liId = FetchData(lsSql, 3, giDb);

                    // TimeLine holen für Objekte
                    lsSql = RdQueries.GetSqlSelect(5, liId, "", "", ldtFrom, ldtTo, giFiliale, gsConnectString, giDb);
                    liRows = FetchData(lsSql, 8, giDb);
                    lsSqlTimeline = RdQueries.GetSqlSelect(105, liId, "", "", ldtFrom, ldtTo, giFiliale, gsConnectString, giDb);    // Report

                    // Rechnungen zeigen  Art 8 = Rechungen zeigen für Objekte Datum aktiv
                    lsSql = RdQueries.GetSqlSelect(8, liId, "", "", ldtFrom, ldtTo, giFiliale, gsConnectString, giDb);
                    liRows = FetchData(lsSql, 9, giDb);
                    lsSqlRechnungen = RdQueries.GetSqlSelect(108, liId, "", "", ldtFrom, ldtTo, giFiliale, gsConnectString, giDb);  // Report

                    // Combobox Kostenart in rechnungen befüllen Art = 11 Objekt Kennung 1
                    lsSql = RdQueries.GetSqlSelect(11, liIndex, "", "", ldtFrom, ldtTo, giFiliale, gsConnectString, giDb);
                    liRows = FetchData(lsSql, 11, giDb);                        

                    // Zahlungen zeigen Art 14 Zahlungen für Objekte
                    lsSql = RdQueries.GetSqlSelect(24, liId, "", "", ldtFrom, ldtTo, giFiliale, gsConnectString, giDb);
                    liRows = FetchData(lsSql, 14, giDb);
                    lsSqlZahlungen = RdQueries.GetSqlSelect(124, liId, "", "", ldtFrom, ldtTo, giFiliale, gsConnectString, giDb);   // Report

                    // Zählerstände zeigen Art 34 Objekte
                    lsSql = RdQueries.GetSqlSelect(34, liId, "", "", ldtFrom, ldtTo, giFiliale, gsConnectString, giDb);
                    liRows = FetchData(lsSql, 21, giDb);
                    // lsSqlZaehlerstd = RdQueries.GetSqlSelect(134, liId, "", "", ldtFrom, ldtTo,giFiliale,gsConnectString, giDb);   // Report  Ulf!

                    // Tabelle Leerstand befüllen
                    DgrLeerDetail.ItemsSource = null;
                    lsSql = RdQueries.GetSqlSelect(212, liId, "", "", ldtFrom, ldtTo, giFiliale, gsConnectString, giDb);
                    liRows = FetchData(lsSql, 18, giDb);

                    // Db Header für Report befüllen für Objekte x_abr_info
                    lsSqlHeader = RdQueries.GetSqlSelect(201, liId, "", "", ldtFrom, ldtTo, giFiliale, gsConnectString, giDb);
                    liRows = FetchData(lsSqlHeader, 17, giDb);

                    // Combobox Zählernummern in Zähler
                    lsSql = RdQueries.GetSqlSelect(22, liId, "", "", ldtFrom, ldtTo, giFiliale, gsConnectString, giDb);
                    liRows = FetchData(lsSql, 22, giDb);

                    // Global Objekt Id
                    giObjekt = liId;
                    giObjektTeil = 0;
                    giMieter = 0;

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

                    // Die TeilObjekt ID ermitteln
                    lsSql = RdQueries.GetSqlSelect(3, giFiliale, gsItemHeader, "2", ldtFrom, ldtTo, giFiliale, gsConnectString, giDb);
                    liId = FetchData(lsSql, 4, giDb);

                    // TimeLine holen für ObjektTeile
                    lsSql = RdQueries.GetSqlSelect(6, liId, "", "", ldtFrom, ldtTo, giFiliale, gsConnectString, giDb);
                    liRows = FetchData(lsSql, 8, giDb);
                    lsSqlTimeline = RdQueries.GetSqlSelect(106, liId, "", "", ldtFrom, ldtTo, giFiliale, gsConnectString, giDb);      // Report

                    // Rechnungen zeigen  Art 9 = Rechungen zeigen für Teilobjekte Datum aktiv
                    lsSql = RdQueries.GetSqlSelect(9, liId, "", "", ldtFrom, ldtTo, giFiliale, gsConnectString, giDb);
                    liRows = FetchData(lsSql, 9, giDb);
                    lsSqlRechnungen = RdQueries.GetSqlSelect(109, liId, "", "", ldtFrom, ldtTo, giFiliale, gsConnectString, giDb);  // Report

                    // Zahlungen zeigen Art 15 Zahlungen für ObjektTeile
                    lsSql = RdQueries.GetSqlSelect(25, liId, "", "", ldtFrom, ldtTo, giFiliale, gsConnectString, giDb);
                    liRows = FetchData(lsSql, 14, giDb);
                    lsSqlZahlungen = RdQueries.GetSqlSelect(125, liId, "", "", ldtFrom, ldtTo, giFiliale, gsConnectString, giDb);     // Report

                    // Zählerstände zeigen Art 35 ObjektTeile
                    lsSql = RdQueries.GetSqlSelect(35, liId, "", "", ldtFrom, ldtTo, giFiliale, gsConnectString, giDb);
                    liRows = FetchData(lsSql, 21, giDb);
                    lsSqlZaehlerstd = ""; //RdQueries.GetSqlSelect(135, liId, "", "", ldtFrom, ldtTo,giFiliale,gsConnectString, giDb);   // Report TODO Ulf!

                    // Db Header für Report befüllen für ObjektTeile x_abr_info
                    lsSqlHeader = RdQueries.GetSqlSelect(202, liId, "", "", ldtFrom, ldtTo, giFiliale, gsConnectString, giDb);
                    liRows = FetchData(lsSqlHeader, 17, giDb);

                    // Tabelle Leerstand befüllen
                    DgrLeerDetail.ItemsSource = null;
                    lsSql = RdQueries.GetSqlSelect(213, liId, "", "", ldtFrom, ldtTo, giFiliale, gsConnectString, giDb);
                    liRows = FetchData(lsSql, 18, giDb);

                    // Combobox Kostenart in rechnungen befüllen Art = 11 ObjektTeil Kennung 2
                    lsSql = RdQueries.GetSqlSelect(11, liIndex, "", "", ldtFrom, ldtTo, giFiliale, gsConnectString, giDb);
                    liRows = FetchData(lsSql, 11, giDb);                        

                    // Combobox Zählernummern in Zähler
                    lsSql = RdQueries.GetSqlSelect(222, liId, "", "", ldtFrom, ldtTo, giFiliale, gsConnectString, giDb);
                    liRows = FetchData(lsSql, 22, giDb);

                    // Global TeilObjekt Id
                    giObjekt = 0;
                    giObjektTeil = liId;
                    giMieter = 0;

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

                    // Die Mieter ID ermitteln
                    lsSql = RdQueries.GetSqlSelect(3, giFiliale, gsItemHeader, "3", ldtFrom, ldtTo, giFiliale, gsConnectString, giDb);
                    liId = FetchData(lsSql, 5, giDb);

                    // Die Objekt Id für die Darstellung der ObjektKosten besorgen
                    liObjektIdTmp = Timeline.getIdObj(liId, gsConnectString, 1);

                    // TimeLine holen für Mieter
                    lsSql = RdQueries.GetSqlSelect(7, liId, "", "", ldtFrom, ldtTo, giFiliale, gsConnectString, giDb);
                    liRows = FetchData(lsSql, 8, giDb);
                    lsSqlTimeline = RdQueries.GetSqlSelect(107, liId, "", "", ldtFrom, ldtTo,giFiliale,gsConnectString, giDb);               // Report Nebenkosten Hauptteil
                    lsSqlTimeline2 = RdQueries.GetSqlSelect(116, liObjektIdTmp, "", "", ldtFrom, ldtTo,giFiliale,gsConnectString, giDb);     // Darstellung der ObjektKosten in der NKA
                    lsSqlTimeline3 = RdQueries.GetSqlSelect(140, liId, "" , "", ldtFrom,ldtTo,giFiliale,gsConnectString, giDb);              // Für das Einsetzen der Rechnungsnummer in die Timeline
                    // Rechnungen zeigen  Art 10 = Rechungen zeigen für Mieter Datum aktiv
                    lsSql = RdQueries.GetSqlSelect(10, liId, "", "", ldtFrom, ldtTo,giFiliale,gsConnectString, giDb);
                    liRows = FetchData(lsSql, 9, giDb);
                    lsSqlRechnungen = RdQueries.GetSqlSelect(110, liId, "", "", ldtFrom, ldtTo,giFiliale,gsConnectString, giDb);  // Report

                    // Zahlungen zeigen Art 13 Zahlungen für Mieter
                    lsSql = RdQueries.GetSqlSelect(23, liId, "", "", ldtFrom, ldtTo,giFiliale,gsConnectString, giDb);
                    liRows = FetchData(lsSql, 14, giDb);
                    lsSqlZahlungen = RdQueries.GetSqlSelect(123, liId, "", "", ldtFrom, ldtTo,giFiliale,gsConnectString, giDb);     // Report
                    lsSqlSumme = RdQueries.GetSqlSelect(115, liId, "", "", ldtFrom, ldtTo,giFiliale,gsConnectString, giDb);         // Report Summendarstellung Zahlbetrag

                    // Tabelle Leerstand nicht befüllen, sondern leeren.
                    // Für Mieter gibt es keinen Leerstand
                    DgrLeer.ItemsSource = null;
                    DgrLeerDetail.ItemsSource = null;

                    // Zählerstände gibts nicht für Mieter
                    DgrCounters.ItemsSource = null;

                    // Db Header für Report befüllen für Mieter x_abr_info
                    lsSqlHeader = RdQueries.GetSqlSelect(203, liId, "", "", ldtFrom, ldtTo,giFiliale,gsConnectString, giDb);        // Header
                    liRows = FetchData(lsSqlHeader, 17, giDb);

                    // Combobox Kostenart in rechnungen befüllen Art = 11
                    lsSql = RdQueries.GetSqlSelect(11, liIndex, "", "", ldtFrom, ldtTo,giFiliale,gsConnectString, giDb);
                    liRows = FetchData(lsSql, 11, giDb);		 

                    // Global Mieter Id
                    giObjekt = 0;
                    giObjektTeil = 0;
                    giMieter = liId;
                    break;
                default:
                    break;
            }

            // ID Unabhängige Daten 
            // Combobox Mwst in rechnungen befüllen Art = 11
            lsSql = RdQueries.GetSqlSelect(12, 0, "", "", ldtFrom, ldtTo,giFiliale,gsConnectString, giDb);
            liRows = FetchData(lsSql, 12, giDb);
            // Combobox Kostenverteilung in Rechnungen befüllen Art = 16
            lsSql = RdQueries.GetSqlSelect(16, 0, "", "", ldtFrom, ldtTo,giFiliale,gsConnectString, giDb);
            liRows = FetchData(lsSql, 16, giDb);
            // Combobox Kostenart in Zahlungen befüllen Art = 11/15 Objekt Kennung 4
            lsSql = RdQueries.GetSqlSelect(11, 4, "", "", ldtFrom, ldtTo,giFiliale,gsConnectString, giDb);
            liRows = FetchData(lsSql, 15, giDb);

            // hier die Where Klausel vom Sql-Statement für Reports speichern
            switch (asArt)
            {
                case 3:
                    // Rechnungen
                    Timeline.saveLastSql(lsSqlRechnungen,"","","","","","","","rechnungen","");
                    break;
                case 4:
                    // Zahlungen
                    Timeline.saveLastSql(lsSqlZahlungen,"","","","","","","","zahlungen","");
                    break;
                case 5: 
                    // Nebenkostenabrechnung 
                    // SqlStatement für die Zieltabelle x_abr_content erzeugen Abrechnung
                    // Das Befüllen der Tabelle erfolgt dann in WndRep
                    lsSqlAbrContent = RdQueries.GetSqlSelect(300, liId, "", "", ldtFrom, ldtTo,giFiliale,gsConnectString, giDb);      // Abrechnung Content x_abr_content
                    // Abrechnungen (Kosten,Kostenverteilung,Kostenverteilung Summen,Zahlungen Summe,Personen,Zähler,Art)
                    Timeline.saveLastSql(lsSqlTimeline,lsSqlAbrContent,"",
                            "",lsSqlZahlungen,lsSqlSumme,"",lsSqlTimeline2,"kosten","");       // direkte Kosten
                    Timeline.saveLastVal(ldtFrom, ldtTo, "Datum");                          // Übergabe des Datumsbereiches 
                    break;
                case 6:
                    // Anschreiben
                    // SqlStatement für die Zieltabelle x_abr_content erzeugen Abrechnung
                    // Das Befüllen der Tabelle erfolgt dann in WndRep
                    lsSqlAbrContent = RdQueries.GetSqlSelect(300, liId, "", "", ldtFrom, ldtTo,giFiliale,gsConnectString, giDb);      // Abrechnung Content x_abr_content
                    lsSqlRgNrAnschreiben = RdQueries.GetSqlSelect(140, liId, "", "", ldtFrom, ldtTo,giFiliale,gsConnectString, giDb); // Speichern der Rechnungsnummer Anschreiben
                    // Abrechnungen (Kosten,Kostenverteilung,Kostenverteilung Summen,Zahlungen Summe,Personen,Zähler,Art, Rechnungsnummer Anschreiben)
                    Timeline.saveLastSql(lsSqlTimeline, lsSqlAbrContent, "",
                            "", lsSqlZahlungen, lsSqlSumme, "", lsSqlTimeline2, "anschreiben", lsSqlRgNrAnschreiben);  // direkte Kosten
                    Timeline.saveLastVal(ldtFrom, ldtTo, "Datum");                          // Übergabe des Datumsbereiches 
                    break;
                case 7:
                    // Nebenkostenabrechnung detailliert 
                    // SqlStatement für die Zieltabelle x_abr_content erzeugen Abrechnung
                    // Das Befüllen der Tabelle erfolgt dann in WndRep
                    lsSqlAbrContent = RdQueries.GetSqlSelect(300, liId, "", "", ldtFrom, ldtTo,giFiliale,gsConnectString, giDb);      // Abrechnung Content x_abr_content
                    // Abrechnungen (Kosten,Kostenverteilung,Kostenverteilung Summen,Zahlungen Summe,Personen,Zähler,Art)
                    Timeline.saveLastSql(lsSqlTimeline, lsSqlAbrContent, "",
                            "", lsSqlZahlungen, lsSqlSumme, "", lsSqlTimeline2, "kostendetail","");       // direkte Kosten detailliert
                    Timeline.saveLastVal(ldtFrom, ldtTo, "Datum");                                     // Übergabe des Datumsbereiches 
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
                        // Startdatum ist Jahresbeginn
                        int liYear = DateTime.Now.Year - 1;
                        string lsStart = (liYear.ToString()) + "-01-01";
                        string lsEnd = (liYear.ToString()) + "-12-31";
                        DateTime ldtStart = DateTime.Parse(lsStart);                 // Jahresanfang VorJahr
                        DateTime ldtEnd = DateTime.Parse(lsEnd); 
                    }

                    // Der Index wird nochmal bei TimeLine Details benötigt
                    giIndex = index;

                    // MessageBox.Show("Verarbeitungsfehler ERROR fetchdata fetchdata RdFunctions 0003\n piArt = " + index.ToString(),
                    //          "Achtung");
                    /// Ulf!

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

            SqlCommandBuilder commandBuilder = new SqlCommandBuilder(sda);

            sda.UpdateCommand = commandBuilder.GetUpdateCommand();
            sda.InsertCommand = commandBuilder.GetInsertCommand();

            sda.Update(tableOne);

            // Timeline bearbeiten    Art 1 = Rechnungen
            Timeline.editTimeline(giTimelineId, giFlagTimeline, gsConnectString);

            // Delete Kommando muss extra erzeugt werden
            // Gibt es eine Datensatz ID zum Löschen (button btnRgDel)
            if (giDelId > 0)
            {

                // Den Import aus wt_hours_add löschen
                String lsSql = "Delete from rechnungen Where id_rechnungen = " + giDelId.ToString();

                SqlConnection connect;
                connect = new SqlConnection(gsConnectString);

                SqlCommand command = new SqlCommand(lsSql, connect);

                // import_file
                try
                {
                    // Db open
                    connect.Open();
                    SqlDataReader queryCommandReader = command.ExecuteReader();
                    connect.Close();
                }
                catch
                {
                    MessageBox.Show("In Tabelle Rechnungen konnte nicht gelöscht werden\n" +
                            "Prüfen Sie bitte die Datenbankverbindung\n",
                            "Achtung",
                                MessageBoxButton.OK);
                }
            }

            // Die IDs und Flags zurücksetzen
            giDelId = 0;
            giTimelineId = 0;
            giMwstSatz = 99;

            // save Button Rechnungen wieder aus
            btnRgSave.IsEnabled = false;
            btnRgSave.Content = "Speichern";
            btnRgAdd.IsEnabled = true;
            updateAllDataGrids(0);
        }

        // Rechnungen Beginn Eingabe
        private void DgrRechnungen_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {

            // gewählten Datensatz ermitteln
            int liTimelineId = 0;

            int liSel = DgrRechnungen.SelectedIndex;
            if (liSel >= 0)
            {
                DataRow dr = tableOne.Rows[liSel];
                if (dr[14] != DBNull.Value)
                {
                    liTimelineId = Int32.Parse(dr[14].ToString());           // TimeLine ID holen
                }
                giTimelineId = liTimelineId;
                giFlagTimeline = 1;                                         // 1 = Rechnung bearbeiten

            }

            // Save Button auf
            btnRgSave.IsEnabled = true;
        }

        // Rechnungen Button zufügen
        private void btnRgAdd_Click(object sender, RoutedEventArgs e)
        {

            int liTimelineId = 0;
            int liRows = tableOne.Rows.Count;

            // ID für Timeline ermitteln Art 1 = Rechnungs ID
            liTimelineId = Timeline.getTimelineId(gsConnectString,1) + 1;

            DataRow dr = tableOne.NewRow();
            dr[8] = giObjekt;
            dr[9] = giObjektTeil;
            dr[10] = giMieter;
            dr[14] = liTimelineId;      // ID für Timeline
            dr[15] = 1;                 // Flag für Timelinebearbeitung erzeugen

            // Datum vorbelegen erst ab dem 2 ten Datensatz
            if (liRows > 0)
            {
                dr[2] = tableOne.Rows[liRows-1][2];       // Rechnungsdatum
                dr[3] = tableOne.Rows[liRows-1][3];       // Start Datum
                dr[4] = tableOne.Rows[liRows-1][4];       // Ende Datum
            }

            tableOne.Rows.Add(dr);

            btnRgAdd.IsEnabled = false;

        }

        // Rechnungen Button löschen
        private void btnRgDel_Click(object sender, RoutedEventArgs e)
        {
            int liTimelineId = 0;

            int liSel = DgrRechnungen.SelectedIndex;
            if (liSel >= 0)
            {

                DataRow dr = tableOne.Rows[liSel];
                giDelId = (int)(dr[0]);                // Id des zu löschenden Datensatzes


                if ( dr[14] != DBNull.Value)
                {
                    liTimelineId = (int)dr[14];          // TimeLine ID holen                    
                    giTimelineId = liTimelineId;
                    tableOne.Rows.Remove(dr);

                    btnRgSave.Content = "wirklich löschen?";
                    btnRgSave.IsEnabled = true;
                    btnRgAdd.IsEnabled = false;

                    giFlagTimeline = 2;                 // Rechnung löschen
                    // delete Button zu
                    btnRgDel.IsEnabled = false;
                }
            }
        }

        // Zahlung Save
        private void btnZlSave_Click(object sender, RoutedEventArgs e)
        {
            int liOk = 0;

            SqlCommandBuilder commandBuilder = new SqlCommandBuilder(sdZlg);

            sdZlg.UpdateCommand = commandBuilder.GetUpdateCommand();
            sdZlg.InsertCommand = commandBuilder.GetInsertCommand();

            sdZlg.Update(tableZlg);

            // Timeline bearbeiten Art 2 = Zahlungen   
            Timeline.editTimeline(giTimelineId, giFlagTimeline, gsConnectString);

            // Delete Kommando muss extra erzeugt werden
            // Gibt es eine Datensatz ID zum Löschen (button btnRgDel)
            if (giDelZlId > 0)
            {
                // Den Import aus wt_hours_add löschen
                String lsSql = "Delete from zahlungen Where id_vz = " + giDelZlId.ToString();

                SqlConnection connect;
                connect = new SqlConnection(gsConnectString);

                SqlCommand command = new SqlCommand(lsSql, connect);

                // import_file
                try
                {
                    // Db open
                    connect.Open();
                    SqlDataReader queryCommandReader = command.ExecuteReader();
                    connect.Close();
                }
                catch
                {
                    MessageBox.Show("In Tabelle Zahlungen konnte nicht gelöscht werden\n" +
                            "Prüfen Sie bitte die Datenbankverbindung\n",
                            "Achtung btnZlSave_Click",
                                MessageBoxButton.OK);
                }
            }

            // Update der Daten
            liOk = updateAllDataGrids(0);

            // Die IDs und Flags zurücksetzen
            giDelZlId = 0;
            giTimelineId = 0;

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
            int liRows = tableZlg.Rows.Count;
            DateTime ldtZlg = DateTime.MinValue;

            // ID für Timeline ermitteln Art 2 = Zahlungs ID
            liTimelineId = Timeline.getTimelineId(gsConnectString,2) + 1;

            // Kostenart ID ermitteln Art 1 = Nebenkostenzahlungen
            liNkId = Timeline.getKsaId(1,gsConnectString);

            DataRow dr = tableZlg.NewRow();
            dr[2] = giObjekt;
            dr[3] = giObjektTeil;
            dr[1] = giMieter;
            dr[10] = liTimelineId;      // ID für Timeline
            dr[11] = 1;                 // Flag für Timelinebearbeitung erzeugen
            dr[12] = liNkId;            // Kostenart Nebenkosten

            // Datum vorbelegen erst ab dem 2 ten Datensatz
            // Der neueste ist immer der oberste 0
            if (liRows > 0 && tableZlg.Rows[0][4] != DBNull.Value)
            {
                ldtZlg = Convert.ToDateTime(tableZlg.Rows[0][4]);
                dr[4] = ldtZlg.AddMonths(1);       // Ende Datum

                if (tableZlg.Rows[0][6] != DBNull.Value)   // Netto
                {
                    dr[6] = tableZlg.Rows[0][6];
                }

                if (tableZlg.Rows[0][7] != DBNull.Value)   // Brutto
                {
                    dr[7] = tableZlg.Rows[0][7];
                }

                giTimelineId = liTimelineId;
                giFlagTimeline = 11;                                         // 11 = Zahlung bearbeiten

                btnZlSave.IsEnabled = true;
            }

            tableZlg.Rows.Add(dr);
            btnZlAdd.IsEnabled = false;
        }

        // Zahlung löschen
        private void btnZlDel_Click(object sender, RoutedEventArgs e)
        {
            int liTimelineId = 0;

            int liSel = DgrZahlungen.SelectedIndex;
            if (liSel >= 0)
            {

                DataRow dr = tableZlg.Rows[liSel];
                giDelZlId = (int)(dr[0]);                // Id des zu löschenden Datensatzes


                if (dr[10] != DBNull.Value)
                {
                    liTimelineId = (int)dr[10];          // TimeLine ID holen                    
                    giTimelineId = liTimelineId;
                    tableZlg.Rows.Remove(dr);

                    btnZlSave.Content = "wirklich löschen?";
                    btnZlSave.IsEnabled = true;
                    btnZlAdd.IsEnabled = false;

                    giFlagTimeline = 12;                // 12 = Zahlung löschen
                    // delete Button zu
                    btnZlDel.IsEnabled = false;
                }
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

                DataRow dr = tableZlWert.Rows[liSel];
                giDelZlWertId = (int)(dr[0]);                // Id des zu löschenden Datensatzes


                if (dr[7] != DBNull.Value || liTest == 1)
                {
                    liTimelineId = (int)dr[7];          // TimeLine ID holen                    
                    giTimelineId = liTimelineId;
                    tableZlWert.Rows.Remove(dr);

                    btnCntSave.Content = "wirklich löschen?";
                    btnCntSave.IsEnabled = true;
                    btnCntAdd.IsEnabled = false;

                    giFlagTimeline = 22;                 // Zählerwert löschen
                    // delete Button zu
                    btnCntDel.IsEnabled = false;
                }
            }
        }

        // Zählerstand zufügen
        private void btnCntAdd_Click(object sender, RoutedEventArgs e)
        {
            int liTimelineId = 0;
            int liKsaId = 0;

            // ID für Timeline ermitteln Art
            liTimelineId = Timeline.getTimelineId(gsConnectString, 3) + 1;
            // KostenstellenartId Zähler ermitteln
            liKsaId = Timeline.getKsaId(2, gsConnectString);

            // Nur wenn das Grid DgrCounters erzeugt wurde
            // Zählerstand ermöglichen
            if (DgrCounters.ItemsSource != null)
            {
                DataRow dr = tableZlWert.NewRow();

                tableZlWert.Rows.Add(dr);
                dr[7] = liTimelineId;       // ID für Timeline
                dr[8] = giObjekt;           // Objekt
                dr[9] = giObjektTeil;       // Teilobjekt
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

            SqlCommandBuilder commandBuilder = new SqlCommandBuilder(sdZlWert);

            sdZlWert.UpdateCommand = commandBuilder.GetUpdateCommand();
            sdZlWert.InsertCommand = commandBuilder.GetInsertCommand();

            sdZlWert.Update(tableZlWert);

            // Timeline bearbeiten Art 21 = Zähler   
            Timeline.editTimeline(giTimelineId, giFlagTimeline, gsConnectString);

            // Delete Kommando muss extra erzeugt werden
            // Gibt es eine Datensatz ID zum Löschen (button btnCntDel)
            if (giDelZlWertId > 0)
            {
                // Den Zählerstand löschen
                String lsSql = "Delete from zaehlerstaende Where id_zs = " + giDelZlWertId.ToString();

                SqlConnection connect;
                connect = new SqlConnection(gsConnectString);

                SqlCommand command = new SqlCommand(lsSql, connect);

                // import_file
                try
                {
                    // Db open
                    connect.Open();
                    SqlDataReader queryCommandReader = command.ExecuteReader();
                    connect.Close();
                }
                catch
                {
                    MessageBox.Show("In Tabelle Zählerstände konnte nicht gelöscht werden\n" +
                            "Prüfen Sie bitte die Datenbankverbindung\n",
                            "Achtung btnCntSave_Click",
                                MessageBoxButton.OK);
                }
            }

            // Update der Daten
            liOk = updateAllDataGrids(0);

            // Die IDs und Flags zurücksetzen
            giDelZlWertId = 0;
            giZlId = 0;                 // globale Zähler Id
            giTimelineId = 0;
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
                    lsArtVertKurz = Timeline.getVerteilungFromString(gsConnectString, lsArtVerteilung);
                    // Wurde eine Bedingte Verteilung gewählt? Auswahlformular öffnen?
                    if (lsArtVertKurz == "fa")
                    {

                        // Objekt Mix neu anlegen mit Objekt ID und 
                        liOk = Timeline.makeChoose(giObjekt,giTimelineId,gsConnectString);
                        // Objekt Mix Parts auswählen
                        WndChooseSet frmChooseSet = new WndChooseSet(this);

                        // Übergabe der TimeLine ID an das Auswahlfenster
                        delPassData delegt = new delPassData(frmChooseSet.getTimelineId);
                        delegt(giTimelineId);
                        // Übergabe der Objekt ID
                        delPassData delegt2 = new delPassData(frmChooseSet.getObjektId);
                        delegt2(giObjekt);
                        // Übergabe, ob Datensatz existiert oder wurde neu angelegt 1,2
                        delPassData delegt3 = new delPassData(frmChooseSet.getArt);
                        delegt3(liOk);

                        frmChooseSet.ShowDialog();                            
                    }
                }

                if (x == 7)     // MwstFeld in globale Variable AUSNAHMSWEISE
                {
                    lsMwstSatz = getCurrentCellValue((ComboBox)e.EditingElement);
                    liMwstSatz = Convert.ToInt16(lsMwstSatz);
                    giMwstSatz = liMwstSatz;

                }

                // Steht etwas im Feld Mehrwertsteuer?
                if (((DgrRechnungen.Items[liSel] as DataRowView).Row.ItemArray[7] != DBNull.Value) || giMwstSatz != 99 )
                {

                    if (x == 8)     // NettoPreis !! Achtung: Der Displayindex ist die Darstellung im 
                                                        // DGR und nicht die Itemliste
                    {
                        // Hier wird die Zelle des DataGrid ausgelesen, oder bei NewRow der Wert aus der globalen Variablen geholt
                        if (giMwstSatz == 99)
                        {
                            liMwstArt = Int32.Parse((DgrRechnungen.Items[liSel] as DataRowView).Row.ItemArray[7].ToString()); // Art Mehrwertsteuer
                            liMwstSatz = Timeline.getMwstSatz(liMwstArt, gsConnectString);
                        }
                        else
                        {
                            liMwstSatz = giMwstSatz;
                        }

                        // Element holen
                        TextBox t1 = e.EditingElement as TextBox;
                        lsNetto = t1.Text.ToString();

                        if (lsNetto.Length > 0 && lsNetto.Substring(lsNetto.Length-1,1) == "€")                             // Das Eurozeichen muss raus
                        {
                            lsNetto = lsNetto.Substring(0, lsNetto.Length - 2);
                        }
                        if (lsNetto.Length > 0)
                        {
                            ldNetto = Convert.ToDecimal(lsNetto);
                            ldBrutto = ldNetto + (ldNetto / 100) * liMwstSatz;                                          // Bruttobetrag
                            if (ldNetto > 0)
                            {
                                tableOne.Rows[liSel][6] = ldBrutto;                                                                
                            }
                        }

                    }
                    if (x == 9)     // Brutto
                    {
                        // Hier wird die Zelle des DataGrid ausgelesen, oder bei NewRow der Wert aus der globalen Variablen geholt
                        if (giMwstSatz == 99)
                        {
                            liMwstArt = Int32.Parse((DgrRechnungen.Items[liSel] as DataRowView).Row.ItemArray[7].ToString()); // Art Mehrwertsteuer                            
                            liMwstSatz = Timeline.getMwstSatz(liMwstArt, gsConnectString);
                        }
                        else
                        {
                            liMwstSatz = giMwstSatz;
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
                            if (ldBrutto > 0)
                            {
                                tableOne.Rows[liSel][5] = ldNetto;                                    
                            }
                        }
                    }
                }
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
                        lsIdObj = giObjekt.ToString();
                        break;
                    case 2:
                        lsIdObj = giObjektTeil.ToString();
                        break;
                    case 3:
                        lsIdObj = giMieter.ToString();
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
                        // Daten für Deatils zeigen
                        lsSql = RdQueries.GetSqlSelect(13, liExternId, giIndex.ToString(), lsIdObj, ldtFrom, ldtTo,giFiliale,gsConnectString, giDb);
                        liOk = FetchData(lsSql, 13, giDb);
                    }
                }
                // Es ist eine Zahlung gewählt
                if (rowview.Row[6] != DBNull.Value)
                {
                    liExternId = Int32.Parse(rowview.Row[6].ToString());
                    if (liExternId > 0)
                    {
                        // Daten für Deatils zeigen
                        lsSql = RdQueries.GetSqlSelect(13, liExternId, giIndex.ToString(), lsIdObj, ldtFrom, ldtTo,giFiliale,gsConnectString, giDb);
                        liOk = FetchData(lsSql, 13, giDb);
                    }
                }
                // Es ist ein Zaehlerstand gewählt
                if (rowview.Row[9] != DBNull.Value)
                {
                    liExternId = Int32.Parse(rowview.Row[9].ToString());
                    if (liExternId > 0)
                    {
                        // Daten für Deatils zeigen
                        lsSql = RdQueries.GetSqlSelect(13, liExternId, giIndex.ToString(), lsIdObj, ldtFrom, ldtTo,giFiliale,gsConnectString, giDb);
                        liOk = FetchData(lsSql, 13, giDb);
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
            get { return gsConnectString; }
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
                if (x == 1 && tableZlg.Rows[liSel][3] != DBNull.Value)        // Teilobjekt ID ist vorhanden
                {
                    if ((int)tableZlg.Rows[liSel].ItemArray.GetValue(3) >= 0)
                    {
                        liObjTeilId = (int)tableZlg.Rows[liSel].ItemArray.GetValue(3);
                    }
                }

                if (x == 2)     
                // NettoPreis !! Achtung: Der Displayindex ist die Darstellung im 
                // DGR und nicht die Itemliste
                {
                    // MwstSatz holen
                    liMwstSatz = Timeline.getMwstFromBez("normal", gsConnectString);
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
                        tableZlg.Rows[liSel][7] = ldBrutto;
                    }
                }
                if (x == 3)     // Brutto
                {
                    // Hier wird die Zelle des DataGrid ausgelesen, oder bei NewRow der Wert aus der globalen Variablen geholt
                    // MwstSatz holen
                    liMwstSatz = Timeline.getMwstFromBez("normal", gsConnectString);
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
                        tableZlg.Rows[liSel][6] = ldNetto;                        
                    }
                }

                if (x == 4)     // Netto Soll !! Achtung: Der Displayindex ist die Darstellung im 
                // DGR und nicht die Itemliste
                {
                    // MwstSatz holen
                    liMwstSatz = Timeline.getMwstFromBez("normal", gsConnectString);
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
                        tableZlg.Rows[liSel][9] = ldBrutto;
                    }

                }
                if (x == 5)     // Brutto Soll
                {
                    // Hier wird die Zelle des DataGrid ausgelesen, oder bei NewRow der Wert aus der globalen Variablen geholt
                    // MwstSatz holen
                    liMwstSatz = Timeline.getMwstFromBez("normal", gsConnectString);
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
                        tableZlg.Rows[liSel][8] = ldNetto;
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
                DataRow dr = tableZlg.Rows[liSel];
                if (dr[10] != DBNull.Value)
                {
                    liTimelineId = Int32.Parse(dr[10].ToString());           // TimeLine ID holen
                }
                giTimelineId = liTimelineId;
                giFlagTimeline = 11;                                         // 11 = Zahlung bearbeiten

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
                DataRow dr = tableZlWert.Rows[liSel];
                if (dr[8] != DBNull.Value)
                {
                    liTimelineId = Int32.Parse(dr[7].ToString());                  // TimeLine ID holen
                    giTimelineId = liTimelineId;
                    giFlagTimeline = 21;                                           // 21 = Zähler bearbeiten
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
                    liZlId = Timeline.getZlId(lsZlName, gsConnectString);
                    giZlId = liZlId;
                }

                if (x == 3)     // Zählerstand wurde eingegeben
                {
                    TextBox t2 = e.EditingElement as TextBox;
                    lsZlStand = t2.Text.ToString();
                    if (lsZlStand.Length > 0)
                    {
                        ldZlStand = Convert.ToDecimal(lsZlStand);
                        if (tableZlWert.Rows[liSel][10] != DBNull.Value)                // Zähler Id aus DataGrid
                        {
                            liZlId = Convert.ToInt32(tableZlWert.Rows[liSel][10]);      // Zähler Id  
                            liFlagNew = 0;  // Datensatz wird editiert
                        }
                        if (giZlId > 0)     // Zähler Id aus globaler Variable
                        {
                            liZlId = giZlId;
                            liFlagNew = 1;  // Neuer Datensatz
                        }

                        if (liZlId > 0)
                        {
                            ldVerbrauch = Timeline.getZlVerbrauch(ldZlStand, liZlId, gsConnectString, liFlagNew);
                            tableZlWert.Rows[liSel][3] = ldVerbrauch;                            
                        }
                    }
                }

                // x == 5 ist die Einheit

                if (x == 6)     // NettoPreis !! Achtung: Der Displayindex ist die Darstellung im 
                // DGR und nicht die Itemliste
                {
                    // MwstSatz holen
                    if (tableZlWert.Rows[liSel][10] == DBNull.Value && giZlId >= 0)
                    {
                        liMwstSatz = Timeline.getMwstSatzZaehler(giZlId, gsConnectString);
                    }
                    else
                    {
                        liMwstSatz = Timeline.getMwstSatzZaehler(Convert.ToInt32(tableZlWert.Rows[liSel][10]), gsConnectString);
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
                        tableZlWert.Rows[liSel][6] = ldBrutto;
                    }
                }
                if (x == 7)     // Brutto
                {
                    // MwstSatz holen
                    if (tableZlWert.Rows[liSel][10] == DBNull.Value && giZlId >= 0)
                    {
                        liMwstSatz = Timeline.getMwstSatzZaehler(giZlId, gsConnectString);
                    }
                    else
                    {
                        liMwstSatz = Timeline.getMwstSatzZaehler(Convert.ToInt32(tableZlWert.Rows[liSel][10]), gsConnectString);
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
                        tableZlWert.Rows[liSel][5] = ldNetto;
                    }
                }
            }
        }


        // DataGrid Leerstände Item gewählt
        private void DgrLeer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int liExternId = 0;
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
                        lsIdObj = giObjekt.ToString();
                        break;
                    case 2:
                        lsIdObj = giObjektTeil.ToString();
                        break;
                    case 3:
                        lsIdObj = giMieter.ToString();
                        break;
                    default:
                        break;
                }

                DataRowView rowview = DgrLeer.SelectedItem as DataRowView;
                // Es ist eine Leerstand gewählt
                if (rowview.Row[5] != DBNull.Value)
                {
                    liExternId = Int32.Parse(rowview.Row[5].ToString());
                    if (liExternId > 0)
                    {
                        // Daten für Leerstand Details zeigen
                        lsSql = RdQueries.GetSqlSelect(13, liExternId, "4", lsIdObj, ldtFrom, ldtTo,giFiliale,gsConnectString, giDb);
                        liOk = FetchData(lsSql, 19, giDb);
                    }
                }
            }
        }

        // Zahlungen vom Datepicker wird das Datum benötigt, um nach der Eingabe den aktuellen Mieter zu ermitteln
        private void dpkZlg_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime ldtZlg = DateTime.MinValue;

            ldtZlg =  (DateTime)e.AddedItems[0];

            // Globale Variable für Event DgrZahlungen_CellEditEnding
            gdtZahlung = ldtZlg;
        }

        // Menü Rechnungen importieren noch nicht TODO Ulf!
        private void mnImpRg_Click(object sender, RoutedEventArgs e)
        {

        }

        // Menü Zahlungen importieren
        private void mnImpZl_Click(object sender, RoutedEventArgs e)
        {
            // Import der Ascii Datei 
            WndZlgImport frmZlgImp = new WndZlgImport(this);
            frmZlgImp.ShowDialog();
        }

        // AUSGABEN --------------------------------------------------------------
        // Menü Ausgaben Kosten
        private void mnOutKosten_Click(object sender, RoutedEventArgs e)
        {
            // Sql Statement für die Rechnungen in XML Datei speichern
            updateAllDataGrids(3);
            
            WndRep frmRep = new WndRep(this);
            frmRep.ShowDialog();
        }

        // Ausgabe Zahlungen
        private void mnOutZahlungen_Click(object sender, RoutedEventArgs e)
        {
            // Sql Statement für die Zahlungen in XML Datei speichern
            updateAllDataGrids(4);

            WndRep frmRep = new WndRep(this);
            frmRep.ShowDialog();

        }
        // Ausgabe Abrechnung
        private void mnOutAbrechnungen_Click(object sender, RoutedEventArgs e)
        {
            // Sql Statement für die Nebenkostenabrechnung in XML Datei speichern
            updateAllDataGrids(5);

            WndRep frmRep = new WndRep(this);
            frmRep.ShowDialog();
        }

        // Ausgabe des Anschreibens
        private void mnOutAnschreiben_Click(object sender, RoutedEventArgs e)
        {
            // Sql Statement für das Anschreiben in XML Datei speichern
            updateAllDataGrids(6);

            WndRep frmRep = new WndRep(this);
            frmRep.ShowDialog();
        }

        // Nebenkostenabrechung detailliert
        private void mnOutAbrechnungDetail_Click(object sender, RoutedEventArgs e)
        {
            // Sql Statement für das Anschreiben in XML Datei speichern
            updateAllDataGrids(7);

            WndRep frmRep = new WndRep(this);
            frmRep.ShowDialog();
        }

        // STAMMDATEN -----------------------------------------------------------
        // Menü Objekte bearbeiten
        private void mnMasterObject_Click(object sender, RoutedEventArgs e)
        {
            WndStammObjekte frmStammObjekte = new WndStammObjekte(this);
            frmStammObjekte.ShowDialog();
        }

        // Menü Objektteile bearbeiten
        private void mnMasterObjPart_Click(object sender, RoutedEventArgs e)
        {
            WndStammObjTeile frmStammObjTeile = new WndStammObjTeile(this);
            frmStammObjTeile.ShowDialog();
        }

        // Menü Mieter bearbeiten
        private void mnMasterMieter_Click(object sender, RoutedEventArgs e)
        {
            WndStammMieter frmStammMieter = new WndStammMieter(this);
            frmStammMieter.ShowDialog();
        }

        // Menü Verträge bearbeiten
        private void mnMasterContract_Click(object sender, RoutedEventArgs e)
        {
            WndStammContract frmStammContract = new WndStammContract(this);
            frmStammContract.ShowDialog();
        }

        // Dialog Kostenarten bearbeiten
        private void mnMasterKsa_Click(object sender, RoutedEventArgs e)
        {
            WndKsa frmKsa = new WndKsa(this);
            frmKsa.ShowDialog();
        }

        // Stammdaten Zähler
        private void mnMasterCounter_Click(object sender, RoutedEventArgs e)
        {
            WndStammZaehler frmStZl = new WndStammZaehler(this);
            frmStZl.ShowDialog();
        }

        // Dialog Gesellschaften bearbeiten
        private void mnMasterCompany_Click(object sender, RoutedEventArgs e)
        {

            WndCompanies frmCmp = new WndCompanies(this);
            frmCmp.ShowDialog();

            // Update der Daten nach Firmenwechsel
            updateAllDataGrids(1);
            tvMain.Items.Clear();
        }

        // Menü Tracetabelle Vorauszahlungen öffnen
        private void mnInfoZahlungenTrace_Click(object sender, RoutedEventArgs e)
        {
            WndZlgTrace frmZlgTrace = new WndZlgTrace(this);
            frmZlgTrace.ShowDialog();
        }

        // Menü SoftwareInfo
        private void mnInfoSoftware_Click(object sender, RoutedEventArgs e)
        {
            WndAboutBox1 frmSoftware = new WndAboutBox1();
            frmSoftware.ShowDialog();
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
