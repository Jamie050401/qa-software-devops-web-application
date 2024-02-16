[![main](https://github.com/Jamie050401/qa-software-devops-web-application/actions/workflows/main-push.yml/badge.svg?branch=main)](https://github.com/Jamie050401/qa-software-devops-web-application/actions/workflows/main-push.yml)
# QA Web Application

This project has been created for my degree apprenticeship (Bsc Digital Technology and Solutions) as part of the Software Engineering and DevOps module.

## Summary of the Application

The application itself has been developed in ASP.NET Core atop .NET 8 with C# as the language driving the backend. It also makes use of SQLite for the database of choice (limited to four tables as per the assignment brief).

Since I work for a financial organisation, I have elected to develop a web application that provides an online journey for a fake bond product. A customer would choose to invest their money in a selection of funds, with the intention of seeing that investment grow based on the funds performance. As such, this application exists to provide projections based on the initial investment amount and other factors associated with the fund i.e. the growth rate which would be derived based on the historic and expected performance of the fund. Of course, since the application only approximates a bond journey, the calculations under the hood will be rather simplistic, but will convey the general idea.  

Through using ASP.NET Core, most, if not all of the CSS and JavaScript, is obscured under the hood through the use of Bootstrap and other libraries. Therefore, there is a greater focus on the backend logic within the application.

The "production" application has been deployed as a docker container, available at: https://qawa.jnetworks.ovh/

...

## Dependencies

...

## Installation/Execution

### Locally

...

### Docker

...
