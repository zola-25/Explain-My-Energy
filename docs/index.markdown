---
# Feel free to add content and custom Front Matter to this file.
# To modify the layout, see https://jekyllrb.com/docs/themes/#overriding-theme-defaults
title: Home
layout: default
nav_order: 1
---


# Explain My Energy Documentation


## Introduction

This is the documentation for the **Explain My Energy** project. It is intended to be used with the Explain My Energy web app.

The link to the live app is listed below, although if you want to dive straight in, it's recommended to have a look at the pre-configured demo version first, available at [demo.explainmyenergy.com](https://demo.explainmyenergy.com).

**Explain My Energy** is an open-source hobby project inspired by some of the ideas of a B2B-focused UK startup, that didn't quite make it to version 1.0.

The goal is to provide a consumer-focused, free, easy-to-use tool for users to understand their home energy usage -  forecasting future energy bills and seeing how seasonal temperature changes affects energy costs.

It is only functional for UK users, but the code could easily be extended to support energy usage data from any source.

- For a pre-configured example setup, see [demo.explainmyenergy.com](https://demo.explainmyenergy.com)

- The source code for this project is available on [GitHub](https://github.com/zola-25/Explain-My-Energy)

- The live version of the app is available at [explainmyenergy.com](https://explainmyenergy.com)


## Purpose

While many energy providers provide their customers with access to their energy usage data, this data is often presented in a way that is difficult to understand. This makes it hard for people to understand how they are using energy and what they can do to reduce their energy usage.

Explain My Energy helps users access their data through the consumer service provided by [n3rgy.com](https://n3rgy.com/). n3rgy provide access to energy usage data for a number of energy providers in the UK.

Once users have access, the application automates the download of their energy usage data, analyses it, and provides both high-level and detailed information about their energy usage. This includes:

- Detailed breakdowns of historical, current and future energy costs and consumption
- How much costs change with weather changes - change the forecast, and see how much your energy usage spikes (or drops) 
- Powerful charting tools that allow you to see everything from granular hourly energy spikes, to whole seasonal energy costs, simply with a mouse scroll (or a swipe)

<p align="center" width="100%">
    <img src="assets/img/Demo1.gif" width="700" height="450" alt="Analysis Demo gif" />
</p>


## How it works

Explain My Energy is an entirely browser-based application. All data is processed and stored in the browser, and no data is sent to any server.

Users need to know their smart meter IHD ('In-Home Device') ID (usually called the MAC ID). This is a 12-digit number that is often printed on the back of the smart meter IHD or accessed through the IHD menu. A useful page with help for finding your IHD MAC ID is provided by howz.com: [howz.com/mac](https://www.howz.com/mac)

They also need to know their electricity and/or gas MPAN and MPRN numbers. These are 13-digit and 10-digit numbers respectively that are printed on their energy bills. 

However, the application also provides easy access to lookup services that allows users to find these numbers by entering their postcode and house number.

With these details, users can access their energy usage data through the n3rgy consumer service. The application helps guide the users through the process of authorizing access to the n3rgy service.

Users also provide their postal area, (the first 3 or 4 characters of their postcode), allowing the application to look up historical and forecast weather data for their area. This is used to show how weather affects their energy usage, and forecast their consumption and costs into the future. We use the open-source and non-commerical weather and climate API provided by [Open-Meteo](https://open-meteo.com/)

Optionally, users can also provide their energy tariff details. Since energy tariffs in the UK have been mostly static over the past two years due to the Energy Price Cap and Energy Price Guarantee, the application provides sensible default historical and present-day tariffs based on the national average at the time. However, as energy prices have now fallen, if users know their tariff details, they can enter them for more accurate cost forecasts. 

## Browser Support

The application should be fully functional on up-to-date desktop browsers. It has been tested on the latest desktop versions of Chrome and Firefox.

The application currently makes extensive use of browser local storage and the initial app download size is quite large. Currently *it may not be functional on some mobile browsers*, as these browsers usually have comparatively smaller local storage and cache limits. 

There is potential to migrate the storage to use IndexedDB, which would allow the application to support more mobile browsers.

## Caveats, precautions and what we can't do

The application is designed to be as accurate and easy-to-use as possible, but there are some caveats to be aware of:

{: .warning }
When using the app, your energy usage data, smart meter IHD MAC ID, and meter serial numbers are stored in your browser's local storage and not encrypted. Other sites, or applications, cannot access this data, but if someone has access to your device, or your device is compromised with malware, this data could be accessed.

{: .note-title }
> Other Data
> 
> - The application does not ask for, or store, any personal details, such as your name, address, email address, on your browser or on any server. 

{: .note-title }
> Energy Data Availability
> 
> - The application is designed to work with smart meters that are connected to the n3rgy consumer service. This is a service that is provided by n3rgy to energy providers, and is not available to all energy providers. If your energy provider does not provide data to n3rgy, then you will not be able to use this application.
> 
> - The data provided by n3rgy is not always up-to-date. There may be a lag of days or even weeks between the data being collected by the smart meter and it being available through the n3rgy consumer service. This means the application may not be able to show you your most recent energy usage data.
>
> - There may limited amount of historical data available. If you've only recently moved into a property, or your energy provider has only recently started providing data to n3rgy, then there may be limited historical data available. This makes it difficult to provide accurate forecasts of future energy usage and costs.
