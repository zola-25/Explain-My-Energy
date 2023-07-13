import * as am5 from "@amcharts/amcharts5";
import * as am5xy from "@amcharts/amcharts5/xy";
import am5themes_Animated from "@amcharts/amcharts5/themes/Animated";
import { ChartDailyForecastReading, ChartReading, MeterChartProfile, TemperaturePoint } from "./types";
import { Axis, AxisRenderer, DateAxis, ValueAxis } from "@amcharts/amcharts5/xy";
import _ from "lodash"

export namespace Charts {


    interface ChartDetails {
        chart: am5xy.XYChart,
        start: number,
        end: number,
        latestReading: number,
        oneMonthInTheFuture: number,
        consumptionSeriesIndex: number,
        costSeriesIndex: number,
        forecastConsumptionSeriesIndex: number,
        forecastCostSeriesIndex: number,
        weatherIconSeriesIndex: number
    }

    class ChartFunctions {

        public static consumptionColor: am5.Color = am5.color(0x594ae2)
        public static costColor: am5.Color = am5.color(0x272c34)
        public static costFill: am5.Color = am5.color(0x93969a)
        public static whiteColor: am5.Color = am5.color(0xffffff)
        public static blackColor: am5.Color = am5.color(0x000000)
        public static highlightColor: am5.Color = am5.color(0xFFA726)

        public activeCharts: { [divId: string]: ChartDetails; } = {};





        public setChart(divId: string, meterChartProfile: MeterChartProfile, temperatureIconPoints: TemperaturePoint[]): void {

            console.log("setChart called");

            // Create root element
            let root = am5.Root.new(divId);
            // Set themes
            root.setThemes([
                am5themes_Animated.new(root)
            ]);

            // Create chart
            let chart = root.container.children.push(am5xy.XYChart.new(root, {
                focusable: true,
                panX: true,
                panY: false,
                wheelX: "panX",
                wheelY: "zoomX"
            }));


            //
            // // Create axes
            let xAxis = chart.xAxes.push(am5xy.DateAxis.new(root, {
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

            let yConsumptionAxisRenderer = am5xy.AxisRendererY.new(root, {
            })
            yConsumptionAxisRenderer.grid.template.set("forceHidden", !meterChartProfile.showCost);

            let yConsumptionAxis = chart.yAxes.push(am5xy.ValueAxis.new(root, {
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

            var seriesIndex = 0;
            // Add series
            let consumptionSeries = chart.series.push(am5xy.LineSeries.new(root, {
                name: `${meterChartProfile.globalId} - Consumption`,
                xAxis: xAxis,
                yAxis: yConsumptionAxis,

                valueYField: "kWh",
                valueXField: "dateTicks",
                valueYGrouped: "sum",
                tooltip: am5.Tooltip.new(root, {
                    labelText: "{valueY.formatNumber('#.#')}kWh",
                }),
                fill: ChartFunctions.consumptionColor,
                visible: !meterChartProfile.showCost
            }));
            var consumptionSeriesIndex = seriesIndex++;

            consumptionSeries.getTooltip().set("forceHidden", meterChartProfile.showCost);

            consumptionSeries.strokes.template.setAll({
                stroke: ChartFunctions.consumptionColor,
                strokeOpacity: 0.9,
                strokeWidth: 2
            })

            consumptionSeries.fills.template.setAll({
                fill: ChartFunctions.consumptionColor,
                fillOpacity: 0.5,
                visible: true
            })



            let yCostAxisRenderer = am5xy.AxisRendererY.new(root, {
            })
            yCostAxisRenderer.grid.template.set("forceHidden", !meterChartProfile.showCost);

            let yCostAxis = chart.yAxes.push(am5xy.ValueAxis.new(root, {
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

            let costSeries = chart.series.push(am5xy.LineSeries.new(root, {
                name: `${meterChartProfile.globalId} - Cost`,
                xAxis: xAxis,
                yAxis: yCostAxis,
                valueYField: "cost",
                valueXField: "dateTicks",
                valueYGrouped: "sum",
                tooltip: am5.Tooltip.new(root, {
                    labelText: "£{valueY.formatNumber('#,###.00')}",
                    forceHidden: true
                }),
                visible: meterChartProfile.showCost,
                fill: ChartFunctions.costFill
            }));
            var costSeriesIndex = seriesIndex++;

            costSeries.getTooltip().set("forceHidden", !meterChartProfile.showCost);


            costSeries.strokes.template.setAll({
                stroke: ChartFunctions.costColor,
                strokeOpacity: 0.9,
                strokeWidth: 2
            })

            costSeries.fills.template.setAll({
                fill: ChartFunctions.costFill,
                fillOpacity: 0.8,
                visible: true,
            })



            let yTempRenderer = am5xy.AxisRendererY.new(root, {
                opposite: true
            });
            yTempRenderer.labels.template.set('visible', false)
            yTempRenderer.grid.template.set("forceHidden", true);

            let yTemperatureAxis = chart.yAxes.push(am5xy.ValueAxis.new(root, {
                extraMin: 0.05,
                extraMax: 0.05,
                renderer: yTempRenderer,
            }));

            let xTempRenderer = am5xy.AxisRendererX.new(root, {})
            xTempRenderer.labels.template.set('visible', false)
            let xTempAxis = chart.xAxes.push(am5xy.DateAxis.new(root, {
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


            let weatherIconSeries = chart.series.push(am5xy.LineSeries.new(root, {
                name: "Weather Summary",
                xAxis: xTempAxis,
                yAxis: yTemperatureAxis,
                opacity: 0,





                valueYField: "temperatureCelsius",
                valueXField: "dateTicks",
                tooltip: am5.Tooltip.new(root, {
                    labelText: "{summary}",
                }),
                groupDataWithOriginals: true,
                groupDataCallback: function (dataItem, interval) {
                    var group = dataItem.get("originals").map(c => c.get("valueY"));
                    const sum = group.reduce((a, b) => a + b, 0);
                    const avg = (sum / group.length) || 0;
                    const roundAvg = Math.round(avg);

                    dataItem.set("valueY", roundAvg)
                    dataItem.set("valueYWorking", roundAvg)

                    // Find the most common summary
                    //var summaries = dataItem.get("originals").map(c => c.);
                    //var frequencyCounter = {};
                    //var maxCount = 0;
                    //var mostCommonSummary = "Variable";

                    //for (var i = 0; i < summaries.length; i++) {
                    //    var summary = summaries[i];
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
            var weatherIconSeriesIndex = seriesIndex++;


            let forecastConsumptionSeries = chart.series.push(am5xy.LineSeries.new(root, {
                name: `${meterChartProfile.globalId} - ForecastConsumption`,
                xAxis: xAxis,
                yAxis: yConsumptionAxis,
                valueYField: "kWh",
                valueXField: "dateTicks",
                valueYGrouped: "sum",
                tooltip: am5.Tooltip.new(root, {
                    labelText: "{valueY.formatNumber('#.#')}kWh",
                }),
                stroke: ChartFunctions.consumptionColor,

                visible: !meterChartProfile.showCost,
                fill: ChartFunctions.consumptionColor,

            }));

            forecastConsumptionSeries.getTooltip().set("forceHidden", meterChartProfile.showCost);


            forecastConsumptionSeries.fills.template.setAll({
                fill: ChartFunctions.consumptionColor,
                fillOpacity: 0,
                visible: true,
            })
            var forecastConsumptionSeriesIndex = seriesIndex++;

            forecastConsumptionSeries.strokes.template.setAll({
                stroke: ChartFunctions.consumptionColor,
                strokeOpacity: 1,
                strokeDasharray: [3, 3]
            })


            let forecastCostSeries = chart.series.push(am5xy.LineSeries.new(root, {
                name: `${meterChartProfile.globalId} - ForecastConsumption`,
                xAxis: xAxis,
                yAxis: yCostAxis,
                valueYField: "cost",
                valueXField: "dateTicks",
                valueYGrouped: "sum",
                tooltip: am5.Tooltip.new(root, {
                    labelText: "£{valueY.formatNumber('#,###.00')}",
                    forceHidden: true
                }),
                stroke: ChartFunctions.costColor,
                visible: meterChartProfile.showCost,
                fill: ChartFunctions.costFill,

            }));
            var forecastCostSeriesIndex = seriesIndex++;

            forecastCostSeries.getTooltip().set("forceHidden", !meterChartProfile.showCost);

            forecastCostSeries.fills.template.setAll({
                fill: ChartFunctions.costColor,
                fillOpacity: 0,
                visible: true,
            })

            forecastCostSeries.strokes.template.setAll({
                stroke: ChartFunctions.costColor,
                strokeOpacity: 1,
                strokeDasharray: [3, 3]
            })


            //iconSeries.bullets.push(function (root, series, dataItem) {
            //    let grouped = dataItem.get("originals") ? true : false;

            //    let avgTemp = dataItem.get("valueY")

            //    return am5.Bullet.new(root, {
            //        sprite: am5.Picture.new(root, {
            //            src: `/images/temperature/celsius${avgTemp}.svg`,
            //            width: 25,
            //            height: 25,
            //            scale: grouped ? 1 : 0.8,
            //            centerX: am5.p50,
            //            centerY: am5.p50
            //        })
            //    })

            //})



            weatherIconSeries.bullets.push(function (root, series, dataItem) {

                let grouped = dataItem.get("originals") ? true : false;

                return am5.Bullet.new(root, {
                    sprite: am5.Circle.new(root, {
                        radius: 15,
                        fill: ChartFunctions.whiteColor,
                        fillOpacity: 0.8,
                        stroke: ChartFunctions.blackColor,
                        strokeWidth: 1.5
                    }),
                });
            });

            weatherIconSeries.bullets.push(function (root, series, dataItem) {
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
            let cursor = chart.set("cursor", am5xy.XYCursor.new(root, {
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

            this.activeCharts[divId] = {
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

            };

            xAxis.zoomToDates(new Date(meterChartProfile.profileStart), new Date(meterChartProfile.oneMonthInTheFuture));
            xTempAxis.zoomToDates(new Date(meterChartProfile.profileStart), new Date(meterChartProfile.oneMonthInTheFuture));

        }


        public removeHighlight(divId: string): void {
            const chart = this.activeCharts[divId].chart
            const profileStart = this.activeCharts[divId].start
            const oneMonthForward = this.activeCharts[divId].oneMonthInTheFuture

            const xAxis = chart.xAxes.getIndex(0) as am5xy.DateAxis<am5xy.AxisRendererX>;


            xAxis.zoomToDates(new Date(profileStart), new Date(oneMonthForward));
            const xTempAxis = chart.xAxes.hasIndex(1) ? (chart.xAxes.getIndex(1) as am5xy.DateAxis<am5xy.AxisRendererX>) : null;

            if (xTempAxis) {
                xTempAxis.zoomToDates(new Date(profileStart), new Date(oneMonthForward))
            }
        }

        public highlightRange(divId: string, highlightStart: number, highlightEnd: number): void {

            const chart = this.activeCharts[divId].chart

            const xAxis = chart.xAxes.getIndex(0) as am5xy.DateAxis<am5xy.AxisRendererX>;
            const xTempAxis = chart.xAxes.hasIndex(1) ? (chart.xAxes.getIndex(1) as am5xy.DateAxis<am5xy.AxisRendererX>) : null;


            const startDate = new Date(highlightStart);
            startDate.setDate(startDate.getDate() - 1);

            const endDate = new Date(highlightEnd);
            endDate.setDate(endDate.getDate() + 1);

            xAxis.zoomToDates(startDate, endDate);

            if (xTempAxis) {
                xTempAxis.zoomToDates(startDate, endDate)
            }

        }

        public toggleCostSeries(divId: string, showCost: boolean): void {

            let chartDetails = this.activeCharts[divId]
            let chart = chartDetails.chart
            let consumptionSeries = chart.series.getIndex(chartDetails.consumptionSeriesIndex)
            let costSeries = chart.series.getIndex(chartDetails.costSeriesIndex)
            let forecastConsumptionSeries = chart.series.getIndex(chartDetails.forecastConsumptionSeriesIndex)
            let forecastCostSeries = chart.series.getIndex(chartDetails.forecastCostSeriesIndex)

            let consumptionAxis = chart.yAxes.getIndex(0)
            let costAxis = chart.yAxes.getIndex(1)

            if (showCost) {
                consumptionAxis.hide()
                costAxis.show()

                consumptionAxis.get("renderer").grid.template.set("forceHidden", true);
                costAxis.get("renderer").grid.template.set("forceHidden", false);

                consumptionSeries.hide();
                consumptionSeries.getTooltip().set("forceHidden", true);
                forecastConsumptionSeries.hide();
                forecastConsumptionSeries.getTooltip().set("forceHidden", true);


                costSeries.show();
                costSeries.getTooltip().set("forceHidden", false);
                forecastCostSeries.show();
                forecastCostSeries.getTooltip().set("forceHidden", false);

            } else {
                costAxis.hide();
                consumptionAxis.show();

                costAxis.get("renderer").grid.template.set("forceHidden", true);
                consumptionAxis.get("renderer").grid.template.set("forceHidden", false);

                costSeries.hide();
                costSeries.getTooltip().set("forceHidden", true);
                forecastCostSeries.hide();
                forecastCostSeries.getTooltip().set("forceHidden", true);

                consumptionSeries.show();
                consumptionSeries.getTooltip().set("forceHidden", false);
                forecastConsumptionSeries.show();
                forecastConsumptionSeries.getTooltip().set("forceHidden", false);
            }
        }

        public setForecastSeries(divId: string, forecastChartReadings: ChartDailyForecastReading[], degreeDifference: number): void {

            console.log("degreeDifference: " + degreeDifference)
            let chartDetails = this.activeCharts[divId]
            let chart = chartDetails.chart
            let latestReading = this.activeCharts[divId].latestReading

            let forecastConsumptionSeries = chart.series.getIndex(chartDetails.forecastConsumptionSeriesIndex)
            let forecastCostSeries = chart.series.getIndex(chartDetails.forecastCostSeriesIndex)


            forecastConsumptionSeries.data.setAll(forecastChartReadings)
            forecastCostSeries.data.setAll(forecastChartReadings)

            if (!chart.series.hasIndex(chartDetails.weatherIconSeriesIndex)) {
                return;
            }

            let weatherIconSeries = chart.series.getIndex(chartDetails.weatherIconSeriesIndex) as am5xy.LineSeries

            const points = (weatherIconSeries.data.values as TemperaturePoint[])

            const newPoints: TemperaturePoint[] = _(points)
                .map((tempPoint: TemperaturePoint) => {
                    if (tempPoint.dateTicks > latestReading) {
                        tempPoint.temperatureCelsius = tempPoint.temperatureCelsiusUnmodified + degreeDifference
                    }
                    return tempPoint
                }).value()


            weatherIconSeries.data.setAll(newPoints)

        }

        public setChartNoTemperature(divId: string, meterChartProfile: MeterChartProfile): void {
            // Create root element
            let root = am5.Root.new(divId);
            // Set themes
            root.setThemes([
                am5themes_Animated.new(root)
            ]);

            // Create chart
            let chart = root.container.children.push(am5xy.XYChart.new(root, {
                focusable: true,
                panX: true,
                panY: false,
                wheelX: "panX",
                wheelY: "zoomX"
            }));


            //
            // // Create axes
            let xAxis = chart.xAxes.push(am5xy.DateAxis.new(root, {
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

            let yConsumptionAxis = chart.yAxes.push(am5xy.ValueAxis.new(root, {
                ariaLabel: "kWh",
                renderer: am5xy.AxisRendererY.new(root, {})
            }));

            yConsumptionAxis.children.unshift(
                am5.Label.new(root, {
                    rotation: -90,
                    text: "kWh",
                    y: am5.p50,
                    centerX: am5.p50
                })
            );


            // Add series
            let consumptionSeries = chart.series.push(am5xy.LineSeries.new(root, {
                name: `${meterChartProfile.globalId} - Consumption`,
                xAxis: xAxis,
                yAxis: yConsumptionAxis,

                valueYField: "kWh",
                valueXField: "dateTicks",
                valueYGrouped: "sum",
                tooltip: am5.Tooltip.new(root, {
                    labelText: "{valueY.formatNumber('#.#')}kWh",
                }),
                fill: ChartFunctions.consumptionColor
            }));

            consumptionSeries.strokes.template.setAll({
                stroke: ChartFunctions.consumptionColor,
                strokeOpacity: 0.9,
                strokeWidth: 2
            })

            consumptionSeries.fills.template.setAll({
                fill: ChartFunctions.consumptionColor,
                fillOpacity: 0.5,
                visible: true
            })



            let yCostAxisRenderer = am5xy.AxisRendererY.new(root, {
            })
            yCostAxisRenderer.grid.template.set("forceHidden", true);

            let yCostAxis = chart.yAxes.push(am5xy.ValueAxis.new(root, {
                ariaLabel: "Pounds",
                numberFormat: "'£'#",
                maxPrecision: 0,
                visible: false,
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

            let costSeries = chart.series.push(am5xy.LineSeries.new(root, {
                name: `${meterChartProfile.globalId} - Cost`,
                xAxis: xAxis,
                yAxis: yCostAxis,
                valueYField: "cost",
                valueXField: "dateTicks",
                valueYGrouped: "sum",
                tooltip: am5.Tooltip.new(root, {
                    labelText: "£{valueY.formatNumber('#,###.00')}",
                    forceHidden: true
                }),
                visible: false,
                fill: ChartFunctions.costFill
            }));

            costSeries.strokes.template.setAll({
                stroke: ChartFunctions.costColor,
                strokeOpacity: 0.9,
                strokeWidth: 2
            })

            costSeries.fills.template.setAll({
                fill: ChartFunctions.costFill,
                fillOpacity: 0.8,
                visible: true,
            })

            const forecastCostRangeDataItem = xAxis.makeDataItem({
                value: meterChartProfile.latestReading,
                endValue: meterChartProfile.profileEnd
            });

            const forecastCostRange = costSeries.createAxisRange(forecastCostRangeDataItem)

            forecastCostRange.strokes.template.setAll({
                stroke: ChartFunctions.costColor,
                strokeOpacity: 0.9,
                strokeWidth: 2,
            })

            forecastCostRange.fills.template.setAll({
                visible: true,
                fillPattern: am5.LinePattern.new(root, {
                    color: ChartFunctions.costColor,
                    fill: ChartFunctions.whiteColor,
                    fillOpacity: 0,
                    rotation: -45,
                    width: 1200,
                    height: 1200,
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
            let cursor = chart.set("cursor", am5xy.XYCursor.new(root, {
                behavior: "zoomX"
            }));
            cursor.lineY.set("visible", false);

            costSeries.data.setAll(meterChartProfile.chartReadings);
            consumptionSeries.data.setAll(meterChartProfile.chartReadings);

            costSeries.hideTooltip();

            this.activeCharts[divId] = {
                chart: chart,
                start: meterChartProfile.profileStart,
                end: meterChartProfile.profileEnd,
                latestReading: meterChartProfile.latestReading,
                oneMonthInTheFuture: meterChartProfile.oneMonthInTheFuture,
                consumptionSeriesIndex: 0,
                costSeriesIndex: 1,
                weatherIconSeriesIndex: 2,
                forecastConsumptionSeriesIndex: 3,
                forecastCostSeriesIndex: 4,
            };

            //xAxis.zoomToDates(new Date(meterProfile.mostRecentWeekStart), new Date(meterProfile.profileEnd));
            //xTempAxis.zoomToDates(new Date(meterProfile.mostRecentWeekStart), new Date(meterProfile.profileEnd));

        }

        public dispose(divId: string): void {

            delete this.activeCharts[divId]

            am5.array.each(am5.registry.rootElements, root => {
                if (root.dom.id === divId) {
                    root.dispose();
                }
            });
        }
    }

    export function Load(): void {
        window['ChartFunctions'] = new ChartFunctions();
    }
}

