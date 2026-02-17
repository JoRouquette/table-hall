var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/", () => TypedResults.Ok("TableHall API"));

await app.RunAsync();

// requis pour WebApplicationFactory<Program> dans les tests
public partial class Program
{
  protected Program() { }
}
