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
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.ReportSource;
using CrystalDecisions.Shared;


namespace Ruddat_NK
{
    /// <summary>
    /// Interaktionslogik für RepKosten.xaml
    /// </summary>
    public partial class RepKosten : Window
    {
        private MainWindow mainWindow;

        // ConnectString übernehmen
        public string psConnect { get; set; }

        public String gsConnect = "";        

        DataTable tableKosten;
        SqlDataAdapter sdKosten;

        public RepKosten(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            
            string lsSql = "";
            int liRows = 0;

            // ConnectString global
            gsConnect = this.mainWindow.psConnect;

            // SqlSelect erstellen
            lsSql = getSql("kos", 1);
            // Daten holen
            liRows = fetchData(lsSql, "kos");
        }

        // Sql zusammenstellen
        private string getSql(string asSql, int aiArt)
        {
            string lsSql = "";

            switch (aiArt)
            {
                case 1:         // Kosten
                    lsSql = @"Select                  
                            timeline.betrag_netto,
						    timeline.betrag_brutto,
                            timeline.dt_monat
                        from timeline";
                    break;
                case 2:         // Teilobjekte
                    lsSql = @"Select                  
                            art_kostenart.bez,
                            timeline.betrag_netto,
						    timeline.betrag_brutto,
                            timeline.dt_monat
                        from timeline
                        Right Join art_kostenart on timeline.id_ksa = art_kostenart.id_ksa";
                    break;
                case 3:         // Mieter
                    lsSql = "select bez,wtl_obj_teil,wtl_mieter,sort,id_ksa,ksa_objekt,ksa_obj_teil,ksa_mieter,ksa_zahlung from art_kostenart Where ksa_mieter = 1 Order by sort";
                    break;
                case 4:         // Zahlung
                    lsSql = "select bez,wtl_obj_teil,wtl_mieter,sort,id_ksa,ksa_objekt,ksa_obj_teil,ksa_mieter,ksa_zahlung from art_kostenart Where ksa_zahlung = 1 Order by sort";
                    break;
                default:
                    break;
            }


            return lsSql;
        }

        // Daten holen
        private int fetchData(string asSql, string p)
        {
            int liRows = 0;

            SqlConnection connect;
            connect = new SqlConnection(gsConnect);

            tableKosten = new DataTable();         // Kostenarten 
            SqlCommand command = new SqlCommand(asSql, connect);
            sdKosten = new SqlDataAdapter(command);

            sdKosten.Fill(tableKosten);

  
            //ReportDocument myRep;
            //myRep = new ReportDocument();
            //myRep.Load("C:\\Users\\Ulf\\Documents\\Visual Studio 2012\\Projects\\Ruddat-NK\\Ruddat-NK\\RpKosten.rpt");
            //// myRep.get
            //myRep.SetDataSource(sdKosten);

            //// repKosten.SetResourceReference = myRep;
            //rpKosten.ShowToolbar = true;

            //rpKosten.ViewerCore.ReportSource = myRep;

            return liRows;
        }

    }
}
