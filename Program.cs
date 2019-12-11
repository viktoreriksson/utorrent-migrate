using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace utorrent_migrate
{
    class Program
    {
        class Torrent {
            public string name;
            public string path;

            public Torrent() {

            }
            public Torrent(string name, string path) {
                this.name = name;
                this.path = path;
            }
        }

        static void Main(string[] args)
        {
            string roamingPath = Environment.GetEnvironmentVariable("appdata"); //Windows
            string uTorrentPath = Path.Combine(roamingPath, "uTorrent");

            StreamReader sr = new StreamReader(Path.Combine(uTorrentPath, "resume.dat"), Encoding.UTF8);
            string resumeData = sr.ReadToEnd();
            sr.Dispose();

            MatchCollection torrentData = Regex.Matches(resumeData, @":caption\d+:(.+?)5:.+?:path\d+:(.+?)6:");
            List<Torrent> torrentList = new List<Torrent>();

            foreach (Match match in torrentData) {
                Torrent torrent = new Torrent();
                torrent.name = match.Groups[1].ToString();
                torrent.path = match.Groups[2].ToString();
                torrentList.Add(torrent);
            }

            if (!Directory.Exists("torrents")) {
                Directory.CreateDirectory("torrents");
            }

            int successful = 0;
            using (StreamWriter sw = new StreamWriter("torrents/paths.txt", false)) {
                foreach (Torrent torrent in torrentList.OrderBy(p => p.name)) {
                    string combinedTorrentPath = Path.Combine(uTorrentPath, torrent.name + ".torrent");

                    if (File.Exists(combinedTorrentPath)) {
                        string outFile = "torrents/" + torrent.name + ".torrent";

                        if (!File.Exists(outFile)) {
                            File.Copy(combinedTorrentPath, outFile);
                            sw.WriteLine(String.Format("{0} -> {1}", torrent.name, torrent.path));

                            ++successful;
                        } else {
                            Console.WriteLine("{0} already exists in /torrents. Skipping.", torrent.name);
                        }
                    }
                }
            }

            Console.WriteLine(File.ReadAllText("torrents/paths.txt"));
            Console.WriteLine();
            Console.WriteLine("This information is also available in /torrents/paths.txt");
            Console.WriteLine(".torrent files are stored in /torrents");
            Console.WriteLine();
            Console.WriteLine("Successful: {0}/{1}", successful, torrentList.OfType<Torrent>().Count());
            Console.WriteLine();
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
