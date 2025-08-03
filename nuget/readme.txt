In-App Billing Plugin for .NET MAUI

Version 9.0+ - .NET 9+
1. Built against Android Billing v7

Version 8.0+ - .NET 8+
1. Updated APIs and you must target .NET 8

Version 7.0+  - Major Android updates
1.) You must compile and target against Android 12 or higher
2.) Android: Now using Android Billing v6
3.) Android: Major changes to Android product details, subscriptions, and more

Please read through: https://developer.android.com/google/play/billing/migrate-gpblv6


Version 5.0+ has significant updates!
1.) We have removed IInAppBillingVerifyPurchase from all methods. All data required to handle this yourself is returned.
2.) iOS ReceiptURL data is avaialble via ReceiptData
3.) We are now using Android Billing version 4
4.) Major breaking chanages across the API including AcknowledgePurchaseAsync being changed to FinalizePurchaseAsync
5.) Tons of new APIs when you get information about products

Please read documentation for all changes at https://github.com/jamesmontemagno/InAppBillingPlugin

Version 4.0 has significant updates.

1.) You must compile and target against Android 10 or higher
2.) On Android you must handle pending transactions and call `FinalizePurchaseAsync` when done
3.) On Android HandleActivityResult has been removed.
4.) We now use Xamarin.Essentials and setup is required per docs.

Find the latest setup guides, documentation, and testing instructions at: 
https://github.com/jamesmontemagno/InAppBillingPlugin
