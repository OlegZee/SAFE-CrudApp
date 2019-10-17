# Good shape application

## Prerequisites

* nodejs 10.x or above
* dotnet SDK 3.0+

## How to build

Use `fake run build` command to build an application.
`fake run tests` would run e2e test suite.

## Setup notes

Use `fake run` command to start the server in development mode.
Use one of the following accounts to access the server:

* admin:123 - gives access to all records
* manager:234 - allows to create/update users
* demo:demo - connect as a regular user

> You could only login under priviledged account from the localhost when server is running in development mode.