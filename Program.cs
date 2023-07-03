using Hangfire;
using Hangfire.Storage.SQLite;
using HangfireBasicAuthenticationFilter;
using HangfireTemplate.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHangfire(config => config
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSQLiteStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 30;
});

builder.Services.AddTransient<IServiceManagement, ServiceManagement>();

var app = builder.Build();

 app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseHangfireDashboard("/hangfire", new DashboardOptions()
{
    DarkModeEnabled = true,
    DashboardTitle = "Drivers Dashboard",
    Authorization = new [] 
    {
        new HangfireCustomBasicAuthenticationFilter()
        {
            Pass = "Password",
            User = "User"
        }
    }
});

app.MapHangfireDashboard();


// HANGFIRE here you need to specify Cron Job (use https://www.freeformatter.com/)
RecurringJob.AddOrUpdate<ServiceManagement>(x => x.SyncData(), "* * * ? * *");


//HANGFIRE 

app.Run();
