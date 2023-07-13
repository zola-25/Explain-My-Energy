
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
}



export interface TemperaturePoint {
    temperatureCelsiusUnmodified: number;
    temperatureCelsius: number;
    dateTicks: number;
    summary: string;
}

