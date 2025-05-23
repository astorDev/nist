run:
	dotnet run

yac:
	httpyac send $(NAME).http --all 

play:
	bash -c -i 'make run & sleep 5 && make yac && fg'

kill:
	kill `lsof -t -i:5058`

v-use:
	cp Versions/$(V)/Program.cs Program.cs

v-save:
	mkdir -p Versions/$(V) && cp Program.cs Versions/$(V)/Program.cs

v-play:
	make v-use
	make play