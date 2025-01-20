## Installation

Use the command below you can install the .NET Template:

```sh
dotnet new install Nist.Template
```

## Usage

Running the command below will add initial project files from the nist template:

```sh
dotnet new nist 
```

This is what the file structure will look after:

```text
📁 protocol
📁 tests
📁 webapi
📄 compose.yml
📄 <folder-name>.sln
```

> 🤓 .NET project names are usually in PascalCase, while repository folders are typically in camelCase or similar. The easiest way to achieve it is by using .NET name while creating project via the template: `dotnet new nist --name My.CoolProject` and then renaming the folder. 

### Ensuring everything works fine

After the files are added, you should be able to successfully run the tests with:

```sh
cd tests && dotnet test
```

You should also be able to run the app via:

```sh
cd webapi && dotnet run
```

Assuming you have [httpyac CLI installed](https://httpyac.github.io/guide/installation_cli) In another terminal session from the `tests` folder run:

```sh
cd tests && httpyac send --all *.http --env=local
```

And you should get something resembling this:

![](/templates/httpyac-demo.png)

### Pack & Publish the Template NuGet

```
dotnet pack 
```

```sh
cd bin/Release
```

```sh
dotnet nuget push Nist.Template.$VERSION.nupkg -s https://api.nuget.org/v3/index.json -k $NUGET_API_KEY
```