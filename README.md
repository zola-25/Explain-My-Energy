![CodeQL](https://github.com/zola-25/Explain-My-Energy/actions/workflows/codeql.yml/badge.svg) ![Main App Playwright Tests](https://dev.azure.com/eme-devops-crunchy/Explain%20My%20Energy/_apis/build/status%2FFull%20App%20Deploy%20and%20Test?branchName=main&stageName=Deploy_production_app_Playwright_Testing&jobName=Run%20Playwright%20Tests%20Production)

# Developer Quick-start

Many client side files are generated at build time. The dotnet tool `nuget-license` is required. Then a NodeJS script can be used to build the necessary client side files:

```bash
cd ./Energy.App.Standalone
dotnet tool install nuget-license
npm install
npm run buildDevDemo
```
Then use `dotnet run` or an IDE to build & run the application.

# Explain My Energy

> [!WARNING]
> *As the n3rgy consumer API has been shut down, this project has been deprecated*
>
> *The demo site remains live -* [demo.explainmyenergy.net](https://demo.explainmyenergy.net)

This is an open-source hobby project that provides a standalone, serverless Blazor WebAssembly app to enable UK-based users easy access to detailed insights on their energy usage and energy costs.

Full documentation can be found at [docs.explainmyenergy.net](https://docs.explainmyenergy.net/)

The application is essentially a client-side wrapper around the consumer API provided by [n3rgy](https://www.n3rgy.com/consumer/), providing detailed energy analysis not available to many consumers - maybe due to the limitations of their energy provider, and often requiring the use of pay-to-use energy apps for the same level of insight.

The analysis includes showing the effects of seasonal temperature on energy usage and costs, as well as daily, weekly and monthly summaries of historical, present and forecast energy usage and costs.

The best way to get a feel of the app is to try the [demo site](https://demo.explainmyenergy.net/), pre-configured with dummy data.

## Visual Example

![Visual Demo Animation](./docs/assets/img/Demo1.gif)

## Note on Browser Support and Security

The application should be fully functional on up-to-date desktop browsers. It has been tested on the latest desktop versions of Chrome, Firefox, Edge and Safari\*.

The application currently makes extensive use of browser local storage and the initial app download size is quite large. Currently ***it may not be functional on some mobile browsers***, as these browsers usually have comparatively smaller local storage and cache limits. 

However both the main app and the demo site have been tested on iPhone 13\* and Galaxy S8, although out of date mobile OS versions may have issues. For iOS ideally use the latest version (17.4 at time of writing), and likewise for Android (version 14 at time of writing). Your mileage may vary.

Initial App download size comes with the territory of Blazor WebAssembly apps right now. App size has been reduced with the cautious use of the [.NET Trimmer](https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/trimming-options?pivots=dotnet-7-0), but there may be more to gain here. 

> [!NOTE]
> **\*WebKit** based browsers such as Safari display intermittent issues using test emulators, regardless of the platform, but appear to be working fine in the wild.

### Security

> [!IMPORTANT]
> Be aware that all data is stored unencrypted in the browser's local storage. No personal data is requested or stored by the application, but to function, the app requires a smart meter IHD (In Home Device) MAC ID, and energy meter MPAN and MPRN numbers. The documentation has more details on these numbers on the [setup page](https://docs.explainmyenergy.net/setup.html).
> 
> > There is a feature to lock and encrypt potentially sensitive data, given a user provided password: [app lock docs](https://docs.explainmyenergy.net/setup/app-data-locking.html).
> > This doesn't require server-held user accounts or third-party authentication providers.
> 
> Data stored in browser local storage is only accessible by the web app that created it, and XSS attack risks have been minimized - however, don't use this app on a shared device or a device that you don't trust.
> 



## Getting Started

There's comprehensive documentation at [docs.explainmyenergy.net](https://docs.explainmyenergy.net/). 

Exploring the **demo** implementation, pre-configured with dummy data and requiring no setup, is recommended and available at [demo.explainmyenergy.net](https://demo.explainmyenergy.net/)

The **live** version that you can to analyze your own energy data, is available at [explainmyenergy.net](https://explainmyenergy.net/)

