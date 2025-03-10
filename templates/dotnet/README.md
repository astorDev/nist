# Template

.NET [NIST](https://github.com/astorDev/nist) API.

## Running the App 🚀

You can run the app directly:

```sh
dotnet run --project host
```

Or via docker

```sh
docker compose up -d
```

## Testing 🧪

An aggregated test can be run using the helper script file:

```sh
sh test.sh
```

The script will run .NET tests, deploy the app via docker compose and run httpyac scripts against the running instance. You can also run all of the tests individually. To test via .NET tests use:

```sh
dotnet test tests
```

To test via [httpyac CLI](https://httpyac.github.io/guide/installation_cli) use:

> This tests are run against a running app instance, so don't forget to run it first

```sh
httpyac send --all tests/*.http --env=local
```