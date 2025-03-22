using FluentValidation;
using FluentValidation.Results;
using TechShelf.Application.Common.Behaviors;

namespace TechShelf.UnitTests.Application.Common.Behaviors;

public class ValidationBehaviorTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IValidator<TestRequest>> _validatorMock;
    private readonly List<IValidator<TestRequest>> _validators;
    private readonly Mock<RequestHandlerDelegate<ErrorOr<TestResponse>>> _nextMock;

    public ValidationBehaviorTests()
    {
        _fixture = new Fixture();
        _validatorMock = new Mock<IValidator<TestRequest>>();
        _validators = [];
        _nextMock = new Mock<RequestHandlerDelegate<ErrorOr<TestResponse>>>();
    }

    [Fact]
    public async Task Handle_ReturnsErrors_WhenValidationFails()
    {
        // Arrange
        var validationFailures = _fixture.CreateMany<ValidationFailure>().ToList();

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        _validators.Add(_validatorMock.Object);

        var behavior = new ValidationBehavior<TestRequest, ErrorOr<TestResponse>>(_validators);

        var request = _fixture.Create<TestRequest>();

        // Act
        var result = await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().HaveCount(validationFailures.Count);

        foreach (var failure in validationFailures)
        {
            result.Errors.Should().Contain(e =>
                e.Code == failure.PropertyName &&
                e.Description == failure.ErrorMessage);
        }

        _nextMock.Verify(next => next(), Times.Never);
    }

    [Fact]
    public async Task Handle_InvokesNext_WhenNoValidatorsAreProvided()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestRequest, ErrorOr<TestResponse>>([]);

        var request = _fixture.Create<TestRequest>();
        var expectedResponse = _fixture.Create<TestResponse>();

        _nextMock.Setup(next => next()).ReturnsAsync(expectedResponse);

        // Act
        var result = await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Message.Should().Be(expectedResponse.Message);

        _nextMock.Verify(next => next(), Times.Once);
    }

    [Fact]
    public async Task Handle_InvokesNext_WhenValidationSucceeds()
    {
        // Arrange
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _validators.Add(_validatorMock.Object);

        var behavior = new ValidationBehavior<TestRequest, ErrorOr<TestResponse>>(_validators);

        var request = _fixture.Create<TestRequest>();
        var expectedResponse = _fixture.Create<TestResponse>();

        _nextMock.Setup(next => next()).ReturnsAsync(expectedResponse);

        // Act
        var result = await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Message.Should().Be(expectedResponse.Message);

        _nextMock.Verify(next => next(), Times.Once);
    }

    public class TestRequest : IRequest<ErrorOr<TestResponse>>
    {
        public string Prop { get; set; } = string.Empty;
    }

    public class TestResponse
    {
        public string Message { get; set; } = string.Empty;
    }

}

