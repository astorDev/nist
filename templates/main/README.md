# Template

.NET [NIST](https://github.com/astorDev/nist) API. To test the functionality holistically use:

```sh
make test
```

Consult [Makefile](./Makefile) for the list of helpful commands. Alternatively, use one of the commands below.

## Running the App 🚀

You can run the app directly:

```sh
dotnet run --project host
```

Or via docker

```sh
docker compose up -d --build
```

## Testing 🧪

You can test the app either via MS Test by running the command below:

```sh
dotnet test tests --logger "console;verbosity=detailed"
```

Or via [httpyac CLI](https://httpyac.github.io/guide/installation_cli):

> These tests are run against a running app instance, so don't forget to run it first

```sh
httpyac send --all tests/*.http --env=local
```
