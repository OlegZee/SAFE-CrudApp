# Good shape application

## Install pre-requisites

You'll need to install the following pre-requisites in order to build SAFE applications

* The [.NET Core SDK](https://www.microsoft.com/net/download)
* [FAKE 5](https://fake.build/) installed as a [global tool](https://fake.build/fake-gettingstarted.html#Install-FAKE)
* The [Yarn](https://yarnpkg.com/lang/en/docs/install/) package manager (you an also use `npm` but the usage of `yarn` is encouraged).
* [Node LTS](https://nodejs.org/en/download/) installed for the front end components.
* If you're running on OSX or Linux, you'll also need to install [Mono](https://www.mono-project.com/docs/getting-started/install/).

## How to build

```bash
fake run
```

Now you could run the application:

```bash
cd src\server
bin\Debug\netcoreapp3.0\server.exe 
```

`fake build target tests` would run e2e test suite.

## Work with the application

To concurrently run the server and the client components in watch mode use the following command:

```bash
fake build -t Run
```

## Setup notes

Use `fake run` command to start the server in development mode.
Use one of the following accounts to access the server:

* admin:123 - gives access to all records
* manager:234 - allows to create/update users
* demo:demo - connect as a regular user

> You could only login under priviledged account from the localhost when server is running in development mode.