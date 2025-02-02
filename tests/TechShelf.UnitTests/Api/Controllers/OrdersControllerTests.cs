using AutoFixture;
using ErrorOr;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
using TechShelf.Application.Common.Pagination;
using TechShelf.Application.Features.Orders.Commands.CreateOrder;
using TechShelf.Application.Features.Orders.Commands.SetPaymentStatus;
using TechShelf.Application.Features.Orders.Common.Dtos;
using TechShelf.Application.Features.Orders.Queries.GetCustomerOrders;
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
        var userId = _fixture.Create<string>();
        var orderDto = _fixture.Create<OrderDto>();
        var stripeUrl = _fixture.Create<string>();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId)
        ], "test"));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
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
            It.Is<CreateOrderCommand>(c => c.UserId == userId),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Checkout_ReturnsUnauthorized_WhenAuthenticatedUserMissingNameIdentifierClaim()
    {
        // Arrange
        var request = _fixture.Create<CreateOrderRequest>();
        var user = new ClaimsPrincipal(new ClaimsIdentity([], "test"));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.Checkout(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
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

    [Fact]
    public async Task GetCustomerOrders_ReturnsOkWithPagedResult_WhenOrdersExist()
    {
        // Arrange
        var pageIndex = _fixture.Create<int>();
        var pageSize = _fixture.Create<int>();
        var totalCount = _fixture.Create<int>();
        var customerId = _fixture.Create<string>();
        var orders = _fixture.CreateMany<OrderDto>(Math.Min(pageSize, totalCount)).ToList();
        var pagedResult = new PagedResult<OrderDto>(orders, totalCount, pageIndex, pageSize);

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetCustomerOrdersQuery>(q =>
                q.CustomerId == customerId &&
                q.PageIndex == pageIndex &&
                q.PageSize == pageSize),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetCustomerOrders(customerId, pageIndex, pageSize);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPagedResult = okResult.Value.Should().BeOfType<PagedResult<OrderDto>>().Subject;

        returnedPagedResult.Items.Should().HaveCount(orders.Count);
        returnedPagedResult.TotalCount.Should().Be(totalCount);
        returnedPagedResult.PageIndex.Should().Be(pageIndex);
        returnedPagedResult.PageSize.Should().Be(pageSize);

        _mediatorMock.Verify(m => m.Send(It.IsAny<GetCustomerOrdersQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCustomerOrders_ReturnsOkWithEmptyResult_WhenNoOrdersExist()
    {
        // Arrange
        var pageIndex = _fixture.Create<int>();
        var pageSize = _fixture.Create<int>();
        var customerId = _fixture.Create<string>();
        var emptyPagedResult = new PagedResult<OrderDto>([], 0, pageIndex, pageSize);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetCustomerOrdersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyPagedResult);

        // Act
        var result = await _controller.GetCustomerOrders(customerId, pageIndex, pageSize);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPagedResult = okResult.Value.Should().BeOfType<PagedResult<OrderDto>>().Subject;

        returnedPagedResult.Items.Should().BeEmpty();
        returnedPagedResult.TotalCount.Should().Be(0);
        returnedPagedResult.PageIndex.Should().Be(pageIndex);
        returnedPagedResult.PageSize.Should().Be(pageSize);

        _mediatorMock.Verify(m => m.Send(It.IsAny<GetCustomerOrdersQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCustomerOrders_ReturnsProblem_WhenErrorOccurred()
    {
        // Arrange
        var customerId = _fixture.Create<string>();
        var error = _fixture.Create<Error>();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetCustomerOrdersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(error);

        // Act
        var result = await _controller.GetCustomerOrders(customerId, _fixture.Create<int>(), _fixture.Create<int>());

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var problemResult = result as ObjectResult;
        problemResult!.Value.Should().BeAssignableTo<ProblemDetails>();

        _mediatorMock.Verify(m => m.Send(It.IsAny<GetCustomerOrdersQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #region GetMyOrders

    [Fact]
    public async Task GetMyOrders_ReturnsForbidden_WhenCustomerIdClaimMissing()
    {
        // Arrange
        var pageIndex = _fixture.Create<int>();
        var pageSize = _fixture.Create<int>();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await _controller.GetMyOrders(pageIndex, pageSize);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task GetMyOrders_ReturnsProblem_WhenErrorOccurs()
    {
        // Arrange
        var customerId = _fixture.Create<string>();
        var pageIndex = _fixture.Create<int>();
        var pageSize = _fixture.Create<int>();
        var error = _fixture.Create<Error>();

        SetupUserWithSubClaim(customerId);

        _mediatorMock
           .Setup(m => m.Send(It.Is<GetCustomerOrdersQuery>(q =>
               q.CustomerId == customerId &&
               q.PageIndex == pageIndex &&
               q.PageSize == pageSize),
               It.IsAny<CancellationToken>()))
           .ReturnsAsync(error);

        // Act
        var result = await _controller.GetMyOrders(pageIndex, pageSize);
        result.Should().BeOfType<ObjectResult>();
        var problemResult = result as ObjectResult;
        problemResult!.Value.Should().BeAssignableTo<ProblemDetails>();
    }

    [Fact]
    public async Task GetMyOrders_ReturnsOkWithPagedResult_WhenOrdersExist()
    {
        // Arrange
        var pageIndex = _fixture.Create<int>();
        var pageSize = _fixture.Create<int>();
        var totalCount = _fixture.Create<int>();
        var customerId = _fixture.Create<string>();

        var orders = _fixture.CreateMany<OrderDto>(Math.Min(pageSize, totalCount)).ToList();
        var pagedResult = new PagedResult<OrderDto>(orders, totalCount, pageIndex, pageSize);

        SetupUserWithSubClaim(customerId);

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetCustomerOrdersQuery>(q =>
                q.CustomerId == customerId &&
                q.PageIndex == pageIndex &&
                q.PageSize == pageSize),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetMyOrders(pageIndex, pageSize);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPagedResult = okResult.Value.Should().BeOfType<PagedResult<OrderDto>>().Subject;

        returnedPagedResult.Items.Should().HaveCount(orders.Count);
        returnedPagedResult.TotalCount.Should().Be(totalCount);
        returnedPagedResult.PageIndex.Should().Be(pageIndex);
        returnedPagedResult.PageSize.Should().Be(pageSize);
    }

    private void SetupUserWithSubClaim(string customerId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, customerId)
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    #endregion
}
