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
    /// Interaktionslogik für WndStammContract.xaml
    /// </summary>
    public partial class WndStammContract : Window
    {
        private MainWindow mainWindow;
        public String gsConnect;

        // ConnectString übernehmen
        public string psConnect { get; set; }
        public int giObjId = 0;

        DataTable tableCmp;
        SqlDataAdapter sdCmp;
        DataTable tableContract;
        SqlDataAdapter sdContract;
        DataTable tableMieter;
        SqlDataAdapter sdMieter;
        DataTable tableObj;
        SqlDataAdapter sdObj;
        DataTable tableObjTeil;
        SqlDataAdapter sdObjTeil;

        // Hier Übergabe des Mainwindows für Übergabe des ConnectStrings
        public WndStammContract(MainWindow mainWindow)
        {
            String lsSql = "";
            int liRows = 0;

            this.mainWindow = mainWindow;
            InitializeComponent();

            // ConnectString global
            gsConnect = this.mainWindow.psConnect;

            // save +  del Button abschalten
            this.btnAdd.IsEnabled = false;
            this.btnSave.IsEnabled = false;
            this.btnDel.IsEnabled = false;

            // SqlSelect Firmen erstellen
            lsSql = getSql("cmp", 1, 0);
            // Daten Firmen holen
            liRows = fetchData(lsSql, 1);

            // SqlSelect Mieter (alle) Combobox Vertrage
            lsSql = getSql("mieter", 52, 0);
            // Daten Firmen holen
            liRows = fetchData(lsSql, 5);

            // SqlSelect Verträge
            lsSql = getSql("contract", 2, 0);
            // Daten Verträge holen
            liRows = fetchData(lsSql, 2);

            // SqlSelect Objekte
            lsSql = getSql("obj", 3, 0);
            // Daten Objekte holen
            liRows = fetchData(lsSql, 3);

            // SqlSelect ObjektTeile
            lsSql = getSql("objteil", 4, 0);
            // Daten ObjektTeile holen
            liRows = fetchData(lsSql, 4);
        }

        // Sql zusammenstellen
        private string getSql(string asSql, int aiArt, int aiId)
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
                case 52:         // Mieter (alle)
                    lsSql = @"Select id_mieter,bez,nr from mieter Order by bez";
                                
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

            SqlConnection connect;
            connect = new SqlConnection(gsConnect);

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
                    // Objekte dazu holen

                    // SqlSelect Objekte
                    lsSql = getSql("obj", 3, liId);
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
                    lsSql = getSql("objteil", 4, liId);
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
                    lsSql = getSql("vertrag", 21, liId);
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
                int liSel = dgrStContract.SelectedIndex;
                int liRows = 0;
                string lsSql2 = "";

                btnSave.IsEnabled = false;
                btnAdd.IsEnabled = true;

                if (btnSave.Content.ToString() == "Speichern")
                {
                    SqlCommandBuilder commandBuilder = new SqlCommandBuilder(sdContract);

                    sdContract.UpdateCommand = commandBuilder.GetUpdateCommand();
                    sdContract.InsertCommand = commandBuilder.GetInsertCommand();
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
                                String lsSql = "Delete from vertrag Where id_vertrag = " + liId.ToString();

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
                                    MessageBox.Show("In Tabelle Vertrag konnte nicht gelöscht werden\n" +
                                            "Prüfen Sie bitte die Datenbankverbindung\n",
                                            "Achtung WndStammContract.Contract.del",
                                                MessageBoxButton.OK);
                                }
                            }
                        }
                    }
                }

                sdContract.Update(tableContract);

                // Daten Vertrag neu holen
                if (liSel >= 0)
                {
                    // Das gewählte Teilobjekt
                    DataRowView rowview = dgrStContract.SelectedItem as DataRowView;

                    if ((rowview.Row[0] != DBNull.Value))
                    {
                        liId = Int32.Parse(rowview.Row[0].ToString());

                        // SqlSelect erstellen
                        lsSql2 = getSql("contract", 22, liId);
                        // Daten holen
                        liRows = fetchData(lsSql2, 2);
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

            lsSql = @"Select id_mieter from zahlungen where id_mieter = " + aiId.ToString();

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
                MessageBox.Show("Es wurden keine Informationen für das Löschen eines Objekts gefunden\n" +
                        "Prüfen Sie bitte die Datenbankverbindung\n",
                        "Achtung (WndStammObj.getdelInfo)",
                         MessageBoxButton.OK);
            }
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

                    // Todo Ulf !!
                    //// Die Combobox einschränken (nur Mieter des Teilobjektes)
                    //lsSql = getSql("mieter", 51, liIdObj);
                    //// Daten Firmen holen
                    //liRows = fetchData(lsSql, 5);

                    DataRow dr = tableContract.NewRow();
                    // Vorgaben eintragen, hier Objekt ID
                    // Die Objekt ID wird hier nur für die Mietersuche verwendet,
                    // sonst ist sie irrelevant, da ja die Mieter über die Verträge mit
                    // den Objekten verbunden sind
                    dr[1] = liIdObj;
                    dr[2] = liIdObjTeil;
                    dr[4] = DateTime.Now;
                    dr[5] = DateTime.MaxValue;
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
        //    liRows = fetchData(lsSql, 2);
        //}
    }
}
