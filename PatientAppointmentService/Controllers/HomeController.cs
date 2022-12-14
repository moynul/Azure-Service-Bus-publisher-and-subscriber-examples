using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PatientAppointment.Domain;
using PatientAppointmentService.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PatientAppointmentService.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //[HttpPost()]
        //public async Task<IActionResult> NewOrder()
        //{
        //    var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("sb://servicebusqueuesnetcore.servicebus.windows.net/new-orders"));

        //    await sendEndpoint.Send(
        //                                new Order
        //                                {
        //                                    OrderId = Guid.NewGuid(),
        //                                    Timestamp = DateTime.UtcNow,
        //                                    PublicOrderId = _random.Next(1, 999).ToString()
        //                                });

        //    return Ok();
        //}
    }
}
