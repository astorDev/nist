run:
	dotnet run

yac:
	httpyac send .http --all 

play:
	bash -c -i 'make run & sleep 5 && make yac && fg'

kill:
	kill `lsof -t -i:5058`