run:
	dotnet run

kill:
	kill `lsof -t -i:5208`

yac:
	httpyac send $(NAME).http --all

play:
	bash -c -i 'make run & sleep 5 && make yac && kill -INT %1'