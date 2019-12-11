using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace utorrent_migrate
{
    class Program
    {
        static void Main(string[] args)
        {
            string roamingPath = Environment.GetEnvironmentVariable("appdata");
            string uTorrentPath = Path.Combine(roamingPath, "uTorrent");

            StreamReader sr = new StreamReader(Path.Combine(uTorrentPath, "resume.dat"));
            string resumeData = sr.ReadToEnd();
            sr.Dispose();

            MatchCollection matches = Regex.Matches(resumeData, ":caption\\d+:(.+?)5:");
            IEnumerable<string> torrents =
                from torrent in matches
                select torrent.Groups[1].Captures[0].Value;

            if(!Directory.Exists("torrents")) {
                Directory.CreateDirectory("torrents");
            }

            int successful = 0;
            foreach(string torrent in torrents) {
                string combinedTorrentPath = Path.Combine(uTorrentPath, torrent + ".torrent");
                if(File.Exists(combinedTorrentPath)) {
                    File.Copy(combinedTorrentPath, "torrents/" + torrent + ".torrent");
                    ++successful;
                }
            }

            Console.WriteLine("Successful: {0}/{1}", successful, torrents.OfType<string>().Count());
            Console.ReadKey();
        }
    }
}
