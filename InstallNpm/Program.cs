using Serilog;
var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.UseWebRoot(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));
// Configure Serilog for file logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()  
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();

var app = builder.Build();
app.UseSerilogRequestLogging(); 

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.MapControllers();
// This is where you should map your controller route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=NodeInstall}/{action=InstallNode}");

app.Run();