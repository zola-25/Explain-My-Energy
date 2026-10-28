# Explain My Energy: end-to-end tests

[Playwright](https://playwright.dev/) smoke tests for the app in **demo mode**. They check that each route shows its
page:

- `/`: the main header, the App Setup Status, Weather Data Status and Analysis Options Status headings, and the
  Welcome Screen button
- `/Setup`: the Household Setup, Meter Setup and Weather Data Status headings
- `/Meter/Electricity` and `/HeatingMeter/Gas`: the Last/This/Next Month headings and the chart
- `/PrivacyPolicy`, `/TermsAndConditions` and `/Credits`: each page's main heading
- the menu and footer links go to those pages

The tests only use what a user sees: routes, headings, links and button names. The exception is the chart, found
by `data-testid="energy-chart"`. So they work the same against any implementation of the app.

## Running the tests

The tests expect the app to be already running in demo mode on <http://localhost:5000>. For the Blazor app:

```bash
cd Energy.App.Standalone
dotnet tool install nuget-license
npm install
npm run buildDevDemo
dotnet run -p:ClientEnv=DemoDevelopment
```

(`dotnet run` serves on port 5000 by default. Add `-c Release` for a faster app.)

Then, in another terminal:

```bash
cd Energy.E2E
npm install
npx playwright install chromium   # once
npm test
```

Other ways to run them:

```bash
npm run test:headed     # watch the browser
npm run test:ui         # Playwright UI mode
npm run report          # open the last HTML report
npm run typecheck
```

Each test loads the app afresh. The app can take a minute or more to load and analyse the demo data, so the tests
allow it up to 3 minutes.

## Mocked weather and postcode APIs

The app's weather and postcode requests are answered with responses recorded in `fixtures/weather/`, so the tests
don't depend on the real APIs:

- **The browser's clock is frozen** at the time the responses were recorded (`fixtures/weather/recording.json`),
  so the app always asks for the same dates.
- **Requests are matched** by host, path and the `start_date`/`end_date` they ask for, so the other query
  parameters can differ in order and formatting.
- **Any other request to an external host** is blocked and fails the test, except the documentation site, which
  gets a 404.

To re-record the responses from the real Open-Meteo and postcodes.io APIs (with the app running):

```bash
npm run record-weather                                         # freezes the clock at midday UTC today
E2E_FIXED_UTC_NOW=2026-10-27T12:00:00Z npm run record-weather  # or at a time of your choosing
```

Re-recording is only needed if the app changes which weather data it requests.
