---
# Feel free to add content and custom Front Matter to this file.
# To modify the layout, see https://jekyllrb.com/docs/themes/#overriding-theme-defaults
title: Explain My Energy Documentation
layout: default
nav_order: 1
---


# Explain My Energy Documentation

## Introduction

This repository contains the documentation for the Explain My Energy project. It is intended to be used with the [Explain My Energy website](https://explainmyenergy.net/).

## About Explain My Energy

Explain My Energy is a free, open-source project that aims to help people understand their energy usage, both historically, in real-time and in the future. 

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

Users need to know their smart meter IHD ID (usually called the MAC ID). This is a 12-digit number that is printed on the back of the smart meter IHD. 

They also need to know their electricity and/or gas MPAN and MPRN numbers. These are 13-digit and 10-digit numbers respectively that are printed on their energy bills. 

However, we provide easy access to lookup services that allows users to find these numbers by entering their postcode and house number.

With these details, users can access their energy usage data through the n3rgy consumer service. The application helps guide the users through the process of authorizing access to the n3rgy service.

Users also provide their postal area, (the first 3 or 4 characters of their postcode), allowing the application to look up historical and forecast weather data for their area. This is used to show how weather affects their energy usage, and forecast their consumption and costs into the future.  

Optionally, users can also provide their energy tariff details. Since energy tariffs in the UK have been mostly static over the past two years, the application provides sensible default historical and present-day tariffs based on the national average at the time. However, if users know their tariff details, they can enter them. 

Energy tariffs are used to calculate the energy costs for the user, and forecast their future costs.

## Caveats and what we can't do

The application is designed to be as accurate as possible, but there are some caveats to be aware of:

- The application is designed to work with smart meters that are connected to the n3rgy consumer service. This is a service that is provided by n3rgy to energy providers, and is not available to all energy providers. If your energy provider does not provide data to n3rgy, then you will not be able to use this application.

- The data provided by n3rgy is not always up-to-date. There may be a lag of days or even weeks between the data being collected by the smart meter and it being available through the n3rgy consumer service. This means the application may not be able to show you your most recent energy usage data.

- There may limited amout of historical data available. If you've only recently moved into a property, or your energy provider has only recently started providing data to n3rgy, then there may be limited historical data available. This makes it difficult to provide accurate forecasts of future energy usage and costs.


