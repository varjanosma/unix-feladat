using ProductManagement.Bll.Extensions;
using ProductManagement.Dal.Extensions;

const string AngularDevCorsPolicy = "AngularDev";

var builder = WebApplication.CreateBuilder(args);

// --- Services (DI container) --------------------------------------------------------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Our own layers: Web only knows about Bll, and Bll registers Dal.
builder.Services.AddBll(builder.Configuration.GetConnectionString("ProductManagement"));

// NSwag: generates the OpenAPI (Swagger) document from the controllers.
builder.Services.AddOpenApiDocument(settings =>
{
    settings.DocumentName = "v1";
    settings.Title = "ProductManagement API";
    settings.Version = "v1";
    settings.Description = "Demo API for the .NET course.";
});

// This host serves both the API and the compiled Angular app, so the browser normally
// makes same-origin requests and needs no CORS. The only exception is `ng serve`
// hitting the API directly instead of going through its proxy.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularDevCorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

// Creates the in-memory database and applies the seed data. The in-memory provider
// keeps everything in the process, so this has to run on every start.
app.Services.InitializeDal();

// --- Middleware pipeline ------------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // /swagger/v1/swagger.json  +  /swagger  (UI)
    app.UseOpenApi();
    app.UseSwaggerUi();
}

// The Angular build output in wwwroot.
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors(AngularDevCorsPolicy);
app.UseAuthorization();

app.MapControllers();

// SPA: every request that is not a file and not a controller route falls back to
// index.html, and Angular takes over the routing.
app.MapFallbackToFile("index.html");

app.Run();
