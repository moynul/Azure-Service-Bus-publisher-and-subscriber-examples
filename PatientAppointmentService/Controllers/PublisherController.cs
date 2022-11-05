
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PatientAppointment.Domain;
using PatientAppointmentService.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientAppointmentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublisherController : ControllerBase
    {
        private IQueueService _queue;
        private readonly IConfiguration _config;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusClient _serviceBusSessionClient;
        public PublisherController(IQueueService Queue, IConfiguration config)
        {
            _queue = Queue;
            _config = config;
            _serviceBusClient = new ServiceBusClient(_config.GetConnectionString("AzureServiceBus"));
            _serviceBusSessionClient = new ServiceBusClient(_config.GetConnectionString("SessionRequestQueueLink"));
        }
        [HttpPost]
        [ProducesResponseType(typeof(Patient), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Patient), StatusCodes.Status409Conflict)]
        [Route("CreateAppointment")]
        public async Task<IActionResult> CreateAppointment(Patient patient)
        {
            var QueueName = _config.GetConnectionString("QueueName");
            var sender = _serviceBusClient.CreateSender(QueueName);
            var message = new ServiceBusMessage(new BinaryData(System.Text.Json.JsonSerializer.Serialize(patient)));
            await sender.SendMessageAsync(message);
            //await _queue.SendMessageAsync(patient, QueueName);
            return Ok();
        }

        [HttpPost]
        [ProducesResponseType(typeof(Patient), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Patient), StatusCodes.Status409Conflict)]
        [Route("GetPatientDetails")]
        public async Task<IActionResult> GetPatientDetails(string patientId)
        {
            var QueueName = _config.GetConnectionString("SessionRequestQueue");
            var sender = _serviceBusSessionClient.CreateSender(QueueName);

            var message = new ServiceBusMessage(new BinaryData(System.Text.Json.JsonSerializer.Serialize(patientId)));
            var sessionId = Guid.NewGuid().ToString();
            message.SessionId = sessionId;

            await sender.SendMessageAsync(message);
            var replyMsg = await ReceiveReplyMessage(sessionId);

            //await _queue.SendMessageAsync(patient, QueueName);
            return Ok(replyMsg);
        }

        private async Task<string> ReceiveReplyMessage(string sessionId)
        {
            try
            {
                var QueueName = _config.GetConnectionString("SessionResponseQueue");
                var receiver = await _serviceBusSessionClient.AcceptSessionAsync(QueueName, sessionId);
                var replyMsg = await receiver.ReceiveMessageAsync();
                if (replyMsg != null)
                {
                    string jsonString = Encoding.UTF8.GetString(replyMsg.Body);

                   // dynamic deserializedObj = JsonConvert.DeserializeObject<Patient>(jsonString);

                    return jsonString;
                }
                else
                {
                    throw new Exception("Failed to get reply from server");
                }
            }
            catch (Exception ex)
            {
                var x = ex.Message.ToString();
                return ex.Message.ToString();
            }
        }
    }
}
