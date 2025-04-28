- [ ] Nist.Queries
    - [ ] `Search` and `SearchInt` extension methods
- [ ] Nist.Webhooks.Dump <VERSION>
    - [ ] Sort by date desc
    - [ ] Allow limit specification with 100 by default
- [ ] Nist.Webhooks.Sender <VERSION>
    - [x] WebhookSender Endpoints
    - [ ] Repeats
        - [ ] `active_since` and `attempt` column
        - [ ] Requeuing

## Webhook Repeat Action

```ruby
services.addContinuousWebhookSending
    delayCalculator => fibonacci.at @attempt
    maxAttempts => 50
```