using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add standard MVC framework services
builder.Services.AddControllersWithViews();

// Register the native MongoDB Client via dependency injection
builder.Services.AddSingleton<IMongoClient>(sp => 
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config["MongoDbSettings:ConnectionString"];
    return new MongoClient(connectionString);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();