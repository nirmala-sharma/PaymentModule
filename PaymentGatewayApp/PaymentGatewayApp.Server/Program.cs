using PaymentGatewayApp.Server.Dependencies;
using PaymentGatewayApp.Server.Middlewares;
using PaymentGatewayApp.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddService(builder.Configuration);

builder.Services.AddControllers();

// Configure CORS to allow requests from the Angular client app during development.
// This enables cross-origin communication between the frontend and backend.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient",
        policy =>
        {
            policy.WithOrigins("http://localhost:60371") // Frontend dev server URL
                  .AllowAnyMethod()                      // Allow all HTTP methods (GET, POST, etc.)
                  .AllowAnyHeader()                      // Allow any HTTP headers
                  .AllowCredentials();                   // Support sending cookies or auth headers
        });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSpaStaticFiles();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAngularClient");
app.UseAuthentication();

app.UseAuthorization();
app.UseMiddleware<GlobalExceptionMiddleware>();

// Map API controller endpoints so they can be accessed by the frontend
app.UseEndpoints(endpoint =>
{
    endpoint.MapControllers();
});

// Integrate Angular SPA with the backend during development.
// This ensures that running the backend also serves or proxies the frontend.
app.UseSpa(spa =>
{
    spa.Options.SourcePath = "../paymentgatewayapp.client"; // Path to Angular project

    if (app.Environment.IsDevelopment())
    {
        // Proxy Angular dev server during development for live reload and fast builds
        spa.UseProxyToSpaDevelopmentServer("http://localhost:60371");
    }
});
app.Run();
