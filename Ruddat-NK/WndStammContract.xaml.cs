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
    public partial class WndStammContract : Window
    {
        private MainWindow mainWindow;
        private String gsConnect;
        private int giDb;

        // ConnectString übernehmen
        private string psConnect { get; set; }
        private int giObjId = 0;
        private int giFiliale = 0;

        SqlDataAdapter sdCmp;
        SqlDataAdapter sdContract;
        SqlDataAdapter sdMieter;
        SqlDataAdapter sdObj;
        SqlDataAdapter sdObjTeil;
        MySqlDataAdapter mysdCmp;
        MySqlDataAdapter mysdContract;
        MySqlDataAdapter mysdMieter;
        MySqlDataAdapter mysdObj;
        MySqlDataAdapter mysdObjTeil;

        DataTable tableCmp;
        DataTable tableContract;
        DataTable tableMieter;
        DataTable tableObj;
        DataTable tableObjTeil;

        // Hier Übergabe des Mainwindows für Übergabe des ConnectStrings
        public WndStammContract(MainWindow mainWindow)
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

        internal void getDb(int aiDb)
        {
            String lsSql = "";
            int liRows = 0;

            giDb = aiDb;
            // SqlSelect Firmen erstellen
            lsSql = getSql(1, 0);
            // Daten Firmen holen
            liRows = fetchData(lsSql, 1);

            // SqlSelect Mieter (gewählte Filiale) Combobox Verträge
            lsSql = getSql(52, 0);
            // Daten Mieter holen
            liRows = fetchData(lsSql, 5);

            // SqlSelect Verträge
            lsSql = getSql(2, 0);
            // Daten Verträge holen
            liRows = fetchData(lsSql, 2);

            // SqlSelect Objekte
            lsSql = getSql(3, 0);
            // Daten Objekte holen
            liRows = fetchData(lsSql, 3);

            // SqlSelect ObjektTeile
            lsSql = getSql(4, 0);
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
                case 2:         // Verträge
                    lsSql = @"Select Id_vertrag,id_objekt,id_objekt_teil,id_mieter,datum_von, datum_bis, vertrag_aktiv, anzahl_personen, bemerkung 
	                            from vertrag
                                Order by datum_von desc";
                    break;
                case 21:         // Verträge mit Objektteil
                    lsSql = @"Select id_vertrag,id_objekt,id_objekt_teil,id_mieter,datum_von, datum_bis, vertrag_aktiv, anzahl_personen, bemerkung 
	                            from vertrag
                                where id_objekt_teil = " + aiId.ToString() + " Order by datum_von desc ";
                    break;
                case 22:         // Verträge mit Vertrags ID
                    lsSql = @"Select id_vertrag,id_objekt,id_objekt_teil,id_mieter,datum_von, datum_bis, vertrag_aktiv, anzahl_personen, bemerkung 
	                            from vertrag
                                where id_vertrag = " + aiId.ToString() + " Order by datum_von desc ";
                    break;
                case 3:         // Objekte
                    lsSql = @"Select Id_objekt,bez as objbez ,nr_obj from objekt
                                where id_filiale = " + aiId.ToString() + " Order by bez";
                    break;
                case 4:         // ObjektTeile
                    lsSql = @"Select Id_objekt_teil,bez as objteilbez from objekt_teil 
                                where id_objekt = " + aiId.ToString() + " Order by bez";
                    break;
                case 5:         // Mieter mit Mieter Id
                    lsSql = @"Select Id_mieter,bez,nr from mieter
                                where id_mieter = " + aiId.ToString() + " Order by bez";
                    break;
                case 51:         // Mieter mit objekt id
                    lsSql = @"Select Id_mieter,bez,nr from mieter
                                where id_objekt = " + aiId.ToString() + " Order by bez";
                    break;
                case 52:         // Mieter (gewählte Filiale)
                    lsSql = @"Select id_mieter,bez,nr from mieter 
                                where id_filiale = " +aiId.ToString() + " Order by bez";
                    break;
                case 8:         // Vertrag löschen
                    lsSql = "Delete from vertrag Where id_vertrag = " + aiId.ToString();
                    break;
                case 9:
                    lsSql = @"Select id_mieter from zahlungen where id_mieter = " + aiId.ToString();
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
                        case 2: // Verträge
                            tableContract = new DataTable();
                            SqlCommand command2 = new SqlCommand(asSql, connect);
                            sdContract = new SqlDataAdapter(command2);
                            sdContract.Fill(tableContract);
                            dgrStContract.ItemsSource = tableContract.DefaultView;
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
                        case 5: // Combobox Mieter
                            tableMieter = new DataTable();
                            SqlCommand command5 = new SqlCommand(asSql, connect);
                            sdMieter = new SqlDataAdapter(command5);
                            sdMieter.Fill(tableMieter);
                            mieter.ItemsSource = tableMieter.DefaultView;
                            break;
                        case 6:     // Vertrag speichern
                            SqlCommandBuilder commandBuilder6 = new SqlCommandBuilder(sdContract);
                            sdContract.Update(tableContract);
                            break;
                        case 8:
                            SqlCommand command8 = new SqlCommand(asSql, connect);
                            SqlDataReader queryCommandReader = command8.ExecuteReader();
                            break;
                        case 9:
                            SqlCommand command9 = new SqlCommand(asSql, connect);
                            var lvId = command9.ExecuteScalar();
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
                        case 2: // Verträge
                            tableContract = new DataTable();
                            MySqlCommand command2 = new MySqlCommand(asSql, myConnect);
                            mysdContract = new MySqlDataAdapter(command2);
                            mysdContract.Fill(tableContract);
                            dgrStContract.ItemsSource = tableContract.DefaultView;
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
                        case 5: // Combobox Mieter
                            tableMieter = new DataTable();
                            MySqlCommand command5 = new MySqlCommand(asSql, myConnect);
                            mysdMieter = new MySqlDataAdapter(command5);
                            mysdMieter.Fill(tableMieter);
                            mieter.ItemsSource = tableMieter.DefaultView;
                            break;
                        case 6:     // Vertrag speichern
                            MySqlCommandBuilder commandBuilder6 = new MySqlCommandBuilder(mysdContract);
                            mysdContract.Update(tableContract);
                            break;
                        case 8:
                            MySqlCommand command8 = new MySqlCommand(asSql, myConnect);
                            MySqlDataReader queryCommandReader = command8.ExecuteReader();
                            break;
                        case 9:
                            MySqlCommand command9 = new MySqlCommand(asSql, myConnect);
                            var lvId = command9.ExecuteScalar();
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
                    break;
                default:
                    break;
            }
            return liRows;
        }

        //// Stammdaten Mieter wurde geändert
        //private void dgrStMieter_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        //{
        //    btnSave.IsEnabled = true;
        //}

        // Firma geändert
        private void dgrStCmp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int liId = 0;
            int liSel = dgrStCmp.SelectedIndex;
            int liRows = 0;
            int liFiliale = 0;
            string lsSql = "";

            if (liSel >= 0)
            {
                // datagrid Verträge leeren
                dgrStContract.ItemsSource = null;

                DataRowView rowview = dgrStCmp.SelectedItem as DataRowView;

                if ((rowview.Row[0] != DBNull.Value))
                {
                    liId = Int32.Parse(rowview.Row[0].ToString());
                    // ComboBoxMieter dazu holen
                    // SqlSelect Mieter (gewählte Filiale) Combobox Verträge
                    lsSql = getSql(52, liId);
                    // Daten Mieter holen
                    liRows = fetchData(lsSql, 5);

                    // Objekte dazu holen
                    // SqlSelect Objekte
                    lsSql = getSql(3, liId);
                    // Daten Firmen holen
                    liRows = fetchData(lsSql, 3);
                }
                btnDel.IsEnabled = false;
            }
        }

        // Objekt angwewählt : Teilobjekte dazu zeigen
        private void dgrStObj_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int liId = 0;
            int liSel = dgrStObj.SelectedIndex;
            int liRows = 0;
            string lsSql = "";

            if (liSel >= 0)
            {

                // datagrid Verträge leeren
                 dgrStContract.ItemsSource = null;

                DataRowView rowview = dgrStObj.SelectedItem as DataRowView;
                if ((rowview.Row[0] != DBNull.Value))
                {
                    liId = Int32.Parse(rowview.Row[0].ToString());
                    // SqlSelect erstellen
                    lsSql = getSql(4, liId);
                    // Daten holen
                    liRows = fetchData(lsSql, 4);
                }
                btnDel.IsEnabled = false;
            }
        }

        // ObjektTeil angewählt, Verträge dazu zeigen
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
                    lsSql = getSql( 21, liId);
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
            int liSel = dgrStContract.SelectedIndex;
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
                    DataRowView rowview = dgrStContract.SelectedItem as DataRowView;
                    if ((rowview.Row[0] != DBNull.Value))
                    {
                        liId = Int32.Parse(rowview.Row[0].ToString());

                        if (liId >= 0)
                        {
                            // Den Vertrag löschen
                            lsSql = getSql(8, liId);
                            liOk = fetchData(lsSql, 8);
                        }
                    }
                }
            }
            // Daten Vertrag neu holen
            if (liSel >= 0)
            {
                // Das gewählte Teilobjekt
                DataRowView rowview = dgrStContract.SelectedItem as DataRowView;

                if ((rowview.Row[0] != DBNull.Value))
                {
                    liId = Int32.Parse(rowview.Row[0].ToString());
                    // SqlSelect erstellen
                    lsSql = getSql(22, liId);
                    // Daten holen
                    liRows = fetchData(lsSql, 2);
                }
            }
            btnSave.Content = "Speichern";
            }

        // Dgr wurde bearbeitet
        private void dgrStContract_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            btnSave.IsEnabled = true;
        }

        // Löschen nur, wen keine Zahlung auf den Vertrag gebucht ist
        private void dgrStContract_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int liMieterId = 0;
            int liSel = dgrStContract.SelectedIndex;

            if (liSel >= 0)
            {
                DataRowView rowview = dgrStContract.SelectedItem as DataRowView;

                //  Mieter Id holen
                if ((rowview.Row[3] != DBNull.Value))
                {
                    liMieterId = Int32.Parse(rowview.Row[3].ToString());
                    // Prüfen 
                    if (getDelInfo(liMieterId) == 0)
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

        // Existiert eine Zahlung zu dem Vertrag mit der gewählten ID?
        private int getDelInfo(int aiId)
        {
            int liId = 0;
            String lsSql = "";

            lsSql = getSql(9, aiId);
            liId = fetchData(lsSql, 9);

            return liId;
        }

        // Contract zufügen
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            int liSel = dgrObjTeil.SelectedIndex;
            int liIdObj = 0;
            int liIdObjTeil = 0;

            // Buttons 
            btnAdd.IsEnabled = false;
            btnSave.IsEnabled = true;
            if (liSel >= 0)
            {
                // Gewähltes Objekt
                DataRowView rowviewObj = dgrStObj.SelectedItem as DataRowView;
                // Gewähltes Teilobjekt
                DataRowView rowviewOt = dgrObjTeil.SelectedItem as DataRowView;


                if ((rowviewOt.Row[0] != DBNull.Value) && (rowviewObj.Row[0] != DBNull.Value))
                {
                    liIdObjTeil = Int32.Parse(rowviewOt.Row[0].ToString());
                    liIdObj = Int32.Parse(rowviewObj.Row[0].ToString());

                    // Todo Die Combobox einschränken (nur Mieter des Teilobjektes)
                    //lsSql = getSql("mieter", 51, liIdObj);
                    // Daten Firmen holen
                    //liRows = FetchData(lsSql, 5);

                    DataRow dr = tableContract.NewRow();
                    // Vorgaben eintragen, hier Objekt ID
                    // Die Objekt ID wird hier nur für die Mietersuche verwendet,
                    // sonst ist sie irrelevant, da ja die Mieter über die Verträge mit
                    // den Objekten verbunden sind
                    dr[1] = liIdObj;
                    dr[2] = liIdObjTeil;
                    dr[4] = DateTime.Now;
                    dr[5] = DateTime.Now.AddYears(50);
                    // dr[2] = "NEUER VERTRAG";

                    tableContract.Rows.InsertAt(dr, 0);
                }
            }
        }

        // Vertrag löschen
        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            btnSave.IsEnabled = true;
            btnSave.Content = "Wirklich löschen?";
            btnDel.IsEnabled = false;
        }

        // Wieder alle Verträge zeigen 
        //private void dgrStContract_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    string lsSql = "";
        //    int liRows = 0;

        //    // SqlSelect Verträge
        //    lsSql = getSql("contract", 2, 0);
        //    // Daten Verträge holen
        //    liRows = FetchData(lsSql, 2);
        //}
    }
}
