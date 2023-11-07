---
layout: default
title: Analysis Options
nav_order: 2
parent: Analysis
---

# Analysis Options

The options described here allow you to customize the charting and aggregation analysis of your meter readings.

Some options, such as Temperature Forecast vs Historical and Adjusting Temperature Forecast, are only available for the meter set up as your Heating meter (or 'Primary Heating source'), as typically this meter will be affected by seasonal temperature to a much greater extent than the other.

## Chart kWh vs Cost

This switch allows you to toggle between charting your meter readings in kWh or Cost. The default is kWh.

![Chart Daily kWh](../assets/img/analysis/ChartDailyKWh.png){:width="50%"}

When the chart is set to display Cost, the cost is calculated using the tariff rates entered for the meter, and the kWh readings (whether historical or forecast) are converted to cost using these rates. Included with the cost calculation is the Tariff Rate and Standing Charge. Depending on the zoom level, the chart cost may be displayed as a daily or hourly cost.

![Chart Daily Cost](../assets/img/analysis/ChartDailyCost.png){:width="50%"}

## Analysis Term

Changing the Analysis Term has no direct effect on the chart, but it does affect the aggregation analysis. The default is monthly. For example, a monthly analysis term will calculate the historical total kWh, cost and carbon emissions for the previous month, forecast the same for the current month along with the totals used so far, and forecast the amounts for the next month.

Each aggregated term can be zoomed in to display the daily data for that term. For example, zooming in on Last Month's term will set the chart to display the data for the previous month.

![Chart Zoom to Last Month Toggle](../assets/img/analysis/ChartZoomToLastMonthToggle.png){:width="50%"}

![Chart Zoom to Last Month Display](../assets/img/analysis/ChartZoomToLastMonthDisplay.png)

## Temperature Forecast vs Historical

In some cases, forecasting based on historical consumption may be more appropriate that forecasting based on the forecast weather. A typical case would be forecasting during warm summer months, where your gas meter is likely to be used only for cooking and hot water, and not for heating. 

This switch allows you to toggle between forecasting based on historical consumption, or based on the forecast weather. The default is to use the forecast weather.

## Adjusting Temperature Forecast

This input allows you to adjust the future temperature forecast, to see how much your energy usage and costs are affected by dramatic changes in weather conditions. It can be increased or decreased by a degree Celsius at a time, and the chart, along with the aggregation analysis, will update to reflect the new forecast.

You can see the future temperature plot on the chart change, along with the change in kWh and cost, and the change in the current and future totals in the aggregation analysis.

## Analysis Options Status

This table, available on the home page, simply displays the current analysis options set for each meter, assuming the meter has been fully set up with energy readings imported and forecast readings calculated. 

For both Gas and Electricity Meters it displays:

- Whether a Temperature Forecast or Historical Forecast is being used

- The adjusted degrees Celsius for the Temperature Forecast

- The aggregate analysis Term (i.e. Daily, Weekly or Monthly)

- Any analysis Term chart zooms toggled 

- Whether the chart is displaying kWh or Cost.