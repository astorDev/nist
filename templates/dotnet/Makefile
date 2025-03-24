run:
	dotnet run --project host

about:
	httpyac send tests/about.http --all --env=local

yac:
	httpyac send tests/*.http --all --env=local

mstest:
	dotnet test tests --logger "console;verbosity=detailed"

play:
	bash -c -i 'make mstest && make run & sleep 5 && make yac && fg'

up:
	docker compose up -d --build

down:
	docker compose down

kill:
	kill `lsof -t -i:1111`

test:
	make mstest
	make up
	sleep 1
	make yac
	make down