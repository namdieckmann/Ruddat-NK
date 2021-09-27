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
    /// Interaktionslogik für WndStammObjekte.xaml
    /// </summary>
    public partial class WndStammObjTeile : Window
    {
        private MainWindow mainWindow;
        public String gsConnect;

        // ConnectString übernehmen
        public string psConnect { get; set; }

        DataTable tableCmp;
        SqlDataAdapter sdCmp;
        DataTable tableObj;
        SqlDataAdapter sdObj;
        DataTable tableTeil;
        SqlDataAdapter sdTeil;
        // DataTable tableAda;
        // SqlDataAdapter sdAda;

        // Hier Übergabe des Mainwindows für Übergabe des ConnectStrings
        public WndStammObjTeile(MainWindow mainWindow)
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
            this.btnAdd.IsEnabled = false;

            // SqlSelect Firmen erstellen
            lsSql = getSql("cmp", 1, 0);
            // Daten Firmen holen
            liRows = fetchData(lsSql, 1);

            // SqlSelect Objekte
            lsSql = getSql("obj", 2, 0);
            // Daten Firmen holen
            liRows = fetchData(lsSql, 2);

            // SqlSelect Adressen
            lsSql = getSql("teil", 3, 0);
            // Daten Firmen holen
            liRows = fetchData(lsSql, 3);

            //// SqlSelect AdressArten
            //lsSql = getSql("ada", 4, 0);
            //// Daten Firmen holen
            //liRows = FetchData(lsSql, 4);

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
                case 2:         // Objekte
                    lsSql = @"Select Id_objekt,bez,Id_Adresse,Id_filiale,nr_obj,kst,flaeche_gesamt from objekt
                                where id_filiale = " + aiId.ToString() + " Order by bez";
                    break;
                case 3:         // Teil
                    lsSql = @"Select Id_objekt_teil,id_objekt,Id_Adresse,bez,geschoss,
                                    lage,flaeche_anteil,prozent_anteil,personen_anteil_flag,nr_obj_teil,kst
                                from objekt_teil
                                where id_objekt = " + aiId.ToString() + " Order by geschoss,lage";
                    break;

                //case 4:         // Adressarten
                //    lsSql = @"Select id_art_adresse,bez from art_adresse Order by sort";
                //    break;

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
                case 2: // Objekte
                    tableObj = new DataTable();
                    SqlCommand command2 = new SqlCommand(asSql, connect);
                    sdObj = new SqlDataAdapter(command2);
                    sdObj.Fill(tableObj);
                    dgrStObj.ItemsSource = tableObj.DefaultView;

                    break;
                case 3: // Teile
                    tableTeil = new DataTable();
                    SqlCommand command3 = new SqlCommand(asSql, connect);
                    sdTeil = new SqlDataAdapter(command3);
                    sdTeil.Fill(tableTeil);
                    dgrObjTeil.ItemsSource = tableTeil.DefaultView;

                    break;

                //case 4: // Adressarten
                //    tableAda = new DataTable();
                //    SqlCommand command4 = new SqlCommand(asSql, connect);
                //    sdAda = new SqlDataAdapter(command4);
                //    sdAda.Fill(tableAda);
                //    adressenart.ItemsSource = tableAda.DefaultView;

                //    break;
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
                    lsSql2 = getSql("obj", 2, liId);
                    // Daten holen
                    liRows = fetchData(lsSql2, 2);
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

                    // Teile dazu holen
                    // SqlSelect erstellen
                    lsSql2 = getSql("teil", 3, liId);
                    // Daten holen
                    liRows = fetchData(lsSql2, 3);

                    btnAdd.IsEnabled = true;
                }
            }
        }

        // Objektteil Anwahl geändert
        private void dgrObjTeil_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int liId = 0;
            int liSel = dgrObjTeil.SelectedIndex;

            if (liSel >= 0)
            {
                DataRowView rowview = dgrObjTeil.SelectedItem as DataRowView;

                if ((rowview.Row[0] != DBNull.Value))
                {
                    liId = Int32.Parse(rowview.Row[0].ToString());

                    if (getDelInfo(liId) == 0)
                    {
                        // Es darf gelöscht werden
                        btnDel.IsEnabled = true;
                        btnSave.IsEnabled = true;
                    }
                    else
                    {
                        btnDel.IsEnabled = false;
                    }

                    btnAdd.IsEnabled = true;
                }
            }
        }

        // Objektteil editiert
        private void dgrObjTeil_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            btnDel.IsEnabled = true;
            btnSave.IsEnabled = true;
        }

        // Teil speichern, löschen
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            int liId = 0;
            int liSel = dgrStObj.SelectedIndex;
            int liRows = 0;
            string lsSql = "";

            btnSave.IsEnabled = false;
            btnAdd.IsEnabled = true;

            if (btnSave.Content.ToString() == "Speichern")
            {
                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(sdTeil);

                sdTeil.UpdateCommand = commandBuilder.GetUpdateCommand();
                sdTeil.InsertCommand = commandBuilder.GetInsertCommand();
            }
            else  // Löschen
            {
                if (liSel >= 0)
                {
                    DataRowView rowview = dgrObjTeil.SelectedItem as DataRowView;
                    if ((rowview.Row[0] != DBNull.Value))
                    {
                        liId = Int32.Parse(rowview.Row[0].ToString());

                        if (liId >= 0)
                        {
                            // Den Import aus wt_hours_add löschen
                            lsSql = "Delete from objekt_teil Where id_objekt_teil = " + liId.ToString();

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
                                MessageBox.Show("In Tabelle Objekte konnte nicht gelöscht werden\n" +
                                        "Prüfen Sie bitte die Datenbankverbindung\n",
                                        "Achtung WndStammObjekte.Obj.del",
                                            MessageBoxButton.OK);
                            }
                        }
                    }
                }
            }

            sdTeil.Update(tableTeil);

            // Neu holen
            DataRowView rowview2 = dgrStObj.SelectedItem as DataRowView;
            if (rowview2 != null)
            {
                if ((rowview2.Row[0] != DBNull.Value))
                {
                    liId = Int32.Parse(rowview2.Row[0].ToString());
                    // SqlSelect erstellen
                    lsSql = getSql("teil", 3, liId);
                    // Daten holen
                    liRows = fetchData(lsSql, 3);
                }
            }

            btnSave.Content = "Speichern";
            btnSave.IsEnabled = false;
            btnDel.IsEnabled = true;
            btnAdd.IsEnabled = true;
        }

        // ObjektTeil zufügen
        private void btnAdd_Click(object sender, RoutedEventArgs e)
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
                    liId = Int32.Parse(rowview.Row[0].ToString());
                    DataRow dr = tableTeil.NewRow();
                    // Vorgaben eintragen, hier Objekt ID 
                    dr[1] = liId;
                    dr[3] = "NEUE MIETFLÄCHE";
                    tableTeil.Rows.InsertAt(dr,0);
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


        // Existiert eine Rechnung oder Zahlung zu dem TeilObjekt mit der gewählten ID?
        // Dann nicht löschen
        private int getDelInfo(int aiId)
        {
            int liId = 0;
            String lsSql = "";

            lsSql = @"Select id_objekt_teil from vertrag where id_objekt_teil = " + aiId.ToString();

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
                        "Achtung (WndStammObjTeil.getdelInfo)",
                         MessageBoxButton.OK);
            }
            return liId;
        }
    }
}
