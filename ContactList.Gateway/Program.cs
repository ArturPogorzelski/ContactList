using ContactList.Gateway.Extansion;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddOcelotWithPolly(builder.Configuration);

builder.Services.AddControllers(); // Dodajemy kontrolery, jeœli s¹ potrzebne (np. do health checków)

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); // Mapujemy kontrolery, jeœli s¹ dodane
});

await app.UseOcelot(); // U¿ywamy Ocelot

app.Run();
