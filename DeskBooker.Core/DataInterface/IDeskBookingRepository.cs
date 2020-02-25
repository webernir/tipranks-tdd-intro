using DeskBooker.Core.Domain;

namespace DeskBooker.Core.DataInterface
{
    public interface IDeskBookingRepository
    {
        int? Save(BookDesk item);
    }
}