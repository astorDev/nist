using System.Text.Json;

public record WebhookCandidate(
    JsonDocument Body,
    string Url
);