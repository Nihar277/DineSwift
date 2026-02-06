using System.IO;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
// ================= SERVICES =================

// MVC Controllers + Views
builder.Services.AddControllersWithViews();


// Session Configuration (UNCHANGED)
// Persist data-protection keys so session cookies can be unprotected across restarts
var dataProtectionKeysPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DineSwift-DataProtection-Keys");
System.IO.Directory.CreateDirectory(dataProtectionKeysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new System.IO.DirectoryInfo(dataProtectionKeysPath))
    .SetApplicationName("DineSwiftApp");

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ================= BUILD =================
var app = builder.Build();

// ================= MIDDLEWARE =================

// Production error handling (UNCHANGED)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Core middlewares (UNCHANGED)
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session (ONLY ONCE – LOGIC SAME)
app.UseSession();

// Authorization pipeline (UNCHANGED INTENT)
app.UseAuthorization();

// ================= ROUTING =================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// ================= RUN =================
app.Run();
