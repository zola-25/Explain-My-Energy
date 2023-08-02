import { XYChart } from "@amcharts/amcharts5/xy";
import { color, Color} from "@amcharts/amcharts5"; 
export interface ChartReading {
    dateTicks: number;
    pencePerKWh: number;
    cost: number;
    dailyStandingCharge: number;
    kWh: number;
    halfHourlyStandingCharge: number;
    tariffAppliesFrom: Date;
    tariffType: string;
    isForecast: boolean;
}

export interface ChartDailyForecastReading {
    dateTicks: number,
    pencePerKWh: number,
    cost: number,
    dailyStandingCharge: number,
    kWh: number,
    tariffAppliesFrom: Date
}

export interface MeterChartProfile {
    showCost: boolean,
    globalId: string,
    profileStart: number,
    profileEnd: number,
    mostRecentWeekStart: number,
    oneMonthInTheFuture: number,
    latestReading: number,
    chartReadings: ChartReading[],
    chartDailyForecastReadings: ChartDailyForecastReading[]
    highlightStart: number | null,
    highlightEnd: number | null,
}



export interface TemperaturePoint {
    temperatureCelsiusUnmodified: number;
    temperatureCelsius: number;
    dateTicks: number;
    summary: string;
}

export interface ChartDetails {
        chart: XYChart,
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

export  class ChartDefaults {
    
    public static consumptionColor: Color = color(0x594ae2)
    public static costColor: Color = color(0x272c34)
    public static costFill: Color = color(0x93969a)
    public static whiteColor: Color = color(0xffffff)
    public static blackColor: Color = color(0x000000)
    public static highlightColor: Color = color(0xFFA726)
    public static tariffLabelFormat: string = "[bold]£{valueY.formatNumber('#,###.00')}[/]\nTariff: [/]{tariffAppliesFrom.formatDate('MMM dt, yyyy')}[/]\nRate: {pencePerKWh}p/kWh[/]\nStanding Charge (daily): {dailyStandingCharge}p"
    public static tariffForecastLabelFormat: string = "[bold]Forecast: £{valueY.formatNumber('#,###.00')}[/]\nTariff: [/]{tariffAppliesFrom.formatDate('MMM dt, yyyy')}[/]\nRate: {pencePerKWh}p/kWh[/]\nStanding Charge (daily): {dailyStandingCharge}p"

}