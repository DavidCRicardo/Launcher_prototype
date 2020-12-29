using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Launcher
{
    class ReadFromFile
    {
        public static List<string> fileItemsServer = new List<string>();
        public static List<string> ItemsToDownload_cli = new List<string>();
        public static string GameVersionServer;
        public static bool read()
        {
            System.Net.WebClient client = new System.Net.WebClient();
            using (var stream = client.OpenRead("https://your_server.com/Paks/server.txt"))
            using (var reader = new StreamReader(stream))
            {
                string line;
                int counter = 0;
                while ((line = reader.ReadLine()) != null && counter < 1)
                {
                    counter++;
                    GameVersionServer = line;
                }
            }
            System.IO.StreamReader fileClient_path = new System.IO.StreamReader(@"C:\VisualStudioPath\Client\client.txt");

            string txtclient = fileClient_path.ReadLine();

            fileClient_path.Close();

            if (txtclient == GameVersionServer)
            {
                MessageBox.Show("Use the most recent version...");
                return true;
            }
            else
            {

                using (var stream = client.OpenRead("https://your_server.com/Paks/server.txt"))
                using (var reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        fileItemsServer.Add(line);
                    }
                }

                string[] Lines2 = File.ReadAllLines(@"C:\VisualStudioPath\Client\client.txt");
                for (int line = 1; line < fileItemsServer.Count; line++)
                {
                    if (line < Lines2.Length)
                    {
                        if (fileItemsServer[line].Equals(Lines2[line]))
                        {
                            //                               MessageBox.Show("Same Lines"); // dont do nothing // lines from both files are same
                        }
                        else
                        {
                            //                               MessageBox.Show("Error 100"); //exists but with another name or missing
                        }
                    }
                    else
                    {
                        string a = @"C:\VisualStudioPath\Client\client.txt";

                        using (StreamWriter sw = File.AppendText(a))
                        {
                            ItemsToDownload_cli.Add(fileItemsServer[line]);
                        }
                    }
                }
                return false;
            }
        }
    }
}