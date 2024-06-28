---
type: article
status: draft
---

# ASP .NET Core Observability Solution With Elastic Stack

Today we'll discuss a simple, yet powerful way to enhance the observability of any ASP .NET Core app. Earlier I wrote articles about [Request-Response Logging]() in ASP .NET Core, [JSON Logging]() in .NET, and [Exporting Logs]() from Docker to Elastic Stack. This time, I'll combine those articles and set up Kibana, as a cherry on top. Don't worry, I'll start from scratch and show you all the necessary steps, so you don't **have** to read the previous texts. But feel free to check out those articles, as they go in-depth on the topic and shed light on how architectural choices are being made.

![Preview of Dashboards](dotnet-elastic-thumb.png)

## The ASP .NET Core app

## Logs Export

## Kibana Dashboards

We'll build an example panel to get a grasp of how to create one. 

As you may see, although creating a panel is trivial, the text explanation of it is surprisingly tedious. Instead, I've prepared http request files importing complete dashboards.


First, let's create a dashboard for a single service:

> The http files can be run using VS Code and [HttpYac extension](https://marketplace.visualstudio.com/items?itemName=anweber.vscode-httpyac). Or just used as a reference for tools like Postman.

![Download, run, and see dashboard](single-board-creation.gif)

and combined [for all APIs]():

![](combined-board-creation.gif)

## Finalizing

We've ended up with two simple yet informative dashboards:

![](single-board.png)

![](combined-board.png)

Besides that, we've got request-response logs that can be searched and used to build practically any dashboard you can imagine. Pleasantly enough, that is done without creating any dependency between ASP .NET Core and Elasticsearch. You can also use just parts of the solution and even exchange Elastic with a completely different Observability stack (if it is able to import json for structured logs).

Having that said... Claps are appreciated! 👉👈
