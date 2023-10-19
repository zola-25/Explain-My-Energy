---
layout: default
title: Meter Authorization
nav_order: 4
parent: Setup
---

# Meter Authorization

Once you have entered your household details, and set up your Gas and/or Electricity meters, you can authorize each meter with n3rgy. This is a one-off process, and once authorized, the application will retrieve the meter data, and analyse it.

### Authorization Process

The authorization process is the same for both Gas and Electricity meters, but requires separate authorization for each meter. 

An embedded display is shown to authorize access to your historical Gas/Electricity Meter usage with n3rgy. For convenience, the necessary IHD MAC ID and MPRN/MPAN number are made available as one click copy buttons. 

Signing up with n3rgy simply requires you enter either your MPRN/MPAN number, which the service will check to confirm it has access to. Not all energy providers provide data to n3rgy, so if the lookup fails, it won't be possible to use the application with this meter.

If you find the embedded window too small to work with, you can click *Open in separate tab* to open the authorization window in a separate browser tab.

![n3rgy Sign-up Open Separate Tab](../assets/img/setup/n3rgySignupOpenInSeparateTab.png){:width="50%"}

If the lookup succeeds, you will be prompted confirm the Terms and Conditions, and then asked to enter your IHD MAC ID, as only you have access to this number.

![n3rgy Sign-up Confirm IHD MAC ID](../assets/img/setup/n3rgySignupScreenConfirmIHD_MAC.png)

 Once you have entered the IHD MAC ID, and the service notifies you that it has been successfully authorized.
 
 ![n3rgy Auth Success 1](../assets/img/setup/n3rgyAuthSuccess.png)

 You can also click 'Return to Provider', but this is not necessary.

 ![n3rgy Auth Success 2](../assets/img/setup/n3rgyAuthSuccessStep2.png)

If you performed the sign-up process in a separate tab, you can close the tab and return to the application. 

At this point, ensure you now click *Check Authorization*. This makes the application test that it can now access your Meter data from n3rgy. 

If the authorization with n3rgy was completed successfully, this step should work without any issue and the application will begin to retrieve and analyse your historical Meter data. This can take a few seconds.
