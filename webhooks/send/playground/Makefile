run:
	dotnet run

up:
	docker compose up -d

yac:
	httpyac send $(NAME).http --all

down:
	docker compose down
	docker volume prune --force

reset:
	make down
	make up

play:
	make up
	bash -c -i 'make run & sleep 5 && make yac NAME=get-dump && fg'

kill:
	kill `lsof -t -i:5195`