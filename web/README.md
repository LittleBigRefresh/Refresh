# Important: This branch is on feature lock!
This legacy version is to be entirely replaced by a rewrite of `refresh-web`. This branch will stay maintained for the time being as the rewrite matures on the `main` branch, but the legacy version will not receive new features or major upgrades (such as a migration to Angular 17).

You may track progress [here](https://github.com/LittleBigRefresh/refresh-web/tree/main).

# Refresh Website

This is an eternally work-in-progress project intending to build a proper frontend for [Refresh](https://github.com/LittleBigRefresh/Refresh).

![A look at the front page](https://github.com/LittleBigRefresh/refresh-web/assets/40577357/440a45f1-08c5-4a61-b8dd-0a312e059d72)

Suggestions and criticism regarding design and general layout are welcome!

This project was generated with [Angular CLI](https://github.com/angular/angular-cli).
You can set up an Angular environment using [this guide](https://angular.io/guide/setup-local).

## Development server

Run `ng serve` in a terminal for a development server that listens at `http://localhost:4200/`.
The page will automatically refresh itself when source code is changed.

Make sure you are also running Refresh *(or any other server compatible with Refresh's APIv3)* at `http://localhost:10061` or else the website will not function.
You can also point the `environment.development.ts` to use the official production server.
