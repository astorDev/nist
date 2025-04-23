run:
	dotnet run

up:
	docker compose up -d

yac:
	httpyac send .http --all

down:
	docker compose down
	docker volume prune --force

reset:
	make down
	make up