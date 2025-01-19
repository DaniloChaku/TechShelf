using AutoFixture;
using ErrorOr;
using FluentAssertions;
using Moq;
using TechShelf.Application.Features.Orders.Commands.CreateOrder;
using TechShelf.Application.Features.Orders.Common.Dtos;
using TechShelf.Application.Interfaces.Data;
using TechShelf.Domain.Entities;
using TechShelf.Domain.Entities.OrderAggregate;
using TechShelf.Domain.Errors;
using TechShelf.Domain.Specifications.Products;

namespace TechShelf.UnitTests.Application.Features.Orders.CreateOrder;

public class CreateOrderCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<Product>> _productRepositoryMock;
    private readonly Mock<IRepository<Order>> _orderRepositoryMock;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _fixture = new Fixture();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepositoryMock = new Mock<IRepository<Product>>();
        _orderRepositoryMock = new Mock<IRepository<Order>>();

        _unitOfWorkMock
            .Setup(x => x.Repository<Product>())
            .Returns(_productRepositoryMock.Object);
        _unitOfWorkMock
            .Setup(x => x.Repository<Order>())
            .Returns(_orderRepositoryMock.Object);

        _handler = new CreateOrderCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsOrderId_WhenAllProductsExistAndHaveEnoughStock()
    {
        // Arrange
        var command = CreateValidCommand();
        var products = CreateValidProducts(command.BasketItems);
        SetupProductRepository(products);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBe(Guid.Empty);

        _orderRepositoryMock.Verify(
            x => x.Add(It.IsAny<Order>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsInvalidProductError_WhenProductDoesNotExist()
    {
        // Arrange
        var command = CreateValidCommand();
        _productRepositoryMock
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<ProductByIdSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as Product);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(OrderErrors.InvalidProductInBasket(command.BasketItems.First().ProductId));

        _orderRepositoryMock.Verify(
            x => x.Add(It.IsAny<Order>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsNotEnoughStockError_WhenProductHasInsufficientStock()
    {
        // Arrange
        var command = CreateValidCommand();
        var product = _fixture.Build<Product>()
            .With(x => x.Stock, 0)
            .Create();

        _productRepositoryMock
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<ProductByIdSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Should().Be(OrderErrors.NotEnoughStock(product.Name, product.Stock));

        _orderRepositoryMock.Verify(
            x => x.Add(It.IsAny<Order>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_UpdatesProductStock_WhenOrderIsCreated()
    {
        // Arrange
        var command = CreateValidCommand();
        var products = CreateValidProducts(command.BasketItems);
        SetupProductRepository(products);
        var expectedStockLevels = products.ToDictionary(
            p => p.Id,
            p => p.Stock - command.BasketItems.First(b => b.ProductId == p.Id).Quantity);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        products.Should().AllSatisfy(product =>
            product.Stock.Should().Be(expectedStockLevels[product.Id]));
    }

    private CreateOrderCommand CreateValidCommand()
    {
        return _fixture.Build<CreateOrderCommand>()
            .With(x => x.BasketItems,
            [
                _fixture.Build<BasketItem>()
                    .With(b => b.Quantity, 1)
                    .Create()
            ])
            .With(x => x.ShippingAddress, _fixture.Build<AddressDto>()
            .With(a => a.Country, Address.AllowedCounties[0])
                .Create())
            .Create();
    }

    private List<Product> CreateValidProducts(IEnumerable<BasketItem> basketItems)
    {
        return basketItems.Select(item => _fixture.Build<Product>()
            .With(x => x.Id, item.ProductId)
            .With(x => x.Stock, 10)
            .Create())
            .ToList();
    }

    private void SetupProductRepository(List<Product> products)
    {
        foreach (var product in products)
        {
            _productRepositoryMock
                .Setup(x => x.FirstOrDefaultAsync(
                    It.Is<ProductByIdSpec>(spec => spec.IsSatisfiedBy(product)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);
        }
    }
}
