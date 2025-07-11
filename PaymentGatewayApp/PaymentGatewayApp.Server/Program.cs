using Microsoft.AspNetCore.RateLimiting;
using PaymentGatewayApp.Server.Dependencies;
using PaymentGatewayApp.Server.Interfaces;
using PaymentGatewayApp.Server.Middlewares;
using Serilog;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Register HttpClientFactory
builder.Services.AddHttpClient();
builder.Services.AddService(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddSignalR();

// Configure rate limiting to allow 1 request per minute per IP.
// If the limit is exceeded, 1 request can wait in queue; others are rejected with 429 status code.
builder.Services.AddRateLimiter(options => // Register rate limiting services
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests; // Return 429 when limit is exceeded
    options.AddPolicy("FixedPolicy", context => // Define a named rate limit policy
        RateLimitPartition.GetFixedWindowLimiter(
            // Use client IP as partition key and rate limits are applied individually per IP address
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown", 
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 1, // Allow 1 request per window
                Window = TimeSpan.FromMinutes(1), // Set window duration to 1 minute
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst, // Queue oldest request first
                QueueLimit = 1 // Allow 1 request to wait in the queue
            }));
});

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

// Add the rate limiter middleware to intercept and enforce rate limits on incoming requests.
app.UseRateLimiter();

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
    // Maps controller routes (e.g., API endpoints)
    endpoint.MapControllers();

    // Maps the SignalR ChatHub to the specified route
    // Clients will connect to this hub at /api/ChatHub
    endpoint.MapHub<ChatHub>("/api/ChatHub");
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

// Create a scope to access application services.
// Get the seed service and run it to add initial data to the database
// (like default users or roles) when the app starts.
using (var scope = app.Services.CreateScope())
{
    var seedService = scope.ServiceProvider.GetRequiredService<ISeedService>();
    await seedService.SeedAsync();
}
// Middleware Orders: Serve static > log > secure > route > auth > handle > map endpoints > serve SPA.

app.Run();

