# sg-ms-route

## Docker listeners

Compose keeps HTTP/1 on port 8080 for Kong and enables HTTP/2 without TLS on
port 5001 for internal gRPC clients. Publishing a port alone does not make
Kestrel listen on it. The listener URLs can be overridden with the corresponding
`Kestrel__Endpoints__Http__Url` and `Kestrel__Endpoints__Grpc__Url` variables.
`Fleet__GrpcAddress` defaults to `http://ms-fleet:5001`; configure
`Fleet__ApiKey` privately when required by fleet.

The gRPC clients also use the existing internal service addresses when no
override is supplied: `http://ms-fleet:5001` and
`http://ms-user-management:5001`. `Fleet__GrpcAddress` and
`Family__GrpcAddress` continue to override them. This applies to direct
container launches as well as Compose; it does not change configured
connections or API keys.

Previously, resolving `RouteDetailController` without these keys threw
`InvalidOperationException` before reading the student assignment. The HTTP
middleware reports such exceptions as 409, so a saved assignment could not be
displayed by either client. A production-registration regression now reproduces
the constructor failure and checks that the controller resolves without opening
a database connection. A deployed 409 still requires its ProblemDetails `detail`
to confirm its exact cause; HTTP status alone is not sufficient.

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

## Student, stop and route assignment

Student assignment uses the IAM profile ID. `RouteStudentAssignments.RouteStopId`
references `RouteStop.Id`, not the physical `Stop.Id`; that link resolves both the
route and the student's stop. Student/parent stop filters compare those link IDs.
New assignments only accept active route-stop links, so an inactive historical
link cannot hide an otherwise successful assignment.

Regression checks (no database connection required):

```shell
dotnet test tests/ms-route.Tests --filter "FullyQualifiedName~RouteStopFilterTests|FullyQualifiedName~AssignStudentToRouteServiceTests|FullyQualifiedName~GetStudentRouteServiceTests|FullyQualifiedName~GetStudentStopOnRouteServiceTests"
```

The full suite passes 118 tests locally, including controller dependency
resolution and assign/read/reassign across student profile, stop, route, bus
and driver. The relationship tests use in-memory repository fakes: they do not
certify real SQL persistence or reachability/authentication of deployed gRPC
services. No schema migration or deployed data change is part of this fix.
