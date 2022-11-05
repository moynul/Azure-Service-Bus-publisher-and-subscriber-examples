using AppointmentProcessingService.Models;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PatientAppointment.Domain;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AppointmentProcessingService.Service
{
    public class SessionQueueBackgroundService : BackgroundService
    {
        private readonly ILogger<SessionQueueBackgroundService> logger;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public SessionQueueBackgroundService(ILogger<SessionQueueBackgroundService> logger, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            this.logger = logger;
            _configuration = configuration;
            var connectionString = _configuration.GetConnectionString("SessionRequestQueueLink");
            _serviceBusClient = new ServiceBusClient(connectionString);
            _serviceScopeFactory = serviceScopeFactory; 
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var options = new ServiceBusSessionProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentSessions = 100,
                PrefetchCount = 100
            };
            var QueueName = _configuration.GetConnectionString("SessionRequestQueue");
            var processor = _serviceBusClient.CreateSessionProcessor(QueueName, options);
            processor.ProcessMessageAsync += Processor_ProcessMessageAsync;
            processor.ProcessErrorAsync += Processor_ProcessErrorAsync;

            await processor.StartProcessingAsync();

        }
        private static Task Processor_ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            return Task.CompletedTask;
        }

        private async Task Processor_ProcessMessageAsync(ProcessSessionMessageEventArgs arg)
        {

            var z = arg.Message.Body;

            var body = Regex.Match(arg.Message.Body.ToString(), @"\d+").Value; arg.Message.Body.ToString();

            var Id = System.Convert.ToInt32(body);

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _applicationDbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                var Patient = _applicationDbContext.Patients.Where(c => c.Id == Id);

                await SendReplyMessage(Patient, arg.SessionId);
            }
            await arg.CompleteMessageAsync(arg.Message);
        }

        private async Task SendReplyMessage<T>(T msg, string sessionId)
        {
            try
            {
                var QueueName = _configuration.GetConnectionString("SessionResponseQueue");
                var sender = _serviceBusClient.CreateSender(QueueName);
                var objectser = System.Text.Json.JsonSerializer.Serialize(msg);

                var requestMsg = new ServiceBusMessage(new BinaryData(objectser));
                requestMsg.SessionId = sessionId;
                await sender.SendMessageAsync(requestMsg);
            }
            catch (Exception Ex)
            {
                var x = Ex.Message.ToString();
               // return Ex.Message.ToString();
            }
        }


    }
}
