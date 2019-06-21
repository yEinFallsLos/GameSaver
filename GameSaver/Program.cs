﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameSaver
{
    class Program
    {
        static void Main(string[] args)
        {
            Config c = Config.Load("config.json");

            while(true)
            {
                bool updated = false;
                foreach(Location l in c.Locations)
                {
                    if (l.GetProcessState() || l.CheckValidity())
                        continue;

                    Location.Path[] changedFiles = l.GetChangedFiles();

                    if (changedFiles.Length <= 0)
                        continue;

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("[{0}] Detected changes, updating {1} files..", l.Name, changedFiles.Length);
                    Console.ResetColor();

                    foreach (Location.Path path in changedFiles)
                    {
                        Console.Write("[{0}] copying file: '{1}'..", l.Name, path.Source);

                        try
                        {
                            CopyFile(path.Source, path.Destination);
                        } catch (IOException e)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(" failed! ({0})", e.ToString());
                            Console.ResetColor();
                            continue;
                        }

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(" done");
                        Console.ResetColor();
                    }

                    l.LastUpdated = DateTime.UtcNow;
                    updated = true;
                }

                if (updated)
                    c.Save("config.json");
                Thread.Sleep(c.RefreshInterval);
            }
        }

        static void CopyFile(string source, string dest)
        {
            if (!File.Exists(source))
                return;

            ICollection<string> directoriesToAdd = new List<string>();
            for(string p = Path.GetDirectoryName(dest); !Directory.Exists(p); p = Path.GetDirectoryName(p))
            {
                directoriesToAdd.Add(p);
            }
            foreach(string d in directoriesToAdd.Reverse())
            {
                Directory.CreateDirectory(d);
            }

            File.Copy(source, dest, true);
        }
    }
}
