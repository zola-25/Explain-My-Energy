/* eslint-disable @typescript-eslint/no-namespace */
import { array, registry } from "@amcharts/amcharts5";
import { DateAxis, AxisRendererX, LineSeries } from "@amcharts/amcharts5/xy";
import _ from "lodash";
import { EnergyOnlyChart } from "./energyOnlyChart";
import { ChartDailyForecastReading, ChartDetails, MeterChartProfile, TemperaturePoint } from "./types";
import { WeatherIconChart } from "./weatherIconChart";


export class ChartFunctions {

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

        const xAxis = chart.xAxes.getIndex(0) as DateAxis<AxisRendererX>;

        xAxis.zoomToDates(new Date(profileStart), new Date(oneMonthForward));
        const xTempAxis = chart.xAxes.hasIndex(1) ? (chart.xAxes.getIndex(1) as DateAxis<AxisRendererX>) : null;

        if (xTempAxis) {
            xTempAxis.zoomToDates(new Date(profileStart), new Date(oneMonthForward))
        }
    }

    public highlightRange(divId: string, highlightStart: number, highlightEnd: number): void {

        const chart = this.activeCharts[divId].chart

        const xAxis = chart.xAxes.getIndex(0) as DateAxis<AxisRendererX>;
        const xTempAxis = chart.xAxes.hasIndex(1) ? (chart.xAxes.getIndex(1) as DateAxis<AxisRendererX>) : null;


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

        const chartDetails = this.activeCharts[divId]
        const chart = chartDetails.chart
        const consumptionSeries = chart.series.getIndex(chartDetails.consumptionSeriesIndex)
        const costSeries = chart.series.getIndex(chartDetails.costSeriesIndex)
        const forecastConsumptionSeries = chart.series.getIndex(chartDetails.forecastConsumptionSeriesIndex)
        const forecastCostSeries = chart.series.getIndex(chartDetails.forecastCostSeriesIndex)

        const consumptionAxis = chart.yAxes.getIndex(0)
        const costAxis = chart.yAxes.getIndex(1)

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
        const chartDetails = this.activeCharts[divId]
        const chart = chartDetails.chart
        const latestReading = this.activeCharts[divId].latestReading

        const forecastConsumptionSeries = chart.series.getIndex(chartDetails.forecastConsumptionSeriesIndex)
        const forecastCostSeries = chart.series.getIndex(chartDetails.forecastCostSeriesIndex)


        forecastConsumptionSeries.data.setAll(forecastChartReadings)
        forecastCostSeries.data.setAll(forecastChartReadings)

        if (!chart.series.hasIndex(chartDetails.weatherIconSeriesIndex)) {
            return;
        }

        const weatherIconSeries = chart.series.getIndex(chartDetails.weatherIconSeriesIndex) as LineSeries

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

        array.each(registry.rootElements, root => {
            if (root.dom.id === divId) {
                root.dispose();
            }
        });
    }

    public static Load(): void {
        window['ChartFunctions'] = new ChartFunctions();
    }
}

