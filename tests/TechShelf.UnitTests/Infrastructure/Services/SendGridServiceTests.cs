using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using Moq;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using TechShelf.Application.Exceptions;
using TechShelf.Infrastructure.Services.SendGrid;

namespace TechShelf.UnitTests.Infrastructure.Services;

public class SendGridServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ISendGridClient> _sendGridClientMock;
    private readonly FakeLogger<SendGridService> _logger;
    private readonly SendGridOptions _options;
    private readonly SendGridService _sut;

    public SendGridServiceTests()
    {
        _fixture = new Fixture();
        _sendGridClientMock = new Mock<ISendGridClient>();
        _logger = new FakeLogger<SendGridService>();

        _options = new SendGridOptions
        {
            FromEmail = _fixture.Create<string>(),
            IsSandboxEnabled = false
        };
        var optionsMock = new Mock<IOptions<SendGridOptions>>();
        optionsMock.Setup(x => x.Value).Returns(_options);

        _sut = new SendGridService(
            _sendGridClientMock.Object,
            optionsMock.Object,
            _logger);
    }

    [Fact]
    public async Task SendPlainTextEmailAsync_SendsEmail_WhenAllParametersAreValid()
    {
        // Arrange
        var toEmail = _fixture.Create<string>();
        var subject = _fixture.Create<string>();
        var message = _fixture.Create<string>();
        var response = new Response(HttpStatusCode.OK, null, null);

        _sendGridClientMock
            .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _sut.SendPlainTextEmailAsync(toEmail, subject, message);

        // Assert
        _sendGridClientMock.Verify(
            x => x.SendEmailAsync(
                It.Is<SendGridMessage>(msg =>
                    msg.From.Email == _options.FromEmail &&
                    msg.Personalizations[0].Subject == subject &&
                    msg.Contents[0].Value == message),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _logger.Collector.GetSnapshot()
            .Should().Contain(e =>
                e.Level == LogLevel.Information &&
                e.Message.Contains(toEmail) &&
                e.Message.Contains(subject));
    }

    [Fact]
    public async Task SendPlainTextEmailAsync_EnablesSandboxMode_WhenSandboxIsEnabled()
    {
        // Arrange
        _options.IsSandboxEnabled = true;
        var response = new Response(HttpStatusCode.OK, null, null);

        _sendGridClientMock
            .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _sut.SendPlainTextEmailAsync(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>());

        // Assert
        _sendGridClientMock.Verify(
            x => x.SendEmailAsync(
                It.Is<SendGridMessage>(msg => msg.MailSettings.SandboxMode.Enable == true),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPlainTextEmailAsync_ThrowsEmailSendingException_WhenSendGridClientFails()
    {
        // Arrange
        var toEmail = _fixture.Create<string>();
        var response = new Response(HttpStatusCode.BadRequest, new StringContent(""), null);

        _sendGridClientMock
            .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var act = () => _sut.SendPlainTextEmailAsync(
            toEmail,
            _fixture.Create<string>(),
            _fixture.Create<string>());

        // Assert
        await act.Should().ThrowAsync<EmailSendingException>()
            .WithMessage($"Failed to send email to {toEmail}*");

        _logger.Collector.GetSnapshot()
            .Should().Contain(e =>
                e.Level == LogLevel.Error &&
                e.Message.Contains("Failed to send email") &&
                e.Message.Contains(HttpStatusCode.BadRequest.ToString()));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task SendPlainTextEmailAsync_ThrowsArgumentNullException_WhenToEmailIsNullOrWhiteSpace(string? toEmail)
    {
        // Act
        var act = () => _sut.SendPlainTextEmailAsync(
            toEmail!,
            _fixture.Create<string>(),
            _fixture.Create<string>());

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName(nameof(toEmail));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task SendPlainTextEmailAsync_ThrowsArgumentNullException_WhenSubjectIsNullOrWhiteSpace(string? subject)
    {
        // Act
        var act = () => _sut.SendPlainTextEmailAsync(
            _fixture.Create<string>(),
            subject!,
            _fixture.Create<string>());

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName(nameof(subject));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task SendPlainTextEmailAsync_ThrowsArgumentNullException_WhenMessageIsNullOrWhiteSpace(string? message)
    {
        // Act
        var act = () => _sut.SendPlainTextEmailAsync(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            message!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName(nameof(message));
    }

    [Fact]
    public async Task SendPlainTextEmailAsync_LogsSuccessMessage_WhenEmailIsSentSuccessfully()
    {
        // Arrange
        var toEmail = _fixture.Create<string>();
        var response = new Response(HttpStatusCode.OK, null, null);

        _sendGridClientMock
            .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _sut.SendPlainTextEmailAsync(toEmail, _fixture.Create<string>(), _fixture.Create<string>());

        // Assert
        _logger.Collector.GetSnapshot()
            .Should().Contain(e =>
                e.Level == LogLevel.Information &&
                e.Message.Contains("Successfully sent email") &&
                e.Message.Contains(toEmail));
    }
}