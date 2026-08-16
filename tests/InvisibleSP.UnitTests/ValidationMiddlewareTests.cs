namespace InvisibleSP.UnitTests;

public sealed class ValidationMiddlewareTests
{
    [Fact]
    public async Task Fluent_message_validator_should_execute_registered_validator()
    {
        var services = new ServiceCollection();
        services.AddScoped<IValidator<RegisterCommand>, RegisterCommandValidator>();
        using var provider = services.BuildServiceProvider();
        var validator = new FluentMessageValidator(provider);

        var valid = () => validator.ValidateAsync(
            new RegisterCommand("user@example.com", "Password1!"),
            CancellationToken.None);
        await valid();

        var invalid = () => validator.ValidateAsync(
            new RegisterCommand("invalid", "short"),
            CancellationToken.None);
        await invalid.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Fluent_message_validator_should_skip_unregistered_messages()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();
        var validator = new FluentMessageValidator(provider);

        await validator.ValidateAsync(new object(), CancellationToken.None);
    }
}
