using Docker.DotNet;
using Docker.DotNet.Models;
using MCServerWebWrapper.Server.Data.Models;
using MCServerWebWrapper.Server.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable IDE0052 // Remove unread private members
#pragma warning disable IDE1006 // Naming Styles
namespace MCServerWebWrapper.Server.Models
{
    public class StreamManager : IDisposable
    {
        public string ServerId { get; private set; }
        public string ContainerId { get; private set; }
        public Output LastOutput { get; private set; }
        public CancellationToken Token
        {
            get
            {
                return _tokenSource.Token;
            }
        }
        public Progress<ContainerStatsResponse> StatProgress { get; }
        public CpuData CpuData { get; private set; }
        public RamData RamData { get; private set; }

        public event EventHandler<OutputReceivedEventArgs> OutputReceived;
        public event EventHandler<OutputReceivedEventArgs> ErrorReceived;
        public event EventHandler<StatusUpdatedEventArgs> StatusUpdated;

        private readonly CancellationTokenSource _tokenSource;
        private readonly MultiplexedStream _stream;
        private Stream _stdout;
        private Stream _stderr;
        private DateTime? _lastStatusUpdateTimeStamp;

        public StreamManager(string serverId, string containerId, MultiplexedStream stream, CancellationTokenSource tokenSource = null)
        {
            ServerId = serverId;
            ContainerId = containerId;
            _stream = stream;
            _tokenSource = tokenSource != null ? tokenSource : new CancellationTokenSource();
            StatProgress = new Progress<ContainerStatsResponse>();
            StatProgress.ProgressChanged += OnProcessStatistics;
            InitializeState();
        }

        public void InitializeState()
        {
            _stdout = new MemoryStream();
            _stderr = new MemoryStream();
            //Task.Run(async () => await _stream.CopyFromAsync(_stdin, Token));
            Task.Run(async () => await _stream.CopyOutputToAsync(Stream.Null, _stdout, _stderr, Token));
            Task.Run(async () => await ReadOutput(_tokenSource.Token));
        }

        private async Task ReadOutput(CancellationToken token)
        {
            await foreach (var output in _stream.StreamOutputAsync(Token))
            {

            }
        }

        private void ProcessOutput(string output)
        {
            var eArgs = new OutputReceivedEventArgs()
            {
                Data = output,
            };
            OutputReceived.Invoke(ServerId, eArgs);
        }

        private void ProcessError(string error)
        {
            var eArgs = new OutputReceivedEventArgs()
            {
                Data = error,
            };
            ErrorReceived.Invoke(ServerId, eArgs);
        }

        private void OnProcessStatistics(object sender, ContainerStatsResponse response)
        {
            try
            {
                var updateTimeStamp = DateTime.UtcNow;
                if (!_lastStatusUpdateTimeStamp.HasValue || updateTimeStamp - _lastStatusUpdateTimeStamp >= TimeSpan.FromSeconds(1))
                {
                    if (!_lastStatusUpdateTimeStamp.HasValue)
                    {
                        CpuData = new CpuData();
                        RamData = new RamData(Convert.ToInt32(response.MemoryStats.Limit / 1000000));
                    }

                    _lastStatusUpdateTimeStamp = updateTimeStamp;

                    var cpuDelta = response.CPUStats.CPUUsage.TotalUsage - response.PreCPUStats.CPUUsage.TotalUsage;
                    var systemCpuDelta = response.CPUStats.SystemUsage - response.PreCPUStats.SystemUsage;
                    var numberCpus = response.CPUStats.CPUUsage.PercpuUsage.Count;
                    var containerCpuUsage = Convert.ToInt32(((double)cpuDelta / (double)systemCpuDelta) * (double)numberCpus * 100.0);
                    var cpuUsage = containerCpuUsage > 100 ? 100 : containerCpuUsage;
                    var cpuUsageString = CpuData.AddDataAndGetString(cpuUsage);

                    var usedRam = Convert.ToInt32((response.MemoryStats.Usage - response.MemoryStats.Stats["cache"]) / 1000000);
                    RamData.MaxRamMB = Convert.ToInt32(response.MemoryStats.Limit / 1000000);
                    var ramUsageString = RamData.AddDataAndGetString(usedRam);

                    var update = new StatusUpdate()
                    {
                        CpuUsagePercent = cpuUsage,
                        CpuUsageString = cpuUsageString,
                        RamUsageMB = usedRam,
                        RamUsageString = ramUsageString,
                    };

                    StatusUpdated.Invoke(ServerId, new StatusUpdatedEventArgs()
                    {
                        StatusUpdate = update,
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task SendInput(string msg)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(msg);
            await _stream.WriteAsync(bytes, 0, bytes.Length, Token).ConfigureAwait(false);
            //await _writer.WriteLineAsync(msg);
            //await _writer.FlushAsync();
        }

        public void Dispose()
        {
            _tokenSource.Cancel();
            _stdout.Dispose();
            _stderr.Dispose();
            StatProgress.ProgressChanged -= OnProcessStatistics;
            _stream.Dispose();
        }
    }
}
