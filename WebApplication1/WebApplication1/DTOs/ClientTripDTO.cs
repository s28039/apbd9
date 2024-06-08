using System;

namespace WebApp9.DTOs
{
    public class ClientTripDTO
    {
        public int IdClient { get; set; }
        public int IdTrip { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}