using Kanji_teacher_backend.dbContext;
using Kanji_teacher_backend.Util;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
Env.Load(".env");
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder
                .AllowAnyOrigin()     // Allow all origins
                .AllowAnyMethod()     // Allow all HTTP methods (GET, POST, etc.)
                .AllowAnyHeader();    // Allow all headers
        });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<KTContext>();
builder.Services.AddSingleton<FirebaseService>();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";

app.Urls.Add($"http://0.0.0.0:{port}");


//Helper function for prod. ensuring database existence.
using (var context = new KTContext())
{
    context.Database.EnsureCreated();
}

app.Run();
