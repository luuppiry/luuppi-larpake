![image](https://github.com/user-attachments/assets/02668fb1-df04-47ac-91d2-2f3162df7630)

# Lärpäke
Electronical Luuppi Lärpäke. Track who attends the most events in their freshman year!

## About

This repository contains implementation for the new generation of freshman event attendance cards (aka Lärpäke). 

Luuppi is student organization for computer science, mathematics and statistical data analysis students in Tampere University Finland. 

Fuksilärpäke is traditional part of freshman year for students in Luuppi. Fresmen compete on who gathers the most points during their freshman year. You can get points by fro example attending different events organized by Luuppi and other university organizations. Those with the most points are awarded with different prizes at the Luuppi Christmas party.  

## Technologies

### Web API

`ASP.NET Core` `.NET9` `C#` `C# 13` `Web API` `Controllers` `REST` `JWT` `OpenApi Scalar` `Dependency Injection`

In the heart of the Lärpäke is the REST api implemented using ASP.NET Core Web API with controllers. Api is implemented using RESTful architechture and fro easier use, OpenApi Scalar is used to visualize api endpoints and their request data. Authentication and authorization are implemented using json web tokens (JWT). As usual, services are managed by dependency injection (DI). 


#### Database
`SQLite` `Dapper`

The web API uses SQLite database to store all the data created by user clients. Dapper ORM is used in the C# code to map sql queries into.
For those interested, database graphs can be found `Wiki` -pages under `Database Graphs` ([here](https://github.com/henrivain/Larpake/wiki/Diagrams)).

#### Api Events

`SSE`

Web api serves Server Sent Events (SSE) for subscribed clients when user's event attendance is marked as completed hereby causing UI update. 

### Front End

Front end technologies are in the works. It will probably be web UI and not a mobile application at first.
For UI design drawings see `Wiki` -> `Design` ([here](https://github.com/henrivain/Larpake/wiki/Design)).

## Project Timeline

This project is meant to be production ready for Freshmen starting their studies in fall 2025. Front end development will start soon.  



### Client
TODO: Choose which one of the millions of js frameworks to use

## Contributors
@henrivain

## Support & Features

For support and feature requests open issue in Github (see `Issues` -page)
You can also email at
matikkaeditorinkaantaja(at)gmail.com (don't ask why this is one of my email addresses)
