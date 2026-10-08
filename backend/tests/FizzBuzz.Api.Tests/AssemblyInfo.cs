// Program.cs installe un logger Serilog global (bootstrap logger) qui ne peut être figé qu'une fois par processus :
// démarrer plusieurs hôtes de test en parallèle provoque "The logger is already frozen".
[assembly: CollectionBehavior(DisableTestParallelization = true)]
