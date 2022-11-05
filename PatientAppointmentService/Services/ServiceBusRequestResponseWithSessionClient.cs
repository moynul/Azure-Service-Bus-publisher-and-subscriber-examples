using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

namespace PatientAppointmentService.Services
{
    public interface IServiceBusRequestResponse
    {
        Task<TResponse> SendAsync<TResponse>(string queue, object @object) where TResponse : class;
    }

    public class ServiceBusRequestResponseWithSessionClient : IServiceBusRequestResponse
    {
        private readonly ILogger<ServiceBusRequestResponseWithSessionClient> _logger;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IConfiguration _configuration;
        public ServiceBusRequestResponseWithSessionClient(ILogger<ServiceBusRequestResponseWithSessionClient> logger, ServiceBusClient serviceBusClient, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            var connectionString = _configuration.GetConnectionString("AzureServiceBus");
            _serviceBusClient = new ServiceBusClient(connectionString);

        }

        public async Task<TResponse> SendAsync<TResponse>(string queue, object @object) where TResponse : class
        {
            var replyMsg = "";

            var sender = _serviceBusClient.CreateSender(queue);

            var sessionId = Guid.NewGuid().ToString();
            await SendRequestMessage("Hello from client", sessionId);

            replyMsg = await ReceiveReplyMessage(sessionId);

            return null;
        }
        public async Task SendRequestMessage(string msg, string sessionId)
        {
            var sender = _serviceBusClient.CreateSender("testrequest");
            var requestMsg = new ServiceBusMessage(msg);
            requestMsg.SessionId = sessionId;
            await sender.SendMessageAsync(requestMsg);
        }

        public async Task<string> ReceiveReplyMessage(string sessionId)
        {
            var receiver = await _serviceBusClient.AcceptSessionAsync("testresponse", sessionId);
            var replyMsg = await receiver.ReceiveMessageAsync();
            if (replyMsg != null)
            {
                return replyMsg.Body.ToString();
            }
            else
            {
                throw new Exception("Failed to get reply from server");
            }
        }
    }
}
