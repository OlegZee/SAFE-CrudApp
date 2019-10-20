IF NOT EXIST paket.lock (
    START /WAIT .paket/paket.exe install
)
dotnet restore src/server
dotnet build src/server

dotnet restore src/server.Tests
dotnet build src/server.Tests
dotnet test src/server.Tests
