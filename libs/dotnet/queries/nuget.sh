nuget pack
cd bin/Debug
dotnet nuget push Nist.Queries.1.0.0.nupkg --api-key <NUGET_API_KEY> --source https://api.nuget.org/v3/index.json