var builder = WebApplication.CreateBuilder(args);
builder.AddInvisibleSP();

var app = builder.Build();
app.UseInvisibleSP();

app.Run();
