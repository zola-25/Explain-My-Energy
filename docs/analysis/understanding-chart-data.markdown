---
layout: default
title: Understanding the Chart Data
nav_order: 1
parent: Analysis
---

# Understanding the Chart Data

The chart makes available all energy data for the meter, including historical data, and daily forecasts several months into the future. 

## Temperature-dependent Energy Chart

For near-term forecasts, the chart forecasts your energy consumption and costs with the forecast average daily temperature. The chart displays this and also the forecast weather summary. For long-term forecasts (more than 10 days), the chart uses the typical climate forecast for your location, which is only the average temperature for each day.

Known, historical energy data is displayed as a solid line with fill, and forecast data is displayed as a dashed line with no fill. 

![Chart Forecast vs Historical Display](../assets/img/analysis/ChartForecastVsHistoricalDisplay.png){:width="70%"}

When the chart is zoomed out, the daily temperature readings are averaged depending on the level of zoom. For example, when zoomed out to a year, the daily temperature readings are averaged to a monthly average temperature. 

Zooming in further will average the daily temperature readings to a weekly average temperature, and then to a daily average temperature at the maximum zoom level. 

The averaged temperature readings displayed have no effect on the forecast energy consumption and costs, which are always calculated using the actual daily temperature readings.

## Temperature-independent Energy Chart

For the meter that is not set as your Heating meter (or 'Primary Heating source'), the chart does not use temperature data, and the energy consumption and costs are not affected by temperature. Forecasts are always based on historical consumption.

