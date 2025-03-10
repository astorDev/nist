dotnet test tests --logger "console;verbosity=detailed"

docker compose up -d --build
sleep 3

httpyac send --all tests/*.http --env=local