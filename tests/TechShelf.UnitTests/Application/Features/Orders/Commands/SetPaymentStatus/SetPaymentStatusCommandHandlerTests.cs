using AutoFixture;
using FluentAssertions;
using MediatR;
using Moq;
using TechShelf.Application.Features.Orders.Commands.SetPaymentStatus;
using TechShelf.Application.Interfaces.Data;
using TechShelf.Domain.Orders;
using TechShelf.Domain.Orders.Enums;
using TechShelf.Domain.Orders.Specs;

namespace TechShelf.UnitTests.Application.Features.Orders.Commands.SetPaymentStatus;

public class SetPaymentStatusCommandHandlerTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<Order>> _orderRepositoryMock;
    private readonly SetPaymentStatusCommandHandler _handler;

    public SetPaymentStatusCommandHandlerTests()
    {
        _fixture = new Fixture();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _orderRepositoryMock = new Mock<IRepository<Order>>();

        _unitOfWorkMock
            .Setup(x => x.Repository<Order>())
            .Returns(_orderRepositoryMock.Object);

        _handler = new SetPaymentStatusCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenOrderExists()
    {
        // Arrange
        var command = _fixture.Create<SetPaymentStatusCommand>();
        var order = _fixture.Create<Order>();

        _orderRepositoryMock
            .Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<OrderByIdSpec>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Unit.Value);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_UpdatesOrderPaymentStatus_WhenOrderExists()
    {
        // Arrange
        var command = _fixture.Build<SetPaymentStatusCommand>()
            .With(c => c.IsPaymentSuccessful, true)
            .Create();
        var order = _fixture.Create<Order>();

        _orderRepositoryMock
            .Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<OrderByIdSpec>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        order.PaymentIntentId.Should().Be(command.PaymentIntentId);
        order.History[^1].Status.Should().Be(OrderStatus.PaymentSucceeded);
    }

    [Fact]
    public async Task Handle_ReturnsNotFoundError_WhenOrderDoesNotExist()
    {
        // Arrange
        var command = _fixture.Create<SetPaymentStatusCommand>();

        _orderRepositoryMock
            .Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<OrderByIdSpec>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(OrderErrors.OrderNotFound(command.OrderId));

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
