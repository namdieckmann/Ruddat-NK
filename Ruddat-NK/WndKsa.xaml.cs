using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;


namespace Ruddat_NK
{
    /// <summary>
    /// Interaktionslogik für WndKsa.xaml
    /// </summary>
    public partial class WndKsa : Window
    {
        private MainWindow mainWindow;
        private String gsConnect;
        private int giDb;

        // ConnectString übernehmen
        private string psConnect { get; set; }

        DataTable tableKsa;
        SqlDataAdapter sdKsa;
        MySqlDataAdapter mysdKsa;

        // Hier Übergabe des Mainwindows für Übergabe des ConnectStrings
        public WndKsa(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();

            // ConnectString global
            gsConnect = this.mainWindow.psConnect;

            // save +  del Button abschalten
            this.btnSave.IsEnabled = false;
            this.btnDel.IsEnabled = false;
            this.rbObj.IsChecked = true;
        }


        internal void getDb(int aiDb)
        {
            String lsSql = "";
            int liRows = 0;

            giDb = aiDb;

            // SqlSelect erstellen
            lsSql = getSql(1, 1, 0);
            // Daten holen
            liRows = fetchData(lsSql, 1);
        }


        // Sql zusammenstellen
        private string getSql(int aiArt, int aiChoose, int aiId)
        {
            string lsSql = "";

            switch (aiArt)
            {
                case 1:
                    switch (aiChoose)
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
                    break;
                case 3:
                    lsSql = "Delete from art_kostenart Where id_ksa = " + aiId.ToString();
                    break;
                case 4:
                    lsSql = @"Select id_ksa from Rechnungen where id_ksa = " + aiId.ToString();
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
            btnDel.IsEnabled = false;
            btnAdd.IsEnabled = true;

            switch (giDb)
            {
                case 1:
                    SqlConnection connect;
                    connect = new SqlConnection(gsConnect);
                    connect.Open();
                    switch (aiArt)
                    {
                        case 1:
                            tableKsa = new DataTable();         // Kostenarten 
                            SqlCommand command = new SqlCommand(asSql, connect);
                            sdKsa = new SqlDataAdapter(command);
                            sdKsa.Fill(tableKsa);
                            dgrKsa.ItemsSource = tableKsa.DefaultView;
                            break;
                        case 2:
                            SqlCommandBuilder commandBuilder = new SqlCommandBuilder(sdKsa);
                            sdKsa.Update(tableKsa);
                            break;
                        case 3:
                            SqlCommand command3 = new SqlCommand(asSql, connect);
                            SqlDataReader queryCommandReader = command3.ExecuteReader();
                            break;
                        case 4:
                            SqlCommand command4 = new SqlCommand(asSql, connect);
                            var lvId = command4.ExecuteScalar();

                            if (lvId != DBNull.Value)
                            {
                                liRows = Convert.ToInt32(lvId);     // LiRows wird als Id missbraucht
                            }
                            else
                            {
                                liRows = 0;
                            }
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
                        case 1:
                            tableKsa = new DataTable();         // Kostenarten 
                            MySqlCommand mycommand = new MySqlCommand(asSql, myConnect);
                            mysdKsa = new MySqlDataAdapter(mycommand);
                            mysdKsa.Fill(tableKsa);
                            dgrKsa.ItemsSource = tableKsa.DefaultView;
                            break;
                        case 2:
                            MySqlCommandBuilder commandBuilder = new MySqlCommandBuilder(mysdKsa);
                            mysdKsa.Update(tableKsa);
                            break;
                        case 3:
                            MySqlCommand command3 = new MySqlCommand(asSql, myConnect);
                            MySqlDataReader queryCommandReader = command3.ExecuteReader();
                            break;
                        case 4:
                            MySqlCommand command4 = new MySqlCommand(asSql, myConnect);
                            var lvId = command4.ExecuteScalar();

                            if (lvId != DBNull.Value)
                            {
                                liRows = Convert.ToInt32(lvId);     // LiRows wird als Id missbraucht
                            }
                            else
                            {
                                liRows = 0;
                            }
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
                lsSql = getSql(1,1,0);
                // Daten holen
                liRows = fetchData(lsSql, 1);

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
                lsSql = getSql(1, 2, 0);
                // Daten holen
                liRows = fetchData(lsSql, 1);

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
                lsSql = getSql(1, 3, 0);
                // Daten holen
                liRows = fetchData(lsSql, 1);

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
                lsSql = getSql(1, 4, 0);
                // Daten holen
                liRows = fetchData(lsSql, 1);

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
                lsSql = getSql(1, 5, 0);
                // Daten holen
                liRows = fetchData(lsSql, 1);

                dgrKsa.Columns[1].Visibility = Visibility.Collapsed;
                dgrKsa.Columns[2].Visibility = Visibility.Collapsed;
                dgrKsa.Columns[3].Visibility = Visibility.Visible;
            }
        }


        // Datensatz zufügen
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
            int liOk = 0;
            int liSel = dgrKsa.SelectedIndex;
            int liRows = 0;
            string lsSql = "";

            btnSave.IsEnabled = false;
            btnAdd.IsEnabled = true;

            if (btnSave.Content.ToString() == "Speichern")
            {

                liOk = fetchData("", 2);
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
                            // Löschen
                            lsSql = getSql(3, 0, liId);
                            liOk = fetchData(lsSql, 3);
                        }
                    }
                }
            }

            // Kostenart Objekt, ObjektTeil oder Mieter
            // SqlSelect erstellen
            if (rbObj.IsChecked == true)
            {
                lsSql = getSql(1, 1, 0);
            }
            if (rbObjTeil.IsChecked == true)
            {
                lsSql = getSql(1, 2, 0);
            }
            if (rbMieter.IsChecked == true)
            {
                lsSql = getSql(1, 3, 0);
            }
            if (rbzahlung.IsChecked == true)
            {
                lsSql = getSql(1, 4, 0);
            }
            if (rbzaehler.IsChecked == true)
            {
                lsSql = getSql(1, 5, 0);
            }

            // Daten holen
            liRows = fetchData(lsSql, 1);
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

            lsSql = getSql(4, 0, aiId);
            liId = fetchData(lsSql, 4);

            return liId;
        }


        // Kostenart löschen
        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            btnSave.IsEnabled = true;
            btnSave.Content = "Wirklich löschen?";
            btnDel.IsEnabled = false;
        }
    }
}
