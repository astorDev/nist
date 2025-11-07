TEST ?= main

run:
	dotnet run

kill:
	kill `lsof -t -i:11891`

yac:
	httpyac send $(TEST).http --all

play:
	bash -c -i 'make run & sleep 5 && make yac && kill -INT %1'

v-use:
	cp Versions/$(V)/Program.cs Program.cs

v-save:
	mkdir -p Versions/$(V) && cp Program.cs Versions/$(V)/Program.cs

v-play:
	make v-use
	make play

test:
	dotnet test --filter FullyQualifiedName~Nist.Crud.Playground.$(NAME) --logger "console;verbosity=detailed"