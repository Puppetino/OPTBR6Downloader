using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;

namespace OPTBR6Downloader
{
    class Program
    {
        static Dictionary<string, string> checkStatus = new(StringComparer.OrdinalIgnoreCase)
        {
            ["OneDrive"] = "pending",
            ["7z.exe"] = "pending",
            ["DepotDownloader.dll"] = "pending",
            ["Cracks"] = "pending",
            ["cmdmenusel.exe"] = "pending",
            ["localization.lang"] = "pending"
        };

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "OPTB R6 Downloader";

            try
            {
                Console.SetWindowSize(120, 35);
                Console.SetBufferSize(120, 35);
                ConsoleUtils.disableResize();
            }
            catch { }

            runStartupChecks();
            mainMenu();
        }

        public static class ConsoleUtils
        {
            private const int GWL_STYLE = -16;
            private const int WS_MAXIMIZEBOX = 0x00010000;
            private const int WS_MINIMIZEBOX = 0x00020000;
            private const int WS_SIZEBOX = 0x00040000;

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern IntPtr GetConsoleWindow();

            [DllImport("user32.dll", SetLastError = true)]
            private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

            [DllImport("user32.dll")]
            private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

            public static void disableResize()
            {
                IntPtr handle = GetConsoleWindow();
                int style = GetWindowLong(handle, GWL_STYLE);

                // Remove the maximize, minimize, and resize options
                style &= ~WS_MAXIMIZEBOX;
                style &= ~WS_MINIMIZEBOX;
                style &= ~WS_SIZEBOX;

                SetWindowLong(handle, GWL_STYLE, style);
            }
        }

        public static void runStartupChecks()
        {
            // Create table
            var table = new Table().Border(TableBorder.Rounded).Title("[bold yellow]Startup Checks[/]").Centered();
            table.AddColumn("[bold]Component[/]");
            table.AddColumn("[bold]Status[/]");

            foreach (var kv in checkStatus)
                table.AddRow(kv.Key, "[grey]Pending...[/]");

            AnsiConsole.Live(table)
                .Start(ctx =>
                {
                    void Update(string key, Action check)
                    {
                        checkStatus[key] = "running";
                        updateTable(ctx, table);
                        ctx.Refresh();

                        check();

                        checkStatus[key] = key switch
                        {
                            "OneDrive" => "ok",
                            "7z.exe" => File.Exists(Path.Combine("Resources", "7z.exe")) ? "ok" : "failed",
                            "DepotDownloader.dll" => File.Exists(Path.Combine("Resources", "DepotDownloader.dll")) ? "ok" : "failed",
                            "cmdmenusel.exe" => File.Exists(Path.Combine("Resources", "cmdmenusel.exe")) ? "ok" : "failed",
                            "localization.lang" => File.Exists(Path.Combine("Resources", "localization.lang")) ? "ok" : "failed",
                            "Cracks" => Directory.Exists(Path.Combine("Resources", "Cracks")) ? "ok" : "failed",
                            _ => "failed"
                        };

                        updateTable(ctx, table);
                    }

                    Update("OneDrive", oneDriveCheck);
                    Update("7z.exe", check7Zip);
                    Update("DepotDownloader.dll", depotCheck);
                    Update("Cracks", crackCheck);
                    Update("cmdmenusel.exe", cmdCheck);
                    Update("localization.lang", localizationCheck);
                });

            AnsiConsole.MarkupLine("\n[green]All checks completed![/]");
            AnsiConsole.MarkupLine("[grey]Press any key to continue to the main menu...[/]");
            Console.ReadKey(true);
            AnsiConsole.Clear();
        }

        private static void updateTable(LiveDisplayContext ctx, Table table)
        {
            table.Rows.Clear();
            foreach (var kv in checkStatus)
            {
                string status = kv.Value switch
                {
                    "ok" => "[green]✔ OK[/]",
                    "running" => "[yellow]⏳ Running...[/]",
                    "failed" => "[red]✖ Missing/Failed[/]",
                    _ => "[grey]Pending...[/]"
                };
                table.AddRow(kv.Key, status);
            }
            ctx.UpdateTarget(table);
        }

        static Tree buildCheckTree()
        {
            var tree = new Tree("[white]Startup checks[/]");
            var resources = tree.AddNode("[white]Resources[/]");

            resources.AddNode(formatNode("7z.exe", checkStatus["7z.exe"]));
            resources.AddNode(formatNode("DepotDownloader.dll", checkStatus["DepotDownloader.dll"]));
            resources.AddNode(formatNode("cmdmenusel.exe", checkStatus["cmdmenusel.exe"]));
            resources.AddNode(formatNode("localization.lang", checkStatus["localization.lang"]));
            resources.AddNode(formatNode("Cracks (folder)", checkStatus["Cracks"]));

            tree.AddNode(formatNode("OneDrive check", checkStatus["OneDrive"]));
            return tree;
        }

        static string formatNode(string name, string status)
        {
            return status switch
            {
                "ok" => $"[green]{name} - OK[/]",
                "running" => $"[yellow]{name} - running...[/]",
                "failed" => $"[red]{name} - missing/failed[/]",
                _ => $"[grey]{name} - pending[/]"
            };
        }


        public static void oneDriveCheck()
        {
            string currentDir = Directory.GetCurrentDirectory();

            if (currentDir.Contains("OneDrive", StringComparison.OrdinalIgnoreCase))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://shorturl.at/qk3SX",
                    UseShellExecute = true
                });

                AnsiConsole.MarkupLine("[red]You ran this downloader inside of a OneDrive folder.[/]");
                AnsiConsole.MarkupLine("[grey]Move the downloader to a different location.[/]");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        public static void check7Zip()
        {
            string resourcesPath = Path.Combine("Resources");
            string sevenZipPath = Path.Combine(resourcesPath, "7z.exe");

            if (!Directory.Exists(resourcesPath))
                Directory.CreateDirectory(resourcesPath);

            if (!File.Exists(sevenZipPath))
            {
                using var client = new WebClient();
                string tempFile = Path.GetTempFileName();
                string url = "https://github.com/DataCluster0/R6TBBatchTool/raw/master/Requirements/7z.exe";
                client.DownloadFile(url, tempFile);
                File.Move(tempFile, sevenZipPath, true);
            }
        }

        public static void depotCheck()
        {
            string depotFile = Path.Combine("Resources", "DepotDownloader.dll");
            if (File.Exists(depotFile)) return;

            string zipFile = Path.Combine(Directory.GetCurrentDirectory(), "depot.zip");
            string url = "https://github.com/SteamRE/DepotDownloader/releases/download/DepotDownloader_3.4.0/DepotDownloader-framework.zip";

            using (var client = new WebClient())
                client.DownloadFile(url, zipFile);

            string sevenZip = Path.Combine("Resources", "7z.exe");
            string outputDir = Path.Combine("Resources");

            var psi = new ProcessStartInfo
            {
                FileName = sevenZip,
                Arguments = $"x -y -o\"{outputDir}\" \"{zipFile}\" -aoa",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            proc.WaitForExit();
            File.Delete(zipFile);
        }

        public static void crackCheck()
        {
            string crackDir = Path.Combine("Resources", "Cracks");
            if (!Directory.Exists(crackDir))
            {
                Directory.CreateDirectory(crackDir);
                // Optional: status message can be shown in live table instead
            }
        }

        public static void cmdCheck()
        {
            string cmdPath = Path.Combine("Resources", "cmdmenusel.exe");
            if (File.Exists(cmdPath)) return;

            using var client = new WebClient();
            string temp = Path.GetTempFileName();
            string url = "https://github.com/SlejmUr/R6-AIOTool-Batch/raw/master/Requirements/cmdmenusel.exe";
            client.DownloadFile(url, temp);
            File.Move(temp, cmdPath, true);
        }

        public static void localizationCheck()
        {
            string langFile = Path.Combine("Resources", "localization.lang");
            if (File.Exists(langFile)) return;

            using var client = new WebClient();
            string temp = Path.GetTempFileName();
            string url = "https://github.com/JOJOVAV/r6-downloader/raw/refs/heads/main/localization.lang";
            client.DownloadFile(url, temp);
            File.Move(temp, langFile, true);
        }

        public static void mainMenu()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[bold blue]OPTB R6 Downloader[/]").RuleStyle("grey").LeftJustified());
            AnsiConsole.MarkupLine("[grey]Made by Puppetino[/]");
            AnsiConsole.MarkupLine("[green]Select a task below:[/]\n");

            var options = new[]
            {
                "🎮 Game Downloader",
                "🧪 Test Server Downloader",
                "🖼️ 4K Textures Download",
                "🧰 Modding / Extra Tools",
                "🎁 Claim Siege on Steam (Free)",
                "⚙️ Downloader Settings",
                "📖 Installation Guide & FAQ",
                "❌ Exit"
            };

            int choice = showMenu(options, "Main Menu");

            switch (choice)
            {
                case 0: downloadMenu(); break;
                case 4:
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://store.steampowered.com/app/359550/",
                        UseShellExecute = true
                    });
                    mainMenu();
                    break;
                case 6:
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://puppetino.github.io/Throwback-FAQ/",
                        UseShellExecute = true
                    });
                    mainMenu();
                    break;
                case 7:
                    Environment.Exit(0);
                    break;
                default:
                    AnsiConsole.MarkupLine("[red]This feature is currently disabled.[/]");
                    Console.ReadKey(true);
                    mainMenu();
                    break;
            }
        }

        public static void downloadMenu()
        {
            AnsiConsole.Clear();

            AnsiConsole.Write(new Rule("[bold blue]🎮 Siege Version Downloader[/]").RuleStyle("grey").LeftJustified());
            AnsiConsole.MarkupLine("[grey]Select a version to download:[/]\n");

            var versions = new[]
            {
                "[red]<- Back to Main Menu[/]",                                                                                 
                "",
                "[bold yellow]Year 1[/]",
                "[green]Vanilla[/]           | [grey]Y1S0[/] | [blue]14.2 GB[/]",
                "[green]Black Ice[/]         | [grey]Y1S1[/] | [blue]16.7 GB[/]",
                "[green]Dust Line[/]         | [grey]Y1S2[/] | [blue]20.9 GB[/]",
                "[green]Skull Rain[/]        | [grey]Y1S3[/] | [blue]25.1 GB[/]",
                "[green]Red Crow[/]          | [grey]Y1S4[/] | [blue]28.5 GB[/]",
                "",
                "[bold yellow]Year 2[/]",
                "[green]Velvet Shell[/]      | [grey]Y2S1[/] | [blue]33.2 GB[/]",
                "[green]Health[/]            | [grey]Y2S2[/] | [blue]34.0 GB[/]",
                "[green]Blood Orchid[/]      | [grey]Y2S3[/] | [blue]34.3 GB[/]",
                "[green]White Noise[/]       | [grey]Y2S4[/] | [blue]48.7 GB[/]",
                "",
                "[bold yellow]Year 3[/]",
                "[green]Chimera[/]           | [grey]Y3S1[/] | [blue]58.8 GB[/] | Outbreak Event",
                "[green]Para Bellum[/]       | [grey]Y3S2[/] | [blue]63.3 GB[/]",
                "[green]Grim Sky[/]          | [grey]Y3S3[/] | [blue]72.6 GB[/] | Mad House Event",
                "[green]Wind Bastion[/]      | [grey]Y3S4[/] | [blue]76.9 GB[/]",
                "",
                "[bold yellow]Year 4[/]",
                "[green]Burnt Horizon[/]     | [grey]Y4S1[/] | [blue]59.7 GB[/] | Rainbow Is Magic Event",
                "[green]Phantom Sight[/]     | [grey]Y4S2[/] | [blue]67.1 GB[/] | Showdown Event",
                "[green]Ember Rise[/]        | [grey]Y4S3[/] | [blue]69.6 GB[/] | Doktor's Curse + Money Heist Event",
                "[green]Shifting Tides[/]    | [grey]Y4S4[/] | [blue]75.2 GB[/] | Stadium Event",
                "",
                "[bold yellow]Year 5[/]",
                "[green]Void Edge[/]         | [grey]Y5S1[/] | [blue]74.3 GB[/] | Grand Larceny + Golden Gun Event",
                "[green]Steel Wave[/]        | [grey]Y5S2[/] | [blue]81.3 GB[/] | M.U.T.E. Protocol Event",
                "[green]Shadow Legacy[/]     | [grey]Y5S3[/] | [blue]88.0 GB[/] | Sugar Fright Event",
                "[green]Neon Dawn (HM)[/]    | [grey]Y5S4[/] | [blue]??.? GB[/] | Heated Metal",
                "[green]Neon Dawn[/]         | [grey]Y5S4[/] | [blue]??.? GB[/] | Road To S.I. 2021",
                "",
                "[bold yellow]Year 6[/]",
                "[green]Crimson Heist[/]     | [grey]Y6S1[/] | [blue]??.? GB[/] | Rainbow Is Magic 2 + Apocalypse",
                "[green]North Star[/]        | [grey]Y6S2[/] | [blue]??.? GB[/] | Nest Destruction",
                "[green]Crystal Guard[/]     | [grey]Y6S3[/] | [blue]??.? GB[/] | Showdown Event",
                "[green]High Calibre[/]      | [grey]Y6S4[/] | [blue]??.? GB[/] | Stadium + Snowball",
                "",
                "[bold yellow]Year 7[/]",
                "[green]Demon Veil[/]        | [grey]Y7S1[/] | [blue]??.? GB[/] | TOKY Event",
                "[green]Vector Glare[/]      | [grey]Y7S2[/] | [blue]??.? GB[/] | M.U.T.E Protocol Reboot",
                "[green]Brutal Swarm[/]      | [grey]Y7S3[/] | [blue]??.? GB[/] | Doctor’s Sniper Event",
                "[green]Solar Raid[/]        | [grey]Y7S4[/] | [blue]??.? GB[/] | Snow Brawl",
                "",
                "[bold yellow]Year 8[/]",
                "[green]Commanding Force[/]  | [grey]Y8S1[/] | [blue]??.? GB[/] | RIM + TOKY Event",
                "[green]Dread Factor[/]      | [grey]Y8S2[/] | [blue]??.? GB[/] | Rengoku Event",
                "[green]Heavy Mettle[/]      | [grey]Y8S3[/] | [blue]??.? GB[/] | Doktor's Curse + No Operators",
                "[green]Deep Freeze[/]       | [grey]Y8S4[/] | [blue]52.9 GB[/] | No Operators",
                "",
                "[bold yellow]Year 9[/]",
                "[green]Deadly Omen[/]       | [grey]Y9S1[/] | [blue]??.? GB[/] | No Operators",
                "[green]New Blood[/]         | [grey]Y9S2[/] | [blue]??.? GB[/] | No Operators",
                "[green]Twin Shells[/]       | [grey]Y9S3[/] | [blue]59.2 GB[/] | No Operators",
                "[green]Collision Point[/]   | [grey]Y9S4[/] | [blue]59.2 GB[/] | No Operators",
                "",
                "[bold yellow]Year 10[/]",
                "[green]Prep Phase[/]        | [grey]Y10S1[/] | [blue]51.4 GB[/] | No Operators"
            };

            int choice = showMenu(versions, "Select a Siege Version to Download");

            switch (choice)
            {
                case 0: mainMenu(); return;

                // Year 1
                case 3: vanilla(); break;
                case 4: blackIce(); break;
                case 5: dustLine(); break;
                case 6: skullRain(); break;
                case 7: redCrow(); break;

                // Year 2
                case 10: velvetShell(); break;
                case 11: health(); break;
                case 12: bloodOrchide(); break;
                case 13: whiteNoise(); break;

                // Year 3
                case 16: chimera(); break;
                case 17: parraBellum(); break;
                case 18: grimSky(); break;
                case 19: windBastion(); break;

                // Year 4
                case 22: burntHorizon(); break;
                case 23: phantomSight(); break;
                case 24: emberRise(); break;
                case 25: shiftingTides(); break;

                // Year 5
                case 28: voidEdge(); break;
                case 29: steelWave(); break;
                case 30: shadowLegazy(); break;
                case 31: hmNeonDawn(); break;
                case 32: neonDawn(); break;

                // Year 6
                case 35: crimsonHeist(); break;
                case 36: northStar(); break;
                case 37: crystalGuard(); break;
                case 38: highCaliber(); break;

                // Year 7
                case 41: demonVeil(); break;
                case 42: vectorGlare(); break;
                case 43: brutalSwarm(); break;
                case 44: solarRaid(); break;

                // Year 8
                case 47: commandingForce(); break;
                case 48: dreadFactor(); break;
                case 49: heavyMettle(); break;
                case 50: deepFreeze(); break;

                // Year 9
                case 53: deadlyOmen(); break;
                case 54: newBlood(); break;
                case 55: twinShells(); break;
                case 56: collisionPoint(); break;

                // Year 10
                case 59: prepPhase(); break;

                default:
                    AnsiConsole.MarkupLine("[red]This is not an Option![/]");
                    Console.ReadKey(true);
                    downloadMenu();
                    break;
            }
        }

        public static int showMenu(string[] options, string title)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[yellow]{title}[/]")
                    .PageSize(30)
                    .HighlightStyle(new Style(foreground: Color.Cyan1))
                    .AddChoices(options)
            );

            return Array.IndexOf(options, choice);
        }


        static void vanilla()
        {
            var depots = new (string, string)[]
            {
                ("377237", "8358812283631269928"),
                ("377238", "6835384933146381100"),
                ("359551", "3893422760579204530")
            };

            downloadSeason(
                seasonName: "Y1S0_Vanilla",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void blackIce()
        {
            var depots = new (string, string)[]
            {
                ("377237", "5188997148801516344"),
                ("377238", "5362991837480196824"),
                ("359551", "7932785808040895147")
            };

            downloadSeason(
                seasonName: "Y1S1_BlackIce",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void dustLine()
        {
            var depots = new (string, string)[]
            {
                ("377237", "2303064029242396590"),
                ("377238", "3040224537841664111"),
                ("359551", "2206497318678061176")
            };

            downloadSeason(
                seasonName: "Y1S2_DustLine",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void skullRain()
        {
            var depots = new (string, string)[]
            {
                ("377237", "5819137024728546741"),
                ("377238", "2956768406107766016"),
                ("359551", "5851804596427790505")
            };

            downloadSeason(
                seasonName: "Y1S3_SkullRain",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void redCrow()
        {
            var depots = new (string, string)[]
            {
                ("377237", "3576607363557872807"),
                ("377238", "912564683190696342"),
                ("359551", "8569920171217002292")
            };

            downloadSeason(
                seasonName: "Y1S4_RedCrow",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void velvetShell()
        {
            var depots = new (string, string)[]
            {
                ("377237", "2248734317261478192"),
                ("377238", "2687181326074258760"),
                ("359551", "8006071763917433748")
            };

            downloadSeason(
                seasonName: "Y2S1_VelvetShell",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void health()
        {
            var depots = new (string, string)[]
            {
                ("377237", "5875987479498297665"),
                ("377238", "8542242518901049325"),
                ("359551", "708773000306432190")
            };

            downloadSeason(
                seasonName: "Y2S2_Health",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void bloodOrchide()
        {
            var depots = new (string, string)[]
            {
                ("377237", "6708129824495912434"),
                ("377238", "4662662335520989204"),
                ("359551", "1613631671988840841")
            };

            downloadSeason(
                seasonName: "Y2S3_BloodOrchid",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void whiteNoise()
        {
            var depots = new (string, string)[]
            {
                ("377237", "8748734086032257441"),
                ("377238", "8421028160473337894"),
                ("359551", "4221297486420648079")
            };

            downloadSeason(
                seasonName: "Y2S4_WhiteNoise",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void chimera()
        {
            var depots = new (string, string)[]
            {
                ("377237", "5071357104726974256"),
                ("377238", "4768963659370299631"),
                ("359551", "4701787239566783972")
            };

            downloadSeason(
                seasonName: "Y3S1_Chimera",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void parraBellum()
        {
            var depots = new (string, string)[]
            {
                ("377237", "6507886921175556869"),
                ("377238", "7995779530685147208"),
                ("359551", "8765715607275074515")
            };

            downloadSeason(
                seasonName: "Y3S2_ParaBellum",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void grimSky()
        {
            var depots = new (string, string)[]
            {
                ("377237", "5562094852451837435"),
                ("377238", "3144556314994867170"),
                ("359551", "7781202564071310413")
            };

            downloadSeason(
                seasonName: "Y3S3_GrimSky",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void windBastion()
        {
            var depots = new (string, string)[]
            {
                ("377237", "6502258854032233436"),
                ("377238", "3144556314994867170"),
                ("359551", "7659555540733025386")
            };

            downloadSeason(
                seasonName: "Y3S4_WindBastion",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void burntHorizon()
        {
            var depots = new (string, string)[]
            {
                ("377237", "8356277316976403078"),
                ("377238", "3777349673527123995"),
                ("359551", "5935578581006804383")
            };

            downloadSeason(
                seasonName: "Y4S1_BurntHorizon",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void phantomSight()
        {
            var depots = new (string, string)[]
            {
                ("377237", "693082837425613508"),
                ("377238", "3326664059403997209"),
                ("359551", "5408324128694463720")
            };

            downloadSeason(
                seasonName: "Y4S2_PhantomSight",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void emberRise()
        {
            var depots = new (string, string)[]
            {
                ("377237", "3546781236735558235"),
                ("377238", "684480090862996679"),
                ("359551", "7869081741739849703")
            };

            downloadSeason(
                seasonName: "Y4S3_EmberRise",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void shiftingTides()
        {
            var depots = new (string, string)[]
            {
                ("377237", "299124516841461614"),
                ("377238", "510172308722680354"),
                ("359551", "1842268638395240106")
            };

            downloadSeason(
                seasonName: "Y4S4_ShiftingTides",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void voidEdge()
        {
            var depots = new (string, string)[]
            {
                ("377237", "4736360397583523381"),
                ("377238", "2583838033617047180"),
                ("359551", "6296533808765702678")
            };

            downloadSeason(
                seasonName: "Y5S1_VoidEdge",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void steelWave()
        {
            var depots = new (string, string)[]
            {
                ("377237", "4367817844736324940"),
                ("377238", "5838065097101371940"),
                ("359551", "893971391196952070")
            };

            downloadSeason(
                seasonName: "Y5S2_SteelWave",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void shadowLegazy()
        {
            var depots = new (string, string)[]
            {
                ("377237", "85893637567200342"),
                ("377238", "4020038723910014041"),
                ("359551", "3089981610366186823")
            };

            downloadSeason(
                seasonName: "Y5S3_ShadowLegacy",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void hmNeonDawn()
        {
            var depots = new (string, string)[]
            {
                ("377237", "3390446325154338855"),
                ("377238", "3175150742361965235"),
                ("359551", "6947060999143280245")
            };

            downloadSeason(
                seasonName: "Y5S4_NeonDawnHM",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void neonDawn()
        {
            var depots = new (string, string)[]
            {
                ("377237", "4713320084981112320"),
                ("377238", "3560446343418579092"),
                ("359551", "3711873929777458413")
            };

            downloadSeason(
                seasonName: "Y5S4_NeonDawn",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void crimsonHeist()
        {
            var depots = new (string, string)[]
            {
                ("377237", "7890853311380514304"),
                ("377238", "6130917224459224462"),
                ("359551", "7485515457663576274")
            };

            downloadSeason(
                seasonName: "Y6S1_CrimsonHeist",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void northStar()
        {
            var depots = new (string, string)[]
            {
                ("377237", "8733653062998518164"),
                ("377238", "6767916709017546201"),
                ("359551", "809542866761090243")
            };

            downloadSeason(
                seasonName: "Y6S2_NorthStar",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y1SX-Y6S2",
                includeLocalization: true
            );
        }

        static void crystalGuard()
        {
            var depots = new (string, string)[]
            {
                ("377237", "4859695099882698284"),
                ("377238", "5161489294178683219"),
                ("359551", "6526531850721822265")
            };

            downloadSeason(
                seasonName: "Y6S3_CrystalGuard",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S3",
                includeLocalization: true
            );
        }

        static void highCaliber()
        {
            var depots = new (string, string)[]
            {
                ("377237", "2637055726475611418"),
                ("377238", "2074678920289758165"),
                ("359551", "8627214406801860013")
            };

            downloadSeason(
                seasonName: "Y6S4_HighCalibre",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void demonVeil()
        {
            var depots = new (string, string)[]
            {
                ("377237", "8323869632165751287"),
                ("377238", "1970003626423861715"),
                ("359551", "2178080523228113690")
            };

            downloadSeason(
                seasonName: "Y7S1_DemonVeil",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void vectorGlare()
        {
            var depots = new (string, string)[]
            {
                ("377237", "1363132201391540345"),
                ("377238", "4500117484519539380"),
                ("359551", "133280937611742404")
            };

            downloadSeason(
                seasonName: "Y7S2_VectorGlare",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void brutalSwarm()
        {
            var depots = new (string, string)[]
            {
                ("377237", "6425223567680952075"),
                ("377238", "4623590620762156001"),
                ("359551", "5906302942203575464")
            };

            downloadSeason(
                seasonName: "Y7S3_BrutalSwarm",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void solarRaid()
        {
            var depots = new (string, string)[]
            {
                ("377237", "4466027729495813039"),
                ("377238", "5107849703917033235"),
                ("359551", "1819898955518120444")
            };

            downloadSeason(
                seasonName: "Y7S4_SolarRaid",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void commandingForce()
        {
            var depots = new (string, string)[]
            {
                ("377237", "3275824905781062648"),
                ("377238", "1252692309389076318"),
                ("359551", "5863062164463920572")
            };

            downloadSeason(
                seasonName: "Y8S1_CommandingForce",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void dreadFactor()
        {
            var depots = new (string, string)[]
            {
                ("377237", "3050554908913191669"),
                ("377238", "4293396692730784956"),
                ("359551", "1575870740329742681")
            };

            downloadSeason(
                seasonName: "Y8S2_DreadFactor",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void heavyMettle()
        {
            var depots = new (string, string)[]
            {
                ("377237", "2068160275622519212"),
                ("377238", "2579928666708989224"),
                ("359551", "3005637025719884427")
            };

            downloadSeason(
                seasonName: "Y8S3_HeavyMettle",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void deepFreeze()
        {
            var depots = new (string, string)[]
            {
                ("377237", "7646647065987620875"),
                ("377238", "8339919149418587132"),
                ("359551", "4957295777170965935")
            };

            downloadSeason(
                seasonName: "Y8S4_DeepFreeze",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void deadlyOmen()
        {
            var depots = new (string, string)[]
            {
                ("377237", "1959067516419454682"),
                ("377238", "1619182300337183882"),
                ("359551", "1140469899661941149")
            };

            downloadSeason(
                seasonName: "Y9S1_DeadlyOmen",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void newBlood()
        {
            var depots = new (string, string)[]
            {
                ("377237", "8160812118480939262"),
                ("377238", "2207285510020603118"),
                ("359551", "3303120421075579181")
            };

            downloadSeason(
                seasonName: "Y9S2_NewBlood",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void twinShells()
        {
            var depots = new (string, string)[]
            {
                ("377237", "4296569502001540403"),
                ("377238", "3038245830342960035"),
                ("359551", "825321500774263546")
            };

            downloadSeason(
                seasonName: "Y9S3_TwinShells",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void collisionPoint()
        {
            var depots = new (string, string)[]
            {
                ("377237", "9207916394092784817"),
                ("377238", "6303744364362141965"),
                ("359551", "3039751959139581613")
            };

            downloadSeason(
                seasonName: "Y9S4_CollisionPoint",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void prepPhase()
        {
            var depots = new (string, string)[]
            {
                ("377237", "8382986432868135995"),
                ("377238", "3364322644809414267"),
                ("359551", "2619322944995294928")
            };

            downloadSeason(
                seasonName: "Y10S1_PrepPhase",
                appId: "359550",
                depots: depots,
                crackSubfolder: "Y6S4-Y8SX",
                includeLocalization: true
            );
        }

        static void downloadSeason(
            string seasonName,
            string appId,
            (string depot, string manifest)[] depots,
            string crackSubfolder = null,
            bool includeLocalization = true)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine($"[bold yellow]Starting download for {seasonName}...[/]\n");

            string username = AnsiConsole.Ask<string>("Enter your [green]Steam username[/]:");
            string downloadsPath = Path.Combine("Downloads", seasonName);
            Directory.CreateDirectory(downloadsPath);

            foreach (var (depot, manifest) in depots)
            {
                AnsiConsole.Status().Start($"Downloading depot [blue]{depot}[/]...", _ =>
                {
                    runDepotDownloader(appId, depot, manifest, username, downloadsPath);
                });
            }

            if (!string.IsNullOrEmpty(crackSubfolder))
            {
                string cracksPath = Path.Combine("Resources", "Cracks", crackSubfolder);
                if (Directory.Exists(cracksPath))
                {
                    AnsiConsole.MarkupLine("[yellow]Copying crack files...[/]");
                    runRoboCopy(cracksPath, downloadsPath);
                }
            }

            if (includeLocalization)
            {
                string locFile = Path.Combine("Resources", "localization.lang");
                if (File.Exists(locFile))
                {
                    AnsiConsole.MarkupLine("[yellow]Copying localization file...[/]");
                    runRoboCopy("Resources", downloadsPath, "localization.lang");
                }
            }

            downloadComplete();
        }

        static void runDepotDownloader(string app, string depot, string manifest, string username, string outputDir)
        {
            string depotDownloader = Path.Combine("Resources", "DepotDownloader.dll");

            if (!File.Exists(depotDownloader))
            {
                AnsiConsole.MarkupLine("[red]ERROR: DepotDownloader.dll not found.[/]");
                return;
            }

            string args = $"\"{depotDownloader}\" -app {app} -depot {depot} -manifest {manifest} " +
                          $"-username {username} -remember-password -dir \"{outputDir}\" -validate -max-downloads 25";

            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            proc.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                    AnsiConsole.MarkupLine($"[grey]{Markup.Escape(e.Data)}[/]");
            };
            proc.BeginOutputReadLine();
            proc.WaitForExit();
        }

        static void runRoboCopy(string source, string destination, string extraArgs = "")
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

            using var proc = Process.Start(psi);
            proc.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                    AnsiConsole.MarkupLine($"[grey]{Markup.Escape(e.Data)}[/]");
            };
            proc.BeginOutputReadLine();
            proc.WaitForExit();
        }

        static void downloadComplete()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[bold green]Download Complete![/]").RuleStyle("grey").LeftJustified());
            AnsiConsole.MarkupLine("[green]Your selected version has been downloaded successfully.[/]");
            AnsiConsole.MarkupLine("[grey]Press any key to return to the main menu...[/]");
            Console.ReadKey(true);
            mainMenu();
        }
    }
}