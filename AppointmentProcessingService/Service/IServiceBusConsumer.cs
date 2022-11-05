using AppointmentProcessingService.Models;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PatientAppointment.Domain;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentProcessingService.Service
{
    public interface IServiceBusConsumer
    {
        Task RegisterOnMessageHandlerAndReceiveMessages();
        Task CloseQueueAsync();
        ValueTask DisposeAsync();
    }
    public class ServiceBusConsumer : IServiceBusConsumer
    {
        private readonly IConfiguration _configuration;
        private readonly ServiceBusClient _client;
        private const string QUEUE_NAME = "testqueue";
        private readonly ILogger _logger;
        private ServiceBusProcessor _processor;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ServiceBusConsumer(
            IConfiguration configuration,
            ILogger<ServiceBusConsumer> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _configuration = configuration;
            _logger = logger;

            var connectionString = _configuration.GetConnectionString("AzureServiceBus");
            _client = new ServiceBusClient(connectionString);
            _serviceScopeFactory= serviceScopeFactory;
        }

        public async Task RegisterOnMessageHandlerAndReceiveMessages()
        {
            ServiceBusProcessorOptions _serviceBusProcessorOptions = new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 1,
                AutoCompleteMessages = false,
            };

            _processor = _client.CreateProcessor(QUEUE_NAME, _serviceBusProcessorOptions);
            _processor.ProcessMessageAsync += ProcessMessagesAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;
            await _processor.StartProcessingAsync().ConfigureAwait(false);
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            _logger.LogError(arg.Exception, "Message handler encountered an exception");
            _logger.LogDebug($"- ErrorSource: {arg.ErrorSource}");
            _logger.LogDebug($"- Entity Path: {arg.EntityPath}");
            _logger.LogDebug($"- FullyQualifiedNamespace: {arg.FullyQualifiedNamespace}");

            return Task.CompletedTask;
        }

        private async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
        {

            string jsonString = Encoding.UTF8.GetString(args.Message.Body);

            dynamic deserializedObj = JsonConvert.DeserializeObject<Patient>(jsonString);

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _applicationDbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                var patient = new Patient();
                patient.Address = deserializedObj.Address;
                patient.Age = deserializedObj.Age;
                patient.Name = deserializedObj.Name;

                _applicationDbContext.Patients.Add(patient);
                _applicationDbContext.SaveChanges();
            }

            await args.CompleteMessageAsync(args.Message).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            if (_processor != null)
            {
                await _processor.DisposeAsync().ConfigureAwait(false);
            }

            if (_client != null)
            {
                await _client.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task CloseQueueAsync()
        {
            await _processor.CloseAsync().ConfigureAwait(false);
        }
    }
}
