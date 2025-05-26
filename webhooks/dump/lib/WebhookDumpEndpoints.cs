using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Nist;

public static class WebhookDumpEndpoints
{
    private static readonly HashSet<string> registeredPostPaths = [];
    private static readonly HashSet<string> registeredGetPaths = [];

    public static void MapWebhookDump<TDb>(
        this WebApplication app,
        string postPath = "/webhooks/dump/{*extra}",
        string getPath = "/webhooks/dump"
    ) where TDb : DbContext, IDbWithWebhookDump
    {
        app.UseRequestBodyStringReader();

        if (!registeredPostPaths.Contains(postPath))
        {
            app.MapWebhookDumpPost<TDb>(postPath);
            registeredPostPaths.Add(postPath);
        }

        if (!registeredGetPaths.Contains(getPath))
        {
            app.MapWebhookDumpGet<TDb>(getPath);
            registeredGetPaths.Add(getPath);
        }
    }

    public static void MapWebhookDumpPost<TDb>(
        this IEndpointRouteBuilder app,
        string postPath = "/webhooks/dump/{*extra}"
    ) where TDb : DbContext, IDbWithWebhookDump
    {
        app.MapPost(postPath, async (HttpContext context, TDb db) =>
        {
            var record = new WebhookDump
            {
                Path = context.Request.Path.ToString(),
                Body = JsonDocument.Parse(context.GetRequestBodyString()),
                Time = DateTime.UtcNow
            };

            db.WebhookDumps.Add(record);
            await db.SaveChangesAsync();
            return record;
        });
    }

    public static void MapWebhookDumpGet<TDb>(
        this IEndpointRouteBuilder app,
        string getPath = "/webhooks/dump"
    ) where TDb : DbContext, IDbWithWebhookDump
    {
        app.MapGet(getPath, async (TDb db, [FromQuery] int limit = 100) =>
        {
            return await db.WebhookDumps
                .OrderByDescending(x => x.Id)
                .Take(limit)
                .ToArrayAsync();
        });
    }
}

public class WebhookDump
{
    public int Id { get; set; }
    public required string Path { get; set; }
    public required JsonDocument Body { get; set; }
    public required DateTime Time { get; set; }

    public static WebhookDump From(HttpContext context) => new()
    {
        Path = context.Request.Path.ToString(),
        Body = JsonDocument.Parse(context.GetRequestBodyString()),
        Time = DateTime.UtcNow
    };
}

public interface IDbWithWebhookDump
{
    DbSet<WebhookDump> WebhookDumps { get; set; }
}