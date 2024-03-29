﻿using System;
using System.Windows;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using WinMin.Functions;
using Newtonsoft.Json;
using System.Management;
using System.Windows.Threading;

namespace WinMin_Launcher
{
    public partial class App : Application
    {
        public readonly string rootPath = @"C:\Users\Public\WinMin";
        public readonly static string patchPath = "C:\\Users\\Public\\WinMin\\Patches";
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            if(args.Length == 2)
            {
                if (args[1].Equals("startup"))
                {
                    var interval = new TimeSpan(0, 0, 1);
                    const string isWin32Process = "TargetInstance isa \"Win32_Process\"";

                    WqlEventQuery stopQuery = new WqlEventQuery("__InstanceDeletionEvent", interval, isWin32Process);
                    var _stopWatcher = new ManagementEventWatcher(stopQuery);
                    _stopWatcher.Start();
                    _stopWatcher.EventArrived += OnStopEventArrived;                    
                }
                else if (args[1].Equals("admin"))
                {
                    Process process2 = new Process();
                    ProcessStartInfo startInfo2 = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/C net user administrator /active:yes",
                        WindowStyle = ProcessWindowStyle.Minimized
                    };
                    process2.StartInfo = startInfo2;
                    process2.Start();
                    process2.WaitForExit();
                    Process process = new Process();
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/C net user administrator Password1",
                        WindowStyle = ProcessWindowStyle.Minimized
                    };
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();
                    Current.Shutdown();
                }
                else
                {
                    //TODO: add support to launch and run the wmpatch
                    if (args[1].Substring(args[1].Length - 7).Equals("wmpatch"))
                    {
                        MessageBox.Show("Running wmpatch files isn't supported yet");
                    }
                    else
                    {
                        File.WriteAllText($"{rootPath}\\Program.txt", args[1]);
                    }
                    MainWindow window = new MainWindow();
                    window.Show();
                }
            }
            else
            {
                MainWindow window = new MainWindow();
                window.Show();
            }
        }
        
        void OnStopEventArrived(object sender, EventArrivedEventArgs e)
        {
            var o = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            string name = (string)o["Name"];
            if (name.Equals("gpscript.exe"))
            {
                foreach (string directory in Directory.GetDirectories($"{patchPath}\\"))
                {
                    string json = File.ReadAllText($"{directory}\\manifest.json");
                    WMManifest manifest = JsonConvert.DeserializeObject<WMManifest>(json);

                    PluginManager pluginManager = new PluginManager();
                    pluginManager.LoadFiles(manifest);
                }
                Dispatcher.Invoke(() =>
                {
                    Current.Shutdown();
                });
            }
            if (name.Equals("LogonUI.exe"))
            {
                //We shutdown just incase gpscript doesnt happen
                Dispatcher.Invoke(() =>
                {
                    Current.Shutdown();
                });
            }
        }
        
    }
}
