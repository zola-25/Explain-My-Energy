---
layout: default
title: Meter Authorization
nav_order: 6
parent: Setup
---

# Meter Authorization

Once you have entered your household details, and set up your Gas and/or Electricity meters, you can authorize each meter with n3rgy. This is a one-off process, and once authorized, the application will retrieve the meter data, and analyze it.

### Authorization Process

The authorization process is the same for both Gas and Electricity meters, but requires separate authorization for each meter. 

An embedded display is shown to authorize access to your historical Gas/Electricity Meter usage with n3rgy. For convenience, the necessary IHD MAC ID and MPRN/MPAN number are made available as one click copy buttons. Just click the blue clipboard icon next to the number, and it will be copied to your clipboard. You can then paste it into the relevant field in the n3rgy authorization window.

![n3rgy Sign-up Copy Buttons](../assets/img/setup/n3rgyAuthCopyButtons.png){:width="80%"}

If you find the embedded window too small to work with, you can click *Open in separate tab* to open the authorization window in a separate browser tab.

![n3rgy Sign-up Open Separate Tab](../assets/img/setup/n3rgySignupOpenInSeparateTab.png){:width="50%"}

Signing up with n3rgy first requires you enter or paste your MPRN/MPAN number, which the service will check to confirm it has access to. Not all energy providers provide data to n3rgy, so if the lookup fails, it won't be possible to use the application with this meter.

Enter or paste your MPRN/MPAN number in the *'MPxN*'* field on the initial screen, and click *'Sign up'*.

![n3rgy Sign-up Screen MPxN Entered](../assets/img/setup/n3rgySignupScreenCredentialsEntered.png){:width="60%"}

If the lookup succeeds, you will be prompted confirm the Terms and Conditions, and then asked to enter/paste your IHD MAC ID, which is used as authentication along with your Meter Number.

![n3rgy Sign-up Confirm IHD MAC ID](../assets/img/setup/n3rgySignupScreenConfirmIHD_MAC.png){:width="60%"}

 Once you have entered the IHD MAC ID and submitted the form, the n3rgy service will ensure it applies to the Meter MPRN/MPAN number, and if successful will notify that the IHD MAC ID is authorized to access the energy meter's historical data.
 
 ![n3rgy Auth Success 1](../assets/img/setup/n3rgyAuthSuccess.png){:width="70%"}

 You can also click *'Return to Provider'*, but this is not necessary.

 ![n3rgy Auth Success 2](../assets/img/setup/n3rgyAuthSuccessStep2.png){:width="50%"}

If you performed the sign-up process in a separate tab, you can close the tab and return to the application. 

At this point, ensure you now click *Check Authorization*. This makes the application test that it can now access your Meter data from n3rgy. 

![n3rgy Auth Success Check](../assets/img/setup/n3rgyAuthSuccessCheckAuthorization.png)

If the authorization with n3rgy was completed successfully, this step should work without any issue and the application will begin to retrieve and analyze your historical Meter data. This can take a few seconds.
