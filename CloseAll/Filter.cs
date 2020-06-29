﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Management.Instrumentation;
using System.IO;

namespace CloseAll
{
    class Filter
    {
        List<string> StartupApps;

        public bool NoFocus { get; set; }
        public bool IgnoreStartup { get; set; }
        public bool UnderException { get; set; }
        public List<string> ExceptList { get; set; }

        public bool GetFilters(string[] args)
        {

            foreach (string arg in args)
            {
                switch (arg.ToLower())
                {
                    case "-help":
                        Console.WriteLine("\n------------------------ Commands ------------------------\n\n" +
                            "COMMAND: closeall -except <app1> <app2> <app3> : Indicated processes won't get killed\n" +
                            "  Example: closeall -except opera    : All the processes will get killed, except Opera\n" +
                            "  Example: closeall -except discord devenv   : All the processes will get killed, except Discord and Visual Studio\n" +
                            "\nCOMMAND: closeall -ignore-startup : Startup processes won't get killed\n" +
                            "\n----------------------------------------------------------");
                        return false;

                    case "-except":
                        UnderException = true;
                        break;

                    case "-nofocus":
                        NoFocus = true;
                        break;

                    case "-ignore-startup":
                        IgnoreStartup = true;
                        break;

                    default:
                        if (UnderException)
                        {
                            Except(arg.ToLower());
                        }
                        else
                        {
                            Console.WriteLine("\nUnknown argument   ¯\\_('-')_/¯  ");
                            Console.WriteLine("   Use closeall -help to get the list of commands.");
                            return false;
                        }
                        break;
                }
            }

            return true;

            //
            // Asta las aici na vseakii slucii
            //
            //foreach (string arg in args)
            //{
            //    if (arg[0] == '-')
            //    {
            //        UnderException = false;

            //        switch (arg.ToLower())
            //        {
            //            case "-help":
            //                Console.WriteLine("\n------------------------ Commands ------------------------\n\n" +
            //                    "COMMAND: closeall -except <app1> <app2> <app3> : Indicated processes won't get killed\n" +
            //                    "  Example: closeall -except opera    : All the processes will get killed, except Opera\n" +
            //                    "  Example: closeall -except discord devenv   : All the processes will get killed, except Discord and Visual Studio\n" +
            //                    "\nCOMMAND: closeall -ignore-startup : Startup processes won't get killed\n" +
            //                    "\n----------------------------------------------------------");
            //                return false;

            //            case "-except":
            //                UnderException = true;
            //                break;

            //            case "-nofocus":
            //                NoFocus = true;
            //                break;

            //            case "-ignore-startup":
            //                IgnoreStartup = true;
            //                break;

            //            default:
            //                Console.WriteLine(@"Unknown argument   ¯\_('-')_/¯  ");
            //                Console.WriteLine(@"   Use closeall -help to get the list of commands.");
            //                return false;
            //        }
            //    }
            //    else
            //    {
            //        if (UnderException)
            //        {
            //            Except(arg.ToLower());
            //        }
            //    }
            //}

        }

        public void Except(string processName)
        {
            ExceptList.Add(processName);
        }

        public bool IgnoreProcess(Process process)
        {
            // The process is included in ExceptList
            foreach (string proc in ExceptList)
                if (proc == process.ProcessName.ToLower())
                    return true;

            // It's a Startup process
            if (IgnoreStartup)
            {
                // ISSUE: Find a way to get only startup applications that are currently enabled.
                // This way we get all startup apps, including disabled ones.

                if(StartupApps == null || StartupApps.Count == 0)
                {
                    // Get & store startup apps
                    ManagementClass mangnmt = new ManagementClass("Win32_StartupCommand");
                    ManagementObjectCollection startupApps = mangnmt.GetInstances();
                    StartupApps = new List<string>();

                    foreach (ManagementObject app in startupApps)
                    {
                        string processName = getProcessName(app["command"] as string);

                        if (processName != null)
                            StartupApps.Add(processName);
                    }
                }

                if (StartupApps.Contains(process.ProcessName.ToLower()))
                    return true;
            }

            // It is focused
            if (NoFocus)
            {
                //stuff
            }

            // Process goes to GULAG
            return false;
        }

        public Filter()
        {
            ExceptList = new List<string>();
        }

        private string getProcessName(string command)
        {
            int startIndex = command.LastIndexOf('\\');

            if (startIndex <= 0)
                return null;

            string processName = command.Substring(++startIndex).ToLower();
            int endIndex = processName.IndexOf(".exe");

            if (endIndex <= 0)
                return null;

            return processName.Substring(0, endIndex);
        }
    }
}
