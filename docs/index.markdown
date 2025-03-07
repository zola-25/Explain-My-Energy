---
# Feel free to add content and custom Front Matter to this file.
# To modify the layout, see https://jekyllrb.com/docs/themes/#overriding-theme-defaults
title: Home
layout: default
nav_order: 1
---


# Explain My Energy Documentation

{: .warning }
> *As the n3rgy consumer API has been shut down, this project has been deprecated*
>
> *The demo site remains live -* [demo.explainmyenergy.net](https://demo.explainmyenergy.net)

## Introduction

This is the documentation for the **Explain My Energy** project. It is intended to be used with the Explain My Energy web app.

The link to the live app is listed below, although if you want to dive straight in, it's recommended to have a look at the pre-configured demo version first, available at [demo.explainmyenergy.net](https://demo.explainmyenergy.net).

**Explain My Energy** is an open-source hobby project inspired from some of the ideas of a B2B-focused UK startup, that didn't quite make it to version 1.0.

The goal is to provide a consumer-focused, free, easy-to-use tool for users to understand their home energy usage -  forecasting future energy bills and seeing how seasonal temperature changes affects energy costs.

It is only functional for UK users, but the code could easily be extended to support energy usage data from any source.

- For a pre-configured example setup, see [demo.explainmyenergy.net](https://demo.explainmyenergy.net)

- The live version of the app is available at [explainmyenergy.net](https://explainmyenergy.net)

- The source code for this project is available on [GitHub](https://github.com/zola-25/Explain-My-Energy)


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

The application should be fully functional on up-to-date desktop browsers. It has been tested on the latest desktop versions of Chrome, Firefox, Edge and Safari*.

The application currently makes extensive use of browser local storage and the initial app download size is quite large. Currently *it may not be functional on some mobile browsers*, as these browsers usually have comparatively smaller local storage and cache limits. 

However both the main app and the demo site have been successfully tested on the iPhone 13* and Galaxy S8, although older devices, or those with out-of-date OS versions may have issues. For iOS ideally use the latest version (17.4 at time of writing), and likewise for Android (version 14 at time of writing). Your mileage may vary.

There is potential to migrate the storage to use IndexedDB, which should allow more support for mobile browsers.

{: .note-title }
> \*WebKit Browsers
> 
> WebKit based browsers such as Safari currently exhibit intermittent issues during testing, regardless of platform, but appear to be working fine in the wild.
> 
 
## Caveats, precautions and what we can't do

The application is designed to be as accurate and easy-to-use as possible, but there are some caveats to be aware of:

{: .warning }
> When using the app, your energy usage data, smart meter IHD MAC ID, and meter serial numbers are stored in your browser's local storage and not encrypted. Other sites, or applications, cannot access this data, but if someone has gained access to your device, or your device's security is compromised (with malware for example), this data could be accessed.
> 
> Without any assumption of your IT knowledge, standard device security measures apply - don't set up this app with your energy details on a publicly shared device or a device you don't trust, and keep your device, anti-virus and browser up to date (especially Windows devices - but this is likely all enabled by default).
>
> There is now the ability to 'Lock' the application when not in use, which will encrypt sensitive data and prevent access to the application without a password. This provides an additional layer of security, but is not a substitute for good device security. See the page [App Locking](./setup/app-data-locking) for more details.



{: .note-title }
> **Other Data**
> 
> - The application does not ask for, or store, any personal details, such as your name, address, email address, on your browser or on any server. 
> - Diagnostic data is not currently collected, but may be in the future. This data is anonymous and will only contain data on application errors and usage, will not record any user-related data such as energy readings. It will be opt-in only.
> - The application does not use any third-party analytics, advertising or tracking services.
> - For all these details in more explicit form, see the Privacy Policy either on the [demo site](https://demo.explainmyenergy.net/PrivacyPolicy) or the [main application](https://explainmyenergy.net/PrivacyPolicy). They are identical.

{: .note-title }
> **Energy Data Availability**
> 
> - The application is designed to work with smart meters that are connected to the n3rgy consumer service. This is a service that is provided by n3rgy to energy providers, and is not available to all energy providers. If your energy provider does not provide data to n3rgy, then you will not be able to use this application.
> 
> - The data provided by n3rgy is not always up-to-date. There may be a lag of days or even weeks between the data being collected by the smart meter and it being available through the n3rgy consumer service. This means the application may not be able to show you your most recent energy usage data.
>
> - There may limited amount of historical data available. If you've only recently moved into a property, or your energy provider has only recently started providing data to n3rgy, then there may be limited historical data available. This makes it difficult to provide accurate forecasts of future energy usage and costs.
