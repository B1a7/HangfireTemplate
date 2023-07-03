using Hangfire;
using HangfireTemplate.Models;
using HangfireTemplate.Services;
using Microsoft.AspNetCore.Mvc;

namespace HangfireTemplate.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DriversController : ControllerBase
    {
        private static List<Driver> _drivers = new List<Driver>();

        private readonly ILogger<DriversController> _logger;

        public DriversController(ILogger<DriversController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public ActionResult AddDriver(Driver driver)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            _drivers.Add(driver);

            //HANGFIRE

            var jobId = BackgroundJob.Enqueue<IServiceManagement>(x => x.SendEmail());

            var parentId = BackgroundJob.Schedule<IServiceManagement>(x => x.SendEmail(), TimeSpan.FromSeconds(30));

            BackgroundJob.ContinueJobWith<IServiceManagement>(parentId, x => x.SyncData());
            //HANGFIRE

            return Ok(driver);           
        }

        [HttpGet]
        public ActionResult GetDrivers(Guid Id)
        {
            var driver = _drivers.FirstOrDefault(x => x.Id == Id);

            if (driver is null)
                return NotFound();
             
            return Ok(driver);
        }

        [HttpDelete]
        public ActionResult DeleteDrivers(Guid Id)
        {
            var driver = _drivers.FirstOrDefault(y => y.Id == Id);

            if (driver is null)
                return NotFound();

            _drivers.Remove(driver);

            RecurringJob.AddOrUpdate<IServiceManagement>(x => x.UpdateDatabase(), Cron.Hourly);


            return NoContent();
        }
    }
}
