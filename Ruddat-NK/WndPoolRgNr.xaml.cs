using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace Ruddat_NK
{
    /// <summary>
    /// Interaktionslogik für WndPoolRgNr.xaml
    /// </summary>
    public partial class WndPoolRgNr : Window
    {
        private MainWindow mainWindow;
        private String gsConnect;
        private int giDb;

        // ConnectString übernehmen
        private string psConnect { get; set; }

        DataTable tableRgNr;
        DataTable tableRgNrUse;
        SqlDataAdapter sdRgNr;
        SqlDataAdapter sdRgNrUse;
        MySqlDataAdapter mysdRgNr;
        MySqlDataAdapter mysdRgNrUse;

        public WndPoolRgNr(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();

            // ConnectString global
            gsConnect = this.mainWindow.psConnect;

            // save +  del Button abschalten
            this.btnSave.IsEnabled = false;
            this.btnDelete.IsEnabled = false;
        }

        // Welche Datenbank
        internal void getDb(int aiDb)
        {
            string lsSql = "";
            int liRows = 0;

            giDb = aiDb;

            // SqlSelect Rechnungsnummern erstellen
            lsSql = getSql(1, 0);
            // Daten Rechnungsnummern holen
            liRows = fetchData(lsSql, 1);

            // SqlSelect Verwendete Rechnungsnummern
            lsSql = getSql(2, 0);
            // Daten Verwendete Rechnungsnummern holen
            liRows = fetchData(lsSql, 2);
        }

        // Sql zusammenstellen
        private string getSql(int aiArt, int aiId)
        {
            string lsSql = "";

            switch (aiArt)
            {
                case 1:         // Rechnungsnummern
                    lsSql = "select id_rg_nr,rgnr,id_mieter,flag_besetzt from rgnr Where flag_besetzt != 1 Order by id_rg_nr DESC";
                    break;
                case 2:         // verwendete Rechnungsnummern
                    lsSql = "select id_rg_nr,rgnr,id_mieter,flag_besetzt from rgnr Where flag_besetzt = 1 Order by id_rg_nr DESC";
                    break;
                case 4:
                    lsSql = "Delete from rgnr Where id_rg_nr = " + aiId.ToString();
                    break;
                 default:
                    break;
            }
            return lsSql;
        }

        // Daten holen
        private int fetchData(string asSql, int aiArt)
        {
            int liRows = 0;

            // Buttons
            btnSave.IsEnabled = false;
            btnAdd.IsEnabled = true;

            switch (giDb)
            {
                case 1:
                    SqlConnection connect;
                    connect = new SqlConnection(gsConnect);
                    connect.Open();

                    switch (aiArt)
                    {
                        case 1: // Rechnungsnummern
                            tableRgNr = new DataTable();
                            SqlCommand command = new SqlCommand(asSql, connect);
                            sdRgNr = new SqlDataAdapter(command);
                            sdRgNr.Fill(tableRgNr);
                            dgrRgNr.ItemsSource = tableRgNr.DefaultView;
                            break;
                        case 2: // verwendete Rechnungsnummern
                            tableRgNrUse = new DataTable();
                            SqlCommand command2 = new SqlCommand(asSql, connect);
                            sdRgNrUse = new SqlDataAdapter(command2);
                            sdRgNrUse.Fill(tableRgNrUse);
                            dgrRgNrUse.ItemsSource = tableRgNrUse.DefaultView;
                            break;
                        case 3:
                            SqlCommandBuilder commandBuilder = new SqlCommandBuilder(sdRgNr);
                            sdRgNr.Update(tableRgNr);
                            break;
                        case 4:
                            SqlCommand command4 = new SqlCommand(asSql, connect);
                            SqlDataReader queryCommandReader = command4.ExecuteReader();
                            break;
                        default:
                            break;
                    }
                    connect.Close();
                    break;
                case 2:
                    MySqlConnection myConnect;
                    myConnect = new MySqlConnection(gsConnect);
                    myConnect.Open();

                    switch (aiArt)
                    {
                        case 1: // Rechnungsnummern
                            tableRgNr = new DataTable();
                            MySqlCommand command = new MySqlCommand(asSql, myConnect);
                            mysdRgNr = new MySqlDataAdapter(command);
                            mysdRgNr.Fill(tableRgNr);
                            dgrRgNr.ItemsSource = tableRgNr.DefaultView;
                            break;
                        case 2: // verwendete Rechnungsnummern
                            tableRgNrUse = new DataTable();
                            MySqlCommand command2 = new MySqlCommand(asSql, myConnect);
                            mysdRgNrUse = new MySqlDataAdapter(command2);
                            mysdRgNrUse.Fill(tableRgNrUse);
                            dgrRgNrUse.ItemsSource = tableRgNrUse.DefaultView;
                            break;
                        case 3:
                            MySqlCommandBuilder commandBuilder = new MySqlCommandBuilder(mysdRgNr);
                            mysdRgNr.Update(tableRgNr);
                            break;
                        case 4:
                            MySqlCommand command4 = new MySqlCommand(asSql, myConnect);
                            MySqlDataReader queryCommandReader = command4.ExecuteReader();
                            break;
                        default:
                            break;
                    }
                    myConnect.Close();
                    break;
                default:
                    break;
            }
            return liRows;
        }

        // Änderung speichern
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            int liId = 0;
            int liOk = 0;
            int liSel = dgrRgNr.SelectedIndex;
            int liRows = 0;
            string lsSql = "";

            btnSave.IsEnabled = false;
            btnAdd.IsEnabled = true;

            if (btnSave.Content.ToString() == "Speichern")
            {
                liOk = fetchData("", 3);
            }
            else  // Löschen
            {
                if (liSel >= 0)
                {
                    DataRowView rowview = dgrRgNr.SelectedItem as DataRowView;
                    if ((rowview.Row[0] != DBNull.Value))
                    {
                        liId = Int32.Parse(rowview.Row[0].ToString());

                        if (liId >= 0)
                        {
                            // Löschen
                            lsSql = getSql(4, liId);
                            liOk = fetchData(lsSql, 4);
                        }
                    }
                }
            }
          
            // SqlSelect erstellen
            lsSql = getSql(1, 0);
            // Daten holen
            liRows = fetchData(lsSql, 1);

            btnSave.Content = "Speichern";
            btnDelete.IsEnabled = true;

        }
        
        // Rechnungsnummern Auswahl
        private void dgrRgNr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnDelete.IsEnabled = true;
        }

        // Verwendete Rechnungsnummern Auswahl
        private void dgrRgUse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        // Celle bearbeitet Ende
        private void dgrRgNr_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            btnSave.IsEnabled = true;
        }

        //Button löschen
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            btnSave.IsEnabled = true;
            btnSave.Content = "Wirklich löschen?";
            btnDelete.IsEnabled = false;
        }

        // Button zufügen
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Buttons 
            // btnAdd.IsEnabled = false;
            btnSave.IsEnabled = true;

            DataRow dr = tableRgNr.NewRow();

            dr[1] = 1111;    // Rechnungsnummer einsetzen
            dr[3] = 0;       // Flag besetzt

            tableRgNr.Rows.Add(dr);
        }

        // Zuordnung aufheben (rechtes Datagrid)
        private void btnRelease_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
