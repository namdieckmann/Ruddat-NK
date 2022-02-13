using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using MySql.Data.MySqlClient;

namespace Ruddat_NK
{
    /// <summary>
    /// Interaktionslogik für WndZlgTrace.xaml
    /// </summary>
    public partial class WndZlgTrace : Window
    {
        private MainWindow mainWindow;
        private String gsConnect;
        private int giDb;

        DataTable tableZlg;

        SqlDataAdapter sdZlg;
        MySqlDataAdapter mysdZlg;

        // ConnectString übernehmen
        private string psConnect { get; set; }

        public WndZlgTrace(MainWindow mainWindow    )
        {
            this.mainWindow = mainWindow;
            InitializeComponent();

            // ConnectString global
            gsConnect = this.mainWindow.psConnect;
        }

        // Welche Datenbank
        internal void getDb(int aiDb)
        {
            string lsSql = "";
            int liRows = 0;

            giDb = aiDb;
            // SqlSelect Firmen erstellen
            lsSql = getSql("zlg", 1, 0);
            // Daten Firmen holen
            liRows = fetchData(lsSql, 1);
        }

        // Sql zusammenstellen
        private string getSql(string asSql, int aiArt, int p3)
        {
            string lsSql = "";

            switch (aiArt)
            {
                case 1:         // Nicht verbuchte Zahlungen
                    lsSql = @"select    datum_von as datum,
		                                betrag_netto as netto,
		                                betrag_brutto as brutto,
		                                objekt.bez as objekt,
		                                objekt_teil.bez as tobjekt,
		                                mieter.bez as mieter,
                                        zahlungen_trace.bez
                                from zahlungen_trace
	                                    left join objekt on objekt.Id_objekt = zahlungen_trace.id_objekt
	                                    left join objekt_teil on objekt_teil.id_objekt_teil = zahlungen_trace.id_objekt_teil
		                                left join mieter on mieter.Id_mieter = zahlungen_trace.id_mieter";
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

                    switch (aiArt)
                    {
                        case 1: // Firmen
                            tableZlg = new DataTable();
                            SqlCommand command = new SqlCommand(asSql, connect);
                            sdZlg = new SqlDataAdapter(command);
                            sdZlg.Fill(tableZlg);
                            dgrZlg.ItemsSource = tableZlg.DefaultView;
                            break;
                        default:
                            break;
                    }
                    break;
                case 2:
                    MySqlConnection myConnect;
                    myConnect = new MySqlConnection(gsConnect);

                    switch (aiArt)
                    {
                        case 1: // Firmen
                            tableZlg = new DataTable();
                            MySqlCommand command = new MySqlCommand(asSql, myConnect);
                            mysdZlg = new MySqlDataAdapter(command);
                            mysdZlg.Fill(tableZlg);
                            dgrZlg.ItemsSource = tableZlg.DefaultView;
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
    }
}
