using Azure.Storage.Blobs;
using AzureBlobDemo.Infrastructure.Context;
using AzureBlobDemo.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//register blob service client by reading StorageAccount connection string from appsettings.json
builder.Services.AddSingleton(x => new BlobServiceClient(builder.Configuration.GetConnectionString("BlobStorageAccount")));

//register database storage using sql server
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
	opt.UseSqlServer(builder.Configuration.GetConnectionString("DbConnectString"));
});

//add services dependency injection
builder.Services.AddTransient<BlobService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Blob}/{action=Index}/{id?}");

app.Run();
