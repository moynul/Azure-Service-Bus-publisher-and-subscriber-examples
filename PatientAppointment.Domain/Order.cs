using System;
using System.Collections.Generic;
using System.Text;

namespace PatientAppointment.Domain
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public string PublicOrderId { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
