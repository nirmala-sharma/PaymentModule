using PaymentGatewayApp.Server.Dependencies;
using PaymentGatewayApp.Server.Middlewares;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Register HttpClientFactory
builder.Services.AddHttpClient();
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

// Looks for default files like index.html if no file is specified in the request.
app.UseDefaultFiles();
// Serves static files (CSS, JS, images). Must come early so static assets are handled quickly.
app.UseStaticFiles();

// Configure the HTTP request pipeline.
// Loads Swagger in development for API testing; uses SPA static files in production to serve the Angular app.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSpaStaticFiles();
}
// Logs HTTP requests early, before processing begins, for better tracking.
app.UseSerilogRequestLogging();
// Redirects HTTP to HTTPS for security.
app.UseHttpsRedirection();
// Set-up the routing system—essential before mapping endpoints.
app.UseRouting();
// Allows cross-origin requests from Angular frontend.
app.UseCors("AllowAngularClient");
// Authenticates the user before checking their permissions.
app.UseAuthentication();
// Enables authorization middleware to enforce security policies on incoming requests
app.UseAuthorization();
//Custom error handling to catch and format exceptions consistently.
app.UseMiddleware<GlobalExceptionMiddleware>();

// Map API controller endpoints so they can be accessed by the frontend 
// Done after routing is enabled

app.UseEndpoints(endpoint =>
{
    endpoint.MapControllers();
});

// Integrate Angular SPA with the backend during development.
// This ensures that running the backend also serves or proxies the frontend.
// Serves or proxies the Angular SPA after all backend logic is set up.
app.UseSpa(spa =>
{
    spa.Options.SourcePath = "../paymentgatewayapp.client"; // Path to Angular project

    if (app.Environment.IsDevelopment())
    {
        // Proxy Angular dev server during development for live reload and fast builds
        spa.UseProxyToSpaDevelopmentServer("http://localhost:60371");
    }
});
// Middleware Orders: Serve static > log > secure > route > auth > handle > map endpoints > serve SPA.
app.Run();

