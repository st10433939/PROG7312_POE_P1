using PROG7312_POE_P1.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// MVC + API Controllers
builder.Services
    .AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

// In-memory Smart-X sensor service
builder.Services.AddSingleton<SensorService>();

var app = builder.Build();

app.UseHttpsRedirection();

// Allows CSS, JavaScript and uploaded files
// inside wwwroot to be accessed.
app.UseStaticFiles();


app.UseRouting();

app.UseAuthorization();

// Enables controllers using [Route(...)]
// such as /api/sensors
app.MapControllers();


// MVC route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();