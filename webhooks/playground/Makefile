prep:
	docker compose up -d

run:
	dotnet run

yac:
	httpyac send .http --all 

play:
	make prep
	bash -c -i `make run & sleep 5 && make yac && fg` 