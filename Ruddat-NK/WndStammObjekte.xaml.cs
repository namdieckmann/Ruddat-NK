using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;


namespace Ruddat_NK
{
    /// <summary>
    /// Interaktionslogik für WndStammObjekte.xaml
    /// </summary>
    public partial class WndStammObjekte : Window
    {
        private MainWindow mainWindow;
        private String gsConnect;
        private int giDb;

        // ConnectString übernehmen
        private string psConnect { get; set; }

        DataTable tableCmp;
        DataTable tableObj;
        DataTable tableAdr;
        DataTable tableAda;

        SqlDataAdapter sdAdr;
        SqlDataAdapter sdObj;
        SqlDataAdapter sdCmp;
        SqlDataAdapter sdAda;

        MySqlDataAdapter mysdAdr;
        MySqlDataAdapter mysdObj;
        MySqlDataAdapter mysdCmp;
        MySqlDataAdapter mysdAda;

        // Hier Übergabe des Mainwindows für Übergabe des ConnectStrings
        public WndStammObjekte(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();

            // ConnectString global
            gsConnect = this.mainWindow.psConnect;

            // save +  del Button abschalten
            this.btnSave.IsEnabled = false;
            this.btnDel.IsEnabled = false;
            this.btnAdd.IsEnabled = false;
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

            // SqlSelect Firmen erstellen
            lsSql = getSql( 1, 0);
            // Daten Firmen holen
            liRows = fetchData(lsSql, 1);

            // SqlSelect Objekte
            lsSql = getSql( 2, 0);
            // Daten Firmen holen
            liRows = fetchData(lsSql, 2);

            // SqlSelect Adressen
            lsSql = getSql( 3, 0);
            // Daten Firmen holen
            liRows = fetchData(lsSql, 3);

            // SqlSelect AdressArten
            lsSql = getSql( 4, 0);
            // Daten Firmen holen
            liRows = fetchData(lsSql, 4);

        }

        // Sql zusammenstellen
        private string getSql( int aiArt, int aiId)
        {
            string lsSql = "";

            switch (aiArt)
            {
                case 1:         // Gesellschaft
                    lsSql = "select id_filiale,name,name_2,bez from filiale order by name";
                    break;
                case 2:         // Objekte
                    lsSql = @"Select Id_objekt,bez,Id_Adresse,Id_filiale,nr_obj,kst,flaeche_gesamt from objekt
                                where id_filiale = " + aiId.ToString() + " Order by bez";
                    break;
                case 3:         // Adressen
                    lsSql = @"select id_adressen, id_art_adresse, id_objekt, id_objekt_teil, id_filiale, Id_mieter, anrede, name, vorname, 
                                    firma, adresse, plz, ort, land, tel, mail, mobil, homepage   
                                from adressen
                                where id_objekt = " + aiId.ToString() + " Order by id_art_adresse";
                    break;
                case 4:         // Adressarten
                    lsSql = @"Select id_art_adresse,bez from art_adresse Order by sort";
                    break;
                case 6:         // Objekt Löschen
                    lsSql = "Delete from objekt Where id_objekt = " + aiId.ToString();
                    break;
                case 7:         // Objekt prüfen
                    lsSql = @"Select id_objekt from objekt_teil where id_objekt = " + aiId.ToString();
                    break;
                case 9:         // Adresse löschen
                    lsSql = "Delete from adressen Where id_adressen = " + aiId.ToString();
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
            btnAdd.IsEnabled = false;

            switch (giDb)
            {
                case 1:
                    SqlConnection connect;
                    connect = new SqlConnection(gsConnect);
                    connect.Open();
                    switch (aiArt)
                    {
                        case 1: // Firmen
                            tableCmp = new DataTable();
                            SqlCommand command = new SqlCommand(asSql, connect);
                            sdCmp = new SqlDataAdapter(command);
                            sdCmp.Fill(tableCmp);
                            dgrStCmp.ItemsSource = tableCmp.DefaultView;
                            break;
                        case 2: // Objekte
                            tableObj = new DataTable();
                            SqlCommand command2 = new SqlCommand(asSql, connect);
                            sdObj = new SqlDataAdapter(command2);
                            sdObj.Fill(tableObj);
                            dgrStObj.ItemsSource = tableObj.DefaultView;
                            break;
                        case 3: // Adressen
                            tableAdr = new DataTable();
                            SqlCommand command3 = new SqlCommand(asSql, connect);
                            sdAdr = new SqlDataAdapter(command3);
                            sdAdr.Fill(tableAdr);
                            dgrAdr.ItemsSource = tableAdr.DefaultView;
                            break;
                        case 4: // Adressarten
                            tableAda = new DataTable();
                            SqlCommand command4 = new SqlCommand(asSql, connect);
                            sdAda = new SqlDataAdapter(command4);
                            sdAda.Fill(tableAda);
                            adressenart.ItemsSource = tableAda.DefaultView;
                            break;
                        case 5:
                            SqlCommandBuilder commandBuilder5 = new SqlCommandBuilder(sdObj);
                            sdObj.Update(tableObj);
                            break;
                        case 6:
                            SqlCommand command6 = new SqlCommand(asSql, connect);
                            SqlDataReader queryCommandReader = command6.ExecuteReader();
                            break;
                        case 7:
                            SqlCommand command7 = new SqlCommand(asSql, connect);
                            var lvId = command7.ExecuteScalar();
                            if (lvId != DBNull.Value)
                            {
                                liRows = Convert.ToInt32(lvId);     // LiRows hier als Id
                            }
                            else
                            {
                                liRows = 0;
                            }
                            break;
                        case 8:     // Adresse speichern
                            SqlCommandBuilder commandBuilder = new SqlCommandBuilder(sdAdr);
                            sdAdr.Update(tableAdr);
                            break;
                        case 9:     // Adresse löschen
                            SqlCommand command9 = new SqlCommand(asSql, connect);
                            SqlDataReader queryCommandReader9 = command9.ExecuteReader();
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
                            tableCmp = new DataTable();
                            MySqlCommand command = new MySqlCommand(asSql, myConnect);
                            mysdCmp = new MySqlDataAdapter(command);
                            mysdCmp.Fill(tableCmp);
                            dgrStCmp.ItemsSource = tableCmp.DefaultView;
                            break;
                        case 2: // Objekte
                            tableObj = new DataTable();
                            MySqlCommand command2 = new MySqlCommand(asSql, myConnect);
                            mysdObj = new MySqlDataAdapter(command2);
                            mysdObj.Fill(tableObj);
                            dgrStObj.ItemsSource = tableObj.DefaultView;
                            break;
                        case 3: // Adressen
                            tableAdr = new DataTable();
                            MySqlCommand command3 = new MySqlCommand(asSql, myConnect);
                            mysdAdr = new MySqlDataAdapter(command3);
                            mysdAdr.Fill(tableAdr);
                            dgrAdr.ItemsSource = tableAdr.DefaultView;
                            break;
                        case 4: // Adressarten
                            tableAda = new DataTable();
                            MySqlCommand command4 = new MySqlCommand(asSql, myConnect);
                            mysdAda = new MySqlDataAdapter(command4);
                            mysdAda.Fill(tableAda);
                            adressenart.ItemsSource = tableAda.DefaultView;
                            break;
                        case 5:
                            MySqlCommandBuilder commandBuilder5 = new MySqlCommandBuilder(mysdObj);
                            mysdObj.Update(tableObj);
                            break;
                        case 6:
                            MySqlCommand command6 = new MySqlCommand(asSql, myConnect);
                            MySqlDataReader queryCommandReader = command6.ExecuteReader();
                            break;
                        case 7:
                            MySqlCommand command7 = new MySqlCommand(asSql, myConnect);
                            var lvId = command7.ExecuteScalar();
                            if (lvId != DBNull.Value)
                            {
                                liRows = Convert.ToInt32(lvId);     // LiRows hier als Id
                            }
                            else
                            {
                                liRows = 0;
                            }
                            break;
                        case 8:     // Adresse speichern
                            MySqlCommandBuilder commandBuilder = new MySqlCommandBuilder(mysdAdr);
                            mysdAdr.Update(tableAdr);
                            break;
                        case 9:     // Adresse löschen
                            MySqlCommand command9 = new MySqlCommand(asSql, myConnect);
                            MySqlDataReader queryCommandReader9 = command9.ExecuteReader();
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

        // Stammdaten Objekte wurde geändert
        private void dgrStObj_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            btnSave.IsEnabled = true;
        }

        // Firma geändert
        private void dgrStCmp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int liId = 0;
            int liSel = dgrStCmp.SelectedIndex;
            int liRows = 0;
            string lsSql2 = "";

            if (liSel >= 0)
            {
                DataRowView rowview = dgrStCmp.SelectedItem as DataRowView;

                if ((rowview.Row[0] != DBNull.Value))
                {
                    liId = Int32.Parse(rowview.Row[0].ToString());
                    // Adressen dazu holen
                    // SqlSelect erstellen
                    lsSql2 = getSql( 2, liId);
                    // Daten holen
                    liRows = fetchData(lsSql2, 2);
                    btnAdd.IsEnabled = true;
                }
            }
        }


        // Anderes Objekt wurde gewählt
        private void dgrStObj_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int liId = 0;
            int liSel = dgrStObj.SelectedIndex;
            int liRows = 0;
            string lsSql2 = "";

            if (liSel >= 0)
            {
                DataRowView rowview = dgrStObj.SelectedItem as DataRowView;

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
                    lsSql2 = getSql( 3, liId);
                    // Daten holen
                    liRows = fetchData(lsSql2, 3);

                }
            }
        }

        // Adresse Editiert
        private void dgrAdr_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            btnAdrSave.IsEnabled = true;
        }

        // Adresse Anwahl geändert
        private void dgrAdr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnAdrDel.IsEnabled = true;
        }

        // Objekt speichern, löschen
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            int liId = 0;
            int liOk = 0;
            int liSel = dgrStObj.SelectedIndex;
            int liRows = 0;
            string lsSql = "";


            btnSave.IsEnabled = false;
            btnAdd.IsEnabled = true;

            if (btnSave.Content.ToString() == "Speichern")
            {
                liOk = fetchData("", 5);
            }
            else  // Löschen
            {
                if (liSel >= 0)
                {
                    DataRowView rowview = dgrStObj.SelectedItem as DataRowView;
                    if ((rowview.Row[0] != DBNull.Value))
                    {
                        liId = Int32.Parse(rowview.Row[0].ToString());

                        if (liId >= 0)
                        {
                            // Objekt Löschen
                            lsSql = getSql(6, liId);
                            liOk = fetchData(lsSql, 6);
                        }
                    }
                }
            }

            // Daten neu holen
            // Daten Objekte neu holen
            if (liSel >= 0)
            {
                DataRowView rowview = dgrStCmp.SelectedItem as DataRowView;

                if ((rowview.Row[0] != DBNull.Value))
                {
                    liId = Int32.Parse(rowview.Row[0].ToString());

                    // SqlSelect erstellen
                    lsSql = getSql( 2, liId);
                    // Daten holen
                    liRows = fetchData(lsSql,2);
                }
            }

            btnSave.Content = "Speichern";
            btnDel.IsEnabled = true;
            btnAdd.IsEnabled = true;
        }

        // Objekt zufügen
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            int liSel = dgrStCmp.SelectedIndex;
            int liId = 0;

            // Buttons 
            btnAdd.IsEnabled = false;
            btnSave.IsEnabled = true;
            if (liSel >= 0)
            {
                DataRowView rowview = dgrStCmp.SelectedItem as DataRowView;

                if ((rowview.Row[0] != DBNull.Value))
                {
                    liId = Int32.Parse(rowview.Row[0].ToString());
                    DataRow dr = tableObj.NewRow();
                    // Vorgaben eintragen, hier Firmen ID id_filiale
                    dr[3] = liId;
                    dr[1] = "NEUES OBJEKT";
                    tableObj.Rows.InsertAt(dr,0);
                    dgrStCmp.SelectedIndex = 0;
                }
            }
        }

        // Objekt löschen
        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            btnSave.IsEnabled = true;
            btnSave.Content = "Wirklich löschen?";
            btnDel.IsEnabled = false;
        }

        // Existiert ein TeilObjekt zu dem Objekt mit der gewählten ID?
        private int getDelInfo(int aiId)
        {
            int liId = 0;
            String lsSql = "";

            lsSql = getSql(7, aiId);
            liId = fetchData(lsSql, 7);

            return liId;
        }

        // Button Adresse Speichern
        private void btnAdrSave_Click(object sender, RoutedEventArgs e)
        {
            int liId = 0;
            int liOk = 0;
            int liSelObj = dgrStObj.SelectedIndex;
            int liSelAdr = dgrAdr.SelectedIndex;
            string lsSql = "";

            btnAdrSave.IsEnabled = false;
            btnAdrAdd.IsEnabled = true;

            if (btnAdrSave.Content.ToString() == "Speichern")
            {
                liOk = fetchData("", 8);
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
                            // Adresse löschen
                            lsSql = getSql(9, liId);
                            liOk = fetchData(lsSql, 9);
                            // Adressen neu holen
                            lsSql = getSql(3, liId);
                            liOk = fetchData(lsSql, 3);
                        }
                    }
                }
            }
            btnAdrSave.Content = "Speichern";
            btnAdrDel.IsEnabled = true;
        }

        // Button Adresse zufügen
        private void btnAdrAdd_Click(object sender, RoutedEventArgs e)
        {
            int liSel = dgrStObj.SelectedIndex;
            int liId = 0;

            // Buttons 
            btnAdd.IsEnabled = false;
            btnSave.IsEnabled = true;

            if (liSel >= 0)
            {
                DataRowView rowview = dgrStObj.SelectedItem as DataRowView;

                if ((rowview.Row[0] != DBNull.Value))
                {
                    // Objekt ID
                    liId = Int32.Parse(rowview.Row[0].ToString());

                    DataRow dr = tableAdr.NewRow();

                    // Vorgaben eintragen, hier Objekt ID
                    dr[2] = liId;

                    tableAdr.Rows.InsertAt(dr,0);
                }
            }
        }

        // Button Adresse löschen
        private void btnAdrDel_Click(object sender, RoutedEventArgs e)
        {
            btnAdrSave.IsEnabled = true;
            btnAdrSave.Content = "Wirklich löschen?";
            btnAdrDel.IsEnabled = false;
        }
    }
}
