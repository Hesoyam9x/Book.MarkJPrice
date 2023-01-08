using Packt.Shared; // AddNorthwindContext extension method
using Microsoft.EntityFrameworkCore;
using static System.Console;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();

string databasePath = Path.Combine("..", "Northwind.db");

builder.Services.AddDbContext<Northwind>(options =>
    options.UseSqlite($"Data Source={databasePath}")
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.Use(async (HttpContext context, Func<Task> next) =>
{
    RouteEndpoint? rep = context.GetEndpoint() as RouteEndpoint;
    
    if (rep is not null)
    {
        WriteLine($"Endpoint name: {rep.DisplayName}");
        WriteLine($"Endpoint route pattern: {rep.RoutePattern.RawText}");
    }
    if (context.Request.Path == "/bonjour")
    {
        // in the case of a match on URL path, this becomes a terminating
        // delegate that returns so does not call the next delegate
        await context.Response.WriteAsync("Bonjour Monde!");
        return;
    }
    // we could modify the request before calling the next delegate
    await next();
    
    // we could modify the response after calling the next delegate
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapRazorPages();

app.MapGet("/hello", () => "Hello World!");

app.Run();
