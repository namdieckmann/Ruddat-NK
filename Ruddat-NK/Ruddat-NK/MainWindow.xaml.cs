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

namespace Ruddat_NK
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Global
        string gsPath = "";                 // DataPath des xml
        String gsConnectString;
        int giEmpId = 0;                    // Emp ID Global (Mieter)

        public MainWindow()
        {
            int liRows = 0;
            int liRow = 0;
            int liEmpId = 0;
            int liOk = 0;
            String lsEmpId = "";
            String lsSql = "";
            String UPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            DateTime ldtWtStart = DateTime.MinValue;
            DateTime ldtWtEnd = DateTime.MinValue;
            gsPath = UPath;             // Pfad der Konfigurationsdatei global verfügbar machen
            InitializeComponent();

            liOk = DbConnect(UPath);

            // SqlSelects zusammenstellen
            // Daten für listbox Filiale holen
            lsSql = getSqlFiliale();

            // Daten holen für location Listbox
            liRows = fetchData(lsSql, 10);

            // Kalender erstmal aus
            clFrom.IsEnabled = false;
            clTo.IsEnabled = false;
            // restliche Checkboxen erstmal aus
            cbLoc.IsEnabled = false;
            cbName.IsEnabled = false;
        }

        // Daten aus der Db holen
        private Int32 fetchData(string asSql, int aiArt)
        {
            Int32 liRows = 0;
            SqlConnection connect;

            DataTable table_1 = new DataTable();       // Grid
            connect = new SqlConnection(gsConnectString);

            try
            {

                // Db open
                connect.Open();

                // Pass both strings to a new SqlCommand object.
                SqlCommand command = new SqlCommand(asSql, connect);

                // Create a SqlDataReader
                SqlDataReader queryCommandReader = command.ExecuteReader();

                // Create a DataTable object to hold all the data returned by the query.
                table_1.Load(queryCommandReader);

                // Datagrid für Employees
                if (aiArt == 1)
                {
                    //DgrEmployee.ItemsSource = table_1.DefaultView;
                    //liRows = DgrEmployee.Items.Count;    
                }

                // Datagrid für Hours
                if (aiArt == 2)
                {
                    //DgrHours.ItemsSource = table_1.DefaultView;
                    //liRows = DgrHours.Items.Count;
                }

                // Datagrid für Hours Summe
                if (aiArt == 3)
                {
                    //DgrHoursSum.ItemsSource = table_1.DefaultView;
                    //liRows = DgrHoursSum.Items.Count;
                }

                // Datagrid für HoursAdd
                if (aiArt == 4)
                {
                    //DgrHoursAdd.ItemsSource = table_1.DefaultView;
                    //liRows = DgrHoursAdd.Items.Count;
                }

                // Datagrid für HoursAddSum
                if (aiArt == 5)
                {
                    //DgrHoursAddSum.ItemsSource = table_1.DefaultView;
                    //liRows = DgrHoursAddSum.Items.Count;
                }

                // ListBox Filiale befüllen
                if (aiArt == 10)
                {
                    lbFiliale.ItemsSource = table_1.DefaultView;
                }

                 // db close
                connect.Close();

            }
            catch
            {
                // Die Anwendung anhalten 
                MessageBox.Show("Verarbeitungsfehler ERROR fetchdata main 0001\n",
                        "Achtung");
            }
            return (liRows);
        }
        
        // SQL-Statement Filiale bauen
        private string getSqlFiliale()
        {
            string lsSql = "";

            lsSql = "Select id_filiale,name from filiale";

            return lsSql;
        }


        // Verbindung zur Datenbank
        private int DbConnect(string p)
        {
            string SqlConnectionString = "";
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

                    XmlTextWriter xmlwriter = new XmlTextWriter(PDataPath + "ruddat_nk_config.xml", null);
                    xmlwriter.Formatting = Formatting.Indented;
                    xmlwriter.WriteStartDocument();
                    xmlwriter.WriteStartElement("Konfiguration");
                    xmlwriter.WriteStartElement("Datenbankverbindung");
                    xmlwriter.WriteStartElement("Server", "");
                    xmlwriter.WriteString("Data Source=ruddat;");
                    xmlwriter.WriteEndElement();
                    xmlwriter.WriteStartElement("Datenbankname", "");
                    xmlwriter.WriteString("Initial Catalog=nk;");
                    xmlwriter.WriteEndElement();
                    xmlwriter.WriteStartElement("Trust", "");
                    xmlwriter.WriteString("Integrated Security=True;");
                    xmlwriter.WriteEndElement();
                    xmlwriter.WriteStartElement("Timeout", "");
                    xmlwriter.WriteString("Connect Timeout=30;");
                    xmlwriter.WriteEndElement();
                    xmlwriter.WriteEndElement();
                    xmlwriter.Close();

                    // Die hier eingetragene Db-Verbindung nehmen
                    SqlConnectionString = "Data Source=ruddat;Initial Catalog=nk;Integrated Security=True";

                    MessageBox.Show("Es wurde eine Standardkonfiguration erzeugt.\n" +
                                    "Die Serververbindung muss noch überprüft werden\n" +
                                    "Die Datei heißt:\n" + PDataPath + "unicar_work_config.xml\n",
                                    "Achtung",
                                    MessageBoxButton.OK);
                }
                catch
                {
                    MessageBox.Show("Konfigurationsdatei konnte nicht erzeugt werden", "Achtung",
                                    MessageBoxButton.OK);
                }
            }

            // Für Testzwecke
            SqlConnectionString = "Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\\Users\\Ulf\\AppData\\Local\\Ruddat\\Nebenkosten\\rdnk.mdf;Integrated Security=True;Connect Timeout=5";
            // MessageBox.Show("Lokale Datenbank wird verwendet", "Achtung", MessageBoxButton.OK);

            // C:\Users\Ulf\AppData\Local\Unicar_work


            //Globaler ConnectString
            gsConnectString = SqlConnectionString;

            return (1);
        }

        private void lbFiliale_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

    }
}
