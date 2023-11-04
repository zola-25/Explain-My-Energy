# Explain My Energy

## Introduction

This is an open-source hobby project that provides a standalone, serverless Blazor WebAssembly app to enable UK-based users easy access to detailed insights on their energy usage and energy costs.

The application is essentially a wrapper around the consumer API provided by [n3rgy](https://www.n3rgy.com/consumer/), allowing easy sign-up for energy data access, and then providing detailed energy analysis not available to many consumers - due to the limitations of their energy provider, and often requiring the use of pay-to-use energy apps for the same level of insight.

The analysis includes showing the effects of seasonal temperature on energy usage and costs, as well as daily, weekly and monthly summaries of historical, present and forecast energy usage.

The best way to get a feel of the app is to try the [demo site](https://demo.explainmyenergy.net/), pre-configured with dummy data.

## Browser Support

As the application makes extensive use of browser local storage, *it may not be functional on some mobile browsers*, as these browsers often have reduced local storage limits.

There is potential to migrate the storage to use IndexedDB, which would allow the application to work on mobile browsers.

The application should be fully functional on desktop browsers. It has been tested on the latest desktop versions of Chrome and Firefox.

## Getting Started

To view the ***demo*** version of the app, pre-configured with dummy data and requiring no setup, visit [demo.explainmyenergy.net](https://demo.explainmyenergy.net/)

To use the ***live*** version that can be used to easily import and analyse your own data, visit the main site at [explainmyenergy.net](https://explainmyenergy.net/)

## Visual Example

![Visual Demo Animation](./docs/Demo1.gif)

