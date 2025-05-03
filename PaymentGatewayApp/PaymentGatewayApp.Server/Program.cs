using PaymentGatewayApp.Server.Dependencies;
using PaymentGatewayApp.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddService(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient",
        policy =>
        {
            policy.WithOrigins("http://localhost:65283") // Adjust if needed
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials(); // Needed if using cookies or authentication
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

app.UseEndpoints(endpoint =>
{
    endpoint.MapControllers();
}); 
app.UseSpa(spa =>
{
    spa.Options.SourcePath = "../paymentgatewayapp.client"; // path to your Angular app

    if (app.Environment.IsDevelopment())
    {
        spa.UseProxyToSpaDevelopmentServer("http://localhost:65283"); 
    }
});
app.Run();
