#!/bin/sh
if [ ! -e "paket.lock" ]
then
    exec mono .paket/paket.exe install
fi
dotnet restore src/server
dotnet build src/server

dotnet restore tests/server.Tests
dotnet build tests/server.Tests
dotnet test tests/server.Tests
