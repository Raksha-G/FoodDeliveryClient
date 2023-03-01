using Serilog;
using Serilog.Sinks.AwsCloudWatch;
using Amazon.CloudWatchLogs;
var builder = WebApplication.CreateBuilder(args);

/*Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
*/

var client = new AmazonCloudWatchLogsClient();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.AmazonCloudWatch(
    logGroup: "/dotnet/logging-demo/serilog",
    logStreamPrefix: DateTime.Now.ToString("yyyyMMddHHmmssfff"),
    cloudWatchClient: client
    )
    .WriteTo.File(
    path: "../logs/CustomerApp-.log",
    rollingInterval: RollingInterval.Day,
    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {CorrelationId} {Level:u3} {Username} {Message:lj}{Exception}{NewLine}"
    )
    .Enrich.FromLogContext()
    .CreateLogger();

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.AmazonCloudWatch(
    logGroup: "/dotnet/logging-demo/serilog",


    logStreamPrefix: DateTime.Now.ToString("yyyyMMddHHmmssfff"),

    cloudWatchClient: client
    )
     .WriteTo.File(
    path: "../logs/CustomerApp-.log",
    rollingInterval: RollingInterval.Day,
    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {CorrelationId} {Level:u3} {Username} {Message:lj}{Exception}{NewLine}"
    )
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(10080);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
