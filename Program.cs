using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;

namespace OPTBR6Downloader
{
    public class Program
    {
        // Win32 imports
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadConsoleInput(IntPtr hConsoleInput, out INPUT_RECORD lpBuffer, uint nLength, out uint lpNumberOfEventsRead);

        const int STD_INPUT_HANDLE = -10;
        const uint ENABLE_MOUSE_INPUT = 0x0010;
        const uint ENABLE_EXTENDED_FLAGS = 0x0080;
        const uint ENABLE_QUICK_EDIT_MODE = 0x0040;

        public enum EventType : ushort
        {
            KEY_EVENT = 1,
            MOUSE_EVENT = 2
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct INPUT_RECORD
        {
            [FieldOffset(0)] public EventType EventType;
            [FieldOffset(4)] public KEY_EVENT_RECORD KeyEvent;
            [FieldOffset(4)] public MOUSE_EVENT_RECORD MouseEvent;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEY_EVENT_RECORD
        {
            public bool bKeyDown;
            public ushort wRepeatCount;
            public ushort wVirtualKeyCode;
            public ushort wVirtualScanCode;
            public char UnicodeChar;
            public uint dwControlKeyState;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSE_EVENT_RECORD
        {
            public COORD dwMousePosition;
            public uint dwButtonState;
            public uint dwControlKeyState;
            public uint dwEventFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct COORD { public short X; public short Y; }

        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            OneDriveCheck();
            CheckAndDownloadResources();
        }

        static void OneDriveCheck()
        {
            string currentDir = Directory.GetCurrentDirectory();

            if (currentDir.Contains("OneDrive", StringComparison.OrdinalIgnoreCase))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://shorturl.at/qk3SX",
                    UseShellExecute = true
                });

                Console.Clear();
                Console.WriteLine("----------------------------------------------------------------------------------------------------------------");
                Console.WriteLine("| You ran this downloader inside of a OneDrive folder, move the downloader to a different location.             |");
                Console.WriteLine("| If you can't figure out how to move it follow this guide: https://shorturl.at/qk3SX                           |");
                Console.WriteLine("| PLEASE just check ALL of the OneDrive folder locations | DONT MAKE HELP POSTS ABOUT THIS                      |");
                Console.WriteLine("----------------------------------------------------------------------------------------------------------------");
                Console.WriteLine("Press any key to close the downloader...");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        static void CheckAndDownloadResources()
        {
            string resourcesPath = Path.Combine("Resources", "Tools");
            string sevenZipPath = Path.Combine("Resources", "7z.exe");

            if (!Directory.Exists(resourcesPath))
            {
                Directory.CreateDirectory(resourcesPath);
            }

            if (!File.Exists(sevenZipPath))
            {
                Console.Title = "Downloading 7-Zip...";
                Console.Clear();
                Console.WriteLine("-------------------------------------------------------------------------------");
                Console.WriteLine("                        Downloading 7-Zip...");
                Console.WriteLine("-------------------------------------------------------------------------------");

                try
                {
                    using (WebClient client = new WebClient())
                    {
                        string tempFile = Path.GetTempFileName();
                        string url = "https://github.com/DataCluster0/R6TBBatchTool/raw/master/Requirements/7z.exe";

                        client.DownloadFile(url, tempFile);

                        string resourcesDir = Path.Combine(Directory.GetCurrentDirectory(), "Resources");
                        if (!Directory.Exists(resourcesDir))
                            Directory.CreateDirectory(resourcesDir);

                        string destination = Path.Combine(resourcesDir, "7z.exe");
                        if (File.Exists(destination)) File.Delete(destination);
                        File.Move(tempFile, destination);
                    }
                    Console.WriteLine("7-Zip downloaded successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to download 7-Zip: {ex.Message}");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    Environment.Exit(1);
                }
            }
            DepotCheck();
        }

        static void DepotCheck()
        {
            string depotDownloader = Path.Combine("Resources", "DepotDownloader.dll");
            if (File.Exists(depotDownloader))
            {
                GetCracks();
            }
            else
            {
                GetDepotDownloader();
            }
        }

        static void GetDepotDownloader()
        {
            Console.Title = "Downloading DepotDownloader...";
            Console.Clear();
            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine("                        Downloading DepotDownloader...");
            Console.WriteLine("-------------------------------------------------------------------------------");

            try
            {
                using (WebClient client = new WebClient())
                {
                    string zipFile = Path.Combine(Directory.GetCurrentDirectory(), "depot.zip");
                    string url = "https://github.com/SteamRE/DepotDownloader/releases/download/DepotDownloader_3.4.0/DepotDownloader-framework.zip";

                    client.DownloadFile(url, zipFile);

                    // Extract with 7z.exe
                    string sevenZip = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "7z.exe");
                    string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Resources");

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = sevenZip,
                        Arguments = $"x -y -o\"{outputDir}\" \"{zipFile}\" -aoa",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    using (var proc = Process.Start(psi))
                    {
                        proc.WaitForExit();
                    }

                    File.Delete(zipFile);
                }

                Console.WriteLine("DepotDownloader downloaded and extracted successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to download DepotDownloader: {ex.Message}");
                Console.ReadKey(true);
                Environment.Exit(1);
            }

            DepotCheck(); // re-check
        }

        static void GetCracks()
        {
            string cracksDir = Path.Combine("Resources", "Cracks");
            string checkDirY1SX = Path.Combine(cracksDir, "Y1SX-Y6S2");
            string checkDirY6S3 = Path.Combine(cracksDir, "Y6S3");
            string checkDirY6S4 = Path.Combine(cracksDir, "Y6S4-Y8SX");

            if (Directory.Exists(cracksDir) && Directory.Exists(checkDirY1SX) && Directory.Exists(checkDirY6S3) && Directory.Exists(checkDirY6S4))
            {
                Console.WriteLine("Cracks are already present. Skipping download.");
                CmdCheck();
            }

            Console.Title = "Downloading Cracks...";
            Console.Clear();
            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine("                        Downloading Cracks...");
            Console.WriteLine("-------------------------------------------------------------------------------");

            string zipFile = Path.Combine(Directory.GetCurrentDirectory(), "Cracks.zip");
            string url = "https://github.com/JOJOVAV/r6-downloader/raw/refs/heads/main/Cracks.zip";

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(url, zipFile);
                }

                string sevenZip = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "7z.exe");
                string outputDir = cracksDir;

                if (!Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = sevenZip,
                    Arguments = $"x -y -o\"{outputDir}\" \"{zipFile}\" -aoa",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (Process proc = Process.Start(psi))
                {
                    proc.WaitForExit();
                }

                File.Delete(zipFile);

                Console.WriteLine("Cracks downloaded and extracted!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to download/extract AdditionalFiles: {ex.Message}");
                Console.ReadKey(true);
                Environment.Exit(1);
            }

            CmdCheck();
        }

        static void CmdCheck()
        {
            string cmdPath = Path.Combine("Resources", "cmdmenusel.exe");
            if (File.Exists(cmdPath))
            {
                LocalizationCheck();
            }
            else
            {
                GetCmd();
            }
        }

        static void GetCmd()
        {
            Console.Title = "Downloading cmdmenusel...";
            Console.Clear();
            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine("                        Downloading cmdmenusel...");
            Console.WriteLine("-------------------------------------------------------------------------------");

            try
            {
                using (WebClient client = new WebClient())
                {
                    string tempFile = Path.Combine(Directory.GetCurrentDirectory(), "cmdmenusel.exe");
                    string url = "https://github.com/SlejmUr/R6-AIOTool-Batch/raw/master/Requirements/cmdmenusel.exe";

                    client.DownloadFile(url, tempFile);

                    string resourcesDir = Path.Combine(Directory.GetCurrentDirectory(), "Resources");
                    if (!Directory.Exists(resourcesDir))
                        Directory.CreateDirectory(resourcesDir);

                    string destination = Path.Combine(resourcesDir, "cmdmenusel.exe");
                    if (File.Exists(destination)) File.Delete(destination);
                    File.Move(tempFile, destination);
                }
                Console.WriteLine("cmdmenusel downloaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to download cmdmenusel: {ex.Message}");
                Console.ReadKey(true);
                Environment.Exit(1);
            }

            CmdCheck();
        }

        static void LocalizationCheck()
        {
            string locPath = Path.Combine("Resources", "localization.lang");
            if (File.Exists(locPath))
            {
                MainMenu();
            }
            else
            {
                GetLocalization();
            }
        }

        static void GetLocalization()
        {
            Console.Title = "Downloading Localization file...";
            Console.Clear();
            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine("                        Downloading Localization file...");
            Console.WriteLine("-------------------------------------------------------------------------------");

            try
            {
                using (WebClient client = new WebClient())
                {
                    string tempFile = Path.Combine(Directory.GetCurrentDirectory(), "localization.lang");
                    string url = "https://github.com/JOJOVAV/r6-downloader/raw/refs/heads/main/localization.lang";

                    client.DownloadFile(url, tempFile);

                    string resourcesDir = Path.Combine(Directory.GetCurrentDirectory(), "Resources");
                    if (!Directory.Exists(resourcesDir))
                        Directory.CreateDirectory(resourcesDir);

                    string destination = Path.Combine(resourcesDir, "localization.lang");
                    if (File.Exists(destination)) File.Delete(destination);
                    File.Move(tempFile, destination);
                }
                Console.WriteLine("Localization file downloaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to download localization file: {ex.Message}");
                Console.ReadKey(true);
                Environment.Exit(1);
            }

            LocalizationCheck();
        }

        static void MainMenu()
        {
            Console.Clear();
            Console.SetWindowSize(120, 30);
            Console.SetBufferSize(120, 300);
            Console.Title = "Rainbow Six Siege Downloader";

            Console.WriteLine("--------------------------------------------------------------------------");
            Console.WriteLine("|      OPTB OLD Rainbow Six Siege Downloader  - Made by Puppetino        |");
            Console.WriteLine("--------------------------------------------------------------------------");
            Console.WriteLine("| YOU MUST CLAIM FOR FREE A COPY OF SIEGE ON STEAM TO USE THE DOWNLOADER |");
            Console.WriteLine("--------------------------------------------------------------------------");
            Console.WriteLine("|         Use ↑/↓ to navigate and press Enter to select an option        |");
            Console.WriteLine("--------------------------------------------------------------------------");
            Console.WriteLine();

            string[] options = new string[]
            {
                "Game Downloader",
                "Test Server Downloader",
                "4K Textures Download",
                "Modding / Extra Tools",
                "Claim Siege on Steam for free",
                "Downloader Settings",
                "Installation Guide and FAQ"
            };

            int choice = ShowMenu(options);

            switch (choice)
            {
                case 0:
                    DownloadMenu();
                    break;
                case 1:
                    Console.WriteLine("This feature is currently disabled.");
                    Console.ReadKey();
                    MainMenu();
                    break;
                case 2:
                    Console.WriteLine("This feature is currently disabled.");
                    Console.ReadKey();
                    MainMenu();
                    break;
                case 3:
                    Console.WriteLine("This feature is currently disabled.");
                    Console.ReadKey();
                    MainMenu();
                    break;
                case 4:
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://store.steampowered.com/app/359550/Tom_Clancys_Rainbow_Six_Siege_X/",
                        UseShellExecute = true
                    });
                    MainMenu();
                    break;
                case 5:
                    Console.WriteLine("This feature is currently disabled.");
                    Console.ReadKey();
                    MainMenu();
                    break;
                case 6:
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://puppetino.github.io/Throwback-FAQ/",
                        UseShellExecute = true
                    });
                    MainMenu();
                    break;
            }
        }

        static int ShowMenu(string[] options)
        {
            Console.CursorVisible = false;
            ConsoleInput.EnableMouse();

            int index = 0;
            int startY = Console.CursorTop;

            DrawOptions(options, index, startY);

            while (true)
            {
                var rec = ConsoleInput.ReadOne();

                // Keyboard navigation
                if (ConsoleInput.IsKeyUp(rec))
                {
                    if (index > 0) { index--; DrawOptions(options, index, startY); }
                    continue;
                }
                if (ConsoleInput.IsKeyDown(rec))
                {
                    if (index < options.Length - 1) { index++; DrawOptions(options, index, startY); }
                    continue;
                }
                if (ConsoleInput.IsKeyEnter(rec))
                {
                    Console.CursorVisible = true;
                    return index;
                }

                // Mouse hover
                if (ConsoleInput.IsMouseMoved(rec, out int mx, out int my))
                {
                    int hovered = my - startY;
                    if (hovered >= 0 && hovered < options.Length && hovered != index)
                    {
                        index = hovered;
                        DrawOptions(options, index, startY);
                    }
                    continue;
                }

                // Mouse click
                if (ConsoleInput.IsMouseClick(rec, out int cx, out int cy))
                {
                    int clickedIndex = cy - startY;
                    if (clickedIndex >= 0 && clickedIndex < options.Length)
                    {
                        Console.CursorVisible = true;
                        return clickedIndex;
                    }
                }
            }
        }

        static void DrawOptions(string[] options, int selectedIndex, int startY)
        {
            int width = Console.WindowWidth;
            for (int i = 0; i < options.Length; i++)
            {
                Console.SetCursorPosition(0, startY + i);
                string text = (i == selectedIndex ? $"> {options[i]}" : $"  {options[i]}");

                // Clear the line completely
                Console.Write(new string(' ', width));

                Console.SetCursorPosition(0, startY + i);
                if (i == selectedIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(text);
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine(text);
                }
            }
        }

        static void DownloadMenu()
        {
            Console.Title = "Game Downloader";

            string[] options = new string[]
            {
                "<- Back to Main Menu",
                "Refresh Menu",
                "Vanilla           | Y1S0 | 14.2 GB |",
                "Black Ice         | Y1S1 | 16.7 GB |",
                "Dust Line         | Y1S2 | 20.9 GB |",
                "Skull Rain        | Y1S3 | 25.1 GB |",
                "Red Crow          | Y1S4 | 28.5 GB |",
                "Velvet Shell      | Y2S1 | 33.2 GB |",
                "Health            | Y2S2 | 34.0 GB |",
                "Blood Orchid      | Y2S3 | 34.3 GB |",
                "White Noise       | Y2S4 | 48.7 GB |",
                "Chimera           | Y3S1 | 58.8 GB | Outbreak Event",
                "Para Bellum       | Y3S2 | 63.3 GB |",
                "Grim Sky          | Y3S3 | 72.6 GB | Mad House Event",
                "Wind Bastion      | Y3S4 | 76.9 GB |",
                "Burnt Horizon     | Y4S1 | 59.7 GB | Rainbow Is Magic Event",
                "Phantom Sight     | Y4S2 | 67.1 GB | Showdown Event",
                "Ember Rise        | Y4S3 | 69.6 GB | Doktor's Curse + Money Heist Event",
                "Shifting Tides    | Y4S4 | 75.2 GB | Stadium event",
                "Void Edge         | Y5S1 | 74.3 GB | Grand Larceny + Golden Gun Event",
                "Steel Wave        | Y5S2 | 81.3 GB | M.U.T.E. Protocol Event",
                "Shadow Legacy     | Y5S3 | 88.0 GB | Sugar Fright Event",
                "Neon Dawn (HM)    | Y5S4 | xx.x GB | Heated Metal",
                "Neon Dawn         | Y5S4 | xx.x GB | Road To S.I. 2021",
                "Crimson Heist     | Y6S1 | xx.x GB | Rainbow Is Magic 2 + Apocalypse",
                "North Star        | Y6S2 | xx.x GB | Nest Destruction",
                "Crystal Guard     | Y6S3 | xx.x GB | Showdown Event",
                "High Calibre      | Y6S4 | xx.x GB | Stadium + Snowball",
                "Demon Veil        | Y7S1 | xx.x GB | TOKY Event",
                "Vector Glare      | Y7S2 | xx.x GB | M.U.T.E Protocol Reboot",
                "Brutal Swarm      | Y7S3 | xx.x GB | Doctor’s Sniper Event",
                "Solar Raid        | Y7S4 | xx.x GB | Snow Brawl",
                "Commanding Force  | Y8S1 | xx.x GB | RIM + TOKY Event",
                "Dread Factor      | Y8S2 | xx.x GB | Rengoku Event",
                "Heavy Mettle      | Y8S3 | xx.x GB | Doktor's Curse + No Operators",
                "Deep Freeze       | Y8S4 | 52.9 GB | No Operators",
                "Deadly Omen       | Y9S1 | xx.x GB | No Operators",
                "New Blood         | Y9S2 | xx.x GB | No Operators",
                "Twin Shells       | Y9S3 | 59.2 GB | No Operators",
                "Collision Point   | Y9S4 | 59.2 GB | No Operators",
                "Prep Phase        | Y10S1| 51.4 GB | No Operators"
            };

            int choice;
            do
            {
                Console.Clear();
                Console.WriteLine("-----------------------------------------------------------------------------------------------------");
                Console.WriteLine("|  Use ↑/↓ to navigate, press Enter to select a version of Rainbow Six Siege to download.           |");
                Console.WriteLine("-----------------------------------------------------------------------------------------------------");
                Console.WriteLine("  Season Name       | Year | Size    | Additional Notes");

                choice = ShowMenu(options);

                switch (choice)
                {
                    case 0: MainMenu(); return;
                    case 1: continue;
                    case 2: Vanilla(); break;
                    case 3: BlackIce(); break;
                    case 4: DustLine(); break;
                    case 5: SkullRain(); break;
                    case 6: RedCrow(); break;
                    case 7: VelvetShell(); break;
                    case 8: Health(); break;
                    case 9: BloodOrchide(); break;
                    case 10: WhiteNoise(); break;
                    case 11: Chimera(); break;
                    case 12: ParraBellum(); break;
                    case 13: GrimSky(); break;
                    case 14: WindBastion(); break;
                    case 15: BurntHorizon(); break;
                    case 16: PhantomSight(); break;
                    case 17: EmberRise(); break;
                    case 18: ShiftingTides(); break;
                    case 19: VoidEdge(); break;
                    case 20: SteelWave(); break;
                    case 21: ShadowLegazy(); break;
                    case 22: HMNeonDawn(); break;
                    case 23: NeonDawn(); break;
                    case 24: CrimsonHeist(); break;
                    case 25: NorthStar(); break;
                    case 26: CrystalGuard(); break;
                    case 27: HighCaliber(); break;
                    case 28: DemonVeil(); break;
                    case 29: VectorGlare(); break;
                    case 30: BrutalSwarm(); break;
                    case 31: SolarRaid(); break;
                    case 32: CommandingForce(); break;
                    case 33: DreadFactor(); break;
                    case 34: HeavyMettle(); break;
                    case 35: DeepFreeze(); break;
                    case 36: DeadlyOmen(); break;
                    case 37: NewBlood(); break;
                    case 38: TwinShells(); break;
                    case 39: CollisionPoint(); break;
                    case 40: PrepPhase(); break;
                }

            } while (true);
        }

        static void Vanilla()
        {
            var depots = new (string, string)[]
            {
                ("377237", "8358812283631269928"),
                ("377238", "6835384933146381100"),
                ("359551", "3893422760579204530")
            };

            DownloadSeason(
                seasonName: "Y1S0_Vanilla",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void BlackIce()
        {
            var depots = new (string, string)[]
            {
                ("377237", "5188997148801516344"),
                ("377238", "5362991837480196824"),
                ("359551", "7932785808040895147")
            };

            DownloadSeason(
                seasonName: "Y1S1_BlackIce",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void DustLine()
        {
            var depots = new (string, string)[]
            {
                ("377237", "2303064029242396590"),
                ("377238", "3040224537841664111"),
                ("359551", "2206497318678061176")
            };

            DownloadSeason(
                seasonName: "Y1S2_DustLine",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void SkullRain()
        {
            var depots = new (string, string)[]
            {
                ("377237", "5819137024728546741"),
                ("377238", "2956768406107766016"),
                ("359551", "5851804596427790505")
            };

            DownloadSeason(
                seasonName: "Y1S3_SkullRain",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void RedCrow()
        {
            var depots = new (string, string)[]
            {
                ("377237", "3576607363557872807"),
                ("377238", "912564683190696342"),
                ("359551", "8569920171217002292")
            };

            DownloadSeason(
                seasonName: "Y1S4_RedCrow",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void VelvetShell()
        {
            var depots = new (string, string)[]
            {
                ("377237", "2248734317261478192"),
                ("377238", "2687181326074258760"),
                ("359551", "8006071763917433748")
            };

            DownloadSeason(
                seasonName: "Y2S1_VelvetShell",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void Health()
        {
            var depots = new (string, string)[]
            {
                ("377237", "5875987479498297665"),
                ("377238", "8542242518901049325"),
                ("359551", "708773000306432190")
            };

            DownloadSeason(
                seasonName: "Y2S2_Health",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void BloodOrchide()
        {
            var depots = new (string, string)[]
            {
                ("377237", "6708129824495912434"),
                ("377238", "4662662335520989204"),
                ("359551", "1613631671988840841")
            };

            DownloadSeason(
                seasonName: "Y2S3_BloodOrchid",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void WhiteNoise()
        {
            var depots = new (string, string)[]
            {
                ("377237", "8748734086032257441"),
                ("377238", "8421028160473337894"),
                ("359551", "4221297486420648079")
            };

            DownloadSeason(
                seasonName: "Y2S4_WhiteNoise",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void Chimera()
        {
            var depots = new (string, string)[]
            {
                ("377237", "5071357104726974256"),
                ("377238", "4768963659370299631"),
                ("359551", "4701787239566783972")
            };

            DownloadSeason(
                seasonName: "Y3S1_Chimera",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void ParraBellum()
        {
            var depots = new (string, string)[]
            {
                ("377237", "6507886921175556869"),
                ("377238", "7995779530685147208"),
                ("359551", "8765715607275074515")
            };

            DownloadSeason(
                seasonName: "Y3S2_ParaBellum",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void GrimSky()
        {
            var depots = new (string, string)[]
            {
                ("377237", "5562094852451837435"),
                ("377238", "3144556314994867170"),
                ("359551", "7781202564071310413")
            };

            DownloadSeason(
                seasonName: "Y3S3_GrimSky",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void WindBastion()
        {
            var depots = new (string, string)[]
            {
                ("377237", "6502258854032233436"),
                ("377238", "3144556314994867170"),
                ("359551", "7659555540733025386")
            };

            DownloadSeason(
                seasonName: "Y3S4_WindBastion",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void BurntHorizon()
        {
            var depots = new (string, string)[]
            {
                ("377237", "8356277316976403078"),
                ("377238", "3777349673527123995"),
                ("359551", "5935578581006804383")
            };

            DownloadSeason(
                seasonName: "Y4S1_BurntHorizon",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void PhantomSight()
        {
            var depots = new (string, string)[]
            {
                ("377237", "693082837425613508"),
                ("377238", "3326664059403997209"),
                ("359551", "5408324128694463720")
            };

            DownloadSeason(
                seasonName: "Y4S2_PhantomSight",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void EmberRise()
        {
            var depots = new (string, string)[]
            {
                ("377237", "3546781236735558235"),
                ("377238", "684480090862996679"),
                ("359551", "7869081741739849703")
            };

            DownloadSeason(
                seasonName: "Y4S3_EmberRise",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void ShiftingTides()
        {
            var depots = new (string, string)[]
            {
                ("377237", "299124516841461614"),
                ("377238", "510172308722680354"),
                ("359551", "1842268638395240106")
            };

            DownloadSeason(
                seasonName: "Y4S4_ShiftingTides",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void VoidEdge()
        {
            var depots = new (string, string)[]
            {
                ("377237", "4736360397583523381"),
                ("377238", "2583838033617047180"),
                ("359551", "6296533808765702678")
            };

            DownloadSeason(
                seasonName: "Y5S1_VoidEdge",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void SteelWave()
        {
            var depots = new (string, string)[]
            {
                ("377237", "4367817844736324940"),
                ("377238", "5838065097101371940"),
                ("359551", "893971391196952070")
            };

            DownloadSeason(
                seasonName: "Y5S2_SteelWave",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void ShadowLegazy()
        {
            var depots = new (string, string)[]
            {
                ("377237", "85893637567200342"),
                ("377238", "4020038723910014041"),
                ("359551", "3089981610366186823")
            };

            DownloadSeason(
                seasonName: "Y5S3_ShadowLegacy",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void HMNeonDawn()
        {
            var depots = new (string, string)[]
            {
                ("377237", "3390446325154338855"),
                ("377238", "3175150742361965235"),
                ("359551", "6947060999143280245")
            };

            DownloadSeason(
                seasonName: "Y5S4_NeonDawnHM",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void NeonDawn()
        {
            var depots = new (string, string)[]
            {
                ("377237", "4713320084981112320"),
                ("377238", "3560446343418579092"),
                ("359551", "3711873929777458413")
            };

            DownloadSeason(
                seasonName: "Y5S4_NeonDawn",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void CrimsonHeist()
        {
            var depots = new (string, string)[]
            {
                ("377237", "7890853311380514304"),
                ("377238", "6130917224459224462"),
                ("359551", "7485515457663576274")
            };

            DownloadSeason(
                seasonName: "Y6S1_CrimsonHeist",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void NorthStar()
        {
            var depots = new (string, string)[]
            {
                ("377237", "8733653062998518164"),
                ("377238", "6767916709017546201"),
                ("359551", "809542866761090243")
            };

            DownloadSeason(
                seasonName: "Y6S2_NorthStar",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void CrystalGuard()
        {
            var depots = new (string, string)[]
            {
                ("377237", "4859695099882698284"),
                ("377238", "5161489294178683219"),
                ("359551", "6526531850721822265")
            };

            DownloadSeason(
                seasonName: "Y6S3_CrystalGuard",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S3",
                includeLocalization: true
            );
        }

        static void HighCaliber()
        {
            var depots = new (string, string)[]
            {
                ("377237", "2637055726475611418"),
                ("377238", "2074678920289758165"),
                ("359551", "8627214406801860013")
            };

            DownloadSeason(
                seasonName: "Y6S4_HighCalibre",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void DemonVeil()
        {
            var depots = new (string, string)[]
            {
                ("377237", "8323869632165751287"),
                ("377238", "1970003626423861715"),
                ("359551", "2178080523228113690")
            };

            DownloadSeason(
                seasonName: "Y7S1_DemonVeil",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void VectorGlare()
        {
            var depots = new (string, string)[]
            {
                ("377237", "1363132201391540345"),
                ("377238", "4500117484519539380"),
                ("359551", "133280937611742404")
            };

            DownloadSeason(
                seasonName: "Y7S2_VectorGlare",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void BrutalSwarm()
        {
            var depots = new (string, string)[]
            {
                ("377237", "6425223567680952075"),
                ("377238", "4623590620762156001"),
                ("359551", "5906302942203575464")
            };

            DownloadSeason(
                seasonName: "Y7S3_BrutalSwarm",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void SolarRaid()
        {
            var depots = new (string, string)[]
            {
                ("377237", "4466027729495813039"),
                ("377238", "5107849703917033235"),
                ("359551", "1819898955518120444")
            };

            DownloadSeason(
                seasonName: "Y7S4_SolarRaid",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void CommandingForce()
        {
            var depots = new (string, string)[]
            {
                ("377237", "3275824905781062648"),
                ("377238", "1252692309389076318"),
                ("359551", "5863062164463920572")
            };

            DownloadSeason(
                seasonName: "Y8S1_CommandingForce",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void DreadFactor()
        {
            var depots = new (string, string)[]
            {
                ("377237", "3050554908913191669"),
                ("377238", "4293396692730784956"),
                ("359551", "1575870740329742681")
            };

            DownloadSeason(
                seasonName: "Y8S2_DreadFactor",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void HeavyMettle()
        {
            var depots = new (string, string)[]
            {
                ("377237", "2068160275622519212"),
                ("377238", "2579928666708989224"),
                ("359551", "3005637025719884427")
            };

            DownloadSeason(
                seasonName: "Y8S3_HeavyMettle",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void DeepFreeze()
        {
            var depots = new (string, string)[]
            {
                ("377237", "7646647065987620875"),
                ("377238", "8339919149418587132"),
                ("359551", "4957295777170965935")
            };

            DownloadSeason(
                seasonName: "Y8S4_DeepFreeze",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void DeadlyOmen()
        {
            var depots = new (string, string)[]
            {
                ("377237", "1959067516419454682"),
                ("377238", "1619182300337183882"),
                ("359551", "1140469899661941149")
            };

            DownloadSeason(
                seasonName: "Y9S1_DeadlyOmen",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void NewBlood()
        {
            var depots = new (string, string)[]
            {
                ("377237", "8160812118480939262"),
                ("377238", "2207285510020603118"),
                ("359551", "3303120421075579181")
            };

            DownloadSeason(
                seasonName: "Y9S2_NewBlood",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void TwinShells()
        {
            var depots = new (string, string)[]
            {
                ("377237", "4296569502001540403"),
                ("377238", "3038245830342960035"),
                ("359551", "825321500774263546")
            };

            DownloadSeason(
                seasonName: "Y9S3_TwinShells",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void CollisionPoint()
        {
            var depots = new (string, string)[]
            {
                ("377237", "9207916394092784817"),
                ("377238", "6303744364362141965"),
                ("359551", "3039751959139581613")
            };

            DownloadSeason(
                seasonName: "Y9S4_CollisionPoint",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void PrepPhase()
        {
            var depots = new (string, string)[]
            {
                ("377237", "8382986432868135995"),
                ("377238", "3364322644809414267"),
                ("359551", "2619322944995294928")
            };

            DownloadSeason(
                seasonName: "Y10S1_PrepPhase",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void DownloadSeason(
            string seasonName,
            string appId,
            (string depot, string manifest)[] depots,
            string crackSubfolder = null,
            bool includeLocalization = true)
        {
            Console.Title = $"Downloading {seasonName}...";
            Console.Clear();

            Console.Write("Enter Steam Username: ");
            string username = Console.ReadLine();

            string downloadsPath = Path.Combine("Downloads", seasonName);
            if (!Directory.Exists(downloadsPath))
                Directory.CreateDirectory(downloadsPath);

            // depots
            foreach (var (depot, manifest) in depots)
            {
                RunDepotDownloader(appId, depot, manifest, username, downloadsPath);
            }

            // crack files
            if (!string.IsNullOrEmpty(crackSubfolder))
            {
                string cracksPath = Path.Combine("Resources", "Cracks", crackSubfolder);
                if (Directory.Exists(cracksPath))
                {
                    RunRoboCopy(cracksPath, downloadsPath);
                }
            }

            // localization
            if (includeLocalization)
            {
                string locFile = Path.Combine("Resources", "localization.lang");
                if (File.Exists(locFile))
                {
                    RunRoboCopy("Resources", downloadsPath, "localization.lang");
                }
            }

            DownloadComplete();
        }

        static void RunDepotDownloader(string app, string depot, string manifest, string username, string outputDir)
        {
            string depotDownloader = Path.Combine("Resources", "DepotDownloader.dll");

            if (!File.Exists(depotDownloader))
            {
                Console.WriteLine("ERROR: DepotDownloader.dll not found in Resources folder.");
                return;
            }

            string args = $"\"{depotDownloader}\" -app {app} -depot {depot} -manifest {manifest} -username {username} -remember-password -dir \"{outputDir}\" -validate -max-downloads 25";


            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = false, 
                RedirectStandardError = false,
                RedirectStandardInput = false, 
                CreateNoWindow = false       
            };

            using (var proc = Process.Start(psi))
            {
                proc.WaitForExit();
            }
        }

        static void RunRoboCopy(string source, string destination, string extraArgs = "")
        {
            var psi = new ProcessStartInfo
            {
                FileName = "robocopy",
                Arguments = $"\"{source}\" \"{destination}\" {extraArgs} /IS /IT",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var proc = Process.Start(psi))
            {
                proc.OutputDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
                proc.ErrorDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine("ERROR: " + e.Data); };
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();
            }
        }

        static void DownloadComplete()
        {
            Console.Title = "Download Complete";
            Console.Clear();
            Console.WriteLine("----------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("|                                           Download Complete!                                                 |");
            Console.WriteLine("----------------------------------------------------------------------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine("Press any key to return to the main menu...");

            Console.ReadKey(true);
            Console.Clear();
            MainMenu();
        }
    }

    public static class ConsoleInput
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadConsoleInput(IntPtr hConsoleInput, out Program.INPUT_RECORD lpBuffer, uint nLength, out uint lpNumberOfEventsRead);

        const int STD_INPUT_HANDLE = -10;
        const uint ENABLE_MOUSE_INPUT = 0x0010;
        const uint ENABLE_EXTENDED_FLAGS = 0x0080;
        const uint ENABLE_QUICK_EDIT_MODE = 0x0040;

        static IntPtr hInput = GetStdHandle(STD_INPUT_HANDLE);

        public static void EnableMouse()
        {
            GetConsoleMode(hInput, out uint mode);

            // disable quick edit, enable mouse
            mode &= ~ENABLE_QUICK_EDIT_MODE;
            mode |= ENABLE_EXTENDED_FLAGS | ENABLE_MOUSE_INPUT;

            SetConsoleMode(hInput, mode);
        }

        public static Program.INPUT_RECORD ReadOne()
        {
            ReadConsoleInput(hInput, out Program.INPUT_RECORD record, 1, out _);
            return record;
        }

        public static bool IsKeyUp(Program.INPUT_RECORD r) =>
            r.EventType == Program.EventType.KEY_EVENT &&
            r.KeyEvent.bKeyDown &&
            r.KeyEvent.wVirtualKeyCode == 38; // Up arrow

        public static bool IsKeyDown(Program.INPUT_RECORD r) =>
            r.EventType == Program.EventType.KEY_EVENT &&
            r.KeyEvent.bKeyDown &&
            r.KeyEvent.wVirtualKeyCode == 40; // Down arrow

        public static bool IsKeyEnter(Program.INPUT_RECORD r) =>
            r.EventType == Program.EventType.KEY_EVENT &&
            r.KeyEvent.bKeyDown &&
            r.KeyEvent.wVirtualKeyCode == 13; // Enter

        public static bool IsMouseMoved(Program.INPUT_RECORD r, out int x, out int y)
        {
            if (r.EventType == Program.EventType.MOUSE_EVENT && r.MouseEvent.dwEventFlags == 1) // Mouse moved
            {
                x = r.MouseEvent.dwMousePosition.X;
                y = r.MouseEvent.dwMousePosition.Y;
                return true;
            }
            x = y = -1;
            return false;
        }

        public static bool IsMouseClick(Program.INPUT_RECORD r, out int x, out int y)
        {
            if (r.EventType == Program.EventType.MOUSE_EVENT && r.MouseEvent.dwButtonState == 1) // Left click
            {
                x = r.MouseEvent.dwMousePosition.X;
                y = r.MouseEvent.dwMousePosition.Y;
                return true;
            }
            x = y = -1;
            return false;
        }
    }
}
