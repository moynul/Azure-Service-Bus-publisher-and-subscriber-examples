using System.Threading.Tasks;

namespace PatientAppointmentService.Services
{
    public interface IQueueService
    {
        Task SendMessageAsync<T>(T serviceBusMessage, string queueName);
    }
}
