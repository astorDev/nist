global using Microsoft.EntityFrameworkCore;
global using Nist;
global using Persic;

var builder = WebApplication.CreateBuilder(args);
var app = await VArticle.Main(builder);
app.Run();