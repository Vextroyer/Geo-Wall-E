.Phony: dev
.Phony: repl
.Phony: test

test:
	dotnet run --project CommandLineInterface --runTest

repl:
	dotnet run --project CommandLineInterface --runRepl --noDebug

dev:
	dotnet run --project CommandLineInterface