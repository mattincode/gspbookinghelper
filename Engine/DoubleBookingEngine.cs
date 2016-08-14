using System;
using System.Collections.Generic;
using System.Linq;

namespace gspbookinghelper
{
    public enum ScheduleType
    {
        Planned
    }
    public class DoubleBookingEngine
    {
        IList<Booking> _bookings = new List<Booking>();
        public DoubleBookingEngine(IList<Booking> bookings)
        {
            _bookings = bookings;
        }

        // * List update history:
        // - DateTime
        // - Bookings in the transaction
        // - Who created the bookings (get SystemUser name and domainname after click on user)
        // - If a booking that was created in the transaction overlaps any earlier booking, set DoubleBookingOnBookingId                  
        public IList<Transaction> GetDoubleBookingHistory()
        {
            var bookingHistory = new List<Transaction>();
            // Create a lookuptable where all bookings are grouped by changetrace.
            var bookingsByChangeTraceId = _bookings.OrderBy(t => t.CreatedOn).GroupBy(b => b.ChangeTraceId).ToList();
            // Group bookings by transaction and order from oldest to newest transaction
            var bookingsGroupedByTrans = _bookings.GroupBy(b => b.CreatedOn).OrderBy(c => c.Key);            
            foreach (var bookingTransaction in bookingsGroupedByTrans)
            {
                var firstBooking = bookingTransaction.First();
                var newTransaction = new Transaction(){TransactionDate = firstBooking.CreatedOn, 
                                                        IsCreatedBySchedulingData = (firstBooking.CreatedBySchedulingDataId != null), 
                                                        CreatedById = firstBooking.CreatedBy,
                                                        Bookings = bookingTransaction.ToList()}; 

                
                // TODO: Group bookings before transaction date on changetraceid
                // TODO: Handle deleted bookings
                // TODO: Handle scheduled/non-scheduled
                // TODO: Write testcases      
                // TODO: Get locked workschedule versions                          

                // Now. Let's iterate every booking in the added transaction and check if there is overlap with any bookings in the previous transactions
                var earlierTransactions = bookingHistory.Where(t => t.TransactionDate != firstBooking.CreatedOn);                
                foreach (var booking in newTransaction.Bookings)
                {   
                    // A deleted booking can never overlap                 
                    if (booking.IsDeleted) continue;

                    // TODO: Get all overlapping bookings

                    foreach (var earlierTransaction in earlierTransactions)
                    {
                        foreach (var earlierBooking in earlierTransaction.Bookings)
                        {
                            bool overlap = false;

                            // Check overlap
                            overlap = booking.HasOverlap(earlierBooking);
                            // Special case, handle absencebookings with percentage < 100&
                            if ((booking.AbsenceTypeId != null && booking.AbsencePercentage != null) ||
                                (earlierBooking.AbsenceTypeId != null && earlierBooking.AbsencePercentage != null))
                            {   
                                // Allow overlap
                                continue;
                            }
                            if (overlap)
                            {
                                booking.DoubleBookingOnBookingId = earlierBooking.BookingId;
                                newTransaction.HasDoubleBookings = true;
                            }
                        }
                    }
                }

                bookingHistory.Add(newTransaction);
                                   
            }

            //TODO: Get CreatedBy name

            return bookingHistory;
        }
    }

    public class Transaction
    {
        public DateTime TransactionDate { get; set; }
        public bool IsCreatedBySchedulingData { get; set; }
        public int CreatedById { get; set; }
        public string CreatedBy { get; set; }
        public IList<Booking> Bookings { get; set; }
        public bool HasDoubleBookings { get; set; }

    }
}