# Expense recorder application

The application demonstrates what can be accomplished in 30-40 hours with SAFE stack with a very basic knowledge of the SAFE.
Application features:

* Giraffe based backend
* using Postgres with F# Data Providers for persisting app data
* simplistic REST Api
* authentication and authorization
* sharing F# code between client and server to ensure contracts
* client routing to ensure application is navigation friendly
* Fulma to build nice looking UI with a very basic HTML design skills

I would love to use some newer things to simplify say client-server communication (kudos to Zaid Ajaj) but I was asked to expose REST Api.

## Install pre-requisites

You'll need to install the following pre-requisites in order to build SAFE applications:

* The [.NET Core SDK](https://www.microsoft.com/net/download) 3 or newer
* install tools by running `dotnet tool restore` in the project folder
* The [Yarn](https://yarnpkg.com/lang/en/docs/install/) package manager (you an also use `npm` but the usage of `yarn` is encouraged).
* [Node LTS](https://nodejs.org/en/download/) installed for the front end components.
* If you're running on OSX or Linux, you'll also need to install [Mono](https://www.mono-project.com/docs/getting-started/install/).

## How to build

* setup local Postgresql and roll out "expenses" database
* restore database schema by running `psql -d expenses -a -f create-db.sql` from src/server folder
* correct connection settings in src/server/DataAccess.fs
* build using `dotnet fake build` command line

Now you could run the application:

```bash
cd src/server
dotnet run
```

* now navigate to [http://localhost:8085]
* if the database is new connect under admin/\<pwd\>, where pwd is some number you can find in server logs

`fake build target Run` starts development mode.
