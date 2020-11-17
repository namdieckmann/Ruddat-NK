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
    /// Interaktionslogik für WndKsa.xaml
    /// </summary>
    public partial class WndKsa : Window
    {
        private MainWindow mainWindow;
        public String gsConnect;

        // ConnectString übernehmen
        public string psConnect { get; set; }

        DataTable tableKsa;
        SqlDataAdapter sdKsa;

        // Hier Übergabe des Mainwindows für Übergabe des ConnectStrings
        public WndKsa(MainWindow mainWindow)
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
            this.rbObj.IsChecked = true;

            // SqlSelect erstellen
            lsSql = getSql("ksa",1);
            // Daten holen
            liRows = fetchData(lsSql, "ksa");
        }

        // Sql zusammenstellen
        private string getSql(string asSql, int aiArt)
        {
            string lsSql = "";

            switch (aiArt)
            {
                case 1:         // Objekte
                    lsSql = "select bez,wtl_obj_teil,wtl_mieter,sort,id_ksa,ksa_objekt,ksa_obj_teil,ksa_mieter,ksa_zahlung,ksa_zaehler from art_kostenart Where ksa_objekt = 1 Order by bez";
                    break;
                case 2:         // Teilobjekte
                    lsSql = "select bez,wtl_obj_teil,wtl_mieter,sort,id_ksa,ksa_objekt,ksa_obj_teil,ksa_mieter,ksa_zahlung,ksa_zaehler from art_kostenart Where ksa_obj_teil = 1 Order by bez";
                    break;
                case 3:         // Mieter
                    lsSql = "select bez,wtl_obj_teil,wtl_mieter,sort,id_ksa,ksa_objekt,ksa_obj_teil,ksa_mieter,ksa_zahlung,ksa_zaehler from art_kostenart Where ksa_mieter = 1 Order by bez";
                    break;
                case 4:         // Zahlung
                    lsSql = "select bez,wtl_obj_teil,wtl_mieter,sort,id_ksa,ksa_objekt,ksa_obj_teil,ksa_mieter,ksa_zahlung,ksa_zaehler from art_kostenart Where ksa_zahlung = 1 Order by bez";
                    break;
                case 5:         // Zähler
                    lsSql = "select bez,wtl_obj_teil,wtl_mieter,sort,id_ksa,ksa_objekt,ksa_obj_teil,ksa_mieter,ksa_zahlung,ksa_zaehler from art_kostenart Where ksa_zaehler = 1 Order by bez";
                    break;
                default:
                    break;
            }


            return lsSql;
        }

        // Daten holen
        private int fetchData(string asSql, string p)
        {
            int liRows = 0;

            // Buttons
            btnSave.IsEnabled = false;
            btnDel.IsEnabled = false;
            btnAdd.IsEnabled = true;

            SqlConnection connect;
            connect = new SqlConnection(gsConnect);

            tableKsa = new DataTable();         // Kostenarten 
            SqlCommand command = new SqlCommand(asSql, connect);
            sdKsa = new SqlDataAdapter(command);
            sdKsa.Fill(tableKsa);

            dgrKsa.ItemsSource = tableKsa.DefaultView;


            return liRows;
        }

        // Objekte angewählt
        private void rbObj_Checked(object sender, RoutedEventArgs e)
        {
            string lsSql = "";
            int liRows = 0;

            // Buttons
            btnSave.IsEnabled = false;
            btnDel.IsEnabled = false;
            btnAdd.IsEnabled = true;

            if (rbObj.IsChecked == true)
            {
                // SqlSelect erstellen
                lsSql = getSql("ksa", 1);
                // Daten holen
                liRows = fetchData(lsSql, "ksa");

                dgrKsa.Columns[1].Visibility = Visibility.Visible;
                dgrKsa.Columns[2].Visibility = Visibility.Visible;

            }
        }

        // Teilobjekte angewählt
        private void rbObjTeil_Checked(object sender, RoutedEventArgs e)
        {
            string lsSql = "";
            int liRows = 0;

            // Buttons
            btnSave.IsEnabled = false;
            btnDel.IsEnabled = false;
            btnAdd.IsEnabled = true;

            if (rbObjTeil.IsChecked == true)
            {
                // SqlSelect erstellen
                lsSql = getSql("ksa", 2);
                // Daten holen
                liRows = fetchData(lsSql, "ksa");

                dgrKsa.Columns[1].Visibility = Visibility.Collapsed;
                dgrKsa.Columns[2].Visibility = Visibility.Visible;
                dgrKsa.Columns[3].Visibility = Visibility.Visible;
            }
        }

        // Mieter angewählt
        private void rbMieter_Checked(object sender, RoutedEventArgs e)
        {
            string lsSql = "";
            int liRows = 0;

            if (rbMieter.IsChecked == true)
            {
                // SqlSelect erstellen
                lsSql = getSql("ksa", 3);
                // Daten holen
                liRows = fetchData(lsSql, "ksa");

                dgrKsa.Columns[1].Visibility = Visibility.Collapsed;
                dgrKsa.Columns[2].Visibility = Visibility.Collapsed;
                dgrKsa.Columns[3].Visibility = Visibility.Visible;
            }
        }

        // Zahlung
        private void rbzahlung_Checked(object sender, RoutedEventArgs e)
        {
            string lsSql = "";
            int liRows = 0;

            if (rbzahlung.IsChecked == true)
            {
                // SqlSelect erstellen
                lsSql = getSql("ksa", 4);
                // Daten holen
                liRows = fetchData(lsSql, "ksa");

                dgrKsa.Columns[1].Visibility = Visibility.Collapsed;
                dgrKsa.Columns[2].Visibility = Visibility.Collapsed;
                dgrKsa.Columns[3].Visibility = Visibility.Visible;
            }
        }

        // Zähler
        private void rbzaehler_Checked(object sender, RoutedEventArgs e)
        {
            string lsSql = "";
            int liRows = 0;

            if (rbzaehler.IsChecked == true)
            {
                // SqlSelect erstellen
                lsSql = getSql("ksa", 5);
                // Daten holen
                liRows = fetchData(lsSql, "ksa");

                dgrKsa.Columns[1].Visibility = Visibility.Collapsed;
                dgrKsa.Columns[2].Visibility = Visibility.Collapsed;
                dgrKsa.Columns[3].Visibility = Visibility.Visible;
            }
        }


        // datensatz zufügen
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Buttons 
            btnAdd.IsEnabled = false;
            btnSave.IsEnabled = true;

            DataRow dr = tableKsa.NewRow();

            // Kostenart Objekt, ObjektTeil oder Mieter
            if (rbObj.IsChecked == true)
            {
                dr[5] = 1;
            }
            if (rbObjTeil.IsChecked == true)
            {
                dr[6] = 1;
            }
            if (rbMieter.IsChecked == true)
            {
                dr[7] = 1;
            }
            if (rbzahlung.IsChecked == true)
            {
                dr[8] = 1;
            }
            if (rbzaehler.IsChecked == true)
            {
                dr[9] = 1;
            }

            tableKsa.Rows.Add(dr);
        }


        // Änderung abspeichern oder löschen
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            int liId = 0;
            int liSel = dgrKsa.SelectedIndex;
            int liRows = 0;
            string lsSql2 = "";

            btnSave.IsEnabled = false;
            btnAdd.IsEnabled = true;

            if (btnSave.Content.ToString() == "Speichern")
            {
                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(sdKsa);

                sdKsa.UpdateCommand = commandBuilder.GetUpdateCommand();
                sdKsa.InsertCommand = commandBuilder.GetInsertCommand();
            }
            else
            {
                if (liSel >= 0)
                {
                    DataRowView rowview = dgrKsa.SelectedItem as DataRowView;
                    if ((rowview.Row[4] != DBNull.Value))
                    {
                        liId = Int32.Parse(rowview.Row[4].ToString());

                        if (liId >= 0)
                        {
                            // Den Import aus wt_hours_add löschen
                            String lsSql = "Delete from art_kostenart Where id_ksa = " + liId.ToString();

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
                                MessageBox.Show("In Tabelle Kostenarten konnte nicht gelöscht werden\n" +
                                        "Prüfen Sie bitte die Datenbankverbindung\n",
                                        "Achtung",
                                            MessageBoxButton.OK);
                            }
                        }
                    }
                }
            }

            sdKsa.Update(tableKsa);

            // Kostenart Objekt, ObjektTeil oder Mieter
            // SqlSelect erstellen
            if (rbObj.IsChecked == true)
            {
                lsSql2 = getSql("ksa", 1);
            }
            if (rbObjTeil.IsChecked == true)
            {
                lsSql2 = getSql("ksa", 2);
            }
            if (rbMieter.IsChecked == true)
            {
                lsSql2 = getSql("ksa", 3);
            }
            if (rbzahlung.IsChecked == true)
            {
                lsSql2 = getSql("ksa", 4);
            }
            if (rbzaehler.IsChecked == true)
            {
                lsSql2 = getSql("ksa", 5);
            }

            // Daten holen
            liRows = fetchData(lsSql2, "ksa");
            btnSave.Content = "Speichern";
        }

        // Es wurde etwas geändert; speichern öffnen
        private void dgrKsa_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            btnSave.IsEnabled = true;
        }


        // Prüfen, ob ein datensatz gelöscht werden darf
        // Existiert die id_ksa in Rechnungen?
        private void dgrKsa_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int liId = 0;
            int liSel = dgrKsa.SelectedIndex;

            if (liSel >= 0)
            {

                DataRowView rowview = dgrKsa.SelectedItem as DataRowView;

                if (rowview.Row[4] != DBNull.Value)
                {
                    liId = Int32.Parse(rowview.Row[4].ToString());

                    if (getDelInfo(liId) == 0)
                    {
                        btnDel.IsEnabled = true;
                    }
                    else
                    {
                        btnDel.IsEnabled = false;
                    }                    
                }
            }
        }

        // Existiert in Rechnungen eine Kostenart mit der gewählten ID?
        private int getDelInfo(int aiId)
        {
            int liId = 0;
            String lsSql = "";

            lsSql = @"Select id_ksa from Rechnungen where id_ksa = "+ aiId.ToString();

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
                MessageBox.Show("Es wurden keine Rechnungsinformation gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (WndKsa.getdelInfo)",
                         MessageBoxButton.OK);
            }
            return liId;
        }


        // Kostenart löschen
        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            SqlCommandBuilder commandBuilder = new SqlCommandBuilder(sdKsa);

            btnSave.IsEnabled = true;
            btnSave.Content = "Wirklich löschen?";
            btnDel.IsEnabled = false;

        }
    }
}
