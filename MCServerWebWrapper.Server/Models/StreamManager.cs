﻿using Docker.DotNet;
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
        public MultiplexedStream Stream { get; private set; }
        public Output LastOutput { get; private set; }
        public bool IsRunning { get; private set; }

        public event EventHandler<OutputReceivedEventArgs> OutputReceived;
        public event EventHandler<OutputReceivedEventArgs> ErrorReceived;
        public event EventHandler ServerStarted;
        public event EventHandler ServerStopped;

        private readonly CancellationTokenSource TokenSource = new CancellationTokenSource();

        public StreamManager(string serverId, string containerId, MultiplexedStream stream)
        {
            ServerId = serverId;
            ContainerId = containerId;
            Stream = stream;
            Task.Run(ReadOutput);
        }

        private string _stdout;


        private string stdout
        {
            get { return _stdout; }
            set 
            {
                _stdout = value;
                ProcessOutput(_stdout);
            }
        }

        private string _stderr;

        private string stderr
        {
            get { return _stderr; }
            set 
            { 
                _stderr = value;
                ProcessError(_stderr);
            }
        }

        private async Task ReadOutput()
        {
            (stdout, stderr) = await Stream.ReadOutputToEndAsync(TokenSource.Token);
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

        public void Dispose()
        {
            TokenSource.Cancel();
            Stream.Dispose();
        }
    }
}