using System.Threading.Tasks;

namespace PatientAppointmentService.Services
{
    public interface IServiceBusCustomSender
    {
        Task SendMessage<T>(T payload);
    }
}
