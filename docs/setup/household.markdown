---
layout: default
title: Household
nav_order: 2
parent: Setup
---

# Household Setup

### Move-in date

Select the month you moved in to your current property. This is required for authorization with n3rgy, used to determine the applicable historical tariff rates, and to ensure that only data relevant to your occupancy is used in the analysis.

<img src="../assets/img/setup/MoveInDateInput.png" alt="Move In Date Input" width="500" height="75" >

### Smart Meter IHD MAC ID

This is a 16-digit number that is printed on the back of your smart meter IHD (In-home Display), or accessed from the menu using the IHD itself. A useful page with help for finding your IHD MAC ID is provided by howz.com: [howz.com/mac](https://www.howz.com/mac)

It is used to authorize a  meter with n3rgy, and to retrieve the meter data. Only you have access to this number, so keep it safe. 

Validation checks are performed on the IHD ID to ensure that it is a valid MAC ID. If you enter an invalid IHD ID, you will be prompted to re-enter it.

<img src="../assets/img/setup/IhdMacIdInput.png" alt="In-home Device (smart meter) MAC ID" width="550" height="75" >

### Postal Area

This is the first part of your postcode, e.g. 'SW1A'. It is used to gather historical and forecast weather data for your location.

The input is an auto-complete text field, so you can enter the first two characters of your postal area, and select from the list of matching areas.

<img src="../assets/img/setup/PostalAreaInput.png" alt="Postal Area Input" width="300" height="200" > 

### Primary Heating source

This is the energy type (Gas or Electric), and hence the energy meter, that is used for your primary heating. In 80% of UK homes, this is Gas, though Electric heating is more common in flats and apartments.

This meter's data will be analysed with respect to your local weather data, to help forecast future consumption based on forecast weather, and further ahead, local climate data. More details on this are documented in the Chart Data Analysis section here: [Understanding Chart Data](../analysis/understanding-chart-data#understanding-the-chart-data)

### Household completion

Once you have entered and saved your household details, the application will automatically retrieve the historical and future weather data for your location. This can take a few seconds.