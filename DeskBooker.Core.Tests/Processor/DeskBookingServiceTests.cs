using DeskBooker.Core.DataInterface;
using DeskBooker.Core.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace DeskBooker.Core.Processor
{
    [TestClass]
    public class DeskBookingServiceTests
    {
        BookDeskService _processor;
        BookDeskRequest _request;
        Mock<IDeskBookingRepository> _deskBookingRepositoryMock;
        List<Desk> _availableDesks;
        Mock<IDeskRepository> _deskRepositoryMock;

        [TestInitialize]
        public void Init()
        {
            _request = new BookDeskRequest
            {
                FirstName = "Nir",
                LastName = "Weber",
                Email = "webernir@tipranks.com",
                Date = new DateTime(2020, 02, 14)
            };

            _deskBookingRepositoryMock = new Mock<IDeskBookingRepository>();

            _availableDesks = new List<Desk> { new Desk { Id = 7 } };

            _deskRepositoryMock = new Mock<IDeskRepository>();
            _deskRepositoryMock
                .Setup(t => t.GetAvailableDesks(It.IsAny<DateTime>()))
                .Returns(_availableDesks);

            _processor = new BookDeskService(
                _deskBookingRepositoryMock.Object,
                _deskRepositoryMock.Object);
        }

        [TestMethod]
        public void BookDesk_WhenSimpleRequest_ReturnResult()
        {
            // Act
            var result = _processor.BookDesk(_request);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(_request.FirstName, result.FirstName);
            Assert.AreEqual(_request.LastName, result.LastName);
            Assert.AreEqual(_request.Email, result.Email);
            Assert.AreEqual(_request.Date, result.Date);

        }

        [TestMethod]
        public void BookDesk_WhenNullPassed_ThrowArgumentNullException()
        {
            var ex = Assert.ThrowsException<ArgumentNullException>(
                () => _processor.BookDesk(null));

            // Assert
            Assert.AreEqual("request", ex.ParamName);
        }

        [TestMethod]
        public void BookDesk_WhenValidRequestAndAvailableDesks_SaveToDb()
        {
            // Arrange
            BookDesk bookDeskResult = null;

            _deskBookingRepositoryMock.Setup(t => t.Save(It.IsAny<BookDesk>()))
                .Callback<BookDesk>(bookDesk =>
                {
                    bookDeskResult = bookDesk;
                });

            // Act
            _processor.BookDesk(_request);

            // Assert
            _deskBookingRepositoryMock.Verify(
                t => t.Save(It.IsAny<BookDesk>()), Times.Once);

            Assert.AreEqual(bookDeskResult.FirstName, _request.FirstName);
        }

        [DataTestMethod]
        [DataRow(true, 1)]
        [DataRow(false, 0)]
        public void BookDesk_OnAvailablity_SaveOrNot(bool isAvailable, int count)
        {
            if (!isAvailable)
            {
                _availableDesks.Clear();
            }
            // Act
            _processor.BookDesk(_request);

            // Assert
            _deskBookingRepositoryMock
                .Verify(t => t.Save(It.IsAny<BookDesk>()), Times.Exactly(count));

        }

        [TestMethod]
        public void BookDesk_WhenSave_StoreDeskId()
        {
            BookDesk saved = null;

            _deskBookingRepositoryMock.Setup(t => t.Save(It.IsAny<BookDesk>())).Callback<BookDesk>(bookDesk =>
            {
                saved = bookDesk;
            });

            var result = _processor.BookDesk(_request);

            Assert.AreEqual(7, saved.DeskId);
        }

        [DataTestMethod]
        [DataRow(true, DeskBookingResultCode.Success)]
        [DataRow(false, DeskBookingResultCode.NotAvailableDesks)]
        public void BookDesk_WhenSaving_ShouldReturnResultCode(bool isAvailable, DeskBookingResultCode resultCode)
        {

            if (!isAvailable)
            {
                _availableDesks.Clear();
            }

            var result = _processor.BookDesk(_request);

            Assert.AreEqual(resultCode, result.ResultCode);
        }

        [DataTestMethod]
        [DataRow(true, 42)]
        [DataRow(false, null)]
        public void BookDesk_WhenSaving_ResultShouldHaveBookingId(bool isAvailable, int? savedId)
        {
            if (!isAvailable)
            {
                _availableDesks.Clear();
            }

            _deskBookingRepositoryMock.Setup(t => t.Save(It.IsAny<BookDesk>())).Returns(savedId);

            var result = _processor.BookDesk(_request);

            Assert.AreEqual(savedId, result.BookingId);

        }

    }
}
