# Webhooks in .NET 9

> Implement a Webhook Sender with .NET and PostgreSQL: A Comprehensive Guide

Webhooks are practically the only option to build an eventually consistent, event-based communication between servers without relying on any shared resouce or service. However, .NET does not provide much tooling or guidance on implementing them. This artilcle is another attempt of mine to fixing this unfrairness. In [the previous article]() I've covered webhook testing, this time we'll build a solution for a server sending webhooks. Let's get going!

> Or jump straight to the [TLDR;](#tldr) in the end of this article

## TLDR;
