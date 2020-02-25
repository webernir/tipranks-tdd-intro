namespace DeskBooker.Core.Domain
{
    public class BookDeskResult : BookDeskAbstract
    {
        public DeskBookingResultCode ResultCode { get; set; }
        public object BookingId { get; set; }
    }
}