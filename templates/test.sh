docker compose up -d --build --pull always
sleep 3
curl localhost:1111/about
echo
docker logs templates-dotnet-1
docker stop templates-dotnet-1
docker rm templates-dotnet-1
echo