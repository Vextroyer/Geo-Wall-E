.Phony: dev
.Phony: repl

repl:
	dotnet run --project CommandLineInterface --runRepl --noDebug

dev:
	dotnet run --project CommandLineInterface