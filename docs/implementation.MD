# Implementation overview

## Client application

The client is an SPA application created with Fable and Elmish.
There's isolated login page that is not part of SPA. That is made to improve startup time.

## Server application

The application is based on Giraffe which is thin layer on top of AspNetCore API.
Server exposes the basic authentication enpoints, REST api endpoint and static resources to deliver client to user browser.

### Database

For simplicity of the setup I do use Sqlite (really bad idea).
Due to poor multiuser performance of Sqlite I'm going to serialize db calls using mailbox processor (similar to Actor in Akka).

Database is stored in the appdata/ folder (not in git).

### Authentication endpoints

The following endpoints serve authentication/signup process:

* GET /login - gets the login page (html)
* POST /login - processes credentials, start user session, stores the session identifier in cookies
* GET /signup - gets the signup page
* POST /signup - registers a new user

### Rest API

TBD