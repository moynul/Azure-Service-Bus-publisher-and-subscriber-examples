using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AppointmentProcessingService.Service
{
    public class ServiceBusWorkerService : IHostedService, IDisposable
    {
        private readonly ILogger<ServiceBusWorkerService> _logger;
        private readonly IServiceBusConsumer _serviceBusConsumer;

        public ServiceBusWorkerService(ILogger<ServiceBusWorkerService> logger,
                                       IServiceBusConsumer serviceBusConsumer)
        {
            _logger = logger;
            _serviceBusConsumer = serviceBusConsumer;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Starting the service bus queue consumer and the subscription");
            await _serviceBusConsumer.RegisterOnMessageHandlerAndReceiveMessages().ConfigureAwait(false);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Stopping the service bus queue consumer and the subscription");
            await _serviceBusConsumer.CloseQueueAsync().ConfigureAwait(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual async void Dispose(bool disposing)
        {
            if (disposing)
            {
                await _serviceBusConsumer.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
