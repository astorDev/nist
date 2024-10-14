# Testing

1. Deploy container

```sh
docker compose up -d --build
```

1. Open a website
1. Open Developer console
1. Run:

```js
fetch('http://localhost:7100/about').then(res => res.json()).then(console.log).catch(console.error);
```