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
    /// Interaktionslogik für WndPoolRgNr.xaml
    /// </summary>
    public partial class WndPoolRgNr : Window
    {
        private MainWindow mainWindow;
        public String gsConnect;

        // ConnectString übernehmen
        public string psConnect { get; set; }

        DataTable tableRgNr;
        DataTable tableRgNrUse;
        SqlDataAdapter sdRgNr;
        SqlDataAdapter sdRgNrUse;

        public WndPoolRgNr(MainWindow mainWindow)
        {
            string lsSql = "";
            int liRows = 0;

            this.mainWindow = mainWindow;
            InitializeComponent();

            // ConnectString global
            gsConnect = this.mainWindow.psConnect;

            // save +  del Button abschalten
            this.btnSave.IsEnabled = false;
            this.btnDelete.IsEnabled = false;

            // SqlSelect Rechnungsnummern erstellen
            lsSql = getSql("rgnr", 1, 0);
            // Daten Rechnungsnummern holen
            liRows = fetchData(lsSql, 1);

            // SqlSelect Verwendete Rechnungsnummern
            lsSql = getSql("rgnr", 2, 0);
            // Daten Verwendete Rechnungsnummern holen
            liRows = fetchData(lsSql, 2);

        }

        // Sql zusammenstellen
        private string getSql(string asSql, int aiArt, int aiId)
        {
            string lsSql = "";

            switch (aiArt)
            {
                case 1:         // Rechnungsnummern
                    lsSql = "select id_rg_nr,rgnr,id_mieter,flag_besetzt from rgnr Where flag_besetzt != 1 Order by id_rg_nr DESC";
                    break;
                case 2:         // verwendete Rechnungsnummern
                    lsSql = "select top 100 id_rg_nr,rgnr,id_mieter,flag_besetzt from rgnr Where flag_besetzt = 1 Order by id_rg_nr DESC";
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

            SqlConnection connect;
            connect = new SqlConnection(gsConnect);

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
                default:
                    break;
            }

            return liRows;
        }

        // Änderung speichern
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            int liId = 0;
            int liSel = dgrRgNr.SelectedIndex;
            int liRows = 0;
            string lsSql2 = "";

            btnSave.IsEnabled = false;
            btnAdd.IsEnabled = true;

            if (btnSave.Content.ToString() == "Speichern")
            {
                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(sdRgNr);

                sdRgNr.UpdateCommand = commandBuilder.GetUpdateCommand();
                sdRgNr.InsertCommand = commandBuilder.GetInsertCommand();
                sdRgNr.Update(tableRgNr);
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
                            // Den Import aus wt_hours_add löschen
                            String lsSql = "Delete from rgnr Where id_rg_nr = " + liId.ToString();

                            SqlConnection connect;
                            connect = new SqlConnection(gsConnect);
                            SqlCommand command = new SqlCommand(lsSql, connect);

                            try
                            {
                                // Db open
                                connect.Open();
                                SqlDataReader queryCommandReader = command.ExecuteReader();
                                
                                sdRgNr.Update(tableRgNr);
                                connect.Close();
                            }
                            catch
                            {
                                MessageBox.Show("In Tabelle Rechnungsnummern konnte nicht gelöscht werden\n" +
                                        "Prüfen Sie bitte die Datenbankverbindung\n",
                                        "Achtung WndPoolRgNr.Rg.del",
                                            MessageBoxButton.OK);
                            }
                        }
                    }
                }
            }

            
 
            // SqlSelect erstellen
            lsSql2 = getSql("rgnr", 1, 0);
            // Daten holen
            liRows = fetchData(lsSql2, 1);

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
