using AutoFixture;
using Moq;
using TechShelf.Application.Features.Orders.Handlers;
using TechShelf.Application.Interfaces.Services;
using TechShelf.Domain.Events;

namespace TechShelf.UnitTests.Application.Features.Orders.Handlers;

public class OrderPaymentConfirmedDomainEventHandlerTests
{
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly OrderPaymentConfirmedDomainEventHandler _sut;
    private readonly IFixture _fixture;

    public OrderPaymentConfirmedDomainEventHandlerTests()
    {
        _fixture = new Fixture();
        _emailServiceMock = new Mock<IEmailService>();
        _sut = new OrderPaymentConfirmedDomainEventHandler(_emailServiceMock.Object);
    }

    [Fact]
    public async Task Handle_SendsEmail_WhenEventIsReceived()
    {
        // Arrange
        var orderId = _fixture.Create<Guid>();
        var customerEmail = _fixture.Create<string>();
        var notification = new OrderPaymentConfirmedDomainEvent(orderId, customerEmail);

        // Act
        await _sut.Handle(notification, CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(
            x => x.SendPlainTextEmailAsync(
                customerEmail,
                "Order Payment Confirmation",
                It.Is<string>(msg =>
                    msg.Contains(orderId.ToString()) &&
                    msg.Contains("payment") &&
                    msg.Contains("confirmed")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
