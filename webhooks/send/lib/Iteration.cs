using Backi;
using OneOf;

namespace Nist;

public interface IWebhookStore<TRecord> where TRecord : IWebhook
{
    Task RunTransactionWithPendingRecords(Func<TRecord[], Task> changes, CancellationToken cancellationToken);
    Task HandleResult(TRecord record, OneOf<HttpResponseMessage, Exception> result);
}

public class WebhookSendingIteration<TRecord>(IWebhookStore<TRecord> store, WebhookSender sender, ILogger<WebhookSendingIteration<TRecord>> logger) 
    : IContinuousWorkIteration where TRecord : IWebhook
{
    public async Task Run(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting webhook sending iteration");

        await store.RunTransactionWithPendingRecords(async pendingRecords => {
            var processingTasks = pendingRecords.Select(async record => {
                var result = await sender.Send(record);
                await store.HandleResult(record, result);
            });

            await Task.WhenAll(processingTasks);
        }, stoppingToken);

        logger.LogInformation("Webhook sending iteration finished");
        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
    }

    public static async Task OnException(Exception ex, ILogger logger)
    {
        logger.LogError(ex, "Error in webhook sending");
        await Task.Delay(TimeSpan.FromSeconds(5));
    }
}

public static class WebhookSendRegistration
{
    public static IServiceCollection AddWebhookSending<TRecord>(this IServiceCollection services, Func<IServiceProvider, IWebhookStore<TRecord>> storeFactory)
        where TRecord : IWebhook
    {
        services.AddHttpClient();
        services.AddScoped<WebhookSender>();
        services.AddScoped(sp => storeFactory(sp));
        services.AddContinuousBackgroundService<WebhookSendingIteration<TRecord>>();

        return services;
    }
}