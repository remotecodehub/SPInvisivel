namespace InvisibleSP.Application.Pipeline.Validation;

public interface IMessageValidator
{
    Task ValidateAsync(object message, CancellationToken cancellationToken);
}

public sealed class FluentMessageValidator(IServiceProvider serviceProvider) : IMessageValidator
{
    public async Task ValidateAsync(object message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        var validatorType = typeof(IValidator<>).MakeGenericType(message.GetType());
        var validator = serviceProvider.GetService(validatorType) as IValidator;

        if (validator is null)
        {
            return;
        }

        var context = new ValidationContext<object>(message);
        var result = await validator.ValidateAsync(context, cancellationToken);

        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }
}

public sealed class ValidationMiddleware<TContext>(IMessageValidator validator) : IPipeSpecification<TContext>
    where TContext : IContext<IMessage>
{
    public bool ShouldExecute(TContext context, CancellationToken cancellationToken) => true;

    public Task BeforeExecute(TContext context, CancellationToken cancellationToken) =>
        validator.ValidateAsync(context.Message, cancellationToken);

    public Task Execute(TContext context, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task AfterExecute(TContext context, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task OnException(Exception ex, TContext context) => Task.CompletedTask;
}

public static class ValidationMiddlewareExtensions
{
    public static void UseValidation<TContext>(this IPipeConfigurator<TContext> configurator)
        where TContext : IContext<IMessage>
    {
        configurator.AddPipeSpecification(
            new ValidationMiddleware<TContext>(configurator.DependencyScope.Resolve<IMessageValidator>()));
    }
}
