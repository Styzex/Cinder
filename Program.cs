using System.Runtime.CompilerServices;

var builder = WebApplication.CreateBuilder(args);
var ffmpeg = FFMpegCore.FFMpeg.GetCodecs();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
  app.MapOpenApi();
}

app.UseHttpsRedirection();

// HI

app.Run();