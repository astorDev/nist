yac:
	httpyac send $(Name).http --all

test:
	dotnet run &
	sleep 5
	make yac
	make kill

kill:
	kill `lsof -t -i :5155`