# Explain My Energy

This is an open-source hobby project that provides a standalone, serverless Blazor WebAssembly app to enable UK-based users easy access to detailed insights on their energy usage and energy costs.

Full documentation can be found at [docs.explainmyenergy.net](https://docs.explainmyenergy.net/)

The application is essentially a client-side wrapper around the consumer API provided by [n3rgy](https://www.n3rgy.com/consumer/), providing detailed energy analysis not available to many consumers - maybe due to the limitations of their energy provider, and often requiring the use of pay-to-use energy apps for the same level of insight.

The analysis includes showing the effects of seasonal temperature on energy usage and costs, as well as daily, weekly and monthly summaries of historical, present and forecast energy usage and costs.

The best way to get a feel of the app is to try the [demo site](https://demo.explainmyenergy.net/), pre-configured with dummy data.

## Visual Example

![Visual Demo Animation](./docs/Demo1.gif)

## Note on Browser Support and Security

The application should be fully functional on up-to-date desktop browsers. It has been tested on the latest desktop versions of Chrome and Firefox.

The application currently makes extensive use of browser local storage and the initial app download size is quite large. Currently *it may not be functional on some mobile browsers*, as these browsers usually have comparatively smaller local storage and cache limits. 

I'm looking at migrating the storage to use IndexedDB, and reducing the compiled app size. 

Initial App download size comes with the territory of Blazor WebAssembly apps right now. The .NET Trimmer should make a world of difference, when I can find that sweet spot where it doesn't break the both the app code and any third-party libraries that dabble in a little reflection. We'll also see what .NET 8 brings us.

### Security

Be aware that all data is stored unencrypted in the browser's local storage. No personal data is requested or stored by the application, but to function, the app requires a smart meter IHD (In Home Device) MAC ID, and energy meter MPAN and MPRN numbers. The documentation has more details on these numbers in [setup](https://docs.explainmyenergy.net/setup.html).

Data stored in browser local storage is only accessible by the web app that created it, but the usual precautions apply - don't use this app on a shared device or a device that you don't trust, and keep your device, anti-virus and browser up to date.

## Getting Started

There's comprehensive documentation at [docs.explainmyenergy.net](https://docs.explainmyenergy.net/). 

A **demo** implementation, pre-configured with dummy data and requiring no setup, is available at [demo.explainmyenergy.net](https://demo.explainmyenergy.net/)

The **live** version that you can use with your own energy data, is available at [explainmyenergy.net](https://explainmyenergy.net/)



