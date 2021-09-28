using System;
using System.Linq;
using System.Data;
using System.Windows;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Windows.Controls;

namespace Ruddat_NK
{
    public class RdFetchData
    {
        // Zugriff von Mainwindow
        private MainWindow mwnd = new MainWindow();

        public RdFetchData()
        {

        }

        // Daten aus der Db holen
        // public static Int32 FetchData(string psSql, int piArt, string asConnectString, int aiDb)
        public Int32 FetchData(string psSql, int piArt, string asConnectString, int aiDb)
        {
            Int32 liRows = 0;
            string lsObjektBez = "", lsObjektTeilBez = "";
            string lsObjektBezS = "";

            // Daten
            DataTable tableOne;
            DataTable tableTwo;
            DataTable tableThree;
            DataTable tableFour;
            DataTable tableFive;
            DataTable tableSix;
            DataTable tableSeven;
            DataTable tableZlg;
            DataTable tableVert;
            DataTable tableAbrInfo;
            DataTable tableLeerstand;
            DataTable tableZlWert;
            DataTable tableZlNummer;
            SqlDataAdapter sda;
            SqlDataAdapter sdb;
            SqlDataAdapter sdc;
            SqlDataAdapter sdd;
            SqlDataAdapter sde;
            SqlDataAdapter sdf;
            SqlDataAdapter sdg;
            SqlDataAdapter sdZlg;
            SqlDataAdapter sdVert;
            SqlDataAdapter sdAbrInfo;
            SqlDataAdapter sdLeerstand;
            SqlDataAdapter sdZlWert;
            SqlDataAdapter sdZlNummer;
            MySqlDataAdapter mysda;
            MySqlDataAdapter mysdb;
            MySqlDataAdapter mysdc;
            MySqlDataAdapter mysdd;
            MySqlDataAdapter mysde;
            MySqlDataAdapter mysdf;
            MySqlDataAdapter mysdg;
            MySqlDataAdapter mysdZlg;
            MySqlDataAdapter mysdVert;
            MySqlDataAdapter mysdAbrInfo;
            MySqlDataAdapter mysdLeerstand;
            MySqlDataAdapter mysdZlWert;
            MySqlDataAdapter mysdZlNummer;

            switch (aiDb)           // DbChanger
            {
                case 1:             // MsSql

                    try
                    {
                        SqlConnection connect;
                        connect = new SqlConnection(asConnectString);
                        // Pass both strings to a new SqlCommand object.
                        SqlCommand command = new SqlCommand(psSql, connect);
                        // Db open
                        connect.Open();

                        // Daten für Filiale holen
                        if (piArt == 1)
                        {
                            tableThree = new DataTable();   // Filialen
                            sdc = new SqlDataAdapter(command);
                            sdc.Fill(tableThree);
                            mwnd.lbFiliale.ItemsSource = tableThree.DefaultView;
                           
                        }

                        // Daten für Objekte und Teilobjekte holen ab ins Treeview
                        // Für aktive Verträge
                        if (piArt == 2)
                        {
                            tableFour = new DataTable();    // Objekte Teilobjekte
                            sdd = new SqlDataAdapter(command);
                            sdd.Fill(tableFour);

                            if (tableFour.Rows.Count > 0)
                            {
                                int i = 0;
                                mwnd.tvMain.Items.Clear();

                                //  Eine Schleife durch die Tabelle, um das Treview zu befüllen
                                foreach (DataRow dr in tableFour.Rows)
                                {
                                    lsObjektBez = tableFour.Rows[i].ItemArray.GetValue(4).ToString().Trim() + ":" + tableFour.Rows[i].ItemArray.GetValue(0).ToString().Trim();
                                    lsObjektTeilBez = tableFour.Rows[i].ItemArray.GetValue(1).ToString();

                                    TreeViewItem root = new TreeViewItem
                                    {
                                        Header = lsObjektBez
                                    };

                                    // Nur, wenn ein neues Objekt und Teilobjekt in der Liste steht
                                    if (lsObjektBez != lsObjektBezS)
                                    {
                                        mwnd.tvMain.Items.Add(root);
                                        lsObjektBezS = lsObjektBez;
                                    }

                                    mwnd.PopulateTree(i, root, tableFour);

                                    i++;
                                }
                            }
                            else
                            {
                                mwnd.tvMain.Items.Clear();
                            }
                        }

                        // Die Id aus Objekt holen
                        if (piArt == 3)
                        {
                            tableFour = new DataTable();    // Objekte Teilobjekte
                            sdd = new SqlDataAdapter(command);
                            sdd.Fill(tableFour);
                            if (tableFour.Rows.Count > 0)
                            {
                                liRows = Convert.ToInt16(tableFour.Rows[0].ItemArray.GetValue(5).ToString());
                            }
                        }

                        // Die Id aus Teilobjekt holen
                        if (piArt == 4)
                        {
                            tableFour = new DataTable();    // Objekte Teilobjekte
                            sdd = new SqlDataAdapter(command);
                            sdd.Fill(tableFour);
                            if (tableFour.Rows.Count > 0)
                            {
                                liRows = Convert.ToInt16(tableFour.Rows[0].ItemArray.GetValue(6).ToString());
                            }
                        }

                        // Die Id aus Mieter holen
                        if (piArt == 5)
                        {
                            tableFour = new DataTable();    // Objekte Teilobjekte
                            sdd = new SqlDataAdapter(command);
                            sdd.Fill(tableFour);
                            if (tableFour.Rows.Count > 0)
                            {
                                liRows = Convert.ToInt16(tableFour.Rows[0].ItemArray.GetValue(7).ToString());
                            }
                        }

                        // DataGrid Timline Summen
                        if (piArt == 8)
                        {
                            tableSeven = new DataTable();   // Timeline Summen 
                            sdg = new SqlDataAdapter(command);
                            sdg.Fill(tableSeven);
                            mwnd.DgrCost.ItemsSource = tableSeven.DefaultView;
                            liRows = mwnd.DgrCost.Items.Count;
                        }

                        // Datagrid für Rechnungen
                        if (piArt == 9)
                        {
                            tableOne = new DataTable();     // Rechnungen
                            sda = new SqlDataAdapter(command);
                            sda.Fill(tableOne);
                            mwnd.DgrRechnungen.ItemsSource = tableOne.DefaultView;
                            liRows = mwnd.DgrRechnungen.Items.Count;
                        }

                        // ListBox Filiale befüllen
                        if (piArt == 10)
                        {
                            tableThree = new DataTable();
                            sdc = new SqlDataAdapter(command);
                            sdc.Fill(tableThree);                                   // Todo tableThree? Was ist da los
                            mwnd.lbFiliale.ItemsSource = tableThree.DefaultView;
                        }

                        // Combobox Kostenart in Rechnungen
                        if (piArt == 11)
                        {
                            tableFive = new DataTable();    // Kostenart
                            sde = new SqlDataAdapter(command);
                            sde.Fill(tableFive);
                            mwnd.kostenart.ItemsSource = tableFive.DefaultView;
                        }

                        // Combobox mwst in Rechnungen
                        if (piArt == 12)
                        {
                            tableSix = new DataTable();     // mwst
                            sdf = new SqlDataAdapter(command);
                            sdf.Fill(tableSix);
                            mwnd.mwst.ItemsSource = tableSix.DefaultView;                // Rechnungen
                        }

                        // DataGrid Timline Detail
                        if (piArt == 13)
                        {
                            tableTwo = new DataTable();     // Timeline
                            sdb = new SqlDataAdapter(command);
                            sdb.Fill(tableTwo);
                            mwnd.DgrCostDetail.ItemsSource = tableTwo.DefaultView;
                            liRows = mwnd.DgrCostDetail.Items.Count;
                        }

                        // DataGrid Zahlungen
                        if (piArt == 14)
                        {
                            tableZlg = new DataTable();     // Zahlungen
                            sdZlg = new SqlDataAdapter(command);
                            sdZlg.Fill(tableZlg);
                            mwnd.DgrZahlungen.ItemsSource = tableZlg.DefaultView;
                            liRows = mwnd.DgrZahlungen.Items.Count;
                        }

                        // DataGrid Leerstand Detail
                        if (piArt == 19)
                        {
                            tableLeerstand = new DataTable();     // Timeline
                            sdLeerstand = new SqlDataAdapter(command);
                            sdLeerstand.Fill(tableLeerstand);
                            mwnd.DgrLeerDetail.ItemsSource = tableLeerstand.DefaultView;
                            liRows = mwnd.DgrLeerDetail.Items.Count;
                        }

                        // Combobox Kostenart in Zahlungen
                        if (piArt == 15)
                        {
                            tableFive = new DataTable();    // Kostenart
                            sde = new SqlDataAdapter(command);
                            sde.Fill(tableFive);
                            mwnd.kostenartZlg.ItemsSource = tableFive.DefaultView;
                        }

                        // Combobox Verteilung in Rechnungen und Zähler
                        if (piArt == 16)
                        {
                            tableVert = new DataTable();    // Verteilung Rechnungen
                            sdVert = new SqlDataAdapter(command);
                            sdVert.Fill(tableVert);
                            mwnd.kostenvert.ItemsSource = tableVert.DefaultView;
                            mwnd.kostenvertZl.ItemsSource = tableVert.DefaultView;
                        }


                        // Tabelle Infos für Abrechnung
                        if (piArt == 17)
                        {
                            tableAbrInfo = new DataTable();    // Abrechnung
                            sdAbrInfo = new SqlDataAdapter(command);
                            sdAbrInfo.Fill(tableAbrInfo);
                        }

                        // Tabelle Leerstände
                        if (piArt == 18)
                        {
                            tableLeerstand = new DataTable();    // Leerstand
                            sdLeerstand = new SqlDataAdapter(command);
                            sdLeerstand.Fill(tableLeerstand);
                            mwnd.DgrLeer.ItemsSource = tableLeerstand.DefaultView;
                            liRows = mwnd.DgrLeer.Items.Count;
                        }

                        // Tabelle Zählerwerte
                        if (piArt == 21)
                        {
                            tableZlWert = new DataTable();    // Zählerwert
                            sdZlWert = new SqlDataAdapter(command);
                            sdZlWert.Fill(tableZlWert);
                            mwnd.DgrCounters.ItemsSource = tableZlWert.DefaultView;
                            liRows = mwnd.DgrCounters.Items.Count;
                        }

                        // Combobox Zählernummern
                        if (piArt == 22)
                        {
                            tableZlNummer = new DataTable();    // Kostenart
                            sdZlNummer = new SqlDataAdapter(command);
                            sdZlNummer.Fill(tableZlNummer);
                            mwnd.zlNummer.ItemsSource = tableZlNummer.DefaultView;
                            mwnd.zleh.ItemsSource = tableZlNummer.DefaultView;
                            mwnd.zlmw.ItemsSource = tableZlNummer.DefaultView;
                        }

                        // db close
                        connect.Close();
                    }
                    catch (SqlException ex)
                    {
                        for (int i = 0; i < ex.Errors.Count; i++)
                        {
                            MessageBox.Show("Index #" + i + "\n" +
                                "Error: " + ex.Errors[i].ToString() + "\n", "Achtung");
                        }
                        Console.ReadLine();

                        // Die Anwendung anhalten 
                        MessageBox.Show("Verarbeitungsfehler ERROR fetchdata main 0001\n piArt = " + piArt.ToString(),
                                 "Achtung");

                        throw;
                    }

                    break;
                case 2:                                     // MySql

                    try
                    {
                        MySqlConnection con;
                        con = new MySqlConnection(asConnectString);

                        MySqlCommand com = new MySqlCommand(psSql, con);

                        // Daten für Filiale holen
                        if (piArt == 1)
                        {
                            tableThree = new DataTable();   // Filialen
                            mysdc = new MySqlDataAdapter(com);
                            mysdc.Fill(tableThree);
                            mwnd.lbFiliale.ItemsSource = tableThree.DefaultView;
                        }

                        // Daten für Objekte und Teilobjekte holen ab ins Treeview
                        // Für aktive Verträge
                        if (piArt == 2)
                        {
                            tableFour = new DataTable();    // Objekte Teilobjekte
                            mysdd = new MySqlDataAdapter(com);
                            mysdd.Fill(tableFour);

                            if (tableFour.Rows.Count > 0)
                            {
                                int i = 0;
                                mwnd.tvMain.Items.Clear();

                                //  Eine Schleife durch die Tabelle, um das Treview zu befüllen
                                foreach (DataRow dr in tableFour.Rows)
                                {
                                    lsObjektBez = tableFour.Rows[i].ItemArray.GetValue(4).ToString().Trim() + ":" + tableFour.Rows[i].ItemArray.GetValue(0).ToString().Trim();
                                    lsObjektTeilBez = tableFour.Rows[i].ItemArray.GetValue(1).ToString();

                                    TreeViewItem root = new TreeViewItem
                                    {
                                        Header = lsObjektBez
                                    };

                                    // Nur, wenn ein neues Objekt und Teilobjekt in der Liste steht
                                    if (lsObjektBez != lsObjektBezS)
                                    {
                                        mwnd.tvMain.Items.Add(root);
                                        lsObjektBezS = lsObjektBez;
                                    }

                                    mwnd.PopulateTree(i, root, tableFour);

                                    i++;
                                }
                            }
                            else
                            {
                                mwnd.tvMain.Items.Clear();
                            }
                        }

                        // Die Id aus Objekt holen
                        if (piArt == 3)
                        {
                            tableFour = new DataTable();    // Objekte Teilobjekte
                            mysdd = new MySqlDataAdapter(com);
                            mysdd.Fill(tableFour);
                            if (tableFour.Rows.Count > 0)
                            {
                                liRows = Convert.ToInt16(tableFour.Rows[0].ItemArray.GetValue(5).ToString());
                            }
                        }

                        // Die Id aus Teilobjekt holen
                        if (piArt == 4)
                        {
                            tableFour = new DataTable();    // Objekte Teilobjekte
                            mysdd = new MySqlDataAdapter(com);
                            mysdd.Fill(tableFour);
                            if (tableFour.Rows.Count > 0)
                            {
                                liRows = Convert.ToInt16(tableFour.Rows[0].ItemArray.GetValue(6).ToString());
                            }
                        }

                        // Die Id aus Mieter holen
                        if (piArt == 5)
                        {
                            tableFour = new DataTable();    // Objekte Teilobjekte
                            mysdd = new MySqlDataAdapter(com);
                            mysdd.Fill(tableFour);
                            if (tableFour.Rows.Count > 0)
                            {
                                liRows = Convert.ToInt16(tableFour.Rows[0].ItemArray.GetValue(7).ToString());
                            }
                        }

                        // DataGrid Timline Summen
                        if (piArt == 8)
                        {
                            tableSeven = new DataTable();   // Timeline Summen 
                            mysdg = new MySqlDataAdapter(com);
                            mysdg.Fill(tableSeven);
                            mwnd.DgrCost.ItemsSource = tableSeven.DefaultView;
                            liRows = mwnd.DgrCost.Items.Count;
                        }

                        // Datagrid für Rechnungen
                        if (piArt == 9)
                        {
                            tableOne = new DataTable();     // Rechnungen
                            mysda = new MySqlDataAdapter(com);
                            mysda.Fill(tableOne);
                            mwnd.DgrRechnungen.ItemsSource = tableOne.DefaultView;
                            liRows = mwnd.DgrRechnungen.Items.Count;
                        }

                        // ListBox Filiale befüllen
                        if (piArt == 10)
                        {
                            tableThree = new DataTable();
                            mysdc = new MySqlDataAdapter(com);
                            mysdc.Fill(tableThree);
                            mwnd.lbFiliale.ItemsSource = tableThree.DefaultView;
                        }

                        // Combobox Kostenart in Rechnungen
                        if (piArt == 11)
                        {
                            tableFive = new DataTable();    // Kostenart
                            mysde = new MySqlDataAdapter(com);
                            mysde.Fill(tableFive);
                            mwnd.kostenart.ItemsSource = tableFive.DefaultView;
                        }

                        // Combobox mwst in Rechnungen
                        if (piArt == 12)
                        {
                            tableSix = new DataTable();     // mwst
                            mysdf = new MySqlDataAdapter(com);
                            mysdf.Fill(tableSix);
                            mwnd.mwst.ItemsSource = tableSix.DefaultView;                // Rechnungen
                        }

                        // DataGrid Timline Detail
                        if (piArt == 13)
                        {
                            tableTwo = new DataTable();     // Timeline
                            mysdb = new MySqlDataAdapter(com);
                            mysdb.Fill(tableTwo);
                            mwnd.DgrCostDetail.ItemsSource = tableTwo.DefaultView;
                            liRows = mwnd.DgrCostDetail.Items.Count;
                        }

                        // DataGrid Zahlungen
                        if (piArt == 14)
                        {
                            tableZlg = new DataTable();     // Zahlungen
                            mysdZlg = new MySqlDataAdapter(com);
                            mysdZlg.Fill(tableZlg);
                            mwnd.DgrZahlungen.ItemsSource = tableZlg.DefaultView;
                            liRows = mwnd.DgrZahlungen.Items.Count;
                        }

                        // DataGrid Leerstand Detail
                        if (piArt == 19)
                        {
                            tableLeerstand = new DataTable();     // Timeline
                            mysdLeerstand = new MySqlDataAdapter(com);
                            mysdLeerstand.Fill(tableLeerstand);
                            mwnd.DgrLeerDetail.ItemsSource = tableLeerstand.DefaultView;
                            liRows = mwnd.DgrLeerDetail.Items.Count;
                        }

                        // Combobox Kostenart in Zahlungen
                        if (piArt == 15)
                        {
                            tableFive = new DataTable();    // Kostenart
                            mysde = new MySqlDataAdapter(com);
                            mysde.Fill(tableFive);
                            mwnd.kostenartZlg.ItemsSource = tableFive.DefaultView;
                        }

                        // Combobox Verteilung in Rechnungen und Zähler
                        if (piArt == 16)
                        {
                            tableVert = new DataTable();    // Verteilung Rechnungen
                            mysdVert = new MySqlDataAdapter(com);
                            mysdVert.Fill(tableVert);
                            mwnd.kostenvert.ItemsSource = tableVert.DefaultView;
                            mwnd.kostenvertZl.ItemsSource = tableVert.DefaultView;
                        }


                        // Tabelle Infos für Abrechnung
                        if (piArt == 17)
                        {
                            tableAbrInfo = new DataTable();    // Abrechnung
                            mysdAbrInfo = new MySqlDataAdapter(com);
                            mysdAbrInfo.Fill(tableAbrInfo);
                        }

                        // Tabelle Leerstände
                        if (piArt == 18)
                        {
                            tableLeerstand = new DataTable();    // Leerstand
                            mysdLeerstand = new MySqlDataAdapter(com);
                            mysdLeerstand.Fill(tableLeerstand);
                            mwnd.DgrLeer.ItemsSource = tableLeerstand.DefaultView;
                            liRows = mwnd.DgrLeer.Items.Count;
                        }

                        // Tabelle Zählerwerte
                        if (piArt == 21)
                        {
                            tableZlWert = new DataTable();    // Zählerwert
                            mysdZlWert = new MySqlDataAdapter(com);
                            mysdZlWert.Fill(tableZlWert);
                            mwnd.DgrCounters.ItemsSource = tableZlWert.DefaultView;
                            liRows = mwnd.DgrCounters.Items.Count;
                        }

                        // Combobox Zählernummern
                        if (piArt == 22)
                        {
                            tableZlNummer = new DataTable();    // Kostenart
                            mysdZlNummer = new MySqlDataAdapter(com);
                            mysdZlNummer.Fill(tableZlNummer);
                            mwnd.zlNummer.ItemsSource = tableZlNummer.DefaultView;
                            mwnd.zleh.ItemsSource = tableZlNummer.DefaultView;
                            mwnd.zlmw.ItemsSource = tableZlNummer.DefaultView;
                        }

                        // db close
                        con.Close();
                    }
                    catch (MySqlException myex)
                    {
                        // Die Anwendung anhalten 
                        MessageBox.Show("Verarbeitungsfehler ERROR fetchdata main 0001\n piArt = " + piArt.ToString(),
                                 "Achtung");
                        throw;
                    }
                    break;
                default:
                    break;
            }

            return (liRows);     // oder Ausnahmsweise die gefundene ID bei art 3-5
        }
    }
}
