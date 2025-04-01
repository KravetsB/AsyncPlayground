// Welcome to the Asynchronous Programming examination playground!
// This is a simple console application that demonstrates the use of async and await in C#.
// Please, complete a number of tasks stated below to optimize the usage of async/await.
//  You are free to modify the code in any way you see fit, as long as the tasks are completed.

using AsyncPlayground;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var services = CreateServices();
var application = services.GetRequiredService<Application>();

await application.Go();

static ServiceProvider CreateServices()
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("config.json", optional: false);

    IConfiguration config = builder.Build();

    var serviceProvider = new ServiceCollection()
        .AddSingleton<Application>()
        .AddDbContext<ApplicationContext>(options => options.UseInMemoryDatabase(config.GetConnectionString("AsyncPlaygroundDb") ?? string.Empty))
        .BuildServiceProvider();

    return serviceProvider;
}

