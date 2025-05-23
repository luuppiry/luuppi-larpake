[![Build and Deploy](https://github.com/luuppiry/luuppi-larpake/actions/workflows/production.yaml/badge.svg)](https://github.com/luuppiry/luuppi-larpake/actions/workflows/production.yaml)

# Lärpäke
Electronical Luuppi Lärpäke. Track who attends the most events in their freshman year!

## About

<img src="https://github.com/user-attachments/assets/02668fb1-df04-47ac-91d2-2f3162df7630" width="320" align="right" alt="Image of Lärpäke" />

This repository contains implementation for the new generation of freshman event attendance cards (aka Lärpäke). 

Luuppi is student organization for computer science, mathematics and statistical data analysis students in Tampere University Finland. 

Fuksilärpäke is traditional part of freshman year for students in Luuppi. Fresmen compete on who gathers the most points during their freshman year. You can get points by fro example attending different events organized by Luuppi and other university organizations. Those with the most points are awarded with different prizes at the Luuppi Christmas party.  

## Getting started

Instructions for running the project can be found on [wiki](https://github.com/luuppiry/luuppi-larpake/wiki) 

### Requirements

Docker + NodeJs (for minimal front end dev setup)

## Technologies

### Web API

<img src="https://github.com/user-attachments/assets/e20ade74-437f-49c9-aa22-6472866b1986" width="60" align="right" alt="c sharp logo" />

`ASP.NET Core` `.NET9` `C#` `C# 13` `Web API` `Controllers` `REST` `JWT` `OpenApi Scalar` `Dependency Injection`

In the heart of the Lärpäke is the REST api implemented using ASP.NET Core Web API with controllers. Api is implemented using RESTful architechture and fro easier use, OpenApi Scalar is used to visualize api endpoints and their request data. Authentication and authorization are implemented using json web tokens (JWT). As usual, services are managed by dependency injection (DI). 


#### Database
`PostgreSQL` `Dapper`

The web API uses PostgreSQL database to store all the data created by user clients. Dapper ORM is used in the C# code to map sql query results into objects. For those interested, database graphs can be found `Wiki` -pages under `Database Graphs` ([here](https://github.com/henrivain/Larpake/wiki/Diagrams)).

#### Api Events

`SSE`

Web api serves Server Sent Events (SSE) for subscribed clients when user's event attendance is marked as completed hereby causing UI update. 

### Front End

`Vite` `Typescript` `CSS` `HTML`

Front end is built with minimal vite + Typescript, HTML, CSS stack. 

For UI design drawings see `Wiki` -> `Design` ([here](https://github.com/henrivain/Larpake/wiki/Design)).

### Authentication

Project uses luuppi's existing Azure Entra Id auth system with minor tweaks.

## Project Timeline

Project is up an running in larpake.luuppi.fi. Project is in beta stage and should be ready to take real users in the the end of the summer in 2025.

## Contributing

Contributions are always welcomed! Project is still at its youth so no contributing guidelines are written so far. If you are interested in improving Lärpäke, contact us! 

## Contributors
@henrivain & @JurnetFIN

## Support & Features

For support and feature requests preferably open a issue in Github (see `Issues` -page)
Or send email to  

henri.vainio(at)luuppi.fi or juliusz.kotelba(at)luuppi.fi

## Bug reports

Send email to webmaster(at)luuppi.fi or open Github issue. If bug is visual, please include screenshot that demonstrates the error. If error might be caused by data fetching (action requires some data which causes error), please add TraceId from console, if you are provided one. If you don't know where to aquire TraceId from, Lärpäke team is always here to help you! 

<details>

<summary>Aika on rahaa?</summary>

### Tähän projektiin kulutettu aika

#### @henrivain
[![wakatime](https://wakatime.com/badge/user/33becafa-9125-4b20-b55e-a824ccc490f1/project/ad9cdb99-5373-46b4-8af9-a789d14f6abc.svg)](https://wakatime.com/badge/user/33becafa-9125-4b20-b55e-a824ccc490f1/project/ad9cdb99-5373-46b4-8af9-a789d14f6abc)

#### @JurnetFIN
[![wakatime](https://wakatime.com/badge/user/c8b15fbf-a210-457e-9da7-a0093d788004/project/0f7f4037-1fcd-45ae-a399-158209116f96.svg)](https://wakatime.com/badge/user/c8b15fbf-a210-457e-9da7-a0093d788004/project/0f7f4037-1fcd-45ae-a399-158209116f96)

</details>
