using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Net;
using System.Threading;
using System.Windows.Threading;

namespace Launcher
{
    public partial class MainWindow : Window
    {
        private long h;
        private long hTotal;
        private string f;
        private double lastDownload = 0;
        public static bool g = false;

        Timer timer = new System.Threading.Timer((e) =>
        {
            secondWaited();
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

        private static void secondWaited()
        {
           g = true;
        }

        public MainWindow()
        {
            InitializeComponent();

            bool r = ReadFromFile.read();
            r = false; // for debug
            if (r)                  //Ready to Play
            {
                btn_play_update.Content = "Play";
                btn_play_update.Background = Brushes.DarkRed;
                lbl_bytes.Content = "";
                lbl_TaskStatus.Content = "";
                btn_play_update.IsEnabled = true;
            }
            else                    //Update the game
            {
                btn_play_update.Background = Brushes.Gray;
                //btn_play_update.Content = "Updating";
                Start();
            } 
        }
        
            string FileToDownload(string url) //url =  ftp://your_server.com/public_html/Paks
        {
            string _fileToDownload = url.ToString().Substring(url.ToString().LastIndexOf("Paks"));
            return _fileToDownload;
        }
        long checkFileFTP(Uri url)
        {
            f = FileToDownload(url.ToString());
            /**Check File FTP Size  **/
            FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create("ftp://your_server.com/public_html/" + f);

            request.Credentials = new NetworkCredential("name", "password");
            request.KeepAlive = false;
            //Use the GetFileSize FILE SIZE Protocol method
            request.Method = WebRequestMethods.Ftp.GetFileSize;
            request.UseBinary = true;
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            long fileSize = response.ContentLength;

            response.Close();
            request.Abort();

            return fileSize;
        }
        private async Task PerformLenghtTaskAsync()
        {
            try
            {
                using (System.Net.WebClient webClient = new System.Net.WebClient()) //get Total bytes from all files
                {
                    foreach (string item in ReadFromFile.ItemsToDownload_cli)
                    {
                        h = checkFileFTP(new Uri(item));
                        hTotal += h;
                    }
                }

                using (System.Net.WebClient webClient = new System.Net.WebClient())
                {
                    webClient.Credentials = new System.Net.NetworkCredential("name", "password");
                    webClient.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(Completed);
                    webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged);
                    foreach (string item in ReadFromFile.ItemsToDownload_cli)
                    {

                        h = checkFileFTP(new Uri(item));
                        f = FileToDownload(item);

                        await webClient.DownloadFileTaskAsync(new Uri(item), @"C:\VisualStudioPath\Client\" + f);
                    }//download with ftp by the list order
                }
                //After finish all downloads
                /**To Update GameVersion from file**/
                System.IO.File.WriteAllText(@"C:\VisualStudioPath\Client\client.txt", ReadFromFile.GameVersionServer);
                btn_play_update.Content = "Play";
                btn_play_update.Background = Brushes.DarkRed;
                btn_play_update.IsEnabled = true;
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to download File: " + e);
            }
        }

        private void Start()
        {
            PerformLenghtTaskAsync();
        }

        void DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            lbl_TaskStatus.Content = "Update in progress...";

            /*Check current and total bytes*/
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(h.ToString()); // e.TotalBytesToReceive.ToString());

            double percentage = bytesIn / totalBytes * 100;

            double totalBytesTotal = double.Parse(hTotal.ToString());
            double percentageTotal = bytesIn / totalBytesTotal * 100;

            lbl_bytes.Content = "Downloaded " + ConvertFileSize(e.BytesReceived) + " of " + ConvertFileSize(totalBytes); //e.TotalBytesToReceive; 

            if (g)
            {
                g = false;
                lbl_DownloadSpeed.Content = GetDownloadSpeed(bytesIn);
            }
            
            /*Update Progress Bar*/  
            progressBarXAML.Value = int.Parse(Math.Truncate(percentage).ToString());
            progressBarXAMLTotal.Value = int.Parse(Math.Truncate(percentageTotal).ToString());
        }
        private string GetDownloadSpeed(double bytesIn)
        {
            double bytesDownloadedLastSecond = bytesIn - lastDownload;
            lastDownload = bytesIn;

            string formatText = "";
            if (bytesDownloadedLastSecond >= 1048576.0)
            {
                formatText = String.Format("{0:##.##}", bytesDownloadedLastSecond / 1048576.0) + " MB/sec";
            }
            else if (bytesDownloadedLastSecond >= 1024.0)
            {
                formatText = String.Format("{0:##.##}", bytesDownloadedLastSecond / 1024.0) + " KB/sec";
            }
            else if (bytesDownloadedLastSecond > 0 && bytesDownloadedLastSecond < 1024)
            {
                formatText = bytesDownloadedLastSecond.ToString() + " Bytes/sec";
            }

            return formatText;
        }
        private string ConvertFileSize(double byteCount)
        {
            string size = "0 Bytes";
            if (byteCount >= 1073741824.0)
                size = String.Format("{0:##.##}", byteCount / 1073741824.0) + " GB";     //GB = bytes / 1024 / 1024 / 1024;
            else if (byteCount >= 1048576.0)
                size = String.Format("{0:##.##}", byteCount / 1048576.0) + " MB";   //MB = bytes / 1024 / 1024;
            else if (byteCount >= 1024.0)
                size = String.Format("{0:##.##}", byteCount / 1024.0) + " KB";  //KB = bytes / 1024;
            else if (byteCount > 0 && byteCount < 1024.0)
                size = byteCount.ToString() + " Bytes";

            return size;
        }

        void Completed(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            lbl_TaskStatus.Content = "Task Completed";
        }

        private void Btn_Play_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process[] pname = System.Diagnostics.Process.GetProcessesByName("your_game");
            if (pname.Length == 0)
            {
                System.Diagnostics.Process.Start(@"C:\your_path\your_game.exe");
            }
            else
            {
                MessageBox.Show("Process is already running");
            }
        }
    }
}