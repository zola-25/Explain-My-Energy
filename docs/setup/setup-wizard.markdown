---
layout: default
title: Setup Wizard
nav_order: 1
parent: Setup
---

# Setup Wizard

The setup wizard covers all the required steps to enable the application to retrieve and analyse your smart meter data, in a single flow. It's possible to skip either Electricity or Gas meter setup, if one of them is not required or available. If you only have a Gas smart meter, you can skip the Electricity Meter setup, and vice versa.

The setup wizard proceeds as follows:  Household -> Gas Meter -> Gas Meter Authorization -> Electricity Meter -> Electricity Meter Authorization -> Final Summary.
 
The setup wizard is available from the main menu and the home page: <img src="../assets/img/setup/SetupWizardIcon.png" alt="Setup Wizard Icon" width="100" height="25">

All the wizard does is wrap the setup stages into a single flow. You can navigate directly to the more manual *Setup* page from the main menu, where all the setup options are also available.

As soon as you successfully authorize a meter, the application will retrieve data for that meter, and analyse it. This can take a few seconds. 

The following sections describe each of the setup wizard stages, although the same setup screens are available in the *Setup* page, and work in the same way. 

## Household 

The first step is to enter your household details. This is a one-off process, and you can change the details at any time, although changing the smart meter IHD ID will require you to re-authorize each meter.

<img src="../assets/img/setup/HouseholdSetupScreen.png" alt="Household Setup Screen" width="300" height="300" >

### Move-in date

Select the month you moved in to your current property. This is required for authorization with n3rgy, used to determine the applicable historical tariff rates, and to ensure that only data relevant to your occupancy is used in the analysis.

<img src="../assets/img/setup/MoveInDateInput.png" alt="Move In Date Input" width="500" height="75" >

### Smart Meter IHD ID

This is a 16-digit number that is printed on the back of your smart meter IHD (In-home Display). It is used to authorize a  meter with n3rgy, and to retrieve the meter data. Only you have access to this number, so keep it safe. 

Validation checks are performed on the IHD ID to ensure that it is a valid MAC ID. If you enter an invalid IHD ID, you will be prompted to re-enter it.

<img src="../assets/img/setup/IhdMacIdInput.png" alt="In-home Device (smart meter) MAC ID" width="550" height="75" >

### Postal Area

This is the first part of your postcode, e.g. 'SW1A'. It is used to gather historical and forecast weather data for your location.

The input is an auto-complete text field, so you can enter the first two characters of your postal area, and select from the list of matching areas.

<img src="../assets/img/setup/PostalAreaInput.png" alt="Postal Area Input" width="300" height="200" > 

### Primary Heating source

This is the energy type (Gas or Electric), and hence the energy meter, that is used for your primary heating. In 80% of UK homes, this is Gas, though Electric heating is more common in flats and apartments.

This meter's data will be analysed with respect to your local weather data, to help forecast future consumption based on forecast weather, and further ahead, local climate data. More details on this are documented in the Heating Meter Analysis section here: [Heating Meter Analysis](../Analysis/analysis-options.markdown#heating-meter-analysis)

### Household completion

Once you have entered and saved your household details, the application will automatically retrieve the historical and future weather data for your location. This can take a few seconds.

## Gas Meter

![Gas Meter Setup Screen](../assets/img/setup/GasMeterSetupScreen.png){:width="70%"}

If you have a Gas Meter is a smart meter, which most in the UK are, you can enter the Gas Meter MPRN number here. If you don't have a Gas Meter hasn't been set up as a smart meter (for example, you still have your readings manually read by a technician), you can skip Gas Meter setup by clicking *Skip to Electricity*.

An MPRN is a Meter Point Reference Number, which is a unique number that identifies your Gas Meter.

You can find your Gas Meter MPRN number on your energy bill, or by using the link to the findmysupplier.energy website.

Enter your MPRN number and click save. The MPRN number is checked to ensure that it is valid. If you enter an invalid MPRN number, a warning will show and it won't be saved. If for some reason your cannot validate your MPRN number, you can click *Skip to Electricity* to skip Gas Meter setup.

Once you have entered and saved your Gas meter details, a green *Authorize* button will be enabled to proceed to authorize the meter with n3rgy:

![Gas Meter Setup Success](../assets/img/setup/GasMeterSetupSuccessMessage.png){:width="70%"}

## Gas Meter authorization

This step opens an embedded display to authorize access to your historical Gas Meter usage with n3rgy. For convenience, the necessary IHD MAC ID and MPRN number are made available as one click copy buttons. 

Signing up with n3rgy simply requires you enter your Gas Meter MPRN number, which the service will check to confirm it has access to. Not all energy providers provide data to n3rgy, so if the lookup fails, it won't be possible to use the application with your Gas meter.

Enter your MPRN and click *Sign up*:

![n3rgy Sign-up MPRN Entered](../assets/img/setup/n3rgySignupScreenCredentialsEntered.png){:width="70%"}

If you find the embedded window too small to work with, you can click *Open in separate tab* to open the authorization window in a separate browser tab:

![n3rgy Sign-up Open Separate Tab](../assets/img/setup/n3rgySignupOpenInSeparateTab.png){:width="50%"}


If the lookup succeeds, you will be prompted confirm the Terms and Conditions, and then asked to enter your IHD MAC ID, as only you have access to this number:

![n3rgy Sign-up Confirm IHD MAC ID](../assets/img/setup/n3rgySignupScreenConfirmIHD_MAC.png)

 Once you have entered the IHD MAC ID, and the service notifies you that it has been successfully authorized:
 
 ![n3rgy Auth Success 1](../assets/img/setup/n3rgyAuthSuccess.png)

 or if you also click 'Return to Provider':

 ![n3rgy Auth Success 2](../assets/img/setup/n3rgyAuthSuccessStep2.png)

If you performed the sign-up process in a separate tab, you can close the tab and return to the application. 

At this point, ensure you now click *Check Authorization*. This makes the application test that it can now access your Gas Meter data from n3rgy. 

If the authorization with n3rgy was completed successfully, this step should work without any issue and the application will begin to retrieve and analyse your historical Gas Meter data. This can take a few seconds.

## Electricity Meter

This step works in the same way as the Gas Meter [step](#gas-meter), but for your Electricity Meter 

If you have an Electricity Meter that is enabled as a smart meter (for example, no technician needs to manually read your current meter readings), you can enter the meter MPAN number here. If you don't have an Electricity smart meter, you can skip this step.

Whereas Gas Meters as designated by Meter Point Reference Numbers (MPRNs), Electricity Meters are designated by Meter Point Administration Numbers (MPANs). 

As with your Gas Meter, you can find your Electricity smart meter MPAN number on your energy bill. Alternatively you can use the embedded link to show the National Grid lookup service, where you can enter your postcode and house number to find your MPAN number.

The MPAN number is checked to ensure that it is valid, and that it is an Electricity meter. If you enter an invalid MPAN number, you will be prompted to re-enter it. Just like Gas Meter MPRNs, MPANs have a fixed format, so it's easy to check that the number is valid.

Once you have entered and saved your Electricity meter MPAN, it is also necessary to authorize the meter with n3rgy.

## Electricity Meter authorization

This step works in the same way as the Gas Meter [authorization](#gas-meter-authorization), but for your Electricity Meter. 

Your MPAN and IHD MAC ID are made available as one click copy buttons. If you authorized your Gas Meter, still choose the sign up option again, instead of logging in with your Gas Meter MPRN, as they are separate authorizations.

![n3rgy Sign-up](../assets/img/setup/n3rgySignupScreen.png)

After successful authorization you should again expect to see:

![n3rgy Auth Success 1](../assets/img/setup/n3rgyAuthSuccess.png)

or if you click 'Return to Provider':

![n3rgy Auth Success 2](../assets/img/setup/n3rgyAuthSuccessStep2.png)