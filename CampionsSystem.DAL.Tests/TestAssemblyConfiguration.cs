using Xunit;

// Each integration fixture migrates a complete isolated PostgreSQL schema. Bound
// concurrent fixtures so a default local server's lock table is not exhausted.
// Intentional competing transactions inside concurrency tests remain parallel.
[assembly: CollectionBehavior(MaxParallelThreads = 2)]
