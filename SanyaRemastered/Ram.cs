﻿using SanyaRemastered.Data;
using MEC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SanyaRemastered
{
    public class Ram
    {
        public class MemoryService
        {
            public static MemoryMetrics CurrentTotalMetrics { get; set; }
            public static double CurrentProcessRamUsage { get; set; }
            public BackgroundWorker BackgroundWorker { get; private set; }
            public int ProcessId { get; private set; }

            private readonly bool _isUnix;

            public MemoryService(int processId)
            {
                _isUnix = IsUnix();
                ProcessId = processId;
                SetUpBackgroundWorker();
            }

            public void DeconstructBackgroundWorker()
            {
                BackgroundWorker.DoWork -= BackgroundWorker_DoWork;
                BackgroundWorker.RunWorkerCompleted -= BackgroundWorker_RunWorkerCompleted;
                BackgroundWorker.Dispose();
            }
            public void SetUpBackgroundWorker()
            {
                BackgroundWorker = new BackgroundWorker
                {
                    WorkerSupportsCancellation = true
                };
                BackgroundWorker.DoWork += BackgroundWorker_DoWork;
                BackgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
            }

            private async void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
            {
                if (e.Cancelled || BackgroundWorker.CancellationPending)
                {
                    return;
                }
                await Task.Delay(150);
                BackgroundWorker.RunWorkerAsync();
            }
            private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
            {
                if (BackgroundWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                CurrentTotalMetrics = GetMetrics();
                CurrentProcessRamUsage = GetProcessRamUsage();
            }

            private MemoryMetrics GetMetrics()
            {
                if (_isUnix)
                {
                    return GetUnixTotalMetrics();
                }

                return GetWindowsTotalMetrics();
            }
            private double GetProcessRamUsage()
            {
                if (_isUnix)
                {
                    return GetUnixProcessRamUsage();
                }

                return GetWindowsProcessRamUsage();
            }

            private bool IsUnix()
            {
                var isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                             RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
                return isUnix;
            }

            private MemoryMetrics GetWindowsTotalMetrics()
            {
                var output = "";

                var info = new ProcessStartInfo
                {
                    FileName = "wmic",
                    Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };

                using (var process = Process.Start(info))
                {
                    output = process.StandardOutput.ReadToEnd();
                }

                var lines = output.Trim().Split('\n');
                var freeMemoryParts = lines[0].Split(new[] { '=' }, options: StringSplitOptions.RemoveEmptyEntries);
                var totalMemoryParts = lines[1].Split(new[] { '=' }, options: StringSplitOptions.RemoveEmptyEntries);

                var metrics = new MemoryMetrics
                {
                    Total = Math.Round(double.Parse(totalMemoryParts[1]) / 1024, 0),
                    Free = Math.Round(double.Parse(freeMemoryParts[1]) / 1024, 0)
                };
                metrics.Used = metrics.Total - metrics.Free;

                return metrics;
            }
            private MemoryMetrics GetUnixTotalMetrics()
            {
                var output = "";

                var info = new ProcessStartInfo("free -m")
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"free -m\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };

                using (var process = Process.Start(info))
                {
                    output = process.StandardOutput.ReadToEnd();
                }

                var lines = output.Split('\n');
                var memory = lines[1].Split(new[] { ' ' }, options: StringSplitOptions.RemoveEmptyEntries);

                var metrics = new MemoryMetrics
                {
                    Total = Math.Round(double.Parse(memory[1], 0)),
                    Used = Math.Round(double.Parse(memory[2], 0)),
                    Free = Math.Round(double.Parse(memory[3], 0))
                };

                return metrics;
            }

            private double GetWindowsProcessRamUsage()
            {
                var output = "";

                var info = new ProcessStartInfo
                {
                    FileName = "wmic",
                    Arguments = $"process where processid={ProcessId} get WorkingSetSize",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };

                using (var process = Process.Start(info))
                {
                    output = process.StandardOutput.ReadToEnd();
                }

                var lines = output.Trim().Split('\n');
                var memoryUsage = Math.Round(ulong.Parse(lines[1]) / 1024D / 1024D, 0);

                return memoryUsage;
            }
            private double GetUnixProcessRamUsage()
            {
                Process process = Process.GetCurrentProcess();

                process.Refresh();

                double memoryUsage = process.WorkingSet64 / 1024 / 1024;

                return memoryUsage;
            }
        }
    }
}
