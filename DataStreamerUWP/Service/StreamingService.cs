﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

using Microsoft.DataStreamer.UWP;

namespace DataStreamer.App.Core
{
    public abstract class StreamingService : IStreamingService
    {
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private readonly IStreamingRepository _repository;
        private volatile int _delay = 20;

        public StreamingService(IStreamingRepository repository, int delay = 20)
        {
            _repository = repository;
            _delay = delay;
        }

        public virtual async Task<string> Connect()
        {
            return await Task.FromResult(JsonConvert.SerializeObject(_repository.Manifest));
        }

        public IStreamingRepository Repository => _repository;

        public virtual async Task Disconnect()
        {
            await StopData();
            await Task.CompletedTask;
        }

        public int Delay { get { return _delay; } set {_delay = value; } }

        public virtual async Task StartData()
        {
            _cancellationToken = new CancellationTokenSource();

            Task.Run( async ()=>
            {
                while(!_cancellationToken.Token.IsCancellationRequested)
                {
                    await SendData(await _repository.GetData());

                    await Task.Delay(_delay, _cancellationToken.Token);
                }

                _cancellationToken?.Dispose();
                _cancellationToken = null;
            });

            await Task.CompletedTask;
        }

        public virtual async Task StopData()
        {
            _cancellationToken?.Cancel();
            await Task.CompletedTask;
        }

        protected abstract Task SendData(string data);

        public abstract Task StartRecording(string fileName);
        public abstract Task StopRecording();
        public abstract Task Reset();
        public abstract Task Ready();
        public abstract Task NotReady();
        public abstract Task UpdateManifest();
    }
}
