---
layout: default
title: Local Weather
nav_order: 7
parent: Setup
---

# Local Weather

The application uses the Open-Meteo API service provided from [Open-Meteo](https://open-meteo.com/), to retrieve accurate near-term and long-term weather data specific to your location. 

Using this data, the application can calculate your typical energy usage for different weather conditions, and if you add your Tariff Rates, near-term and long-term climate forecasts can predict your future energy costs, well beyond a typical 7-day Met Office forecast.

The application includes the ability to manually adjust future temperature forecasts, to see how much your energy usage and costs are affected by dramatic changes in weather conditions.

This enables cost forecast for entire seasons, and to understand how much upcoming winter weather could cost you. It may help you identify the best opportunities to switch energy providers or tariffs, or consider what cost reductions you may gain from implementing energy reduction measures such as home insulation.

## Weather Data Status

As soon as your setup your household details described at [Household Setup](./household.markdown), including adding your local 3 or 4 digit Postal Area, the application will retrieve both historical and forecast weather data for your area. 

Once you have setup and authorized your Gas and/or Electricity meters, a detailed breakdown can be viewed for each meter from the Sidebar menu. The full documentation for this is available here: [Meter Analysis](../Analysis/analysis-options.markdown).

You can see a simple summary of the application's weather data for your location in the Setup page. Usually on every revisit to the application, the weather data should be automatically updated, but if for any reason its not (such as application malfunction or more likely an internet connection issue), you can see the current status of the app's weather data in the *Weather Data Status* section on the Setup page.

![Weather Data Status](../assets/img/setup/WeatherDataStatus.png)

A Latest Reading of today's or yesterday's date, along with the green tick, usually indicates everything is up-to-date. If the date is older than this, or the short-term, near-term and climate forecast dates appear to be out-of-date, first click the *View Data* button to bring up a page with all the weather data stored for your location, in a sortable table.

## Weather Data Table

This table shows all the weather data stored for your location, including historical data, near-term and long-term forecasts.

This table is not an essential feature, and viewing your weather data on the *Meter Analysis* pages is usually more useful, but it can be useful to see the raw data, and you can sort the dates by the daily Weather Summaries or highest and lowest temperatures, but in the past or in future forecasts.

More essential is the *Load/Reload Data* button. This will force the application to retrieve the latest weather data for your location. This can take a few seconds, and you should see the table update with the latest data, and on navigating back to the Setup page, the *Weather Data Status* section will be up-to-date.

![Weather Data Table](../assets/img/setup/WeatherDataTable.png)
