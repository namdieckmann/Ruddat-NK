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
    /// Interaktionslogik für WndCompanies.xaml
    /// </summary>
    public partial class WndCompanies : Window
    {
        private MainWindow mainWindow;
        public String gsConnect;

        // ConnectString übernehmen
        public string psConnect { get; set; }

        DataTable tableCmp;
        SqlDataAdapter sdCmp;
        DataTable tableAdr;
        SqlDataAdapter sdAdr;
        DataTable tableAda;
        SqlDataAdapter sdAda;


        // Hier Übergabe des Mainwindows für Übergabe des ConnectStrings
        public WndCompanies(MainWindow mainWindow)
        {
            String lsSql = "";
            int liRows = 0;

            this.mainWindow = mainWindow;
            InitializeComponent();

            // ConnectString global
            gsConnect = this.mainWindow.psConnect;

            // save +  del Button abschalten
            this.btnSave.IsEnabled = false;
            this.btnDel.IsEnabled = false;
            this.btnAdrSave.IsEnabled = false;
            this.btnAdrDel.IsEnabled = false;
            this.btnAdrAdd.IsEnabled = false;

            // SqlSelect Firmen erstellen
            lsSql = getSql("cmp", 1, 0);
            // Daten Firmen holen
            liRows = fetchData(lsSql,1);

            // SqlSelect AdressArten
            lsSql = getSql("ada", 3, 0);
            // Daten Firmen holen
            liRows = fetchData(lsSql, 3);
        }

        // Sql zusammenstellen
        private string getSql(string asSql, int aiArt, int aiId)
        {
            string lsSql = "";

            switch (aiArt)
            {
                case 1:         // Gesellschaft
                    lsSql = "select id_filiale,name,name_2,bez from filiale Order by id_filiale";
                    break;
                case 2:         // Adressen
                    lsSql = @"select id_adressen, id_art_adresse, id_objekt, id_objekt_teil, id_filiale, Id_mieter, anrede, name, vorname, 
                                    firma, adresse, plz, ort, land, tel, mail, mobil, homepage   
                                from adressen
                                where id_filiale = " + aiId.ToString() + " Order by id_art_adresse";
                    break;
                case 3:
                    lsSql = @"Select id_art_adresse,bez from art_adresse Order by sort";
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
                case 1: // Firmen
                    tableCmp = new DataTable();         
                    SqlCommand command = new SqlCommand(asSql, connect);
                    sdCmp = new SqlDataAdapter(command);
                    sdCmp.Fill(tableCmp);
                    dgrCmp.ItemsSource = tableCmp.DefaultView;
                    break;
                case 2: // Adressen
                    tableAdr = new DataTable();         
                    SqlCommand command2 = new SqlCommand(asSql, connect);
                    sdAdr = new SqlDataAdapter(command2);
                    sdAdr.Fill(tableAdr);
                    dgrAdr.ItemsSource = tableAdr.DefaultView;
                    break;
                case 3: // Adressarten
                    tableAda = new DataTable();         
                    SqlCommand command3 = new SqlCommand(asSql, connect);
                    sdAda = new SqlDataAdapter(command3);
                    sdAda.Fill(tableAda);
                    adressenart.ItemsSource = tableAda.DefaultView;
                    break;
                default:
                    break;
            }

            return liRows;
        }

        // Datensatz zufügen
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Buttons 
            btnAdd.IsEnabled = false;
            btnSave.IsEnabled = true;

            DataRow dr = tableCmp.NewRow();

            tableCmp.Rows.Add(dr);
        }


        // Änderung abspeichern
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            int liId = 0;
            int liSel = dgrCmp.SelectedIndex;
            int liRows = 0;
            string lsSql2 = "";


            btnSave.IsEnabled = false;
            btnAdd.IsEnabled = true;

            if (btnSave.Content.ToString() == "Speichern")
            {
                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(sdCmp);

                sdCmp.UpdateCommand = commandBuilder.GetUpdateCommand();
                sdCmp.InsertCommand = commandBuilder.GetInsertCommand();
            }
            else  // Löschen
            {
                if (liSel >= 0)
                {
                    DataRowView rowview = dgrCmp.SelectedItem as DataRowView;
                    if ((rowview.Row[0] != DBNull.Value))
                    {
                        liId = Int32.Parse(rowview.Row[0].ToString());

                        if (liId >= 0)
                        {
                            // Den Import aus wt_hours_add löschen
                            String lsSql = "Delete from filiale Where id_filiale = " + liId.ToString();

                            SqlConnection connect;
                            connect = new SqlConnection(gsConnect);
                            SqlCommand command = new SqlCommand(lsSql, connect);

                            try
                            {
                                // Db open
                                connect.Open();
                                SqlDataReader queryCommandReader = command.ExecuteReader();
                                connect.Close();
                            }
                            catch
                            {
                                MessageBox.Show("In Tabelle Gesellschaften konnte nicht gelöscht werden\n" +
                                        "Prüfen Sie bitte die Datenbankverbindung\n",
                                        "Achtung WndCompanies.Cmp.del",
                                            MessageBoxButton.OK);
                            }
                        }
                    }
                }
            }

            sdCmp.Update(tableCmp);
            // SqlSelect erstellen
            lsSql2 = getSql("cmp", 1, 0);
            // Daten holen
            liRows = fetchData(lsSql2, 1);

            btnSave.Content = "Speichern";
            btnDel.IsEnabled = true;

        }

        // Es wurde etwas geändert; speichern öffnen
        private void dgrCmp_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            btnSave.IsEnabled = true;
        }


        // Prüfen, ob ein datensatz gelöscht werden darf
        // Existiert die id_filiale in Objekten?
        private void dgrCmp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int liId = 0;
            int liSel = dgrCmp.SelectedIndex;
            int liRows = 0;
            string lsSql2 = "";

            if (liSel >= 0)
            {
                DataRowView rowview = dgrCmp.SelectedItem as DataRowView;

                if ((rowview.Row[0] != DBNull.Value))
                {
                    liId = Int32.Parse(rowview.Row[0].ToString());

                    if (getDelInfo(liId) == 0)
                    {
                        btnDel.IsEnabled = true;
                        btnAdrDel.IsEnabled = false;
                        btnAdrAdd.IsEnabled = true;
                    }
                    else
                    {
                        btnDel.IsEnabled = false;
                        btnAdrAdd.IsEnabled = true;
                    }                     

                    // Adressen dazu holen
                    // SqlSelect erstellen
                    lsSql2 = getSql("adr", 2, liId);
                    // Daten holen
                    liRows = fetchData(lsSql2, 2);

                }
            }
        }

        // Existiert ein Objekt zu der Firma mit der gewählten ID?
        private int getDelInfo(int aiId)
        {
            int liId = 0;
            String lsSql = "";

            lsSql = @"Select id_filiale from objekt where id_filiale = " + aiId.ToString();

            SqlConnection connect;
            connect = new SqlConnection(gsConnect);
            SqlCommand command = new SqlCommand(lsSql, connect);

            // art_day
            try
            {
                // Db open
                connect.Open();
                var lvId = command.ExecuteScalar();

                if (lvId != DBNull.Value)
                {
                    liId = Convert.ToInt32(lvId);
                }
                else
                {
                    liId = 0;
                }

                connect.Close();
            }
            catch
            {
                MessageBox.Show("Es wurden keine Informationen für das Löschen einer Filiale gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (WndCmp.getdelInfo)",
                         MessageBoxButton.OK);
            }
            return liId;
        }


        // Gesellschaft löschen ( nur offen, wenn sie nicht verwendet wird
        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            btnSave.IsEnabled = true;
            btnSave.Content = "Wirklich löschen?";
            btnDel.IsEnabled = false;
        }

        // Adresse speichern oder löschen
        private void btnAdrSave_Click(object sender, RoutedEventArgs e)
        {
            int liId = 0;
            int liIdCmp = 0;
            int liSelCmp = dgrCmp.SelectedIndex;
            int liSelAdr = dgrAdr.SelectedIndex;
            int liRows = 0;
            string lsSql = "";

            btnAdrSave.IsEnabled = false;
            btnAdrAdd.IsEnabled = true;

            if (btnAdrSave.Content.ToString() == "Speichern")
            {
                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(sdAdr);

                sdAdr.UpdateCommand = commandBuilder.GetUpdateCommand();
                sdAdr.InsertCommand = commandBuilder.GetInsertCommand();

            }
            else  // Löschen
            {
                if (liSelAdr >= 0)
                {
                    DataRowView rowview = dgrAdr.SelectedItem as DataRowView;
                    if ((rowview.Row[0] != DBNull.Value))
                    {
                        liId = Int32.Parse(rowview.Row[0].ToString());

                        if (liId >= 0)
                        {
                            // Firma löschen
                            String lsSql2 = "Delete from adressen Where id_adressen = " + liId.ToString();

                            SqlConnection connect;
                            connect = new SqlConnection(gsConnect);
                            SqlCommand command2 = new SqlCommand(lsSql2, connect);

                            try
                            {
                                // Db open
                                connect.Open();
                                SqlDataReader queryCommandReader = command2.ExecuteReader();
                                connect.Close();
                            }
                            catch
                            {
                                MessageBox.Show("In Tabelle Adressen konnte nicht gelöscht werden\n" +
                                        "Prüfen Sie bitte die Datenbankverbindung\n",
                                        "Achtung WndCompanies.Adr.delete",
                                            MessageBoxButton.OK);
                            }
                        }
                    }
                }
            }
            sdAdr.Update(tableAdr);

            // Daten Adresse neu holen
            if (liSelCmp >= 0)
            {
                DataRowView rowview = dgrCmp.SelectedItem as DataRowView;

                if ((rowview.Row[0] != DBNull.Value))
                {
                    liIdCmp = Int32.Parse(rowview.Row[0].ToString());

                    // SqlSelect erstellen
                    lsSql = getSql("adr", 2, liIdCmp);
                    // Daten holen
                    liRows = fetchData(lsSql, 2);
                }
            }

            btnAdrSave.Content = "Speichern";
            btnAdrDel.IsEnabled = true;
        }

        // Adresse zufügen
        private void btnAdrAdd_Click(object sender, RoutedEventArgs e)
        {
            int liSel = dgrCmp.SelectedIndex;
            int liId = 0;

            // Buttons 
            btnAdd.IsEnabled = false;
            btnSave.IsEnabled = true;

            if (liSel >= 0)
            {
                DataRowView rowview = dgrCmp.SelectedItem as DataRowView;

                if ((rowview.Row[0] != DBNull.Value))
                {
                    // ID Filiale, Gesellschaft
                    liId = Int32.Parse(rowview.Row[0].ToString());

                    DataRow dr = tableAdr.NewRow();

                    // Vorgaben eintragen, hier Firmen ID id_filiale
                    dr[4] = liId;

                    tableAdr.Rows.Add(dr);
                }
            }
        }

        // Adresse löschen (nur, wenn sie nicht verwendet wird)
        private void btnAdrDel_Click(object sender, RoutedEventArgs e)
        {
            btnAdrSave.IsEnabled = true;
            btnAdrSave.Content = "Wirklich löschen?";
            btnAdrDel.IsEnabled = false;
        }

        // Adressen - Eingabe verändert
        private void dgrAdr_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            btnAdrSave.IsEnabled = true;
        }

        // Andere Adresse angewählt
        private void dgrAdr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnAdrDel.IsEnabled = true;
        }

    }
}
