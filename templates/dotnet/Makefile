run:
	dotnet run --project host

about:
	make yac NAME=about

yac:
	httpyac send tests/$(NAME).http --all --env=local

mstest:
	dotnet test tests --filter FullyQualifiedName~"Template.Tests.$(NAME)" --logger "console;verbosity=detailed"

play:
	bash -c -i 'make run & sleep 5 && make yac && fg'

build:
	docker compose build

up:
	docker compose up -d

down:
	docker compose down

kill:
	kill `lsof -t -i:1111`

test:
	make mstest
	make build
	make up
	sleep 1
	make yac
	make down