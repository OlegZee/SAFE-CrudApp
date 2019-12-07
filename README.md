# Good shape application

## Install pre-requisites

You'll need to install the following pre-requisites in order to build SAFE applications

* The [.NET Core SDK](https://www.microsoft.com/net/download)
* [FAKE 5](https://fake.build/) installed as a [global tool](https://fake.build/fake-gettingstarted.html#Install-FAKE)
* The [Yarn](https://yarnpkg.com/lang/en/docs/install/) package manager (you an also use `npm` but the usage of `yarn` is encouraged).
* [Node LTS](https://nodejs.org/en/download/) installed for the front end components.
* If you're running on OSX or Linux, you'll also need to install [Mono](https://www.mono-project.com/docs/getting-started/install/).

## How to build

* setup local Postgresql and roll out "expenses" database
* restore database schema by running `psql -d expenses -a -f create-db.sql` from src/server folder
* correct connection settings in src/server/DataAccess.fs
* build using `fake build` command line

Now you could run the application:

```bash
cd src\server
bin\Debug\netcoreapp3.0\server.exe 
```

* now navigate to [http://localhost:8085]
* if the database is new connect under admin/<pwd>, where pwd is some number you can find in server logs

`fake build target Run` start development mode.

## Setup notes (DRAFT, Not correct)

Use `fake run` command to start the server in development mode.
Use one of the following accounts to access the server:

* admin:123 - gives access to all records
* manager:234 - allows to create/update users
* demo:demo - connect as a regular user

> You could only login under priviledged account from the localhost when server is running in development mode.