using AutoFixture;
using ErrorOr;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using Stripe;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using TechShelf.API.Common.Requests.Orders;
using TechShelf.API.Common.Responses;
using TechShelf.API.Controllers;
using TechShelf.Application.Features.Orders.Commands.CreateOrder;
using TechShelf.Application.Features.Orders.Commands.SetPaymentStatus;
using TechShelf.Application.Features.Orders.Common.Dtos;
using TechShelf.Application.Features.Users.Common;
using TechShelf.Application.Features.Users.Queries.GetUserInfo;
using TechShelf.Application.Interfaces.Services;

namespace TechShelf.UnitTests.Api.Controllers;

public class OrdersControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IStripeService> _stripeServiceMock;
    private readonly FakeLogger<OrdersController> _fakeLogger;
    private readonly IFixture _fixture;
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        _fixture = new Fixture();
        _mediatorMock = new Mock<IMediator>();
        _stripeServiceMock = new Mock<IStripeService>();
        _fakeLogger = new FakeLogger<OrdersController>();

        _controller = new OrdersController(
            _mediatorMock.Object,
            _stripeServiceMock.Object,
            _fakeLogger);
    }

    [Fact]
    public async Task Checkout_ReturnsOk_WhenCheckoutSucceeds()
    {
        // Arrange
        var request = _fixture.Create<CreateOrderRequest>();
        var orderDto = _fixture.Create<OrderDto>();
        var stripeUrl = _fixture.Create<string>();
        var expectedResponse = new StripeRedirectionResponse(stripeUrl);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderDto);

        _stripeServiceMock
            .Setup(s => s.CreateCheckoutSessionAsync(orderDto))
            .ReturnsAsync(stripeUrl);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await _controller.Checkout(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);

        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _stripeServiceMock.Verify(s => s.CreateCheckoutSessionAsync(orderDto), Times.Once);
    }

    [Fact]
    public async Task Checkout_ReturnsProblem_WhenOrderCreationFails()
    {
        // Arrange
        var request = _fixture.Create<CreateOrderRequest>();
        var errors = _fixture.Create<Error>();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(errors);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await _controller.Checkout(request);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var problemResult = result as ObjectResult;
        problemResult!.Value.Should().BeAssignableTo<ProblemDetails>();
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Checkout_IncludesUserId_WhenUserIsAuthenticated()
    {
        // Arrange
        var request = _fixture.Create<CreateOrderRequest>();
        var email = _fixture.Create<string>();
        var userDto = _fixture.Create<UserDto>();
        var orderDto = _fixture.Create<OrderDto>();
        var stripeUrl = _fixture.Create<string>();

        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(JwtRegisteredClaimNames.Email, email)
        ], "test"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUserInfoQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderDto);

        _stripeServiceMock
            .Setup(s => s.CreateCheckoutSessionAsync(orderDto))
            .ReturnsAsync(stripeUrl);

        // Act
        var result = await _controller.Checkout(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mediatorMock.Verify(m => m.Send(
            It.Is<CreateOrderCommand>(c => c.UserId == userDto.Id),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task StripeWebhook_ReturnsOk_WhenEventIsValid()
    {
        // Arrange
        var paymentIntentId = _fixture.Create<string>();
        var orderId = _fixture.Create<Guid>();
        var paymentResult = new StripePaymentResult(orderId, true, paymentIntentId);

        SetupStripeRequest();

        _stripeServiceMock
            .Setup(s => s.HandleStripeEvent(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(paymentResult);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<SetPaymentStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _controller.StripeWebhook();

        // Assert
        result.Should().BeOfType<OkResult>();
        _mediatorMock.Verify(m => m.Send(
            It.Is<SetPaymentStatusCommand>(c =>
                c.OrderId == orderId &&
                c.PaymentIntentId == paymentIntentId &&
                c.IsPaymentSuccessful),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task StripeWebhook_ReturnsBadRequest_WhenStripeExceptionOccurs()
    {
        // Arrange
        SetupStripeRequest();

        _stripeServiceMock
            .Setup(s => s.HandleStripeEvent(It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new StripeException("Stripe error"));

        // Act
        var result = await _controller.StripeWebhook();

        // Assert
        result.Should().BeOfType<BadRequestResult>();
        _fakeLogger.Collector.GetSnapshot().Should().Contain(log =>
            log.Level == LogLevel.Error &&
            log.Message.Contains("Stripe error"));
    }

    [Fact]
    public async Task StripeWebhook_ReturnsOk_WhenPaymentResultIsNull()
    {
        // Arrange
        SetupStripeRequest();

        _stripeServiceMock
            .Setup(s => s.HandleStripeEvent(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((StripePaymentResult?)null);

        // Act
        var result = await _controller.StripeWebhook();

        // Assert
        result.Should().BeOfType<OkResult>();
        _mediatorMock.Verify(m => m.Send(It.IsAny<SetPaymentStatusCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task StripeWebhook_LogsError_WhenPaymentStatusUpdateFails()
    {
        // Arrange
        var paymentIntentId = _fixture.Create<string>();
        var orderId = _fixture.Create<Guid>();
        bool paymentSuccess = true;
        var paymentResult = new StripePaymentResult(orderId, paymentSuccess, paymentIntentId);
        var paymentStatusUpdateError = _fixture.Create<Error>();

        SetupStripeRequest();

        _stripeServiceMock
            .Setup(s => s.HandleStripeEvent(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(paymentResult);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<SetPaymentStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentStatusUpdateError);

        // Act
        var result = await _controller.StripeWebhook();

        // Assert
        result.Should().BeOfType<OkResult>();
        _fakeLogger.Collector.GetSnapshot().Should().Contain(log =>
            log.Level == LogLevel.Error &&
            log.Message.Contains($"Payment status update failed for order {orderId}. Payment success: {paymentSuccess}"));
    }

    private void SetupStripeRequest()
    {
        var json = JsonSerializer.Serialize(new { });
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                Request = { Body = stream }
            }
        };

        _controller.ControllerContext.HttpContext.Request.Headers.Append("Stripe-Signature", "test_signature");
    }
}
