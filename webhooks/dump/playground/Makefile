yac:
	httpyac send $(Name).http --all

test:
	bash -c 'dotnet run & pid=$$!; sleep 5; make yac; kill $$pid'