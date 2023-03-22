using AuthServer.Services;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped(x 
    => ActivatorUtilities.CreateInstance<OIDCService>(x, x.GetRequiredService<IHttpContextAccessor>().HttpContext?.Request.GetDisplayUrl() ?? ""));


string[] initialScopes = builder.Configuration.GetValue<string>("CallApi:ScopeForAccessToken")?.Split(' ');

builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration)
    .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
    .AddInMemoryTokenCaches();



builder.Services.AddHttpClient();

builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
{
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("corsapp");

app.MapControllers();

app.Run();
