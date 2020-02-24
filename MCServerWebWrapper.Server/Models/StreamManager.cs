using Docker.DotNet;
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
        public bool IsRunning { get; private set; }

        public event EventHandler<OutputReceivedEventArgs> OutputReceived;
        public event EventHandler<OutputReceivedEventArgs> ErrorReceived;
        public event EventHandler ServerStarted;
        public event EventHandler ServerStopped;

        private readonly CancellationTokenSource TokenSource;
        private readonly MultiplexedStream _stream;
        private readonly Stream _stdin;
        private readonly Stream _stdout;
        private readonly Stream _stderr;

        public StreamManager(string serverId, string containerId, MultiplexedStream stream)
        {
            ServerId = serverId;
            ContainerId = containerId;
            _stream = stream;
            _stdin = new MemoryStream();
            _stdout = new MemoryStream();
            _stderr = new MemoryStream();
            TokenSource = new CancellationTokenSource();
            Task.Run(async () => await _stream.CopyOutputToAsync(_stdin, _stdout, _stderr, TokenSource.Token));
            Task.Run(async () => await ReadOutput(TokenSource.Token));
            Task.Run(async () => await ReadError(TokenSource.Token));
        }

        private async Task ReadOutput(CancellationToken token)
        {
            using (StreamReader reader = new StreamReader(_stdout))
            {
                while (!reader.EndOfStream && !token.IsCancellationRequested)
                {
                    var output = await reader.ReadLineAsync();
                    ProcessOutput(output);
                }
            }
        }

        private async Task ReadError(CancellationToken token)
        {
            using (StreamReader reader = new StreamReader(_stderr))
            {
                while (!reader.EndOfStream && !token.IsCancellationRequested)
                {
                    var error = await reader.ReadLineAsync();
                    ProcessError(error);
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

            if (!IsRunning)
            {
                var result = Regex.IsMatch(output, @".*\[[:0-9]{8}\] \[Server thread\/INFO\]: Done \([s.0-9]{6,8}\)! For help, type ""help"".*");
                if (result)
                {
                    IsRunning = true;
                    ServerStarted.Invoke(this, new EventArgs());
                }
            }
        }

        private void ProcessError(string error)
        {
            var eArgs = new OutputReceivedEventArgs()
            {
                Data = error,
            };
            ErrorReceived.Invoke(ServerId, eArgs);
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
            TokenSource.Cancel();
            _stream.Dispose();
        }
    }
}
