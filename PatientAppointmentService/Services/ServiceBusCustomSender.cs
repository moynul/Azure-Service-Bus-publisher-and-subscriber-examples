using Azure.Messaging.ServiceBus;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using PatientAppointment.Domain;
using System;
using System.Text;
using System.Threading.Tasks;

namespace PatientAppointmentService.Services
{
    public class ServiceBusCustomSender : IServiceBusCustomSender
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IConfiguration _config;

        public ServiceBusCustomSender(
            ServiceBusClient client,
            IConfiguration config)
        {
            _serviceBusClient = new ServiceBusClient(_config.GetConnectionString("AzureServiceBus"));
        }

        public async Task SendMessage<T>(T payload)
        {
            var sender = _serviceBusClient.CreateSender(_config.GetConnectionString("QueueName"));
            var message = new ServiceBusMessage(new BinaryData(System.Text.Json.JsonSerializer.Serialize(payload)));
            await sender.SendMessageAsync(message);
        }
    }
}
