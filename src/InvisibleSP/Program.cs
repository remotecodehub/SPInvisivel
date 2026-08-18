await WebApplication
    .CreateBuilder(args)
    .AddInvisibleSP()
    .Build()
    .UseInvisibleSP<App>()
    .RunAsync();
