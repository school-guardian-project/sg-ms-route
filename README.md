# sg-ms-route

## Docker listeners

Compose keeps HTTP/1 on port 8080 for Kong and enables HTTP/2 without TLS on
port 5001 for internal gRPC clients. Publishing a port alone does not make
Kestrel listen on it. The listener URLs can be overridden with the corresponding
`Kestrel__Endpoints__Http__Url` and `Kestrel__Endpoints__Grpc__Url` variables.
`Fleet__GrpcAddress` defaults to `http://ms-fleet:5001`; configure
`Fleet__ApiKey` privately when required by fleet.

The internal `GetRouteByExecutionId` RPC resolves the stored execution and its
route so notification alerts can display the actual route name. Missing records
return `Found=false`; historical/inactive trips are still resolvable.

An older database must first apply the existing
`ms-route-db/01-ddl/04-alter/010-add-direction-to-route-execution.sql`
script from the database repository if `Route.RouteExecution.Direction` is absent.

Focused regression:

```shell
dotnet test tests/ms-route.Tests --filter FullyQualifiedName~GetRouteByExecutionIdServiceTests
```

The existing test project currently has unrelated compilation errors in its
older schedule-creation tests. The new lookup tests and existing route/trip
regressions were also run successfully using an isolated temporary test project
referencing the API and the existing fakes, without changing those older tests.
