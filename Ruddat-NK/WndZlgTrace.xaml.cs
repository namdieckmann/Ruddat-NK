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
    /// Interaktionslogik für WndZlgTrace.xaml
    /// </summary>
    public partial class WndZlgTrace : Window
    {
        private MainWindow mainWindow;
        public String gsConnect;

        DataTable tableZlg;
        SqlDataAdapter sdZlg;

        // ConnectString übernehmen
        public string psConnect { get; set; }

        public WndZlgTrace(MainWindow mainWindow    )
        {
            string lsSql = "";
            int liRows = 0;

            this.mainWindow = mainWindow;
            InitializeComponent();

            // ConnectString global
            gsConnect = this.mainWindow.psConnect;

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

            // Buttons
            // btnSave.IsEnabled = false;
            // btnAdd.IsEnabled = true;

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
            return liRows;
        }
    }
}
