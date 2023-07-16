import * as am5 from "@amcharts/amcharts5";
import * as am5xy from "@amcharts/amcharts5/xy";
import { ChartDailyForecastReading, ChartDetails, ChartReading, MeterChartProfile, TemperaturePoint } from "./types";
import _ from "lodash"
import { WeatherIconChart } from "./weatherIconChart";
import { EnergyOnlyChart } from "./energyOnlyChart";

export namespace Charts {

    class ChartFunctions {

        public activeCharts: { [divId: string]: ChartDetails; } = {};


        public setChart(divId: string, meterChartProfile: MeterChartProfile, temperatureIconPoints: TemperaturePoint[]): void {

            console.log("setChart called");

            const weatherIconChart = new WeatherIconChart();
            
            const chartDetails = weatherIconChart.create(divId, meterChartProfile, temperatureIconPoints);

            this.activeCharts[divId] = chartDetails;

        }

        public setEnergyOnlyChart(divId: string, meterChartProfile: MeterChartProfile): void {
            console.log("setEnergyOnlyChart called");

            const energyOnlyChart = new EnergyOnlyChart();
            const chartDetails = energyOnlyChart.create(divId, meterChartProfile);

            this.activeCharts[divId] = chartDetails;
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

