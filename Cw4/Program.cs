using Cw4.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IDatabaseService, DatabaseService>();
builder.Services.AddControllers();

var app = builder.Build();
app.UseAuthorization();
app.MapControllers();
app.Run();

