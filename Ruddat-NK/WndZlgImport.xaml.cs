using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using MySql.Data.MySqlClient;
using Excel = Microsoft.Office.Interop.Excel;

namespace Ruddat_NK
{
    /// <summary>
    /// Interaktionslogik für WndZlgImport.xaml
    /// </summary>
    public partial class WndZlgImport : Window
    {
        private String gsConnect;
        private String gsPath;
        private String gsFileName;
        private String gsDlgFileName;
        private int      giLocationId = 0;
        private int      giImpId = 0;
        private int      giDb;
        private string gsMonth = "";
        private string gsYear = "";
        private DateTime gdtStart = DateTime.Today;
        private MainWindow mainWindow;

        DataTable tableDirty;
        DataTable tableHeader;
        DataTable tableFiliale;
        DataTable tableInfo;
        DataTable table_dirty;
        DataTable table_zlg;
        DataTable table_trace;

        SqlDataAdapter sdDirty;
        SqlDataAdapter sdHeader;
        SqlDataAdapter sdFiliale;
        SqlDataAdapter sdInfo;
        SqlDataAdapter sdZlg;
        SqlDataAdapter sdTrace;

        MySqlDataAdapter mysdDirty;
        MySqlDataAdapter mysdHeader;
        MySqlDataAdapter mysdFiliale;
        MySqlDataAdapter mysdInfo;
        MySqlDataAdapter mysdZlg;
        MySqlDataAdapter mysdTrace;

        BackgroundWorker worker;
        BackgroundWorker worker2;

        private delegate void DelPassDb(int giDb);

        // ConnectString übernehmen
        public string psConnect { get; set; }

        public WndZlgImport(MainWindow mainWindow)
        {
            String lsConnect;
            String UPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            gsPath = UPath;             // Pfad der Konfigurationsdatei global verfügbar machen

            this.mainWindow = mainWindow;
            InitializeComponent();

            // Buttons usw.
            clWahl.IsEnabled = true;
            clWahl.DisplayMode = CalendarMode.Year;
            btnFind.IsEnabled = false;
            btnImport.IsEnabled = false;          
            btnRollback.IsEnabled = false;
            lbLocation.IsEnabled = false;

            // ConnectString aus Mainwindow
            lsConnect = this.mainWindow.psConnect;
            // Globaler ConnectString
            gsConnect = lsConnect;

        }

        internal void getDb(int aiDb)
        {
            string lsTop = "";
            string lsSql = "";
            int liRows = 0;

            giDb = aiDb;

            // SqlSelects erstellen
            // Daten für listbox Firma holen
            lsTop = "";
            lsSql = getSql(lsTop, "filiale", DateTime.MinValue, "", 0);
            liRows = fetchData(lsSql, "filiale");

            // Daten von Import Info
            lsTop = "200";
            lsSql = getSql(lsTop, "import_info", DateTime.MinValue, "", 0);
            // Daten der ImportTabelle holen
            liRows = fetchData(lsSql, "import_info");
        }

        // Funktionen für den Backgroundworker
        // Worker Starten
        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            int liOk = 0;

            // erstmal x_import_dirty leeren
            liOk = DelImportDirty();
            // Die Tracetabelle Zahlungen löschen         
            liOk = DelImportTrace();
            // dann import durchführen
            liOk = DoImportDirty();
        }

        // Anzeige des Fortschritts
        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int percentFinished = 0;
            percentFinished = e.ProgressPercentage;
            if (e.ProgressPercentage > 10000)
            {
                pbExec.Maximum = e.ProgressPercentage - 10000;
            }
            else
            {
                pbExec.Value = e.ProgressPercentage;
            }
        }

        // Beenden des Backgoundworkers
        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Buttons einstellen
            btnFind.IsEnabled = false;
            clWahl.IsEnabled = true;
            lbLocation.IsEnabled = true;
            btnImport.IsEnabled = false;
            btnRollback.IsEnabled = false;
            btnClose.IsEnabled = true;
            pbExec.Value = 0;
        }

        // Funktionen für den Backgroundworker 2
        // Worker Starten
        // kopieren von import nach zahlungen
        void worker2_DoWork(object sender, DoWorkEventArgs e)
        {
            int liOk = 0;
            int liImportId = 0;

            // In ImportInfo einen Datensatz anlegen
            liOk = CreateImportHeader(Environment.UserName, DateTime.Now);

            // Kopieren der Zahlungen von import_dirty nach Zahlungen
            if (liOk == 1)
            {
                liImportId = TableCopy();
                if (liImportId > 0)
                {
                    // Timeline erzeugen
                    Timeline.editTimeline(liImportId, 13, gsConnect, giDb);
                }
                else
                {
                    MessageBox.Show("Eine Timeline konnte nicht erzeugt werden", "Achtung Fehler worker2_DoWork");
                }
            }
        }

        // Anzeige des Fortschritts
        void worker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int percentFinished = 0;
            percentFinished = e.ProgressPercentage;
            if (e.ProgressPercentage > 10000)
            {
                pbExec.Maximum = e.ProgressPercentage - 10000;
            }
            else
            {
                pbExec.Value = e.ProgressPercentage;
            }
        }

        // Beenden des Backgoundworkers
        void worker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            int liRows = 0;
            string lsTop = "";
            string lsSql2 = "";

            // Buttons einstellen
            btnFind.IsEnabled = false;
            clWahl.IsEnabled = false;
            lbLocation.IsEnabled = false;
            btnImport.IsEnabled = false;
            btnRollback.IsEnabled = false;
            btnClose.IsEnabled = true;
            pbExec.Value = 0;

            // Daten neu holen von Import Info
            lsSql2 = getSql("", "import_info", DateTime.MinValue, "", 0);
            // Daten der ImportTabelle holen
            liRows = fetchData(lsSql2, "import_info");
        }

        // Sql Statements zusasmmenbauen
        private string getSql(String asTop, String asDb, DateTime adtToday, string asUserName, int aiId)
        {
            String lsSql = "";

            if (asDb == "import_info")
            {
                lsSql = @"Select x_import_info.import_date,
                    x_import_info.import_user,
                    x_import_info.import_flag,
                    x_import_info.id_import_info,
                    x_import_info.import_descr
                    from x_import_info 
                    Order by x_import_info.import_date DESC ";
            }
            if (asDb == "filiale")
            {
                lsSql = "Select id_filiale,name from filiale";
            }

            if (asDb == "x_import_dirty")
            {
                lsSql = "Select iid,a,b,c,d,e,f,g,h,i,j,k,l,m,n,o from x_import_dirty";
            }

            // Anzahl der Datensätze in import dirty zählen
            if (asDb == "x_import_zahlungen")
            {
                lsSql = "SELECT COUNT(1) AS Expr1 FROM x_import_dirty";                
            }

            if (asDb == "header")
            {
                switch (giDb)
                {
                    case 1:
                        lsSql = @"insert into x_import_info (import_date,import_flag,import_user,import_descr) values (Convert(DateTime," + "\'"
                        + adtToday.ToString("dd.MM.yyyy HH:mm:ss") + "\',104),9,\'"
                        + asUserName + "\',\'" + gsYear + " | " + gsMonth + " | Datei=" + gsDlgFileName + "\')";
                        break;
                    case 2:
                        lsSql = @"insert into x_import_info (import_date,import_flag,import_user,import_descr) values "
                            +" date_format(\"" + adtToday.ToString("dd.MM.yyyy") + "\",\"%d.%m.%y\") "
                            + asUserName + "\',\'" + gsYear + " | " + gsMonth + " | Datei=" + gsDlgFileName + "\')";
                        break;
                    default:
                        break;
                }
            }
            if (asDb == "delImport")
            {
                lsSql = "Delete from x_import_dirty";
            }
            if (asDb == "delTrace")
            {
                lsSql = "Delete from zahlungen_trace";
            }
            if (asDb == "delTime")
            {
                lsSql = "Delete from timeline Where id_vorauszahlung = " + aiId.ToString();
            }
            if (asDb == "delZl")
            {
                lsSql = "Delete from zahlungen Where id_import = " + aiId.ToString();
            }
            if (asDb == "getTimeLine")
            {
                lsSql = @"Select id_extern_timeline from zahlungen where id_import = " + aiId.ToString();
            }
            if (asDb == "flag")
            {
                lsSql = @"Select id_import_info From x_import_info Where import_flag = 9";
            }
            if (asDb == "impInfo")
            {
                lsSql = @"Update x_import_info Set import_flag = 0 Where Id_import_info = " + aiId.ToString();
            }
            if (asDb == "dirty")
            {
                lsSql = "Select iid,a,b,c,d,e,f,g,h,i,j,k,l,m,n,o from x_import_dirty";
            }
            if (asDb == "zlg")
            {
                lsSql = @"select id_vz,id_mieter,id_objekt,id_objekt_teil,datum_von,datum_bis,
                                betrag_netto,betrag_brutto,betrag_netto_soll,betrag_brutto_soll,
                                id_extern_timeline,flag_timeline,id_ksa,id_import 
                            from zahlungen";
            }
            if (asDb == "trace")
            {
                lsSql = @"select id_vz,id_mieter,id_objekt,id_objekt_teil,datum_von,datum_bis,
                            betrag_netto,betrag_brutto,betrag_netto_soll,betrag_brutto_soll,
                            id_extern_timeline,flag_timeline,id_ksa,id_import,bez 
                        from zahlungen_trace";
            }

            return (lsSql);
        }

        // Daten aus der Datenbank holen und zeigen 
        private Int32 fetchData(string asSql, string asDataBase)
        {
            Int32 liValue = 0;

            try
            {
                switch (giDb)
                {
                    case 1:
                        SqlConnection connect;
                        connect = new SqlConnection(gsConnect);
                        connect.Open();

                        // Befüllen der Listbox location
                        if (asDataBase == "filiale")
                        {
                            tableFiliale = new DataTable();       // Grid
                            SqlCommand command1 = new SqlCommand(asSql, connect);
                            sdFiliale = new SqlDataAdapter(command1);
                            sdFiliale.Fill(tableFiliale);
                            lbLocation.ItemsSource = tableFiliale.DefaultView;
                        }

                        // Verbinden mit dem DataGridview WtImport
                        if (asDataBase == "import_info")
                        {
                            DataTable tableInfo = new DataTable();       // Grid
                            SqlCommand command2 = new SqlCommand(asSql, connect);
                            sdInfo = new SqlDataAdapter(command2);
                            sdInfo.Fill(tableInfo);
                            WtImport.ItemsSource = tableInfo.DefaultView;
                        }
                        if (asDataBase == "header")
                        {
                            SqlCommand command3 = new SqlCommand(asSql, connect);
                            SqlDataReader queryCommandReader3 = command3.ExecuteReader();
                        }
                        if (asDataBase == "delImport")
                        {
                            SqlCommand command4 = new SqlCommand(asSql, connect);
                            SqlDataReader queryCommandReader4 = command4.ExecuteReader();
                        }
                        if (asDataBase == "delTrace")
                        {
                            SqlCommand command5 = new SqlCommand(asSql, connect);
                            SqlDataReader queryCommandReader = command5.ExecuteReader();
                        }
                        if (asDataBase == "x_import_dirty")
                        {
                            // TableDirty ist für das Zufügen von Datensätzen Timeline
                            SqlCommand cmdDirty = new SqlCommand(asSql, connect);
                            tableDirty = new DataTable();
                            sdDirty = new SqlDataAdapter(cmdDirty);
                            sdDirty.Fill(tableDirty);
                        }
                        if (asDataBase == "impUpdate")
                        {
                            SqlCommandBuilder commandBuilder = new SqlCommandBuilder(sdDirty);
                            sdDirty.Update(tableDirty);
                        }
                        if (asDataBase == "delRollBack")
                        {
                            SqlCommand command6 = new SqlCommand(asSql, connect);
                            SqlDataReader queryCommandReader = command6.ExecuteReader();
                        }
                        if (asDataBase == "getTimeLIne")
                        {
                            SqlCommand command7 = new SqlCommand(asSql, connect);
                            var lvId = command7.ExecuteScalar();
                            if (lvId != DBNull.Value)
                            {
                                liValue = Convert.ToInt32(lvId);
                            }
                            else
                            {
                                liValue = 0;
                            }
                        }
                        if (asDataBase == "flag")
                        {
                            SqlCommand command8 = new SqlCommand(asSql, connect);
                            var lvId = command8.ExecuteScalar();
                            if (lvId != DBNull.Value)
                            {
                                liValue = Convert.ToInt32(lvId);
                            }
                            else
                            {
                                liValue = 0;
                            }
                        }
                        if (asDataBase == "tobj")
                        {

                            SqlCommand command = new SqlCommand(asSql, connect);
                            var lvGetId = command.ExecuteScalar();

                            if (lvGetId != null)
                            {
                                liValue = (int)lvGetId;
                            }
                            else
                            {
                                liValue = 0;
                            }
                        }
                        if (asDataBase == "impInfo")
                        {
                            SqlCommand command9 = new SqlCommand(asSql, connect);
                            SqlDataReader queryCommandReader9 = command9.ExecuteReader();
                        }
                        if (asDataBase == "dirty")
                        {
                            table_dirty = new DataTable();
                            SqlCommand command10 = new SqlCommand(asSql, connect);
                            SqlDataReader queryCommandReader10 = command10.ExecuteReader();
                            table_dirty.Load(queryCommandReader10);
                        }
                        if (asDataBase == "zlg")
                        {
                            table_zlg = new DataTable();
                            SqlCommand command11 = new SqlCommand(asSql, connect);
                            sdZlg = new SqlDataAdapter(command11);
                            SqlDataReader queryCommandReader11 = command11.ExecuteReader();
                            table_zlg.Load(queryCommandReader11);
                        }
                        if (asDataBase == "trace")
                        {
                            table_trace = new DataTable();
                            SqlCommand command12 = new SqlCommand(asSql, connect);
                            sdTrace = new SqlDataAdapter(command12);
                            SqlDataReader queryCommandReader12 = command12.ExecuteReader();
                            table_trace.Load(queryCommandReader12);
                        }
                        if (asDataBase == "zlgSave")
                        {
                            SqlCommandBuilder commandBuilder = new SqlCommandBuilder(sdZlg);
                            sdZlg.Update(table_zlg);
                        }
                        if (asDataBase == "traceSave")
                        {
                            SqlCommandBuilder commandBuilder = new SqlCommandBuilder(sdTrace);
                            sdTrace.Update(table_trace);
                        }
                        connect.Close();
                        break;
                    case 2:
                        MySqlConnection myConnect;
                        myConnect = new MySqlConnection(gsConnect);
                        myConnect.Open();

                        // Befüllen der Listbox location
                        if (asDataBase == "filiale")
                        {
                            tableFiliale = new DataTable();       // Grid
                            MySqlCommand command1 = new MySqlCommand(asSql, myConnect);
                            mysdFiliale = new MySqlDataAdapter(command1);
                            mysdFiliale.Fill(tableFiliale);
                            lbLocation.ItemsSource = tableFiliale.DefaultView;
                        }

                        // Verbinden mit dem DataGridview WtImport
                        if (asDataBase == "import_info")
                        {
                            DataTable tableInfo = new DataTable();       // Grid
                            MySqlCommand command2 = new MySqlCommand(asSql, myConnect);
                            mysdInfo = new MySqlDataAdapter(command2);
                            mysdInfo.Fill(tableInfo);
                            WtImport.ItemsSource = tableInfo.DefaultView;
                        }
                        if (asDataBase == "header")
                        {
                            MySqlCommand command3 = new MySqlCommand(asSql, myConnect);
                            MySqlDataReader queryCommandReader3 = command3.ExecuteReader();
                        }
                        if (asDataBase == "delImport")
                        {
                            MySqlCommand command4 = new MySqlCommand(asSql, myConnect);
                            MySqlDataReader queryCommandReader4 = command4.ExecuteReader();
                        }
                        if (asDataBase == "delTrace")
                        {
                            MySqlCommand command5 = new MySqlCommand(asSql, myConnect);
                            MySqlDataReader queryCommandReader = command5.ExecuteReader();
                        }
                        if (asDataBase == "x_import_dirty")
                        {
                            // TableDirty ist für das Zufügen von Datensätzen Timeline
                            MySqlCommand cmdDirty = new MySqlCommand(asSql, myConnect);
                            tableDirty = new DataTable();
                            mysdDirty = new MySqlDataAdapter(cmdDirty);
                            mysdDirty.Fill(tableDirty);
                        }
                        if (asDataBase == "impUpdate")
                        {
                            MySqlCommandBuilder commandBuilder = new MySqlCommandBuilder(mysdDirty);
                            mysdDirty.Update(tableDirty);
                        }
                        if (asDataBase == "delRollBack")
                        {
                            MySqlCommand command6 = new MySqlCommand(asSql, myConnect);
                            MySqlDataReader queryCommandReader = command6.ExecuteReader();
                        }
                        if (asDataBase == "getTimeLIne")
                        {
                            MySqlCommand command7 = new MySqlCommand(asSql, myConnect);
                            var lvId = command7.ExecuteScalar();
                            if (lvId != DBNull.Value)
                            {
                                liValue = Convert.ToInt32(lvId);
                            }
                            else
                            {
                                liValue = 0;
                            }
                        }
                        if (asDataBase == "flag")
                        {
                            MySqlCommand command8 = new MySqlCommand(asSql, myConnect);
                            var lvId = command8.ExecuteScalar();
                            if (lvId != DBNull.Value)
                            {
                                liValue = Convert.ToInt32(lvId);
                            }
                            else
                            {
                                liValue = 0;
                            }
                        }
                        if (asDataBase == "tobj")
                        {

                            MySqlCommand command = new MySqlCommand(asSql, myConnect);
                            var lvGetId = command.ExecuteScalar();

                            if (lvGetId != null)
                            {
                                liValue = (int)lvGetId;
                            }
                            else
                            {
                                liValue = 0;
                            }
                        }
                        if (asDataBase == "impInfo")
                        {
                            MySqlCommand command9 = new MySqlCommand(asSql, myConnect);
                            MySqlDataReader queryCommandReader9 = command9.ExecuteReader();
                        }
                        if (asDataBase == "dirty")
                        {
                            table_dirty = new DataTable();
                            MySqlCommand command10 = new MySqlCommand(asSql, myConnect);
                            MySqlDataReader queryCommandReader10 = command10.ExecuteReader();
                            table_dirty.Load(queryCommandReader10);
                        }
                        if (asDataBase == "zlg")
                        {
                            table_zlg = new DataTable();
                            MySqlCommand command11 = new MySqlCommand(asSql, myConnect);
                            mysdZlg = new MySqlDataAdapter(command11);
                            MySqlDataReader queryCommandReader11 = command11.ExecuteReader();
                            table_zlg.Load(queryCommandReader11);
                        }
                        if (asDataBase == "trace")
                        {
                            table_trace = new DataTable();
                            MySqlCommand command12 = new MySqlCommand(asSql, myConnect);
                            mysdTrace = new MySqlDataAdapter(command12);
                            MySqlDataReader queryCommandReader12 = command12.ExecuteReader();
                            table_trace.Load(queryCommandReader12);
                        }
                        if (asDataBase == "zlgSave")
                        {
                            MySqlCommandBuilder commandBuilder = new MySqlCommandBuilder(mysdZlg);
                            mysdZlg.Update(table_zlg);
                        }
                        if (asDataBase == "traceSave")
                        {
                            MySqlCommandBuilder commandBuilder = new MySqlCommandBuilder(mysdTrace);
                            mysdTrace.Update(table_trace);
                        }
                        myConnect.Close();
                        break;
                    default:
                        break;
                }
            }
            catch
            {
                // Die Anwendung anhalten
                MessageBox.Show("Verarbeitungsfehler WndZlgImport " + asDataBase + "\n",
                        "Achtung");
            }

            return (liValue);
        }

        // Den header für den Import erzeugen
        private int CreateImportHeader(string asUserName, DateTime adtToday)
        {
            int liOk = 0;
            string lsSql;

            // Hier wird als ImportKennung eine 9 eingesetzt. Daran kann erkannt werden, welche ID zu importieren ist
            // Nach dem Import ist die Kennung dann 1
            // Bei Rollback wird sie null
            lsSql = getSql("", "header", adtToday, asUserName, 0);
            liOk = fetchData(lsSql, "header");

            return liOk;
        }

        // Tabelle import_dirty leeren
        private int DelImportDirty()
        {
            int liOk = 0;
            string lsSql = "";
            // Die import_dirty kann schonmal gelöscht werden
            lsSql = getSql("", "delImport", DateTime.MinValue, "", 0);
            liOk = fetchData(lsSql, "delImport");

            return liOk;
        }

        // Tracetabelle für unverbuchte Zahlungen leeren
        private int DelImportTrace()
        {
            int liOk = 0;
            string lsSql = "";
            // Die import_dirty kann schonmal gelöscht werden
            lsSql = getSql("", "delTrace", DateTime.MinValue, "", 0);
            liOk = fetchData(lsSql, "delTrace");

            return liOk;
        }

        // Excel-Datei auswählen
        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.ShowDialog();

            if (dlg.FileName.Length > 0)
            {
                gsDlgFileName = dlg.FileName;

                // Der BackgroundWorker
                worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                worker.WorkerSupportsCancellation = false;

                worker.DoWork += new DoWorkEventHandler(worker_DoWork);
                worker.ProgressChanged +=
                            new ProgressChangedEventHandler(worker_ProgressChanged);
                worker.RunWorkerCompleted +=
                           new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

                btnFind.IsEnabled = false;
                btnClose.IsEnabled = false;
                // Der BackgroundWorker
                // den Thread starten
                worker.RunWorkerAsync(pbExec.Value);

            }
        }

        // Import des Excel-Files nach x_import_dirty
        private int DoImportDirty()
        {
            int liOk = 0;
            string lsSql = "";
            string lsCell = "";

            lsSql = getSql("", "x_import_dirty", DateTime.MinValue, "", 0);
            liOk = fetchData(lsSql, "x_import_dirty");

            if(liOk == 1)
            {
                Excel.Application xlApp = new Excel.Application();
                Excel.Workbook xlWorkBook;
                Excel.Worksheet xlWorkSheet;
                Excel.Range xlRange;
                xlWorkBook = xlApp.Workbooks.Open(gsDlgFileName, 0, 1);

                List<string> Folders = new List<string>();

                try
                {
                    xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(gsMonth + " " + gsYear);
                    xlRange = xlWorkSheet.UsedRange;

                    // Fortschrittsanzeige > Maximum übermitteln = 10000 dazuaddieren
                    worker.ReportProgress(xlRange.Rows.Count + 10000);

                    for (int i = 1; i <= xlRange.Rows.Count; i++)
                    {
                        DataRow dr = tableDirty.NewRow();

                        // Es gibt nur 15 Felder in der Tabelle > das geht nicht gut >xlRange.Columns.Count TODO Ulf

                        for (int j = 1; j <=15 ; j++)
                        {
                            // Exceldaten auf die Tabelle x_import_dirty schreiben
                            if (xlRange.Cells[i, j] != null && xlRange.Cells[i, j].Value2 != null)
                            {
                                lsCell = xlRange.Cells[i, j].Value2.ToString();
                                if (lsCell.Length > 50)
                                {
                                    dr[j] = lsCell.Substring(0, 50);
                                }
                                else
                                {
                                    dr[j] = lsCell;
                                }
                            }
                        }
                        tableDirty.Rows.Add(dr);

                        // Fortschrittsanzeige
                        worker.ReportProgress(i);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler " + ex.ToString(), "");
                }
                finally
                {
                    // und alles ab in die Datenbank
                    int LiOk = fetchData("", "impUpdate");

                    // So muss Excel beendet werden; alles andere hakt immer
                    xlApp.DisplayAlerts = false;
                    xlWorkBook.Close(null, null, null);
                    xlApp.Quit();
                }
            }
            return liOk;
        }

        // Fenster dichtmachen
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Import der gewählten csv Datei
        private void btnImport_Click(object sender, RoutedEventArgs e)
        {

            // Der BackgroundWorker2
            worker2 = new BackgroundWorker();
            worker2.WorkerReportsProgress = true;
            worker2.WorkerSupportsCancellation = false;

            worker2.DoWork += new DoWorkEventHandler(worker2_DoWork);
            worker2.ProgressChanged +=
                        new ProgressChangedEventHandler(worker2_ProgressChanged);
            worker2.RunWorkerCompleted +=
                       new RunWorkerCompletedEventHandler(worker2_RunWorkerCompleted);

            // den Thread starten
            worker2.RunWorkerAsync(pbExec.Value);
        }

        // Auswahl der Location getroffen
        private void lbLocation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnImport.IsEnabled = true;
            giLocationId = Convert.ToInt16(lbLocation.SelectedValue.ToString());
        }

        // Import wurde gewählt, die ID dafür ermitteln
        private void WtImport_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            String lsImpId = "";
            int liImpId = 0;

            var rowData = WtImport.SelectedItem as DataRowView;

            if (rowData != null)
            {
                // Mitarbeiter auf der ersten Zeile ermitteln
                // var rowData = DgrEmployee.Items[liRow] as DataRowView;
                lsImpId = rowData[3].ToString();
                liImpId = Convert.ToInt16(lsImpId);

                // In der Importanwahl ist was drin
                if (liImpId > 0)
                {
                    giImpId = liImpId;
                    btnRollback.IsEnabled = true;
                }
            }
        }

        private void btnRollback_Click(object sender, RoutedEventArgs e)
        {
            int liOk = 0;
            int liRows = 0;
            String lsTop = "200";
            String lsSql = "";
            MessageBoxResult lmrResult = 0;

            lmrResult = MessageBox.Show("Den gewählten Import löschen?", "Import löschen", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

            if (lmrResult == MessageBoxResult.Yes)
            {
                // Alles löschen, was diese ID hat
                // giImpId
                liOk = importRollBackZahlungen(giImpId);        // Zahlungen + Timeline
                // Dann das Import-flag auf 0 setzen
                liOk = importDeleted(giImpId);
               
                // Daten neu holen von Import Info
                lsTop = "200";
                lsSql = getSql(lsTop, "import_info", DateTime.MinValue, "", 0);
                // Daten der ImportTabelle holen
                liRows = fetchData(lsSql, "import_info");

                btnRollback.IsEnabled = false;
            }
        }

        // Aus dem Import löschen 
        private int importRollBackZahlungen(int giImpId)
        {
            int liOk = 0;
            string lsSql1 = "";
            string lsSql2 = "";
            int liIdTimeline = 0;

            // ermitteln der Timeline ID (id_vorauszahlung) aus der Import ID
            liIdTimeline = getTimelineId(giImpId);

            lsSql1 = getSql("", "delTime", DateTime.MinValue, "", liIdTimeline);
            lsSql2 = getSql("", "delZl", DateTime.MinValue, "", giImpId);

            liOk = fetchData(lsSql1, "delRollBack");
            liOk = fetchData(lsSql2, "delRollBack");

            return liOk;
        }

        private int getTimelineId(int giImpId)
        {
            string lsSql = "";
            int liTimelineId = 0;

            lsSql = getSql("", "getTimeLine", DateTime.MinValue, "", giImpId);
            liTimelineId = fetchData(lsSql, "getTimeLine");

            return liTimelineId;
        }

        // Das Flag im ImportInfoDatensatz auf 0 setzen
        private int importDeleted(int aiImpId)
        {
            int liOk = 0;
            String lsSql = "";

            lsSql = getSql("", "impInfo", DateTime.MinValue, "", aiImpId);
            liOk = fetchData(lsSql, "impInfo");

            return liOk;
        }

        // Kalender angewählt
        private void clWahl_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime ldtStart = DateTime.MinValue;

            // Welcher Monat/Jahr wurde gewählt?
            if (clWahl.SelectedDate.HasValue)
            {
                ldtStart = clWahl.SelectedDate.Value;
                gsMonth = ldtStart.ToString("MMMM");
                gsYear = ldtStart.Year.ToString();
                gdtStart = ldtStart;
            }
            btnFind.IsEnabled = true;
        }

        // Die ID des Import Headers besorgen (flag = 9), um sie in jeden Datesatz einzubauen
        // Das ist für ein Rollback erforderlich
        private int getImportHeaderId()
        {
            int liHeaderId = 0;
            string lsSql = "";

            lsSql = getSql("", "flag", DateTime.MinValue, "", 0);
            liHeaderId = fetchData(lsSql, "flag");

            return liHeaderId;
        }

        // Teilobjekt ID aus der Kostenstelle ermitteln    
        private int getObjektTeilId(string lsKstObjTeil, string lsKstObj)
        {
            int liObjTeilId = 0;

            String lsSql = @"Select Id_objekt_teil from objekt_teil 
	                            Right join objekt on objekt_teil.id_objekt = objekt.Id_objekt
	                            Where objekt.kst = '" + lsKstObj + "' and objekt_teil.kst = '" + lsKstObjTeil + "'";
            liObjTeilId = fetchData(lsSql, "tobj");

            return liObjTeilId;
        }

        // Hier werden die Daten von import_dirty nach zahlungen umkopiert
        private int TableCopy()
        {
            int liHeaderId = 0;
            int liObjTeilId = 0;
            int liTimelineId = 0;
            int liMwstSatz = 0;
            int liTest = 0;
            int liTraceFlag = 0;
            int liMieter = 0;
            int liOk = 0;
            String lsSqlDirty = "";
            String lsSqlZlg = "";
            string lsSqlTrace = "";
            String lsKstObj = "";
            String lsKstObjTeil = "";
            DateTime dtTmp = DateTime.Now;
            DateTime ldtStart = DateTime.MinValue;
            decimal ldKaltmiete = 0;
            decimal ldNk = 0;
            decimal ldMwst = 0;
            decimal ldBruttoSoll = 0;
            decimal ldBruttoIst = 0;
            decimal ldNkNetto = 0;
            decimal ldNkBrutto = 0;
            decimal ldN2 = 0;
            decimal ldB2 = 0;

            lsSqlDirty = getSql("", "dirty", DateTime.MinValue, "", 0);
            lsSqlZlg = getSql("", "zlg", DateTime.MinValue, "", 0);
            lsSqlTrace = getSql("", "trace", DateTime.MinValue, "", 0);

            liOk = fetchData(lsSqlDirty, "dirty");
            liOk = fetchData(lsSqlZlg, "zlg");
            liOk = fetchData(lsSqlTrace, "trace");

            // Import ID holen
            liHeaderId = getImportHeaderId();

            // Kalender in lokale Variable
            ldtStart = gdtStart;

            // Neue Timeline Id holen Art 2 = Zahlungen
            liTimelineId = Timeline.getTimelineId(gsConnect,2, giDb) + 1;

            // Mehrwertsteuersatz für normal holen
            liMwstSatz = Timeline.getMwstFromBez("normal", gsConnect, giDb);

            // Fortschrittsanzeige > Maximum übermitteln = 10000 dazuaddieren
            worker2.ReportProgress(table_dirty.Rows.Count + 10000);

            // Dann alles in datatable zahlungen kopieren und entsprechend umwandeln
            for (int i = 2; i < table_dirty.Rows.Count; i++)
            {
                // Teilobjekt ID holen
                lsKstObj        = table_dirty.Rows[i].ItemArray.GetValue(10).ToString();
                lsKstObjTeil    = table_dirty.Rows[i].ItemArray.GetValue(11).ToString();

                ldKaltmiete = 0;
                ldNk = 0;
                ldMwst = 0;
                ldBruttoSoll = 0;
                ldBruttoIst = 0;
                ldNkNetto = 0;
                ldNkBrutto = 0;

                if (lsKstObj.Length > 0 && lsKstObjTeil.Length > 0 && liHeaderId > 0)
                {
                    liObjTeilId = getObjektTeilId(lsKstObjTeil.Trim(),lsKstObj.Trim());
                    // Die Zahlung konnte zugeordnet werden > 0
                    if (liObjTeilId > 0)
                    {
                        decimal.TryParse(table_dirty.Rows[i].ItemArray.GetValue(3).ToString(), out ldKaltmiete);  // Kaltmiete soll
                        decimal.TryParse(table_dirty.Rows[i].ItemArray.GetValue(4).ToString(), out ldNk);         // Nebenkosten soll
                        decimal.TryParse(table_dirty.Rows[i].ItemArray.GetValue(6).ToString(), out ldMwst);       // Mwst
                        decimal.TryParse(table_dirty.Rows[i].ItemArray.GetValue(7).ToString(), out ldBruttoSoll); // Zahlung sollBrutto
                        decimal.TryParse(table_dirty.Rows[i].ItemArray.GetValue(9).ToString(), out ldBruttoIst);  // Zahlung Brutto

                        // Mieter eintragen
                        liMieter = Timeline.getAktMieter(liObjTeilId, ldtStart, gsConnect, giDb);

                        if (ldBruttoIst  > 0 && liMieter > 0)           // Nur wenn in den Nebenkosten ein Betrag steht
                        {
                            // Id bleibt frei dr[0] = XX;
                            DataRow dr = table_zlg.NewRow();

                            if (ldMwst > 0)  // Mit Mwst: Nettobetrag wird eingetragen
                            {
                                ldN2 = ldBruttoIst - ldMwst - ldKaltmiete;
                                ldB2 = ldN2 + ((ldN2 / 100) * liMwstSatz);
                                ldNkNetto = ldNk;
                            }
                            else // Keine Mwst: Bruttobetrag wird eingetragen
                            {
                                ldB2 = ldBruttoIst - ldKaltmiete;
                                ldN2 = (ldB2 / (100 + liMwstSatz)) * 100;
                                ldNkBrutto = ldNk;
                            }

                            // nur positive Zahlen eintragen
                            if (ldN2 <= 0)
                            {
                                ldN2 = 0;
                            }

                            if (ldB2 <= 0)
                            {
                                ldB2 = 0;
                            }

                            dr[1] = liMieter;               // Mieter Id
                            // dr[3] = liObjTeilId;         // ObjektTeil ID
                            dr[4] = ldtStart;               // Datum
                            dr[6] = ldN2;                   // Netto
                            dr[7] = ldB2;                   // Brutto
                            dr[8] = ldNkNetto;              // Netto Soll
                            dr[9] = ldNkBrutto;             // Brutto Soll
                            dr[10] = liTimelineId;          // Timeline ID
                            dr[11] = 1;                     // Timelineflag
                            dr[12] = Timeline.getKsaId(1,gsConnect, giDb);  // Kostenart Vorrauszahlung Nebenkosten
                            dr[13] = liHeaderId;            // Import ID

                            // Timeline schreiben
                            Timeline.editTimeline(liTimelineId, 11, gsConnect, giDb);

                            liTimelineId++;
                            table_zlg.Rows.Add(dr);
                        }
                    }
                    else    // Die Zahlung aus Excel konnte nicht verbucht werden, kommt in die TraceTabelle Zahlungen
                    {
                        decimal.TryParse(table_dirty.Rows[i].ItemArray.GetValue(4).ToString(), out ldKaltmiete);
                        decimal.TryParse(table_dirty.Rows[i].ItemArray.GetValue(4).ToString(), out ldNk);
                        decimal.TryParse(table_dirty.Rows[i].ItemArray.GetValue(5).ToString(), out ldMwst);
                        decimal.TryParse(table_dirty.Rows[i].ItemArray.GetValue(6).ToString(), out ldBruttoSoll);

                        liTraceFlag = 1;

                        if (ldNk > 0)           // Nur wenn in den Nebenkosten ein Betrag steht
                        {
                            DataRow drt = table_trace.NewRow();

                            // Id bleibt frei dr[0] = XX;
                            drt[3] = liObjTeilId;            // ObjektTeil ID
                            drt[4] = ldtStart;               // Datum
                            drt[6] = ldNk;                   // Netto
                            drt[10] = liTimelineId;
                            drt[11] = 1;                     // Timelineflag
                            drt[12] = Timeline.getKsaId(1,gsConnect, giDb);  // Kostenart Vorrauszahlung Nebenkosten
                            drt[13] = liHeaderId;            // Import ID
                            drt[14] = "Kostenst: " + lsKstObj.Trim() + "/" + lsKstObjTeil.Trim() + "/" +liObjTeilId.ToString();

                            liTimelineId++;
                            liTest = i;
                            table_trace.Rows.Add(drt);
                        }
                    }
                }
                // Fortschrittsanzeige
                worker2.ReportProgress(i);
            }
            // Zahlungen sichern
            liOk = fetchData("", "zlgSave");

            if (liTraceFlag == 1)
            {
                // Trace sichern
                liOk = fetchData("", "traceSave");
                liTraceFlag = 0;
            }
            return liHeaderId;
        }
    }
}
