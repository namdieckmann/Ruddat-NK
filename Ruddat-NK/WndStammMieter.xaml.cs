using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MySql.Data.MySqlClient;

namespace Ruddat_NK
{
    /// <summary>
    /// Interaktionslogik für WndStammObjekte.xaml
    /// </summary>
    public partial class WndStammMieter : Window
    {
        private MainWindow mainWindow;
        public String gsConnect;
        public int giDb;

        // ConnectString übernehmen
        public string psConnect { get; set; }
        public int giObjId = 0;

        DataTable tableCmp;
        DataTable tableMieter;
        DataTable tableAdr;
        DataTable tableAda;
        DataTable tableObj;

        SqlDataAdapter sdAda;
        SqlDataAdapter sdAdr;
        SqlDataAdapter sdMieter;
        SqlDataAdapter sdCmp;
        SqlDataAdapter sdObj;

        MySqlDataAdapter mysdAda;
        MySqlDataAdapter mysdAdr;
        MySqlDataAdapter mysdMieter;
        MySqlDataAdapter mysdCmp;
        MySqlDataAdapter mysdObj;


        // Hier Übergabe des Mainwindows für Übergabe des ConnectStrings
        public WndStammMieter(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();

            // ConnectString global
            gsConnect = this.mainWindow.psConnect;

            // save +  del Button abschalten
            this.btnAdd.IsEnabled = false;
            this.btnSave.IsEnabled = false;
            this.btnDel.IsEnabled = false;
            this.btnAdrSave.IsEnabled = false;
            this.btnAdrDel.IsEnabled = false;
            this.btnAdrAdd.IsEnabled = false;


        }

        internal void getDb(int aiDb)
        {
            string lsSql = "";
            int liRows = 0;

            giDb = aiDb;
            // SqlSelect Firmen erstellen
            lsSql = getSql(1, 0);
            // Daten Firmen holen
            liRows = fetchData(lsSql, 1);

            // SqlSelect Mieter
            lsSql = getSql(2, 0);
            // Daten Firmen holen
            liRows = fetchData(lsSql, 2);

            // SqlSelect Adressen
            lsSql = getSql(3, 0);
            // Daten Firmen holen
            liRows = fetchData(lsSql, 3);

            // SqlSelect AdressArten
            lsSql = getSql(4, 0);
            // Daten Firmen holen
            liRows = fetchData(lsSql, 4);
        }


        // Sql zusammenstellen
        private string getSql(int aiArt, int aiId)
        {
            string lsSql = "";

            switch (aiArt)
            {
                case 1:         // Gesellschaft
                    lsSql = "select id_filiale,name,name_2,bez from filiale order by id_filiale";
                    break;
                case 2:         // Mieter
                    lsSql = @"Select Id_mieter,id_objekt,bez,nr,netto,id_filiale from mieter
                                Order by bez";
                    break;
                case 21:         // Mieter mit objekt id
                    lsSql = @"Select Id_mieter,id_objekt,bez,nr,netto,id_filiale from mieter
                                where id_objekt = " + aiId.ToString() + " Order by bez";
                    break;
                case 22:         // Mieter nur mit Firmen ID id (Leerstand)
                    lsSql = @"Select Id_mieter,id_objekt,bez,nr,netto,id_filiale from mieter
                                where id_filiale = " + aiId.ToString() + " Order by bez";
                    break;
                case 3:         // Adressen
                    lsSql = @"select id_adressen, id_art_adresse, id_objekt, id_objekt_teil, id_filiale, Id_mieter, anrede, name, vorname, 
                                    firma, adresse, plz, ort, land, tel, mail, mobil, homepage, aktiv   
                                from adressen
                                where id_mieter = " + aiId.ToString() + " Order by id_art_adresse";
                    break;
                case 4:         // Adressarten
                    lsSql = @"Select id_art_adresse,bez from art_adresse Order by sort";
                    break;

                case 5:         // Objekte
                    lsSql = @"Select Id_objekt,bez,nr_obj from objekt
                                where id_filiale = " + aiId.ToString() + " Order by bez";
                    break;
                case 7:         // Mieter löschen
                    lsSql = "Delete from mieter Where id_mieter = " + aiId.ToString();
                    break;
                case 8:         // Püfen auf Löschen
                    lsSql = @"Select id_mieter from vertrag where id_mieter = " + aiId.ToString();
                    break;
                case 9:         // Darf der Mieter für Leerstand gelöscht werden
                    lsSql = @"Select id_mieter from timeline where id_mieter = " + aiId.ToString();
                    break;
                case 11:        // Adresse löschen
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

            switch (giDb)
            {
                case 1:
                    SqlConnection connect;
                    connect = new SqlConnection(gsConnect);
                    connect.Open();
                    switch (aiArt)
                    {
                        case 1:     // Firmen
                            tableCmp = new DataTable();
                            SqlCommand command = new SqlCommand(asSql, connect);
                            sdCmp = new SqlDataAdapter(command);
                            sdCmp.Fill(tableCmp);
                            dgrStCmp.ItemsSource = tableCmp.DefaultView;
                            liRows = tableCmp.Rows.Count;
                            break;
                        case 2:     // Mieter
                            tableMieter = new DataTable();
                            SqlCommand command2 = new SqlCommand(asSql, connect);
                            sdMieter = new SqlDataAdapter(command2);
                            sdMieter.Fill(tableMieter);
                            dgrStMieter.ItemsSource = tableMieter.DefaultView;
                            liRows = tableMieter.Rows.Count;
                            break;
                        case 3:     // Adressen
                            tableAdr = new DataTable();
                            SqlCommand command3 = new SqlCommand(asSql, connect);
                            sdAdr = new SqlDataAdapter(command3);
                            sdAdr.Fill(tableAdr);
                            dgrAdr.ItemsSource = tableAdr.DefaultView;
                            liRows = tableAdr.Rows.Count;
                            break;
                        case 4:     // Adressarten
                            tableAda = new DataTable();
                            SqlCommand command4 = new SqlCommand(asSql, connect);
                            sdAda = new SqlDataAdapter(command4);
                            sdAda.Fill(tableAda);
                            adressenart.ItemsSource = tableAda.DefaultView;
                            liRows = tableAda.Rows.Count;
                            break;
                        case 5:     // Objekte
                            tableObj = new DataTable();
                            SqlCommand command5 = new SqlCommand(asSql, connect);
                            sdObj = new SqlDataAdapter(command5);
                            sdObj.Fill(tableObj);
                            dgrStObj.ItemsSource = tableObj.DefaultView;
                            liRows = tableObj.Rows.Count;
                            break;
                        case 6:     // Mieter speichern
                            SqlCommandBuilder commandBuilder6 = new SqlCommandBuilder(sdMieter);
                            sdMieter.Update(tableMieter);
                            break;
                        case 7:     // Mieter löschen
                            SqlCommand command7 = new SqlCommand(asSql, connect);
                            SqlDataReader queryCommandReader = command7.ExecuteReader();
                            break;
                        case 8:     // Mieter prüfen auf Löschen
                            SqlCommand command8 = new SqlCommand(asSql, connect);
                            var lvId = command8.ExecuteScalar();

                            if (lvId != DBNull.Value)
                            {
                                liRows = Convert.ToInt32(lvId);
                            }
                            else
                            {
                                liRows = 0;
                            }
                            break;
                        case 9:     // Prüfen Löschen Mieter Leerstand
                            SqlCommand command9 = new SqlCommand(asSql, connect);
                            var lvId9 = command9.ExecuteScalar();
                            if (lvId9 != DBNull.Value)
                            {
                                liRows = Convert.ToInt32(lvId9);
                            }
                            else
                            {
                                liRows = 0;
                            }
                            break;
                        case 10:        // Adresse speichern
                            SqlCommandBuilder commandBuilder10 = new SqlCommandBuilder(sdAdr);
                            sdAdr.Update(tableAdr);
                            break;
                        case 11:        // Adresse löschen
                            SqlCommand command11 = new SqlCommand(asSql, connect);
                            SqlDataReader queryCommandReader11 = command11.ExecuteReader();
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
                        case 1:     // Firmen
                            tableCmp = new DataTable();
                            MySqlCommand command = new MySqlCommand(asSql, myConnect);
                            mysdCmp = new MySqlDataAdapter(command);
                            mysdCmp.Fill(tableCmp);
                            dgrStCmp.ItemsSource = tableCmp.DefaultView;
                            liRows = tableCmp.Rows.Count;
                            break;
                        case 2:     // Mieter
                            tableMieter = new DataTable();
                            MySqlCommand command2 = new MySqlCommand(asSql, myConnect);
                            mysdMieter = new MySqlDataAdapter(command2);
                            mysdMieter.Fill(tableMieter);
                            dgrStMieter.ItemsSource = tableMieter.DefaultView;
                            liRows = tableMieter.Rows.Count;
                            break;
                        case 3:     // Adressen
                            tableAdr = new DataTable();
                            MySqlCommand command3 = new MySqlCommand(asSql, myConnect);
                            mysdAdr = new MySqlDataAdapter(command3);
                            mysdAdr.Fill(tableAdr);
                            dgrAdr.ItemsSource = tableAdr.DefaultView;
                            liRows = tableAdr.Rows.Count;
                            break;
                        case 4:     // Adressarten
                            tableAda = new DataTable();
                            MySqlCommand command4 = new MySqlCommand(asSql, myConnect);
                            mysdAda = new MySqlDataAdapter(command4);
                            mysdAda.Fill(tableAda);
                            adressenart.ItemsSource = tableAda.DefaultView;
                            liRows = tableAda.Rows.Count;
                            break;
                        case 5:     // Objekte
                            tableObj = new DataTable();
                            MySqlCommand command5 = new MySqlCommand(asSql, myConnect);
                            mysdObj = new MySqlDataAdapter(command5);
                            mysdObj.Fill(tableObj);
                            dgrStObj.ItemsSource = tableObj.DefaultView;
                            liRows = tableObj.Rows.Count;
                            break;
                        case 6:     // Mieter speichern
                            MySqlCommandBuilder commandBuilder6 = new MySqlCommandBuilder(mysdMieter);
                            mysdMieter.Update(tableMieter);
                            break;
                        case 7:     // Mieter löschen
                            MySqlCommand command7 = new MySqlCommand(asSql, myConnect);
                            MySqlDataReader queryCommandReader = command7.ExecuteReader();
                            break;
                        case 8:     // Mieter prüfen auf Löschen
                            MySqlCommand command8 = new MySqlCommand(asSql, myConnect);
                            var lvId = command8.ExecuteScalar();

                            if (lvId != DBNull.Value)
                            {
                                liRows = Convert.ToInt32(lvId);
                            }
                            else
                            {
                                liRows = 0;
                            }
                            break;
                        case 9:     // Prüfen Löschen Mieter Leerstand
                            MySqlCommand command9 = new MySqlCommand(asSql, myConnect);
                            var lvId9 = command9.ExecuteScalar();
                            if (lvId9 != DBNull.Value)
                            {
                                liRows = Convert.ToInt32(lvId9);
                            }
                            else
                            {
                                liRows = 0;
                            }
                            break;
                        case 10:        // Adresse speichern
                            MySqlCommandBuilder commandBuilder10 = new MySqlCommandBuilder(mysdAdr);
                            mysdAdr.Update(tableAdr);
                            break;
                        case 11:        // Adresse löschen
                            MySqlCommand command11 = new MySqlCommand(asSql, myConnect);
                            MySqlDataReader queryCommandReader11 = command11.ExecuteReader();
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

        // Stammdaten Mieter wurde geändert
        private void dgrStMieter_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            btnSave.IsEnabled = true;
        }

        // Firma geändert
        private void dgrStCmp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int liId = 0;
            int liSel = dgrStCmp.SelectedIndex;
            int liRows = 0;
            string lsSql = "";
            string lsSql2 = "";

            if (liSel >= 0)
            {

                DataRowView rowview = dgrStCmp.SelectedItem as DataRowView;

                if ((rowview.Row[0] != DBNull.Value))
                {
                    liId = Int32.Parse(rowview.Row[0].ToString());
                    // Objekte dazu holen

                    // SqlSelect Objekte
                    lsSql = getSql(5, liId);
                    // Daten Firmen holen
                    liRows = fetchData(lsSql, 5);
                    dgrAdr.ItemsSource=null;
                    dgrStMieter.ItemsSource = null;

                    // SqlSelect Mieter Leerstand
                    // SqlSelect erstellen
                    lsSql2 = getSql(22, liId);
                    // Daten holen
                    liRows = fetchData(lsSql2, 2);

                    if (liRows == 0)
                    {
                        btnAdd.IsEnabled = true;
                        btnDel.IsEnabled = false;
                    }
                    else
                    {
                        // Zufügen nur über Anwahl eines Teilopbjektes
                        btnAdd.IsEnabled = false;

                        // Darf der Mieter Leerstand gelöscht werden
                        if (getDelLeerstandInfo(liId) == 0)
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
                    }


                    dgrAdr.ItemsSource = null;                        
                }
            }
        }

        // Objekt angwewählt : Mieter dazu zeigen
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

                    // SqlSelect erstellen
                    lsSql2 = getSql( 21, liId);
                    // Daten holen
                    liRows = fetchData(lsSql2, 2);

                    btnAdd.IsEnabled = true;
                    btnDel.IsEnabled = false;
                    dgrAdr.ItemsSource = null;
                }
            }
        }

        // Anderer Mieter wurde gewählt
        private void dgrStMieter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int liId = 0;
            int liSel = dgrStMieter.SelectedIndex;
            int liRows = 0;
            string lsSql2 = "";

            if (liSel >= 0)
            {
                DataRowView rowview = dgrStMieter.SelectedItem as DataRowView;

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

        // Mieter speichern, löschen
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            int liId = 0;
            int liOk = 0;
            int liSel = dgrStMieter.SelectedIndex;
            int liRows = 0;
            string lsSql = "";

            btnSave.IsEnabled = false;
            btnAdd.IsEnabled = true;

            if (btnSave.Content.ToString() == "Speichern")
            {
                liOk = fetchData("", 6);
            }
            else  // Löschen
            {
                if (liSel >= 0)
                {
                    DataRowView rowview = dgrStMieter.SelectedItem as DataRowView;
                    if ((rowview.Row[0] != DBNull.Value))
                    {
                        liId = Int32.Parse(rowview.Row[0].ToString());
                        lsSql = getSql(7, liId);
                        liOk = fetchData(lsSql, 7);
                    }
                }
            }

            // Daten Mieter neu holen
            DataRowView rowview3 = dgrStObj.SelectedItem as DataRowView;

            if (rowview3 != null)
            {
                if ((rowview3.Row[0] != DBNull.Value))
                {
                    liId = Int32.Parse(rowview3.Row[0].ToString());

                    // SqlSelect erstellen
                    lsSql = getSql( 21, liId);
                    // Daten holen
                    liRows = fetchData(lsSql, 2);
                }                
            }
            else
            {
                // Die Filiale neu holen
                DataRowView rowview4 = dgrStCmp.SelectedItem as DataRowView;
                if (rowview4 != null)
                {
                    if ((rowview4.Row[0] != DBNull.Value))
                    {
                        liId = Int32.Parse(rowview4.Row[0].ToString());
                        // SqlSelect erstellen
                        lsSql = getSql( 22, liId);
                        // Daten holen
                        liRows = fetchData(lsSql, 2);
                    }
                }
            }

            btnSave.Content = "Speichern";
            btnDel.IsEnabled = true;
        }

        // Mieter zufügen
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {

            int liSel = dgrStObj.SelectedIndex;
            int liSelFiliale = dgrStCmp.SelectedIndex;
            int liId = 0;

            // Buttons 
            btnAdd.IsEnabled = false;
            btnSave.IsEnabled = true;
            if (liSel >= 0)
            {
                DataRowView rowviewObj = dgrStObj.SelectedItem as DataRowView;

                if ((rowviewObj.Row[0] != DBNull.Value))
                {
                    liId = Int32.Parse(rowviewObj.Row[0].ToString());
                    DataRow dr = tableMieter.NewRow();
                    // Vorgaben eintragen, hier Objekt ID
                    // Die Objekt ID wird hier nur für die Mietersuche verwendet,
                    // sonst ist sie irrelevant, da ja die Mieter über die Verträge mit
                    // den Objekten verbunden sind
                    dr[1] = liId;
                    dr[2] = "NEUER MIETER";

                    tableMieter.Rows.InsertAt(dr,0);
                }
            }
            else
            {
                // Mieter für Leerstand zufügen
                if (liSelFiliale >= 0)
                {
                    DataRowView rowview2 = dgrStCmp.SelectedItem as DataRowView;

                    if ((rowview2.Row[0] != DBNull.Value))
                    {
                        liId = Int32.Parse(rowview2.Row[0].ToString());
                        DataRow dr = tableMieter.NewRow();
                        // Vorgaben eintragen, hier Filiale ID
                        dr[5] = liId;
                        dr[2] = "Mieter für Leerstand";

                        tableMieter.Rows.InsertAt(dr, 0);
                    }
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

        // Existiert ein Vertrag zu dem Mieter mit der gewählten ID?
        // Dann nicht löschen
        private int getDelInfo(int aiId)
        {
            int liId = 0;
            String lsSql = "";

            lsSql = getSql(8, aiId);
            liId = fetchData(lsSql, 8);

            return liId;
        }

        // Darf der Mieter für Leerstand gelöscht werden
        private int getDelLeerstandInfo(int aiId)
        {
            int liId = 0;
            String lsSql = "";

            lsSql = getSql(9, aiId);
            liId = fetchData(lsSql, 9);

            return liId;
        }


        // Button Adresse Speichern
        private void btnAdrSave_Click(object sender, RoutedEventArgs e)
        {
            int liId = 0;
            int liOk = 0;
            int liSelObj = dgrStMieter.SelectedIndex;
            int liSelAdr = dgrAdr.SelectedIndex;
            string lsSql = "";

            btnAdrSave.IsEnabled = false;
            btnAdrAdd.IsEnabled = true;

            if (btnAdrSave.Content.ToString() == "Speichern")
            {
                liOk = fetchData("", 10);
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
                            lsSql = getSql(11, liId);
                            liOk = fetchData(lsSql, 11);
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
            int liSel = dgrStMieter.SelectedIndex;
            int liSelObj = dgrStObj.SelectedIndex;
            int liSelMieter = dgrStMieter.SelectedIndex;
            int liId = 0;
            int liIdObj = 0;
            string lsObjBez = "";
            string lsMieterBez = "";

            // Buttons 
            btnAdd.IsEnabled = false;
            btnSave.IsEnabled = true;

            if (liSelObj >= 0)
            {
                DataRowView rowviewObj = dgrStObj.SelectedItem as DataRowView;
                if ((rowviewObj.Row[0] != DBNull.Value))
                {
                    liIdObj = Int32.Parse(rowviewObj.Row[0].ToString());
                    lsObjBez = rowviewObj.Row[1].ToString();

                    DataRowView rowviewMieter = dgrStMieter.SelectedItem as DataRowView;

                    if ((rowviewMieter.Row[0] != DBNull.Value))
                    {
                        lsMieterBez = rowviewMieter.Row[2].ToString();

                        if (liSel >= 0)
                        {
                            // Mieter
                            DataRowView rowview = dgrStMieter.SelectedItem as DataRowView;
                            // Objekt

                            if ((rowview.Row[0] != DBNull.Value))
                            {
                                liId = Int32.Parse(rowview.Row[0].ToString());

                                DataRow dr = tableAdr.NewRow();

                                // Vorgaben eintragen, hier Mieter ID id_mieter
                                dr[5] = liId;
                                dr[10] = lsObjBez;              // Adresse Vorgabe
                                dr[7] = lsMieterBez;            // Mietername
                                dr[1] = 1;                      // Art Adresse

                                tableAdr.Rows.InsertAt(dr, 0);
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Es wurde kein Objekt (Haus) angewählt, keinen Datensatz zugefügt",
                                        "Achtung",
                                    MessageBoxButton.OK);
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

        // Trick: mit Doppelklick wird die Auswahl der Mieter aufgehoben
        // und die Objekt ID wird dem MIeter zugeordnet
        // Der Speichern Button muss auf
        private void dgrStObj_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //int liSel = dgrStObj.SelectedIndex;
            //int liId = 0;
            //int liRows = 0;
            //string lsSql = "";

            //// Daten Mieter neu holen
            //if (liSel >= 0)
            //{
            //    DataRowView rowview = dgrStObj.SelectedItem as DataRowView;

            //    if ((rowview.Row[0] != DBNull.Value))
            //    {
            //        // Objekt ID
            //        liId = Int32.Parse(rowview.Row[0].ToString());
            //        // Die Objekt ID global verfügbar
            //        giObjId = liId;

            //        // SqlSelect erstellen > alle Mieter werden gezeigt
            //        lsSql = getSql("mieter", 2, liId);
            //        // Daten holen
            //        liRows = FetchData(lsSql, 2);
            //    }
            //}
        }

        // Doppelklick auf den Mieter soll hier die Objekt ID eintragen und den Speichern Button öffnen
        private void dgrStMieter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //int liSel = dgrStMieter.SelectedIndex;
            //int liId = 0;

            //// Daten Mieter neu holen
            //if (liSel >= 0)
            //{
            //    DataRowView rowview = dgrStMieter.SelectedItem as DataRowView;

            //    if ((rowview.Row[0] != DBNull.Value))
            //    {
            //        // Mieter Id
            //        liId = Int32.Parse(rowview.Row[0].ToString());

            //        // Objekt ID für den Mieter eintragen
            //        rowview.Row[1] = giObjId;

            //        // Speichern Taste öffnen
            //        btnSave.IsEnabled = true;
            //    }
            //}
        }
    }
}
