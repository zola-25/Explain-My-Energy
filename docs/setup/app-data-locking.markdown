---
layout: default
title: Locking App and Setup Data
nav_order: 10
parent: Setup
---

# Locking the App and Setup Data

This page has three sections. 

- [Rationale](#rationale) explains the reasons for the app's design decisions, and why it is designed to be used by a single user, using single browser, without requiring a user account.

- [Data Stored - Sensitivity and Risks](#data-stored---sensitivity-and-risks) goes into comprehensive detail on the data stored by the app; the risks and benefits of storing data on a local device; which data items are sensitive and which are not; and avoiding any risk of accidentally (or intentionally) breaking Data Protection laws.

- [Locking and Unlocking](#locking-the-app) explains the app's 'Lock' feature, which allows you to encrypt certain Setup Data with a password, and disable the app's features until the correct password is entered along with the correct IHD MAC ID.


## Rationale

Explain My Energy is intended to be as accessible as possible, requiring the least amount of technical knowledge to use, as well as as only the the data that that it needs to function - no more, no less.

As we use the web today, we are constantly being asked to create accounts, and provide personal details, to access services. This is not always necessary, sometimes not handled legally, and often not in our best interests. It is also disempowering, as we are forced to trust third-parties with our data, and have no control over how they use it, or how well they protect it.

### A Story about LastPass

Data breaches affecting large organisations are common, with email addresses being exposed, and then used to attempt phishing - when they occur, we suddenly receive more spam in our mailbox. 

In more serious cases, our passwords, payment card details, and other sensitive data, are exposed. Notable companies that have suffered data breaches include LinkedIn, Adobe, and even LastPass, a password manager with several million users. In that breach, every LastPass user had sensitive data exposed - passwords were stil encrypted, but other details, such as the ***website addresses linked to the passwords***, and the user's ***payment card details*** for their LastPass subscription, were exposed as **plaintext** in the breach.

The most unlucky users were the ones that had been with LastPass the longest. Like all password storage, LastPass require the user just one large master password to unlock all the passwords a user might save. This master password it doesn't store, but creates a hash code from it, and stores that instead. Hash functions used for passwords usually repeat the hashing many thousands of times, to make it computationally expensive to attempt to guess the hash code by making many guesses and chancing upon a match.

The number of iterations is meant to increase as the computational power generally available increases. But LastPass hadn't automatically increased the number of iterations for older accounts, and so the older the account, the easier it was to use typical hacking techniques crack the password. And so older accounts got hacked.

And of course, the breach only needed to happen once, and the data remains exposed forever. Computational power increases, but the hashing algorithm that was used to create the hash of each user's password - the hash that was exposed - remains the same forever. So time is on the side of the hackers. As as a cruel twist, the number of iterations set to hash the password was also exposed as plaintext. So the hackers knew which accounts were the easiest to crack.

For ever LastPass user, the only option to ensure security for the dozens or hundreds of accounts and services a user might have, it to change every single account password. This took users weeks.

### One less user account

So with that said, maybe it's time to reduce the number of accounts we have to create to use minor apps or simply buy from an web-shop. 

Of course, there are many valid reasons for requiring users to part with their email address and a hastily created password. One of them is proving a user's authenticity - are they the same person coming back, who originally created the account. Obviously if it's a personalized service, like healthcare, or a bank, then this is rather important. Could it be done without an account? How does it work in a bank branch? Usually, having a bank card and knowing the PIN is the authentication mechanism here. Exactly the same was as it's done online, with a card reader and a PIN. Think about that. An online bank account doesn't need your email address, or a 15 digit password with 2 special characters and a number. It just needs a card reader and a PIN.

This app is intentionally designed to avoid requiring user accounts, whether it be a dedicated server, or a Third-Party Authentication Providers such as those offered by Google, Facebook, Microsoft or Apple. 

This app trusts that the user using the app is the same user that set it up. The trade-off is that the app stores all data in local browser storage. This doesn't mean it's necessarily any more vulnerable to certain hacks that a server-based app - any browser-based app is vulnerable to attack via the browser, and a Cross-Site Scripting (XSS) attack can wreak havoc on a server-based app just as easily. With fewer third-party dependencies, this app is *arguably more secure* against this type of attack.

But it does mean that the app is more vulnerable if the device is stolen, or is used on a shared device with untrusted users. Accounting for this, it allows the user to lock the app functionality with a password, which also encrypts sensitive data, as described later on this page.

## Data Stored - Sensitivity and Risks

To function, the app requires your In-Home Device (IHD) MAC ID, your Postal Area (the first 3-4 digits of a Post Code), and your Gas and Electricity MPRN/MPAN numbers, and the date you moved-in to the property.

In normal use, with the app 'Unlocked', this data is stored unencrypted in your browser's local storage. While browsers do not allow other websites or web applications to access this data, it is possible for someone with access to your computer to view this data. 

Cross-Site Scripting (XSS) attacks, or devices compromised with malware, could also potentially allow this data to be accessed by malicious actors.

The most sensitive data item stored in by the app is your IHD MAC ID. Most property's meter numbers, MPRNs and MPANs, are publicly available, and the Postal Area, being only half a Post Code, in almost all cases will cover a large area with many properties. However, the IHD MAC ID is unique to your property, and is even relied upon by n3rgy as proof of authenticity when authorizing data access for your meters.

### Move-In Date - Data Protection Implications

The month/year you provide for your Move-In Date may not even have to be the actual month/year you moved in, as n3rgy provides only around 13 months worth, which is also the ideal amount of data the app needs for historical and temperature-based forecasting. It is not even clear if the central UK data-center used by n3rgy stores much beyond this 13 month period, or if many energy providers do either. 
 
If you wish to use a fake Move-In Date at least 13 months in the past, but after you moved into the property, feel free to try authorizing your meters with this date, it may well work. 

{: .warning }
> However, regardless of how long you have lived in your property, you should not set the Move-In Date to anytime before you were a legal resident, and then try to authorize access with n3rgy - whether accidentally or intentionally, you may obtain energy usage data and tariff information that is not yours to access.
> 
> This is a breach of the Terms & Conditions regarding Acceptable Use of Explain My Energy, and likely a breach of the Terms of Service of the n3rgy.com Consumer API, and could break Data Protection laws.

### App Locking - Encrypting Setup Data

Given the data stored by the app is vulnerable is the device is stolen or accessed by untrusted users, the app has a 'Lock' feature that allows you to encrypt certain Setup Data with a password, and disable the app's features.

The app cannot be locked until Household Setup is complete, as without this first step, there is no data to encrypt and the app is in any case unusable.

When 'Locked', the only risk to the user is a forcible corruption of the app's local storage, making it impossible to unlock the app, meaning a hard reset is necessary and the loss of all data corrupted. 

For Chrome, this means pressing F12, finding the Application tab along the top, and then 'Storage' on the left, and 'Local storage' underneath. If your app has been corrupted, whether from a bug, malicious activity or bad luck, you will have manually to wipe the app's storage and start from scratch. Go to Storage on the top left and click 'Clear site data'. Much better guides are available online for Chrome and all other browsers.

When locked the app cannot be used then until it is 'Unlocked' - by supplying the previously specified password, along with the user's IHD MAC ID, which remains encrypted in local storage.

The Lock and Unlocking process is explained in more detail in the next section.

## Locking the App

**Locking**

When the Household Setup is complete, the app can be locked by clicking the 'Lock' button on the Setup page. This will open a dialog with 3 fields. 

The first is the user's IHD MAC ID, which is readonly here, and is essential to note down or save. It has a quick 'copy-to-clipboard' button next to it so it can be quickly copied to another, hopefully secure or temporary, location on your device. Password managers will also interpret this field as a Username, so it can be saved along with the password and quickly auto-filled when the app is unlocked. 

The second two fields are for the password. The first is for the password itself, and the second is for confirmation. The password strength is displayed below the field, and the password is not accepted until it is strong enough.

The password is not by default visible, but can be revealed by clicking the 'eye' icon next to the field. 

The password is not stored anywhere by the app, it is forgotten as soon as the 'Lock Data' button is clicked with matching passwords. The app is frozen while the IHD MAC ID, Postal Area, and any MPRN/MPAN numbers already stored are deleted and replaced by their encrypted versions. If there are errors encountered during this process, the app has a rollback mechanism intended to preserve your data if something doesn't work as expected.

Data that is not encrypted includes your Move-In Date, your Electricity and Gas Tariff Rates (if you input your own over the defaults), and your Meter kWh Readings.

At this point, the app is locked, and the only page that remains usable is the Setup page, where the 'Unlock' feature is located. Attempting to forcibly navigate to any page that might have some functionality, may load the page, but no features will work, if even visible. The app will not access any Third-Party APIs, or display any data in the UI.

Closing the app and returning to it, will not unlock it. The app will remain locked until the correct password is entered, along with the correct IHD MAC ID.

**Unlocking**

The only way to unlock it is to enter the correct password, along with the correct IHD MAC ID, on a form on the Setup page, located in same place as the 'Lock' button was.

Once you enter these details, the app will decrypt the data, and if successful, the app will update all data (such as energy readings and weather data) and the UI will be restored.

The password cannot be recovered if forgotten. If you forget the password, or somehow lose your IHD MAC ID, the only option is to manually delete the app's local storage, which will delete all your setup data, and start the app again from scratch. The in-app option to delete the app's local storage is disabled while Locked, as this would defeat the purpose of the 'Lock' feature. 



