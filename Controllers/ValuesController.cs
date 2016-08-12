using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace gspbookinghelper.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<Transaction> Get()
        {
            // *** Tool for finding double bookings by simple overlap-testing... running bookingengine can be done in a second iteration ***
            // Basic input (first page/settings)
            // - databaseserver
            // - databasename

            // Search bookings (second page)
            // Employeenumber
            // Start, End-date to look in.
            string employeeNumber = "28635";
            DateTime periodStart = new DateTime(2016,1,1);
            DateTime periodEnd = new DateTime(2016,1,31);
            IList<Booking> bookings = new List<Booking>();

            // Test dapper:
            string connection = "Data Source=securitassql;Initial Catalog=GSPSERef160613;Integrated Security=True";
            using (IDbConnection db = new SqlConnection(connection))
            {                    
                var stringFormatter = new StringBuilder();
                stringFormatter.Append($"Select e.EmployeeNumber, e.FirstName, e.LastName, pc.ProfitCenterNumber, p.PlanningUnitNumber, b.*, DoubleBookingOnBookingId = 0 from Booking b");
                stringFormatter.Append($" JOIN Employee e on b.EmployeeId = e.EmployeeId");
                stringFormatter.Append($" LEFT JOIN PlanningUnit p on b.PlanningUnitId = p.PlanningUnitId");
                stringFormatter.Append($" LEFT JOIN ProfitCenter pc on p.ProfitCenterId = pc.ProfitCenterId");
                stringFormatter.Append($" WHERE e.EmployeeNumber = '{employeeNumber}'");
                stringFormatter.Append($" AND ((b.StartDate >= '{periodStart}' AND b.StartDate < '{periodEnd}')"); // Get all bookings that starts in the period
                stringFormatter.Append($"   OR (b.EndDate > '{periodStart}' AND b.EndDate <= '{periodEnd}')");  // Get all bookings that ends in the period
                stringFormatter.Append($"   OR (b.StartDate < '{periodStart}' AND b.EndDate > '{periodEnd}'))"); // Get all bookings that overlaps the entire period                
                bookings = db.Query<Booking>(stringFormatter.ToString()).ToList();
            }
            
            var engine = new DoubleBookingEngine(bookings);
            return engine.GetDoubleBookingHistory();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
