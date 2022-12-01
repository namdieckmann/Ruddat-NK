using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
namespace Ruddat_NK
{
    /// <summary>
    /// Interaktionslogik für WndMandanten.xaml
    /// </summary>
    public partial class WndMandanten : Window
    {
        private MainWindow mainWindow;
        private String gsConnect;

        // ConnectString übernehmen
        private string psConnect { get; set; }
        private int giDb = 0;

        DataTable tableMnd;
        DataTable tableAda;
        DataTable tableAdr;
        SqlDataAdapter sdMnd;
        SqlDataAdapter sdAdr;
        SqlDataAdapter sdAda;
        MySqlDataAdapter mysdMnd;
        MySqlDataAdapter mysdAdr;
        MySqlDataAdapter mysdAda;

        public WndMandanten(MainWindow mainWindow)
        {
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

        }

        // Welche Datenbank
        internal void getDb(int aiDb)
        {
            String lsSql = "";
            int liRows = 0;

            giDb = aiDb;

            // SqlSelect Mandanten erstellen
            lsSql = getSql("mnd", 1, 0);
            // Daten andanten holen
            liRows = fetchData(lsSql, 1);

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
                case 1:         // Mandanten
                    lsSql = "select id_mandant,name,name_2,bez,sel from mandanten Order by name";
                    break;
                case 2:         // Adressen
                    lsSql = @"select id_adressen, id_art_adresse, id_objekt, id_objekt_teil, id_filiale, Id_mieter, anrede, name, vorname, 
                                    firma, adresse, plz, ort, land, tel, mail, mobil, homepage, id_mandant   
                                from adressen
                                where id_mandant = " + aiId.ToString() + " Order by id_art_adresse";
                    break;
                case 3:
                    lsSql = @"Select id_art_adresse,bez from art_adresse Order by sort";
                    break;
                case 5:
                    lsSql = "Delete from mandanten Where id_mandant = " + aiId.ToString();
                    break;
                case 6:
                    lsSql = @"Select id_mandant from filiale where id_mandant = " + aiId.ToString();
                    break;
                case 8:
                    lsSql = "Delete from adressen Where id_adressen = " + aiId.ToString();
                    break;
                default:
                    break;
            }

            return lsSql;
        }

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
                        case 1: // Mandanten
                            tableMnd = new DataTable();
                            SqlCommand command = new SqlCommand(asSql, connect);
                            sdMnd = new SqlDataAdapter(command);
                            sdMnd.Fill(tableMnd);
                            dgrMnd.ItemsSource = tableMnd.DefaultView;
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
                        case 4:
                            SqlCommandBuilder commandBuilder = new SqlCommandBuilder(sdMnd);
                            sdMnd.Update(tableMnd);
                            break;
                        case 5:
                            SqlCommand command5 = new SqlCommand(asSql, connect);
                            SqlDataReader queryCommandReader = command5.ExecuteReader();
                            break;
                        case 6:
                            SqlCommand command6 = new SqlCommand(asSql, connect);
                            var lvId = command6.ExecuteScalar();
                            if (lvId != DBNull.Value)
                            {
                                liRows = Convert.ToInt32(lvId);         // LiRows hier als Id genommen
                            }
                            else
                            {
                                liRows = 0;
                            }
                            break;
                        case 7:
                            SqlCommandBuilder commandBuilder7 = new SqlCommandBuilder(sdAdr);
                            sdAdr.Update(tableAdr);
                            break;
                        case 8:
                            SqlCommand command8 = new SqlCommand(asSql, connect);
                            SqlDataReader queryCommandReader8 = command8.ExecuteReader();
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
                        case 1: // Firmen
                            tableMnd = new DataTable();
                            MySqlCommand command = new MySqlCommand(asSql, myConnect);
                            mysdMnd = new MySqlDataAdapter(command);
                            mysdMnd.Fill(tableMnd);
                            dgrMnd.ItemsSource = tableMnd.DefaultView;
                            break;
                        case 2: // Adressen
                            tableAdr = new DataTable();
                            MySqlCommand command2 = new MySqlCommand(asSql, myConnect);
                            mysdAdr = new MySqlDataAdapter(command2);
                            mysdAdr.Fill(tableAdr);
                            dgrAdr.ItemsSource = tableAdr.DefaultView;
                            break;
                        case 3: // Adressarten
                            tableAda = new DataTable();
                            MySqlCommand command3 = new MySqlCommand(asSql, myConnect);
                            mysdAda = new MySqlDataAdapter(command3);
                            mysdAda.Fill(tableAda);
                            adressenart.ItemsSource = tableAda.DefaultView;
                            break;
                        case 4:
                            MySqlCommandBuilder commandBuilder = new MySqlCommandBuilder(mysdMnd);
                            mysdMnd.Update(tableMnd);
                            break;
                        case 5:
                            MySqlCommand command5 = new MySqlCommand(asSql, myConnect);
                            MySqlDataReader queryCommandReader = command5.ExecuteReader();
                            break;
                        case 6:
                            MySqlCommand command6 = new MySqlCommand(asSql, myConnect);
                            var lvId = command6.ExecuteScalar();
                            if (lvId != DBNull.Value)
                            {
                                liRows = Convert.ToInt32(lvId);     // LiRows hier als Id genommen
                            }
                            else
                            {
                                liRows = 0;
                            }
                            break;
                        case 7:
                            MySqlCommandBuilder commandBuilder7 = new MySqlCommandBuilder(mysdAdr);
                            mysdAdr.Update(tableAdr);
                            break;
                        case 8:
                            MySqlCommand command8 = new MySqlCommand(asSql, myConnect);
                            MySqlDataReader queryCommandReader8 = command8.ExecuteReader();
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


        private void DgrMnd_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            btnSave.IsEnabled = true;
        }

        private void DgrMnd_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int liId = 0;
            int liSel = dgrMnd.SelectedIndex;
            int liRows = 0;
            string lsSql2 = "";

            if (liSel >= 0)
            {
                DataRowView rowview = dgrMnd.SelectedItem as DataRowView;

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

        private void DgrAdr_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            btnAdrSave.IsEnabled = true;
        }

        private void DgrAdr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnAdrDel.IsEnabled = true;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Buttons 
            btnAdd.IsEnabled = false;
            btnSave.IsEnabled = true;

            DataRow dr = tableMnd.NewRow();
            dr[4] = 0;

            tableMnd.Rows.Add(dr);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            int liId = 0;
            int liSel = dgrMnd.SelectedIndex;
            int liRows = 0;
            int liOk = 0;
            string lsSql = "";
            string lsSql2 = "";

            btnSave.IsEnabled = false;
            btnAdd.IsEnabled = true;

            if (btnSave.Content.ToString() == "Speichern")
            {
                fetchData("", 4);
            }
            else  // Löschen
            {
                if (liSel >= 0)
                {
                    DataRowView rowview = dgrMnd.SelectedItem as DataRowView;
                    if ((rowview.Row[0] != DBNull.Value))
                    {
                        liId = Int32.Parse(rowview.Row[0].ToString());

                        if (liId >= 0)
                        {
                            lsSql = getSql("", 5, liId);
                            liOk = fetchData(lsSql, 5);
                        }
                    }
                }
            }

            // SqlSelect erstellen
            lsSql2 = getSql("mnd", 1, 0);
            // Daten holen
            liRows = fetchData(lsSql2, 1);

            btnSave.Content = "Speichern";
            btnDel.IsEnabled = true;
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            btnSave.IsEnabled = true;
            btnSave.Content = "Wirklich löschen?";
            btnDel.IsEnabled = false;
        }

        private void btnAdrSave_Click(object sender, RoutedEventArgs e)
        {
            int liId = 0;
            int liOk = 0;
            int liIdMnd = 0;
            int liSelMnd = dgrMnd.SelectedIndex;
            int liSelAdr = dgrAdr.SelectedIndex;
            int liRows = 0;
            string lsSql = "";

            btnAdrSave.IsEnabled = false;
            btnAdrAdd.IsEnabled = true;

            if (btnAdrSave.Content.ToString() == "Speichern")
            {
                liOk = fetchData("", 7);
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
                            lsSql = getSql("", 8, liId);
                            liOk = fetchData(lsSql, 8);
                        }
                    }
                }
            }

            // Daten Adresse neu holen
            if (liSelMnd >= 0)
            {
                DataRowView rowview = dgrMnd.SelectedItem as DataRowView;

                if ((rowview.Row[0] != DBNull.Value))
                {
                    liIdMnd = Int32.Parse(rowview.Row[0].ToString());

                    // SqlSelect erstellen
                    lsSql = getSql("adr", 2, liIdMnd);
                    // Daten holen
                    liRows = fetchData(lsSql, 2);
                }
            }
            btnAdrSave.Content = "Speichern";
            btnAdrDel.IsEnabled = true;
        }

        // Existiert eine Firma zu dem Mandanten mit der gewählten ID?
        private int getDelInfo(int aiId)
        {
            int liId = 0;
            string lsSql = "";

            lsSql = getSql("", 6, aiId);
            liId = fetchData(lsSql, 6);

            return liId;
        }

        private void btnAdrAdd_Click(object sender, RoutedEventArgs e)
        {
            int liSel = dgrMnd.SelectedIndex;
            int liId = 0;

            // Buttons 
            btnAdd.IsEnabled = false;
            btnSave.IsEnabled = true;

            if (liSel >= 0)
            {
                DataRowView rowview = dgrMnd.SelectedItem as DataRowView;

                if ((rowview.Row[0] != DBNull.Value))
                {
                    // ID Mandant
                    liId = Int32.Parse(rowview.Row[0].ToString());

                    DataRow dr = tableAdr.NewRow();

                    // Vorgaben eintragen, hier mandanten ID
                    dr[18] = liId;
                    tableAdr.Rows.Add(dr);
                }
            }
        }

        private void btnAdrDel_Click(object sender, RoutedEventArgs e)
        {
            btnAdrSave.IsEnabled = true;
            btnAdrSave.Content = "Wirklich löschen?";
            btnAdrDel.IsEnabled = false;
        }
    }
}
