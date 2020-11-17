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
    /// Interaktionslogik für WndChooseSet.xaml
    /// </summary>
    public partial class WndChooseSet : Window
    {

        private MainWindow mainWindow;
        public String gsConnect;
        public int giTimeLineId = 0;
        public int giObjektId = 0;
        public int giArt = 0;
        public int giFlBehalten = 0;

        // ConnectString übernehmen
        public string psConnect { get; set; }

        DataTable tableParts;
        SqlDataAdapter sdParts;

        public WndChooseSet(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();

            // ConnectString global
            gsConnect = this.mainWindow.psConnect;
        }

        // Sql zusammenstellen
        private string getSql(string asSql, int aiArt, int aiId)
        {
            string lsSql = "";
            string lsWhereAdd = "";
            string lsWhereAdd2 = "";

            switch (aiArt)          
            {
                case 1:         // objekt Mix parts neu anlegen
                    lsSql = @"select Id_obj_mix_parts,id_objekt_mix,id_objekt,id_objekt_teil,bez,sel,flaeche_anteil,
                                id_timeline,ges_fl_behalten,erklaerung,geschoss,lage  
                                    from objekt_mix_parts";
                    lsWhereAdd = " where id_timeline is null "; // + giTimeLineId.ToString() + " ";
                    lsWhereAdd2 = " and id_objekt = " + giObjektId.ToString() + " ";
                    lsSql = lsSql + lsWhereAdd + lsWhereAdd2;
                    break;
                case 2:         // objekt Mix parts editieren
                    lsSql = @"select Id_obj_mix_parts,id_objekt_mix,id_objekt,id_objekt_teil,bez,sel,flaeche_anteil,    
                                id_timeline,ges_fl_behalten,erklaerung,geschoss,lage
                                    from objekt_mix_parts";
                    lsWhereAdd = " where id_timeline = " + giTimeLineId.ToString() + " ";
                    lsWhereAdd2 = " and id_objekt = " + giObjektId.ToString() + " ";
                    lsSql = lsSql + lsWhereAdd + lsWhereAdd2;
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
                case 1: // objekt_mix_parts
                    tableParts = new DataTable();
                    SqlCommand command = new SqlCommand(asSql, connect);
                    sdParts = new SqlDataAdapter(command);
                    sdParts.Fill(tableParts);
                    dgrChoose.ItemsSource = tableParts.DefaultView;

                    break;
                default:
                    break;
            }
            return liRows;
        }

        // Auswahl speichern und beenden
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
  
            this.Close();
        }

        // Annahme der TimeLine Id aus MainWindow
        internal void getTimelineId(int aiTimelineId)
        {
            giTimeLineId = aiTimelineId;
        }

        // Annahme der Objekt ID aus Mainwindow
        internal void getObjektId(int aiObjektId)
        {
            giObjektId = aiObjektId;
        }

        // get Art
        internal void getArt(int aiArt)
        {
            String lsSql = "";
            int liRows = 0;

            // Art nochmal global speichern
            giArt = aiArt;

            // SqlSelect ObjektTeile
            lsSql = getSql("part", aiArt, 0);
            // Daten Firmen holen
            liRows = fetchData(lsSql, 1);
        }

        // Checkbox Gesamtfläche Beibehalten
        private void cbGesFl_Checked(object sender, RoutedEventArgs e)
        {
            giFlBehalten = 1;
        }


        // Auch beim Schließen
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int liRows = 0;

            // Neue Daten > Timeline ID eintragen
            if (giArt == 1 || giArt == 2)
            {
                liRows = tableParts.Rows.Count;

                if (tableParts.Rows.Count > 0)
                {
                    foreach (DataRow dr in tableParts.Rows)
                    {
                        dr[7] = giTimeLineId;
                        dr[8] = giFlBehalten;
                    }
                }
            }

            SqlCommandBuilder commandBuilder = new SqlCommandBuilder(sdParts);

            sdParts.UpdateCommand = commandBuilder.GetUpdateCommand();
            sdParts.InsertCommand = commandBuilder.GetInsertCommand();

            sdParts.Update(tableParts);
        }
    }
}
