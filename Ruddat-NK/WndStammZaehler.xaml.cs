using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace Ruddat_NK
{
    /// <summary>
    /// Interaktionslogik für WndStammContract.xaml
    /// </summary>
    public partial class WndStammZaehler : Window
    {
        private MainWindow mainWindow;
        public String gsConnect;
        public int giDb;

        // ConnectString übernehmen
        public string psConnect { get; set; }
        public int giObjId = 0;

        DataTable tableCmp;
        DataTable tableZaehler;
        DataTable tableZaehlerArt;
        DataTable tableObj;
        DataTable tableObjTeil;
        DataTable tableEinheit;
        DataTable tableMwst;

        SqlDataAdapter sdEinheit;
        SqlDataAdapter sdObjTeil;
        SqlDataAdapter sdObj;
        SqlDataAdapter sdZaehlerArt;
        SqlDataAdapter sdZaehler;
        SqlDataAdapter sdCmp;
        SqlDataAdapter sdMwst;

        MySqlDataAdapter mysdEinheit;
        MySqlDataAdapter mysdObjTeil;
        MySqlDataAdapter mysdObj;
        MySqlDataAdapter mysdZaehlerArt;
        MySqlDataAdapter mysdZaehler;
        MySqlDataAdapter mysdCmp;
        MySqlDataAdapter mysdMwst;

        // Hier Übergabe des Mainwindows für Übergabe des ConnectStrings
        public WndStammZaehler(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();

            // ConnectString global
            gsConnect = this.mainWindow.psConnect;

            // save +  del Button abschalten
            this.btnAdd.IsEnabled = false;
            this.btnSave.IsEnabled = false;
            this.btnDel.IsEnabled = false;
        }

        // Datenbankart
        internal void getDb(int aiDb)
        {
            String lsSql = "";
            int liRows = 0;

            giDb = aiDb;

            // SqlSelect Firmen erstellen
            lsSql = getSql( 1, 0);
            // Daten Firmen holen
            liRows = fetchData(lsSql, 1);

            // SqlSelect Zählerart Combobox
            lsSql = getSql( 5, 0);
            // Daten Zählerart holen
            liRows = fetchData(lsSql, 5);

            // SqlSelect Mwst Combobox
            lsSql = getSql( 6, 0);
            // Daten Mwst art holen
            liRows = fetchData(lsSql, 6);

            // SqlSelect Einheit Combobox
            lsSql = getSql( 7, 0);
            // Daten Einheiten Art holen
            liRows = fetchData(lsSql, 7);

            // SqlSelect Zähler
            lsSql = getSql( 2, 0);
            // Daten Zähler holen
            liRows = fetchData(lsSql, 2);

            // SqlSelect Objekte
            lsSql = getSql( 3, 0);
            // Daten Objekte holen
            liRows = fetchData(lsSql, 3);

            // SqlSelect ObjektTeile
            lsSql = getSql( 4, 0);
            // Daten ObjektTeile holen
            liRows = fetchData(lsSql, 4);
        }

        // Sql zusammenstellen
        private string getSql( int aiArt, int aiId)
        {
            string lsSql = "";

            switch (aiArt)
            {
                case 1:         // Gesellschaft
                    lsSql = "select id_filiale,name,name_2,bez from filiale order by id_filiale";
                    break;
                case 2:         // Zähler
                    lsSql = @"Select Id_zaehler, id_objekt, id_objekt_teil, zaehlernummer, zaehlerort, termin_ablesung, id_zaehler_art, zyklus, id_einheit, id_mwst_art  
	                            from zaehler
                                Order by zaehlernummer desc";
                    break;
                case 21:         // Zähler mit Objekt
                    lsSql = @"Select Id_zaehler, id_objekt, id_objekt_teil, zaehlernummer, zaehlerort, termin_ablesung, id_zaehler_art, zyklus, id_einheit, id_mwst_art  
	                            from zaehler
                                where id_objekt = " + aiId.ToString() + " and id_objekt_teil < 1 Order by zaehlernummer desc ";
                    break;
                case 22:         // Zähler mit Objektteil
                    lsSql = @"Select Id_zaehler, id_objekt, id_objekt_teil, zaehlernummer, zaehlerort, termin_ablesung, id_zaehler_art, zyklus, id_einheit, id_mwst_art  
	                            from zaehler
                                where id_objekt_teil = " + aiId.ToString() + " Order by zaehlernummer desc ";
                    break;
                case 3:         // Objekte
                    lsSql = @"Select Id_objekt,bez as objbez ,nr_obj from objekt
                                where id_filiale = " + aiId.ToString() + " Order by bez";
                    break;
                case 4:         // ObjektTeile
                    lsSql = @"Select Id_objekt_teil,bez as objteilbez from objekt_teil 
                                where id_objekt = " + aiId.ToString() + " Order by bez";
                    break;
                case 5:         // ZählerArt
                    lsSql = @"Select Id_zaehler_art as idza, bez from art_zaehler
                                Order by bez";
                    break;
                case 6:         // Mwst Satz
                    lsSql = @"Select id_mwst_art as idmw, mwst from art_mwst
                                Order by bez";
                    break;
                case 7:         // Einheit
                    lsSql = @"Select id_einheit as ideh, bez from art_einheit
                                Order by bez";
                    break;
                case 9:         // Zähler löschen
                    lsSql = "Delete from Zaehler Where id_Zaehler = " + aiId.ToString();
                    break;
                case 10:        // Vor löschen prüfen
                    lsSql = @"Select id_zaehler from zaehlerstaende where id_zaehler = " + aiId.ToString();
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
                        case 1: // Firmen
                            tableCmp = new DataTable();
                            SqlCommand command = new SqlCommand(asSql, connect);
                            sdCmp = new SqlDataAdapter(command);
                            sdCmp.Fill(tableCmp);
                            dgrStCmp.ItemsSource = tableCmp.DefaultView;
                            break;
                        case 2: // Zähler
                            tableZaehler = new DataTable();
                            SqlCommand command2 = new SqlCommand(asSql, connect);
                            sdZaehler = new SqlDataAdapter(command2);
                            sdZaehler.Fill(tableZaehler);
                            dgrStZaehler.ItemsSource = tableZaehler.DefaultView;
                            break;
                        case 3: // Objekte
                            tableObj = new DataTable();
                            SqlCommand command3 = new SqlCommand(asSql, connect);
                            sdObj = new SqlDataAdapter(command3);
                            sdObj.Fill(tableObj);
                            dgrStObj.ItemsSource = tableObj.DefaultView;
                            break;
                        case 4: // ObjektTeile
                            tableObjTeil = new DataTable();
                            SqlCommand command4 = new SqlCommand(asSql, connect);
                            sdObjTeil = new SqlDataAdapter(command4);
                            sdObjTeil.Fill(tableObjTeil);
                            dgrObjTeil.ItemsSource = tableObjTeil.DefaultView;
                            break;
                        case 5: // Combobox ZählerArt
                            tableZaehlerArt = new DataTable();
                            SqlCommand command5 = new SqlCommand(asSql, connect);
                            sdZaehlerArt = new SqlDataAdapter(command5);
                            sdZaehlerArt.Fill(tableZaehlerArt);
                            artZaehler.ItemsSource = tableZaehlerArt.DefaultView;
                            break;
                        case 6: // Combobox Mwst Art
                            tableMwst = new DataTable();
                            SqlCommand command6 = new SqlCommand(asSql, connect);
                            sdMwst = new SqlDataAdapter(command6);
                            sdMwst.Fill(tableMwst);
                            artMwst.ItemsSource = tableMwst.DefaultView;
                            break;
                        case 7: // Combobox Einheit
                            tableEinheit = new DataTable();
                            SqlCommand command7 = new SqlCommand(asSql, connect);
                            sdEinheit = new SqlDataAdapter(command7);
                            sdEinheit.Fill(tableEinheit);
                            artEinheit.ItemsSource = tableEinheit.DefaultView;
                            break;
                        case 8:     // Zähler speichern
                            SqlCommandBuilder commandBuilder = new SqlCommandBuilder(sdZaehler);
                            sdZaehler.Update(tableZaehler);
                            break;
                        case 9:     // Zähler löschen
                            SqlCommand command9 = new SqlCommand(asSql, connect);
                            SqlDataReader queryCommandReader = command9.ExecuteReader();
                            break;
                        case 10:    // Vor Löschen prüfen
                            SqlCommand command10 = new SqlCommand(asSql, connect);
                            var lvId = command10.ExecuteScalar();
                            if (lvId != DBNull.Value)
                            {
                                liRows = Convert.ToInt32(lvId);
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
                        case 1: // Firmen
                            tableCmp = new DataTable();
                            MySqlCommand command = new MySqlCommand(asSql, myConnect);
                            mysdCmp = new MySqlDataAdapter(command);
                            mysdCmp.Fill(tableCmp);
                            dgrStCmp.ItemsSource = tableCmp.DefaultView;
                            break;
                        case 2: // Zähler
                            tableZaehler = new DataTable();
                            MySqlCommand command2 = new MySqlCommand(asSql, myConnect);
                            mysdZaehler = new MySqlDataAdapter(command2);
                            mysdZaehler.Fill(tableZaehler);
                            dgrStZaehler.ItemsSource = tableZaehler.DefaultView;
                            break;
                        case 3: // Objekte
                            tableObj = new DataTable();
                            MySqlCommand command3 = new MySqlCommand(asSql, myConnect);
                            mysdObj = new MySqlDataAdapter(command3);
                            mysdObj.Fill(tableObj);
                            dgrStObj.ItemsSource = tableObj.DefaultView;
                            break;
                        case 4: // ObjektTeile
                            tableObjTeil = new DataTable();
                            MySqlCommand command4 = new MySqlCommand(asSql, myConnect);
                            mysdObjTeil = new MySqlDataAdapter(command4);
                            mysdObjTeil.Fill(tableObjTeil);
                            dgrObjTeil.ItemsSource = tableObjTeil.DefaultView;
                            break;
                        case 5: // Combobox ZählerArt
                            tableZaehlerArt = new DataTable();
                            MySqlCommand command5 = new MySqlCommand(asSql, myConnect);
                            mysdZaehlerArt = new MySqlDataAdapter(command5);
                            mysdZaehlerArt.Fill(tableZaehlerArt);
                            artZaehler.ItemsSource = tableZaehlerArt.DefaultView;
                            break;
                        case 6: // Combobox Mwst Art
                            tableMwst = new DataTable();
                            MySqlCommand command6 = new MySqlCommand(asSql, myConnect);
                            mysdMwst = new MySqlDataAdapter(command6);
                            mysdMwst.Fill(tableMwst);
                            artMwst.ItemsSource = tableMwst.DefaultView;
                            break;
                        case 7: // Combobox Einheit
                            tableEinheit = new DataTable();
                            MySqlCommand command7 = new MySqlCommand(asSql, myConnect);
                            mysdEinheit = new MySqlDataAdapter(command7);
                            mysdEinheit.Fill(tableEinheit);
                            artEinheit.ItemsSource = tableEinheit.DefaultView;
                            break;
                        case 8:     // Zähler speichern
                            MySqlCommandBuilder commandBuilder = new MySqlCommandBuilder(mysdZaehler);
                            mysdZaehler.Update(tableZaehler);
                            break;
                        case 9:     // Zähler löschen
                            MySqlCommand command9 = new MySqlCommand(asSql, myConnect);
                            MySqlDataReader queryCommandReader = command9.ExecuteReader();
                            break;
                        case 10:    // Vor Löschen prüfen
                            MySqlCommand command10 = new MySqlCommand(asSql, myConnect);
                            var lvId = command10.ExecuteScalar();
                            if (lvId != DBNull.Value)
                            {
                                liRows = Convert.ToInt32(lvId);
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

        // Firma geändert
        private void dgrStCmp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int liId = 0;
            int liSel = dgrStCmp.SelectedIndex;
            int liRows = 0;
            string lsSql = "";

            if (liSel >= 0)
            {
                // datagrid Verträge leeren
                dgrStZaehler.ItemsSource = null;

                DataRowView rowview = dgrStCmp.SelectedItem as DataRowView;

                if ((rowview.Row[0] != DBNull.Value))
                {
                    liId = Int32.Parse(rowview.Row[0].ToString());
                    // Objekte dazu holen

                    // SqlSelect Objekte
                    lsSql = getSql( 3, liId);
                    // Daten Firmen holen
                    liRows = fetchData(lsSql, 3);
                }
                btnDel.IsEnabled = false;
            }
        }

        // Objekt angwewählt : Teilobjekte dazu zeigen : Zähler dazu zeigen
        private void dgrStObj_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int liId = 0;
            int liSel = dgrStObj.SelectedIndex;
            int liRows = 0;
            string lsSql = "";

            if (liSel >= 0)
            {

                // datagrid Zähler leeren
                dgrStZaehler.ItemsSource = null;

                DataRowView rowview = dgrStObj.SelectedItem as DataRowView;

                if ((rowview.Row[0] != DBNull.Value))
                {

                    liId = Int32.Parse(rowview.Row[0].ToString());

                    // Objektteile
                    // SqlSelect erstellen
                    lsSql = getSql( 4, liId);
                    // Daten holen
                    liRows = fetchData(lsSql, 4);
                    // Zähler
                    // SqlSelect erstellen
                    lsSql = getSql( 21, liId);
                    // Daten holen
                    liRows = fetchData(lsSql, 2);
                }
                btnAdd.IsEnabled = true;
                btnDel.IsEnabled = false;
            }
        }

        // ObjektTeil angewählt, Zähler dazu zeigen
        private void dgrObjTeil_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int liId = 0;
            int liSel = dgrObjTeil.SelectedIndex;
            int liRows = 0;
            string lsSql = "";

            if (liSel >= 0)
            {
                DataRowView rowview = dgrObjTeil.SelectedItem as DataRowView;

                if ((rowview.Row[0] != DBNull.Value))
                {
                    liId = Int32.Parse(rowview.Row[0].ToString());

                    // SqlSelect erstellen
                    lsSql = getSql( 22, liId);
                    // Daten holen
                    liRows = fetchData(lsSql, 2);
                }
                btnAdd.IsEnabled = true;
                btnDel.IsEnabled = false;
            }
        }

        // Speichern und löschen (nur, wenn keine Zahlung auf den Vertrag gebucht sind)
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            int liId = 0;
            int liOk = 0;
            int liSel = dgrStZaehler.SelectedIndex;
            int liRows = 0;
            string lsSql2 = "";

            btnSave.IsEnabled = false;
            btnAdd.IsEnabled = true;

            if (btnSave.Content.ToString() == "Speichern")
            {
                liOk = fetchData("", 8);
            }
            else  // Löschen
            {
                if (liSel >= 0)
                {
                    DataRowView rowview = dgrStZaehler.SelectedItem as DataRowView;
                    if ((rowview.Row[0] != DBNull.Value))
                    {
                        liId = Int32.Parse(rowview.Row[0].ToString());

                        if (liId >= 0)
                        {
                            // Den Zähler löschen
                            String lsSql = "Delete from Zaehler Where id_Zaehler = " + liId.ToString();
                            liOk = fetchData(lsSql, 9);
                        }
                    }
                }
            }

            // Daten Zähler neu holen
            if (liSel >= 0)
            {
                // Das gewählte Teilobjekt
                DataRowView rowview = dgrStZaehler.SelectedItem as DataRowView;

                if ((rowview.Row[0] != DBNull.Value))
                {
                    liId = Int32.Parse(rowview.Row[0].ToString());

                    // SqlSelect erstellen
                    lsSql2 = getSql( 22, liId);
                    // Daten holen
                    liRows = fetchData(lsSql2, 2);
                }
            }
            btnSave.Content = "Speichern";
        }

        // Dgr wurde bearbeitet
        private void dgrStZaehler_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            btnSave.IsEnabled = true;
        }

        // Löschen nur, wen kein Zählerstand gebucht ist
        private void dgrStZaehler_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int liZaehlerId = 0;
            int liSel = dgrStZaehler.SelectedIndex;
           

            if (liSel >= 0)
            {
                DataRowView rowview = dgrStZaehler.SelectedItem as DataRowView;

                //  Zähler ID holen
                if ((rowview.Row[0] != DBNull.Value))
                {
                    liZaehlerId = Int32.Parse(rowview.Row[0].ToString());
                    // Prüfen 
                    if (getDelInfo(liZaehlerId) == 0)
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

        // Existiert ein Zählerstand mit der gewählten ID?
        private int getDelInfo(int aiId)
        {
            int liId = 0;
            String lsSql = "";

            lsSql = getSql(10, aiId);
            liId = fetchData(lsSql, 10);

            return liId;
        }

        // Zähler zufügen zufügen
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            int liSelOT = dgrObjTeil.SelectedIndex;
            int liSelObj = dgrStObj.SelectedIndex;

            // Buttons 
            btnAdd.IsEnabled = false;
            btnSave.IsEnabled = true;

            DataRow dr = tableZaehler.NewRow();

            if (liSelObj >= 0 &&  liSelOT == -1)
            {
                // Gewähltes Objekt
                DataRowView rowviewObj = dgrStObj.SelectedItem as DataRowView;
                if (rowviewObj.Row[0] != DBNull.Value)
                {
                    dr[1] = Int32.Parse(rowviewObj.Row[0].ToString());
                    dr[2] = 0;
                }
                dr[5] = DateTime.Now;
                dr[6] = 1; // Voragbe elektrisch
                tableZaehler.Rows.InsertAt(dr, 0);
            }
 
            if ((liSelObj >= 0) && (liSelOT >= 0))
            {
                // Gewähltes Teilobjekt
                DataRowView rowviewObj = dgrStObj.SelectedItem as DataRowView;
                DataRowView rowviewOt = dgrObjTeil.SelectedItem as DataRowView;
                if (rowviewOt.Row[0] != DBNull.Value)
                {
                    dr[1] = Int32.Parse(rowviewObj.Row[0].ToString());
                    dr[2] = Int32.Parse(rowviewOt.Row[0].ToString());
                }

                dr[5] = DateTime.Now;
                dr[6] = 1; // Voragbe elektrisch
                
                tableZaehler.Rows.InsertAt(dr, 0);
            }
        }

        // Zähler löschen
        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            btnSave.IsEnabled = true;
            btnSave.Content = "Wirklich löschen?";
            btnDel.IsEnabled = false;
        }
    }
}
