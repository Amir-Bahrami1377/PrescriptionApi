using Xunit;

// CustomWebApplicationFactory has to inject its Testcontainers connection strings through process-
// wide environment variables before Program.cs starts. Two factories running concurrently would
// overwrite each other's values and migrate the wrong transient database.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
