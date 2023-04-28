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
        SqlDataAdapter sdContractNew;
        SqlDataAdapter sdMieter;
        SqlDataAdapter sdMieterNew;
        SqlDataAdapter sdObj;
        SqlDataAdapter sdObjNew;
        SqlDataAdapter sdObjTeil;

        MySqlDataAdapter mysdCmp;
        MySqlDataAdapter mysdContract;
        MySqlDataAdapter mysdOldContract;
        MySqlDataAdapter mysdContractNew;
        MySqlDataAdapter mysdMieter;
        MySqlDataAdapter mysdMieterNew;
        MySqlDataAdapter mysdObj;
        MySqlDataAdapter mysdObjNew;
        MySqlDataAdapter mysdObjTeil;

        DataTable tableCmp;
        DataTable tableContract;
        DataTable tableContractNew;
        DataTable tableOldContract;    // Für das automatische Anlegen neuer Verträge
        DataTable tableObjNew;          // Daten der neuen Objekte besorgen obj_teil_id und Obj_id
        DataTable tableMieter;
        DataTable tableMieterNew;
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
            lsSql = getSql(1, 0, "");
            // Daten Firmen holen
            liRows = fetchData(lsSql, 1);

            // SqlSelect Mieter (gewählte Filiale) Combobox Verträge
            lsSql = getSql(52, 0, "");
            // Daten Mieter holen
            liRows = fetchData(lsSql, 5);

            // SqlSelect Verträge
            lsSql = getSql(2, 0, "");
            // Daten Verträge holen
            liRows = fetchData(lsSql, 2);

            // SqlSelect Objekte
            lsSql = getSql(3, 0, "");
            // Daten Objekte holen
            liRows = fetchData(lsSql, 3);

            // SqlSelect ObjektTeile
            lsSql = getSql(4, 0, "");
            // Daten ObjektTeile holen
            liRows = fetchData(lsSql, 4);
        }

        // Sql zusammenstellen
        private string getSql( int aiArt, int aiId, string asValue)
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
                case 23:         // Verträge mit Objekt
                    lsSql = @"Select id_vertrag,id_objekt,id_objekt_teil,id_mieter,datum_von, datum_bis, vertrag_aktiv, anzahl_personen, bemerkung 
	                            from vertrag
                                where id_objekt = " + aiId.ToString() + " Order by datum_von desc ";
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
                    lsSql = @"Select Id_mieter,bez,nr,netto,id_filiale,leerstand,id_mandant,old_id,id_objekt from mieter
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
                case 53:         // Mieter (Alte Id holen)
                    lsSql = @"Select id_mieter,bez,nr from mieter 
                                where old_id = " + aiId.ToString() + " Order by bez";
                    break;
                case 8:         // Vertrag löschen
                    lsSql = "Delete from vertrag Where id_vertrag = " + aiId.ToString();
                    break;
                case 9:
                    lsSql = @"Select id_mieter from zahlungen where id_mieter = " + aiId.ToString();
                    break;
                case 10:        // Verträge auf neue obj_tei_id und neues objekt übertragen. Hier kommt das alte Objekt rein "20"
                                // und da gehts mit ner Schleife durch
                    lsSql = @"Select vertrag.Id_vertrag
                                    ,vertrag.id_objekt
                                    ,vertrag.id_objekt_teil
                                    ,vertrag.id_mieter
                                    ,vertrag.datum_von
                                    ,vertrag.datum_bis
								    ,vertrag.vertrag_aktiv
                                    ,vertrag.anzahl_personen
                                    ,vertrag.bemerkung 
								    ,objekt_teil.bez
	                            from vertrag
                                Left Join objekt_teil On objekt_teil.Id_objekt_teil = vertrag.Id_objekt_teil
                                Where vertrag.id_objekt = " + aiId.ToString();
                    // + " AND bez like '" + asValue.ToString() + "' ";
                    break;
                case 11:        // Hier bekommt man die neue teil_objekt_id und die neue objekt Id 21
                                // Schreiben der neuen Datensätze auf Table Verträge Case 2:
                    lsSql = @"SELECT Id_objekt_teil
                                    ,id_objekt
                                    ,bez 
                                FROM objekt_teil 
                                Where id_objekt = " + aiId.ToString() + " AND bez like '" + asValue.ToString() + "' ";
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
                        case 21: // Verträge kopieren
                            tableContractNew = new DataTable();
                            MySqlCommand command21 = new MySqlCommand(asSql, myConnect);
                            mysdContractNew = new MySqlDataAdapter(command21);
                            mysdContractNew.Fill(tableContractNew);
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
                            liRows = tableMieter.Rows.Count;
                            break;
                        case 6:     // Vertrag speichern
                            MySqlCommandBuilder commandBuilder6 = new MySqlCommandBuilder(mysdContract);
                            mysdContract.Update(tableContract);
                            break;
                        case 7:     // Vertrag speichern ContractNew
                            MySqlCommandBuilder commandBuilder7 = new MySqlCommandBuilder(mysdContractNew);
                            mysdContractNew.Update(tableContractNew);
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
                        case 10:        // Die Infos aus den alten Verträgen anhand der alten Id = 20
                            tableOldContract = new DataTable();
                            MySqlCommand command10 = new MySqlCommand(asSql, myConnect);
                            mysdOldContract = new MySqlDataAdapter(command10);
                            mysdOldContract.Fill(tableOldContract);
                            liRows = tableOldContract.Rows.Count;
                            // dgrStOldContract.ItemsSource = tableOldContract.DefaultView;
                            break;
                        case 11:        // Die neuen Ids anhand der Raumbezeichnung neue Id = 21
                            tableObjNew = new DataTable();
                            MySqlCommand command11 = new MySqlCommand(asSql, myConnect);
                            mysdObjNew = new MySqlDataAdapter(command11);
                            mysdObjNew.Fill(tableObjNew);
                            liRows = tableObjNew.Rows.Count;
                            break;
                        case 12:     // Mieter neu anlegen beim Kopieren
                            tableMieterNew = new DataTable();
                            MySqlCommand command12 = new MySqlCommand(asSql, myConnect);
                            mysdMieterNew = new MySqlDataAdapter(command12);
                            mysdMieterNew.Fill(tableMieterNew);
                            break;
                        case 13:     // Mieter neu  speichern
                            MySqlCommandBuilder commandBuilder13 = new MySqlCommandBuilder(mysdMieterNew);
                            mysdMieterNew.Update(tableMieterNew);
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
                    lsSql = getSql(52, liId, "");
                    // Daten Mieter holen
                    liRows = fetchData(lsSql, 5);

                    // Objekte dazu holen
                    // SqlSelect Objekte
                    lsSql = getSql(3, liId, "");
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
                    lsSql = getSql(4, liId, "");
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
                    lsSql = getSql( 21, liId, "");
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
                            lsSql = getSql(8, liId, "");
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
                    lsSql = getSql(22, liId, "");
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

            lsSql = getSql(9, aiId, "");
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
                    dgrStContract.SelectedIndex = 0;
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

        // Verträge auf neue ObjektTeile kopieren
        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            int liIdObj = 0;
            int LiRows = 0;
            int LiRows2 = 0;
            int LiRows3 = 0;
            string lsSql = "";

            int LiObjektId = 0;
            int LiObjektTeilId = 0;
            int LiIdMieter = 0;
            DateTime LdtVon = DateTime.MinValue;
            DateTime LdtBis = DateTime.MinValue;
            int LiAktiv = 0;
            int LiAnzPersonen = 0;
            string LsBemerkung = "";
            string LsBez = "";
            int LiObjektIdNew = 0;
            int LiObjektTeilIdNew = 0;

            // Mieter
            int LiMieterId = 0;
            string LsBezMieter = "";
            string LsNrMieter = "";
            int LiNetto = 0;
            int LiFiliale = 0;
            int LiLeerStand = 0;
            int LiMandant = 0;
            int LiOldId = 0;
            int LiObjektIdMieter = 0;


            MessageBoxResult result = MessageBox.Show("Die Verträge werden auf die Neue Version kopiert?", "Bestätigung", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Die aktuell gewählte Objekt Id ist=

                if (tableContract.Rows.Count > 0)
                {
                    liIdObj = Convert.ToInt16(tableContract.Rows[0].ItemArray.GetValue(1).ToString());
                    lsSql = getSql(10, liIdObj, "");
                    LiRows = fetchData(lsSql, 10);       //
                    lsSql = getSql(23, liIdObj, "");
                    LiRows2 = fetchData(lsSql, 21);      // Verträge neu

                    // Schleife durch die alten Verträge
                    for (int i = 0; i < LiRows; i++)
                    {
                        LiObjektId = 0;
                        LiObjektTeilId = 0;
                        LiIdMieter = 0;
                        LdtVon = DateTime.MinValue;
                        LdtBis = DateTime.MinValue;
                        LiAktiv = 0;
                        LiAnzPersonen = 0;
                        LsBemerkung = "";
                        LsBez = "";
                        LiObjektIdNew = 0;
                        LiObjektTeilIdNew = 0;

                        LiObjektId = Convert.ToInt16(tableOldContract.Rows[i].ItemArray.GetValue(1).ToString());
                        LiObjektTeilId = Convert.ToInt16(tableOldContract.Rows[i].ItemArray.GetValue(2).ToString());
                        LiIdMieter = Convert.ToInt16(tableOldContract.Rows[i].ItemArray.GetValue(3).ToString());
                        LdtVon = Convert.ToDateTime(tableOldContract.Rows[i].ItemArray.GetValue(4).ToString());
                        LdtBis = Convert.ToDateTime(tableOldContract.Rows[i].ItemArray.GetValue(5).ToString());
                        if (tableOldContract.Rows[i].ItemArray.GetValue(6) != DBNull.Value)
                        {
                            LiAktiv = Convert.ToInt16(tableOldContract.Rows[i].ItemArray.GetValue(6).ToString());
                        }
                        if (tableOldContract.Rows[i].ItemArray.GetValue(7) != DBNull.Value)
                        {
                            LiAnzPersonen = Convert.ToInt16(tableOldContract.Rows[i].ItemArray.GetValue(7).ToString());
                        }
                        if (tableOldContract.Rows[i].ItemArray.GetValue(8) != DBNull.Value)
                        {
                            LsBemerkung = tableOldContract.Rows[i].ItemArray.GetValue(8).ToString();
                        }
                        LsBez = tableOldContract.Rows[i].ItemArray.GetValue(9).ToString();

                        //-------------------------------------------------------------------------------------------------
                        // Die Objekt ID soll werden
                        int LiObjektIdTmp = 21;
                        // Das soll die neue Filiale sein
                        int LiFilialeTmp = 10;
                        //------------------------------------------------------------------------------------------------

                        // den Mieter mit der neuen Objekt Id anlegen
                        lsSql = lsSql = getSql(5, LiIdMieter, "");
                        int LiRowsTmp = fetchData(lsSql, 5);       //       Mieterdaten holen

                        if (LiRowsTmp > 0)
                        {
                            LiMieterId = 0;
                            LsBezMieter = "";
                            LsNrMieter = "";
                            LiNetto = 0;
                            LiFiliale = 0;
                            LiLeerStand = 0;
                            LiMandant = 0;
                            LiOldId = 0;

                            // Id_mieter,bez,nr,netto,id_filiale,leerstand,id_mandant,old_id,id_objekt
                            LiMieterId = Convert.ToInt16(tableMieter.Rows[0].ItemArray.GetValue(0).ToString());
                            LsBezMieter = tableMieter.Rows[0].ItemArray.GetValue(1).ToString();
                            LsNrMieter = tableMieter.Rows[0].ItemArray.GetValue(2).ToString();
                            if (tableMieter.Rows[0].ItemArray.GetValue(3) != DBNull.Value)
                            {
                                LiNetto = Convert.ToInt16(tableMieter.Rows[0].ItemArray.GetValue(3).ToString());
                            }

                            LiFiliale = Convert.ToInt16(tableMieter.Rows[0].ItemArray.GetValue(4).ToString());

                            if (tableMieter.Rows[0].ItemArray.GetValue(5) != DBNull.Value)
                            {
                                LiLeerStand = Convert.ToInt16(tableMieter.Rows[0].ItemArray.GetValue(5).ToString());
                            }
                            if (tableMieter.Rows[0].ItemArray.GetValue(6) != DBNull.Value)
                            {
                                LiMandant = Convert.ToInt16(tableMieter.Rows[0].ItemArray.GetValue(6).ToString());
                            }
                            if (tableMieter.Rows[0].ItemArray.GetValue(7) != DBNull.Value)
                            {
                                LiOldId = Convert.ToInt16(tableMieter.Rows[0].ItemArray.GetValue(7).ToString());
                            }
                            LiObjektIdMieter = Convert.ToInt16(tableMieter.Rows[0].ItemArray.GetValue(8).ToString());

                            int LiRowsTmp2 = fetchData(lsSql, 12);     //       Neue Mieterdatren schreiben

                            DataRow drMieter = tableMieterNew.NewRow();
                            drMieter[1] = LsBezMieter;
                            drMieter[2] = LsNrMieter;
                            drMieter[3] = LiNetto;
                            drMieter[4] = LiFilialeTmp;
                            drMieter[5] = LiLeerStand;
                            drMieter[6] = LiMandant;
                            drMieter[7] = LiMieterId;  // Alte Mieter Id in das Feld old_id einsetzen
                            drMieter[8] = LiObjektIdTmp;               // hier soll die 21 rein

                            tableMieterNew.Rows.InsertAt(drMieter, 0);
                            LiRowsTmp = fetchData("", 13);                      // Neuen Mieter speichern

                            // Die neue Mieter Id holen, indem der Datensatz über die Old Id geholt wird
                            lsSql = getSql(53, LiMieterId, "");
                            LiRowsTmp = fetchData(lsSql, 5);       //       Mieterdaten holen

                            if (LiRowsTmp > 0)
                            {
                                LiIdMieter = Convert.ToInt16(tableMieter.Rows[0].ItemArray.GetValue(0).ToString());
                            }
                        }

                        // Die neue Objekt und ObjektTeil Id ermitteln anhand der Bezeichnung
                        lsSql = getSql(11, LiObjektIdTmp, LsBez);
                        LiRows3 = fetchData(lsSql, 11);

                        if (LiRows3 > 0)
                        {
                            LiObjektTeilIdNew = Convert.ToInt16(tableObjNew.Rows[0].ItemArray.GetValue(0).ToString());
                            LiObjektIdNew = Convert.ToInt16(tableObjNew.Rows[0].ItemArray.GetValue(1).ToString()); ;

                            DataRow dr = tableContractNew.NewRow();
                            // Vorgaben eintragen, hier Objekt ID
                            // Die Objekt ID wird hier nur für die Mietersuche verwendet,
                            // sonst ist sie irrelevant, da ja die Mieter über die Verträge mit
                            // den Objekten verbunden sind
                            dr[1] = LiObjektIdNew;
                            dr[2] = LiObjektTeilIdNew;
                            dr[3] = LiIdMieter;
                            dr[4] = LdtVon;
                            dr[5] = LdtBis;
                            dr[6] = LiAktiv;
                            dr[7] = LiAnzPersonen;
                            dr[8] = LsBemerkung;

                            tableContractNew.Rows.InsertAt(dr, 0);
                        }
                    }
                    LiRows = fetchData("", 7);
                }
            }
            else
            {
                // Der Benutzer hat Nein ausgewählt oder die MessageBox geschlossen - führen Sie den entsprechenden Code aus
            }

        }

        // Wieder alle Verträge zeigen 
        private void DgrStContract_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //string lsSql = "";
            //int liRows = 0;
            //int liIdObj = 0;

            //if (tableContract.Rows.Count > 0)
            //{
            //    liIdObj = Convert.ToInt16(tableContract.Rows[0].ItemArray.GetValue(1).ToString());

            //    // SqlSelect gewählte Verträge
            //    lsSql = getSql(23, liIdObj, "");
            //    // Daten Verträge holen
            //    liRows = fetchData(lsSql, 2);

            //    btnCopy.IsEnabled = true;
            //}
            //else
            //{
            //    MessageBoxResult result = MessageBox.Show("Es sind keine Verträge sichtbar?", "Info", MessageBoxButton.OK, MessageBoxImage.Information);


            //}
        }
    }
}
