import * as am5 from "@amcharts/amcharts5"
import * as am5xy from "@amcharts/amcharts5/xy"
import { ChartDailyForecastReading, ChartDetails, MeterChartProfile, TemperaturePoint } from "./types"
import _ from "lodash"
import { WeatherIconChart } from "./weatherIconChart"
import { EnergyOnlyChart } from "./energyOnlyChart"


declare global {
    interface Window { ChartFunctions: Charts.ChartFunctions; }
}



export namespace Charts {

    
    export class ChartFunctions {

        public activeCharts: { [divId: string]: ChartDetails } = {}


        public setChart(divId: string, meterChartProfile: MeterChartProfile, temperatureIconPoints: TemperaturePoint[]): void {

            console.log("setChart called")

            const weatherIconChart = new WeatherIconChart()
            
            const chartDetails = weatherIconChart.create(divId, meterChartProfile, temperatureIconPoints)

            this.activeCharts[divId] = chartDetails

        }

        public setEnergyOnlyChart(divId: string, meterChartProfile: MeterChartProfile): void {
            console.log("setEnergyOnlyChart called")

            const energyOnlyChart = new EnergyOnlyChart()
            const chartDetails = energyOnlyChart.create(divId, meterChartProfile)

            this.activeCharts[divId] = chartDetails
        }


        public removeHighlight(divId: string): void {
            const chartDetails =  this.activeCharts[divId] as ChartDetails
            const profileStart = chartDetails.start
            const oneMonthForward = chartDetails.oneMonthInTheFuture

            const amChart = chartDetails.chart

            const xAxis = chartDetails.chart.xAxes.getIndex(0) as am5xy.DateAxis<am5xy.AxisRendererX>

            xAxis.zoomToDates(new Date(profileStart), new Date(oneMonthForward))
            const xTempAxis = amChart.xAxes.hasIndex(1) ? (amChart.xAxes.getIndex(1) as am5xy.DateAxis<am5xy.AxisRendererX>) : null

            if (xTempAxis) {
                xTempAxis.zoomToDates(new Date(profileStart), new Date(oneMonthForward))
            }
        }

        public highlightRange(divId: string, highlightStart: number, highlightEnd: number): void {

            const chartDetails = this.activeCharts[divId] as ChartDetails

            const amChart = chartDetails.chart
            const xAxis = amChart.xAxes.getIndex(0) as am5xy.DateAxis<am5xy.AxisRendererX>
            const xTempAxis = amChart.xAxes.hasIndex(1) ? (amChart.xAxes.getIndex(1) as am5xy.DateAxis<am5xy.AxisRendererX>) : null


            const startDate = new Date(highlightStart)
            startDate.setDate(startDate.getDate() - 1)

            const endDate = new Date(highlightEnd)
            endDate.setDate(endDate.getDate() + 1)

            xAxis.zoomToDates(startDate, endDate)

            if (xTempAxis) {
                xTempAxis.zoomToDates(startDate, endDate)
            }

        }


        public async toggleCostSeries(divId: string, showCost: boolean): Promise<string> {

            try {
                
                const chartDetails = this.activeCharts[divId] as ChartDetails
                const amChart = chartDetails.chart
                const consumptionSeries = amChart.series.getIndex(chartDetails.consumptionSeriesIndex) as am5xy.LineSeries
                const costSeries = amChart.series.getIndex(chartDetails.costSeriesIndex) as am5xy.LineSeries
                const forecastConsumptionSeries = amChart.series.getIndex(chartDetails.forecastConsumptionSeriesIndex) as am5xy.LineSeries
                const forecastCostSeries = amChart.series.getIndex(chartDetails.forecastCostSeriesIndex) as am5xy.LineSeries 
    
                const consumptionAxis = amChart.yAxes.getIndex(0) as am5xy.ValueAxis<am5xy.AxisRendererY>
                const costAxis = amChart.yAxes.getIndex(1) as am5xy.ValueAxis<am5xy.AxisRendererY>
    
                if (showCost) {
                    await consumptionAxis.hide()
                    await costAxis.show()
    
                    consumptionAxis.get("renderer").grid.template.set("forceHidden", true)
                    costAxis.get("renderer").grid.template.set("forceHidden", false)
    
    
                    await consumptionSeries.hide();
    
                    (consumptionSeries.getTooltip() as am5.Tooltip)?.set("forceHidden", true) ?? console.warn("consumption series not set")
                    
                    await forecastConsumptionSeries.hide();
    
                    (forecastConsumptionSeries.getTooltip() as am5.Tooltip)?.set("forceHidden", true) ?? console.warn("forecast series not set")
    
     
                    await costSeries.show();
                    (costSeries.getTooltip() as am5.Tooltip)?.set("forceHidden", false) ?? console.warn("cost series not set")
                    
                    await forecastCostSeries.show();
                    (forecastCostSeries.getTooltip() as am5.Tooltip)?.set("forceHidden", false) ?? console.warn("forecast cost series not set")
    
    
    
                } else {
                    await costAxis.hide();
                    await consumptionAxis.show();
    
                    costAxis.get("renderer").grid.template.set("forceHidden", true)
                    consumptionAxis.get("renderer").grid.template.set("forceHidden", false)
    
                    await costSeries.hide();
                    (costSeries.getTooltip() as am5.Tooltip)?.set("forceHidden", true) 
                    
                    await forecastCostSeries.hide();
                    (forecastCostSeries.getTooltip() as am5.Tooltip).set("forceHidden", true)
    
                    await consumptionSeries.show();
                    (consumptionSeries.getTooltip() as am5.Tooltip).set("forceHidden", false)
                    
                    await forecastConsumptionSeries.show();
                    (forecastConsumptionSeries.getTooltip() as am5.Tooltip).set("forceHidden", false)
                }
    
            } catch (error) {
                const ensuredError = this.ensureError(error)
                return ensuredError.message
            }
            return "";

        }


        private ensureError(value: unknown): Error {
            if (value instanceof Error) return value
          
            let stringified: string;
            try {
              stringified = JSON.stringify(value)
            } catch {
               stringified = '[Unable to stringify the thrown value]'
            }
          
            const error = new Error(`This value was thrown as is, not through an Error: ${stringified}`)
            return error
          }

        public setForecastSeries(divId: string, forecastChartReadings: ChartDailyForecastReading[], degreeDifference: number): void {

            console.log("degreeDifference: " + degreeDifference)
            const chartDetails = this.activeCharts[divId] as ChartDetails
            const am5chart = chartDetails.chart
            const latestReading = chartDetails.latestReading

            const forecastConsumptionSeries = am5chart.series.getIndex(chartDetails.forecastConsumptionSeriesIndex)
            const forecastCostSeries = am5chart.series.getIndex(chartDetails.forecastCostSeriesIndex)


            forecastConsumptionSeries?.data.setAll(forecastChartReadings) ?? console.warn("No forecast consumption series found")
            forecastCostSeries?.data.setAll(forecastChartReadings) ?? console.warn("No forecast cost series found")

            if (!am5chart.series.hasIndex(chartDetails.weatherIconSeriesIndex)) {
                return
            }

            const weatherIconSeries = am5chart.series.getIndex(chartDetails.weatherIconSeriesIndex) as am5xy.LineSeries

            const points = (weatherIconSeries.data.values as TemperaturePoint[])

            const newPoints: TemperaturePoint[] =   _(points)
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
                    root.dispose()
                }
            })
        }
    }

    export function Initialize(): void {
        window.ChartFunctions = new ChartFunctions()
    }
}

