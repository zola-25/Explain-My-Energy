import * as am5 from "@amcharts/amcharts5";
import * as am5xy from "@amcharts/amcharts5/xy";
import am5themes_Animated from "@amcharts/amcharts5/themes/Animated";

import { ChartDefaults, MeterChartProfile, TemperaturePoint, ChartDetails } from "./types";

export class WeatherIconChart {

    public create(divId: string, meterChartProfile: MeterChartProfile, temperatureIconPoints: TemperaturePoint[]): ChartDetails {
        // Create root element
        const root = am5.Root.new(divId);
        // Set themes
        root.setThemes([
            am5themes_Animated.new(root)
        ]);

        // Create chart
        const chart = root.container.children.push(am5xy.XYChart.new(root, {
            focusable: true,
            panX: true,
            panY: false,
            wheelX: "panX",
            wheelY: "zoomX"
        }));


        //
        // // Create axes
        const xAxis = chart.xAxes.push(am5xy.DateAxis.new(root, {
            min: meterChartProfile.profileStart,
            max: meterChartProfile.profileEnd,
            groupData: true,
            maxDeviation: 0,
            baseInterval: {
                timeUnit: "minute",
                count: 30
            },
            renderer: am5xy.AxisRendererX.new(root, {}),
            tooltip: am5.Tooltip.new(root, {})
        }));

        const yConsumptionAxisRenderer = am5xy.AxisRendererY.new(root, {
        })
        yConsumptionAxisRenderer.grid.template.set("forceHidden", !meterChartProfile.showCost);

        const yConsumptionAxis = chart.yAxes.push(am5xy.ValueAxis.new(root, {
            ariaLabel: "kWh",
            renderer: yConsumptionAxisRenderer,
            visible: !meterChartProfile.showCost
        }));
        yConsumptionAxis.children.unshift(
            am5.Label.new(root, {
                rotation: -90,
                text: "kWh",
                y: am5.p50,
                centerX: am5.p50
            })
        );

        let seriesIndex = 0;
        // Add series
        const consumptionSeries = chart.series.push(am5xy.LineSeries.new(root, {
            name: `${meterChartProfile.globalId} - Consumption`,
            xAxis: xAxis,
            yAxis: yConsumptionAxis,

            valueYField: "kWh",
            valueXField: "dateTicks",
            valueYGrouped: "sum",
            tooltip: am5.Tooltip.new(root, {
                labelText: "{valueY.formatNumber('#.#')}kWh",
            }),
            fill: ChartDefaults.consumptionColor,
            visible: !meterChartProfile.showCost
        }));
        const consumptionSeriesIndex = seriesIndex++;

        consumptionSeries.getTooltip().set("forceHidden", meterChartProfile.showCost);

        consumptionSeries.strokes.template.setAll({
            stroke: ChartDefaults.consumptionColor,
            strokeOpacity: 0.9,
            strokeWidth: 2
        })

        consumptionSeries.fills.template.setAll({
            fill: ChartDefaults.consumptionColor,
            fillOpacity: 0.5,
            visible: true
        })



        const yCostAxisRenderer = am5xy.AxisRendererY.new(root, {
        })
        yCostAxisRenderer.grid.template.set("forceHidden", !meterChartProfile.showCost);

        const yCostAxis = chart.yAxes.push(am5xy.ValueAxis.new(root, {
            ariaLabel: "Pounds",
            numberFormat: "'£'#",
            maxPrecision: 0,
            visible: meterChartProfile.showCost,
            renderer: yCostAxisRenderer
        }));


        yCostAxis.children.unshift(
            am5.Label.new(root, {
                rotation: -90,
                text: "Cost",
                y: am5.p50,
                centerX: am5.p50
            })
        );

        const costSeries = chart.series.push(am5xy.LineSeries.new(root, {
            name: `${meterChartProfile.globalId} - Cost`,
            xAxis: xAxis,
            yAxis: yCostAxis,
            valueYField: "cost",
            valueXField: "dateTicks",
            valueYGrouped: "sum",
            tooltip: am5.Tooltip.new(root, {
                labelText: ChartDefaults.tariffLabelFormat,
                forceHidden: true
            }),
            visible: meterChartProfile.showCost,
            fill: ChartDefaults.costFill
        }));
        const costSeriesIndex = seriesIndex++;

        costSeries.getTooltip().set("forceHidden", !meterChartProfile.showCost);


        costSeries.strokes.template.setAll({
            stroke: ChartDefaults.costColor,
            strokeOpacity: 0.9,
            strokeWidth: 2
        })

        costSeries.fills.template.setAll({
            fill: ChartDefaults.costFill,
            fillOpacity: 0.8,
            visible: true,
        })



        const yTempRenderer = am5xy.AxisRendererY.new(root, {
            opposite: true
        });
        yTempRenderer.labels.template.set('visible', false)
        yTempRenderer.grid.template.set("forceHidden", true);

        const yTemperatureAxis = chart.yAxes.push(am5xy.ValueAxis.new(root, {
            extraMin: 0.05,
            extraMax: 0.05,
            renderer: yTempRenderer,
        }));

        const xTempRenderer = am5xy.AxisRendererX.new(root, {})
        xTempRenderer.labels.template.set('visible', false)
        const xTempAxis = chart.xAxes.push(am5xy.DateAxis.new(root, {
            min: meterChartProfile.profileStart,
            max: meterChartProfile.profileEnd,
            groupData: true,
            groupCount: 60,
            maxDeviation: 0,
            baseInterval: {
                timeUnit: "minute",
                count: 30
            },
            renderer: xTempRenderer,
        }));

        //xTempAxis.set("syncWithAxis", xAxis);


        const weatherIconSeries = chart.series.push(am5xy.LineSeries.new(root, {
            name: "Weather Summary",
            xAxis: xTempAxis,
            yAxis: yTemperatureAxis,
            opacity: 0,
            valueYField: "temperatureCelsius",
            valueXField: "dateTicks",
            tooltip: am5.Tooltip.new(root, {

                labelText: "{summary}"
            }),
            groupDataWithOriginals: true,
            groupDataCallback: function (dataItem) {
                const group = dataItem.get("originals").map(c => c.get("valueY"));
                const sum = group.reduce((a, b) => a + b, 0);
                const avg = (sum / group.length) || 0;
                const roundAvg = Math.round(avg);

                dataItem.set("valueY", roundAvg)
                dataItem.set("valueYWorking", roundAvg)

                // Find the most common summary
                //let summaries = dataItem.get("originals").map(c => c.);
                //let frequencyCounter = {};
                //let maxCount = 0;
                //let mostCommonSummary = "Variable";

                //for (let i = 0; i < summaries.length; i++) {
                //    let summary = summaries[i];
                //    frequencyCounter[summary] = (frequencyCounter[summary] || 0) + 1;
                //    if (frequencyCounter[summary] > maxCount) {
                //        maxCount = frequencyCounter[summary];
                //        mostCommonSummary = summary;
                //    }
                //}

                //// If the most common summary appears only once, display "Variable"
                //if (maxCount === 1) {
                //    mostCommonSummary = "Variable";
                //}
            }
        }))
        const weatherIconSeriesIndex = seriesIndex++;


        const forecastConsumptionSeries = chart.series.push(am5xy.LineSeries.new(root, {
            name: `${meterChartProfile.globalId} - ForecastConsumption`,
            xAxis: xAxis,
            yAxis: yConsumptionAxis,
            valueYField: "kWh",
            valueXField: "dateTicks",
            valueYGrouped: "sum",
            tooltip: am5.Tooltip.new(root, {
                labelText: "Forecast: {valueY.formatNumber('#.#')}kWh",
            }),
            stroke: ChartDefaults.consumptionColor,

            visible: !meterChartProfile.showCost,
            fill: ChartDefaults.consumptionColor,

        }));

        forecastConsumptionSeries.getTooltip().set("forceHidden", meterChartProfile.showCost);


        forecastConsumptionSeries.fills.template.setAll({
            fill: ChartDefaults.consumptionColor,
            fillOpacity: 0,
            visible: true,
        })
        const forecastConsumptionSeriesIndex = seriesIndex++;

        forecastConsumptionSeries.strokes.template.setAll({
            stroke: ChartDefaults.consumptionColor,
            strokeOpacity: 1,
            strokeDasharray: [3, 3]
        })


        const forecastCostSeries = chart.series.push(am5xy.LineSeries.new(root, {
            name: `${meterChartProfile.globalId} - ForecastCost`,
            xAxis: xAxis,
            yAxis: yCostAxis,
            valueYField: "cost",
            valueXField: "dateTicks",
            valueYGrouped: "sum",
            tooltip: am5.Tooltip.new(root, {
                labelText: ChartDefaults.tariffForecastLabelFormat,
                forceHidden: true
            }),
            stroke: ChartDefaults.costColor,
            visible: meterChartProfile.showCost,
            fill: ChartDefaults.costFill,

        }));
        const forecastCostSeriesIndex = seriesIndex++;

        forecastCostSeries.getTooltip().set("forceHidden", !meterChartProfile.showCost);

        forecastCostSeries.fills.template.setAll({
            fill: ChartDefaults.costColor,
            fillOpacity: 0,
            visible: true,
        })

        forecastCostSeries.strokes.template.setAll({
            stroke: ChartDefaults.costColor,
            strokeOpacity: 1,
            strokeDasharray: [3, 3]
        })


        weatherIconSeries.bullets.push(function (root) {


            return am5.Bullet.new(root, {
                sprite: am5.Circle.new(root, {
                    radius: 15,
                    fill: ChartDefaults.whiteColor,
                    fillOpacity: 0.8,
                    stroke: ChartDefaults.blackColor,
                    strokeWidth: 1.5
                }),
            });
        });

        weatherIconSeries.bullets.push(function (root) {
            return am5.Bullet.new(root, {
                sprite: am5.Label.new(root, {
                    text: "{valueY}°",
                    centerX: am5.p50,
                    centerY: am5.p50,
                    populateText: true
                })
            })
        })



        // Add scrollbar
        // https://www.amcharts.com/docs/v5/charts/xy-chart/scrollbars/
        chart.set("scrollbarX", am5.Scrollbar.new(root, {
            orientation: "horizontal",
            //start: 0.9
        }));



        // Add cursor
        // https://www.amcharts.com/docs/v5/charts/xy-chart/cursor/
        const cursor = chart.set("cursor", am5xy.XYCursor.new(root, {
            behavior: "zoomX"
        }));
        cursor.lineY.set("visible", false);

        consumptionSeries.data.setAll(meterChartProfile.chartReadings);
        costSeries.data.setAll(meterChartProfile.chartReadings);
        forecastConsumptionSeries.data.setAll(meterChartProfile.chartDailyForecastReadings);
        forecastCostSeries.data.setAll(meterChartProfile.chartDailyForecastReadings);
        weatherIconSeries.data.setAll(temperatureIconPoints);


        if (meterChartProfile.showCost) {
            costSeries.showTooltip();
            forecastCostSeries.showTooltip();
            consumptionSeries.hideTooltip();
            forecastConsumptionSeries.hideTooltip();
        } else {
            costSeries.hideTooltip();
            forecastCostSeries.hideTooltip();
            consumptionSeries.showTooltip();
            forecastConsumptionSeries.showTooltip();
        }

        consumptionSeries.events.on("datavalidated", () => {

            if (meterChartProfile.highlightStart && meterChartProfile.highlightEnd) {

                const startDate = new Date(meterChartProfile.highlightStart);
                startDate.setDate(startDate.getDate() - 1);

                const endDate = new Date(meterChartProfile.highlightEnd);
                endDate.setDate(endDate.getDate() + 1);

                xAxis.zoomToDates(startDate, endDate);
                xTempAxis.zoomToDates(startDate, endDate);
            } else {
                xAxis.zoomToDates(new Date(meterChartProfile.profileStart), new Date(meterChartProfile.oneMonthInTheFuture));
                xTempAxis.zoomToDates(new Date(meterChartProfile.profileStart), new Date(meterChartProfile.oneMonthInTheFuture));
            }
        });



        const chartDetails: ChartDetails = {
            chart: chart,
            start: meterChartProfile.profileStart,
            end: meterChartProfile.profileEnd,
            latestReading: meterChartProfile.latestReading,
            oneMonthInTheFuture: meterChartProfile.oneMonthInTheFuture,
            consumptionSeriesIndex: consumptionSeriesIndex,
            costSeriesIndex: costSeriesIndex,
            weatherIconSeriesIndex: weatherIconSeriesIndex,
            forecastConsumptionSeriesIndex: forecastConsumptionSeriesIndex,
            forecastCostSeriesIndex: forecastCostSeriesIndex,
        }
        return chartDetails;
    }
}