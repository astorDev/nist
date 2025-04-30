- [ ] Nist.Queries
    - [ ] `Search` and `SearchInt` extension methods
- [ ] Nist.Webhooks.Dump <VERSION>
    - [ ] Sort by date desc
    - [ ] Allow limit specification with 100 by default
- [ ] Nist.Webhooks.Sender <VERSION>
    - [x] WebhookSender Endpoints
    - [x] Repeats
        - [x] `start_at` and `attempt` column
        - [x] Requeuing

## Webhook Repeat Action

```ruby
services.addContinuousWebhookSending
    delayCalculator => fibonacci.at @attempt
    maxAttempts => 50
```