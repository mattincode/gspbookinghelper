using System;

namespace gspbookinghelper {
    public class Booking
    {
        public string EmployeeNumber { get; set; }
        public int BookingId { get; set; }    
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int EmployeeId { get; set; }    
   }

}