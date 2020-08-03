using Docker.DotNet;
using Docker.DotNet.Models;
using MCServerWebWrapper.Server.Data.Models;
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
        public CancellationToken Token { get; }
        public Progress<ContainerStatsResponse> StatProgress { get; }

        public event EventHandler<OutputReceivedEventArgs> OutputReceived;
        public event EventHandler<OutputReceivedEventArgs> ErrorReceived;
        public event EventHandler<StatusUpdatedEventArgs> StatUpdateReceived;
        

        private readonly CancellationTokenSource _tokenSource;
        private readonly MultiplexedStream _stream;
        private readonly Stream _stdin;
        private readonly Stream _stdout;
        private readonly Stream _stderr;

        public StreamManager(string serverId, string containerId, MultiplexedStream stream, CancellationTokenSource tokenSource = null)
        {
            ServerId = serverId;
            ContainerId = containerId;
            _stream = stream;
            _tokenSource = tokenSource != null ? tokenSource : new CancellationTokenSource();
            _stdin = new MemoryStream();
            _stdout = new MemoryStream();
            _stderr = new MemoryStream();
            StatProgress = new Progress<ContainerStatsResponse>();
            StatProgress.ProgressChanged += OnProcessStatistics;
            InitializeState();
        }

        public async void InitializeState()
        {
            Task.Run(async () => await _stream.CopyOutputToAsync(_stdin, _stdout, _stderr, _tokenSource.Token));
            Task.Run(async () => await ReadOutput(_tokenSource.Token));
            Task.Run(async () => await ReadError(_tokenSource.Token));
        }

        private async Task ReadOutput(CancellationToken token)
        {
            using (StreamReader reader = new StreamReader(_stdout))
            {
                long lastPosition = 0;
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        if (_stdout.Position == _stdout.Length)
                        {
                            _stdout.Position = lastPosition;
                        }
                        var output = await reader.ReadLineAsync();
                        if (!string.IsNullOrWhiteSpace(output))
                        {
                            lastPosition = _stdout.Position;
                            ProcessOutput(output);
                        }
                        await Task.Delay(100);
                    }
                    catch { }
                }
            }
        }

        private async Task ReadError(CancellationToken token)
        {
            using (StreamReader reader = new StreamReader(_stderr))
            {
                long lastPosition = 0;
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        if (_stderr.Position == _stderr.Length)
                        {
                            _stderr.Position = lastPosition;
                        }
                        var error = await reader.ReadLineAsync();
                        if (!string.IsNullOrWhiteSpace(error))
                        {
                            lastPosition = _stderr.Position;
                            ProcessError(error);
                        }
                        await Task.Delay(100);
                    }
                    catch { }
                }
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

        }

        public Task SendInput(string msg)
        {
            using (StreamWriter writer = new StreamWriter(_stdin))
            {
                writer.WriteLine(msg);
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _tokenSource.Cancel();
            _stream.Dispose();
        }
    }
}
