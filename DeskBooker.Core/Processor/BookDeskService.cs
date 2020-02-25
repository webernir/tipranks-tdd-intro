using DeskBooker.Core.DataInterface;
using DeskBooker.Core.Domain;
using System;
using System.Linq;

namespace DeskBooker.Core.Processor
{
    public class BookDeskService
    {
        private readonly IDeskBookingRepository _repo;
        private readonly IDeskRepository _deskRepository;

        public BookDeskService(IDeskBookingRepository repo, IDeskRepository deskRepository)
        {
            _repo = repo;
            _deskRepository = deskRepository;
        }

        public BookDeskResult BookDesk(BookDeskRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var isValid = IsValidRequest(request);

            var avaiableDesks = _deskRepository.GetAvailableDesks(request.Date);

            var result = Create<BookDeskResult>(request);


            if (avaiableDesks.FirstOrDefault() is Desk availableDesk)
            {
                var bookDesk = Create<BookDesk>(request);
                bookDesk.DeskId = availableDesk.Id;

                var bookingId = _repo.Save(bookDesk);

                result.ResultCode = DeskBookingResultCode.Success;
                result.BookingId = bookingId;
            }
            else
            {
                result.ResultCode = DeskBookingResultCode.NotAvailableDesks;
            }

            return result;
        }

        private bool IsValidRequest(BookDeskRequest request) => request.Email.Contains("@tipranks.com");

        private T Create<T>(BookDeskRequest request) where T : BookDeskAbstract, new()
        {
            return new T
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Date = request.Date
            };
        }
    }
}