---
type: article
status: draft
---

# Docker Logs in Elasticsearch from scratch.

Elastic Stack still doesn't seem to have a good alternative when it comes to working with structured logs. Trust me, I've spent a lot of time looking for one. It provides a self-hosted solution, has extensive tooling, and a wide range of integrations. 

> Well, there's one valid alternative: OpenSearch. But it is a fork, so I'm not sure if it counts. And it seems to have all the same problem Elastic have with a few on top.

And what could be the most fruitful source of logs if not docker? In this article, we will fire up an elastic stack and ship logs from docker there. So, get on board!

![An AI-generated ship with logs](docker-export-thumb.png)

## Firing Elastic Stack up

## Shipping logs

## Making logs cool

## Wrapping up

In this article, we've created a complete solution for exporting structured logs from docker to elastic stack. We've deployed Elasticsearch and Kibana using docker-compose. Then we deployed filebeat, shipping logs from docker log files to elastic. Finally, we've improved our shipping pipeline to utilize structured logging!

You can check out the final solution [here](). 
Thank you for reading! By the way... claps are appreciated 👉👈
