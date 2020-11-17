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
    public partial class WndStammObjekte : Window
    {
        private MainWindow mainWindow;
        public String gsConnect;

        // ConnectString übernehmen
        public string psConnect { get; set; }

        DataTable tableCmp;
        SqlDataAdapter sdCmp;
        DataTable tableObj;
        SqlDataAdapter sdObj;
        DataTable tableAdr;
        SqlDataAdapter sdAdr;
        DataTable tableAda;
        SqlDataAdapter sdAda;

        // Hier Übergabe des Mainwindows für Übergabe des ConnectStrings
        public WndStammObjekte(MainWindow mainWindow)
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
            this.btnAdrSave.IsEnabled = false;
            this.btnAdrDel.IsEnabled = false;
            this.btnAdrAdd.IsEnabled = false;

            // SqlSelect Firmen erstellen
            lsSql = getSql("cmp", 1, 0);
            // Daten Firmen holen
             liRows = fetchData(lsSql, 1);

            // SqlSelect Objekte
            lsSql = getSql("obj", 2, 0);
            // Daten Firmen holen
            liRows = fetchData(lsSql, 2);

            // SqlSelect Adressen
            lsSql = getSql("adr", 3, 0);
            // Daten Firmen holen
            liRows = fetchData(lsSql, 3);

            // SqlSelect AdressArten
            lsSql = getSql("ada", 4, 0);
            // Daten Firmen holen
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
                    // SqlSelect erstellen
                    lsSql2 = getSql("adr", 3, liId);
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
            int liSel = dgrStObj.SelectedIndex;
            int liRows = 0;
            string lsSql2 = "";


            btnSave.IsEnabled = false;
            btnAdd.IsEnabled = true;

            if (btnSave.Content.ToString() == "Speichern")
            {
                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(sdObj);

                sdObj.UpdateCommand = commandBuilder.GetUpdateCommand();
                sdObj.InsertCommand = commandBuilder.GetInsertCommand();
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
                            // Den Import aus wt_hours_add löschen
                            String lsSql = "Delete from objekt Where id_objekt = " + liId.ToString();

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

            sdObj.Update(tableObj);

            // Daten neu holen
            // Daten Objekte neu holen
            if (liSel >= 0)
            {
                DataRowView rowview = dgrStCmp.SelectedItem as DataRowView;

                if ((rowview.Row[0] != DBNull.Value))
                {
                    liId = Int32.Parse(rowview.Row[0].ToString());

                    // SqlSelect erstellen
                    lsSql2 = getSql("obj", 2, liId);
                    // Daten holen
                    liRows = fetchData(lsSql2,2);
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

            lsSql = @"Select id_objekt from objekt_teil where id_objekt = " + aiId.ToString();

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

        // Button Adresse Speichern
        private void btnAdrSave_Click(object sender, RoutedEventArgs e)
        {
            int liId = 0;
            int liSelObj = dgrStObj.SelectedIndex;
            int liSelAdr = dgrAdr.SelectedIndex;

            btnAdrSave.IsEnabled = false;
            btnAdrAdd.IsEnabled = true;

            if (btnAdrSave.Content.ToString() == "Speichern")
            {
                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(sdAdr);

                sdAdr.UpdateCommand = commandBuilder.GetUpdateCommand();
                sdAdr.InsertCommand = commandBuilder.GetInsertCommand();

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
                            String lsSql2 = "Delete from adressen Where id_adressen = " + liId.ToString();

                            SqlConnection connect;
                            connect = new SqlConnection(gsConnect);
                            SqlCommand command2 = new SqlCommand(lsSql2, connect);

                            try
                            {
                                // Db open
                                connect.Open();
                                SqlDataReader queryCommandReader = command2.ExecuteReader();
                                connect.Close();
                            }
                            catch
                            {
                                MessageBox.Show("In Tabelle Adressen konnte nicht gelöscht werden\n" +
                                        "Prüfen Sie bitte die Datenbankverbindung\n",
                                        "Achtung WndSammObj.Adr.delete",
                                            MessageBoxButton.OK);
                            }
                        }
                    }
                }
            }
            sdAdr.Update(tableAdr);

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
