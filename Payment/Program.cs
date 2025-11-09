using Microsoft.EntityFrameworkCore;
using Payment.Contracts;
using Payment.Data;
using Payment.Processors;
using Payment.Services;
using Stripe;


// 1
var builder = WebApplication.CreateBuilder(args);
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

////////////////////////
//2- services
////////////////////////
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// register gateways
builder.Services.AddScoped<IPaymentGateway, StripeService>();

// register processor
builder.Services.AddScoped<PaymentProcessor>();

// add SQLite
builder.Services.AddDbContext<PaymentsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("PaymentsDb")));

////////////////////////
///3- application
///////////////////////
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// to make sure db and tables exist (SQLite)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
    db.Database.EnsureCreated();
}

app.UseDefaultFiles(); // search index.html automatically
app.UseStaticFiles();  // html, js, css
app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
