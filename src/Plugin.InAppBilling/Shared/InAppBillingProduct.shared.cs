using System;
using System.Collections.Generic;

namespace Plugin.InAppBilling
{
    /// <summary>
    /// Product info specific to Apple Platforms
    /// </summary>
    [Preserve(AllMembers = true)]
    public class InAppBillingProductAppleExtras
    {
        /// <summary>
        /// The identifier of the subscription group to which the subscription belongs.
        /// </summary>
        public string SubscriptionGroupId { get; set; }

        /// <summary>
        /// The period details for products that are subscriptions.
        /// </summary>
        public SubscriptionPeriod SubscriptionPeriod { get; set; }

        /// <summary>
        /// A Boolean value that indicates whether the product is available for family sharing in App Store Connect.
        /// </summary>
        public bool IsFamilyShareable { get; set; }

        /// <summary>
        /// iOS 11.2: gets information about product discount
        /// </summary>
        public InAppBillingProductDiscount IntroductoryOffer { get; set; } = null;

        /// <summary>
        /// iOS 12.2: gets information about product discount
        /// </summary>
        public List<InAppBillingProductDiscount> Discounts { get; set; } = null;
    }

    /// <summary>
    /// Windows Store subscription information extracted from SKUs
    /// </summary>
    [Preserve(AllMembers = true)]
    public class WindowsSubscriptionInfo
    {
        /// <summary>
        /// List of available SKUs for this subscription product
        /// </summary>
        public List<WindowsSkuInfo> Skus { get; set; } = new List<WindowsSkuInfo>();

        /// <summary>
        /// Indicates if any SKU has trial options available
        /// </summary>
        public bool HasTrialOptions { get; set; }

        /// <summary>
        /// The default/recommended SKU (usually the non-trial version)
        /// </summary>
        public WindowsSkuInfo DefaultSku { get; set; }
    }

    /// <summary>
    /// Windows Store SKU information
    /// </summary>
    [Preserve(AllMembers = true)]
    public class WindowsSkuInfo
    {
        /// <summary>
        /// Store ID of this SKU
        /// </summary>
        public string StoreId { get; set; }

        /// <summary>
        /// Title of this SKU
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Description of this SKU
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Price information for this SKU
        /// </summary>
        public WindowsPriceInfo Price { get; set; }

        /// <summary>
        /// Indicates if this SKU is a subscription
        /// </summary>
        public bool IsSubscription { get; set; }

        /// <summary>
        /// Indicates if this SKU is a trial
        /// </summary>
        public bool IsTrial { get; set; }

        /// <summary>
        /// Language for this SKU
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Custom developer data
        /// </summary>
        public string CustomDeveloperData { get; set; }

        /// <summary>
        /// Extended JSON data for this SKU
        /// </summary>
        public string ExtendedJsonData { get; set; }

        /// <summary>
        /// Subscription-specific information if this is a subscription SKU
        /// </summary>
        public WindowsStoreSubscriptionInfo SubscriptionInfo { get; set; }

        /// <summary>
        /// Different pricing availabilities for this SKU
        /// </summary>
        public List<WindowsAvailabilityInfo> Availabilities { get; set; } = new List<WindowsAvailabilityInfo>();
    }

    /// <summary>
    /// Windows Store price information
    /// </summary>
    [Preserve(AllMembers = true)]
    public class WindowsPriceInfo
    {
        /// <summary>
        /// Formatted price with currency symbol
        /// </summary>
        public string FormattedPrice { get; set; }

        /// <summary>
        /// Formatted base price (before any discounts)
        /// </summary>
        public string FormattedBasePrice { get; set; }

        /// <summary>
        /// Formatted recurring price for subscriptions
        /// </summary>
        public string FormattedRecurrencePrice { get; set; }

        /// <summary>
        /// Unformatted price as decimal
        /// </summary>
        public decimal UnformattedPrice { get; set; }

        /// <summary>
        /// Unformatted base price as decimal
        /// </summary>
        public decimal UnformattedBasePrice { get; set; }

        /// <summary>
        /// Unformatted recurring price as decimal
        /// </summary>
        public decimal UnformattedRecurrencePrice { get; set; }

        /// <summary>
        /// Currency code (e.g., USD, EUR)
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Indicates if this price is on sale
        /// </summary>
        public bool IsOnSale { get; set; }

        /// <summary>
        /// When the sale ends (if on sale)
        /// </summary>
        public DateTimeOffset? SaleEndDate { get; set; }
    }

    /// <summary>
    /// Windows Store subscription details from StoreSku.SubscriptionInfo
    /// </summary>
    [Preserve(AllMembers = true)]
    public class WindowsStoreSubscriptionInfo
    {
        /// <summary>
        /// Number of billing periods (e.g., 1 for monthly, 12 for yearly)
        /// </summary>
        public uint BillingPeriod { get; set; }

        /// <summary>
        /// Unit of billing period (e.g., "Month", "Year")
        /// </summary>
        public string BillingPeriodUnit { get; set; }

        /// <summary>
        /// Whether this subscription has a trial period
        /// </summary>
        public bool HasTrialPeriod { get; set; }

        /// <summary>
        /// Number of trial periods
        /// </summary>
        public uint TrialPeriod { get; set; }

        /// <summary>
        /// Unit of trial period (e.g., "Day", "Week")
        /// </summary>
        public string TrialPeriodUnit { get; set; }
    }

    /// <summary>
    /// Windows Store availability information (different pricing options)
    /// </summary>
    [Preserve(AllMembers = true)]
    public class WindowsAvailabilityInfo
    {
        /// <summary>
        /// Store ID for this availability
        /// </summary>
        public string StoreId { get; set; }

        /// <summary>
        /// End date for this availability
        /// </summary>
        public DateTimeOffset? EndDate { get; set; }

        /// <summary>
        /// Price information for this availability
        /// </summary>
        public WindowsPriceInfo Price { get; set; }
    }

    /// <summary>
    /// Product info specific to Windows platform
    /// </summary>
    [Preserve(AllMembers = true)]
    public class InAppBillingProductWindowsExtras
    {
        /// <summary>
        /// Gets the base price for the add-on (also called an in-app product or IAP) with the appropriate formatting for the current market.
        /// </summary>
        public string FormattedBasePrice { get; set; }

        /// <summary>
        /// Gets the recurring price for subscription products
        /// </summary>
        public string FormattedRecurrencePrice { get; set; }
        
        /// <summary>
        /// Complete product data in JSON format
        /// </summary>
        public string ExtendedJsonData { get; set; }

        /// <summary>
        /// Whether the product has downloadable content
        /// </summary>
        public bool HasDigitalDownload { get; set; }

        /// <summary>
        /// In-app offer token for this product
        /// </summary>
        public string InAppOfferToken { get; set; }

        /// <summary>
        /// Whether the user already owns this product
        /// </summary>
        public bool IsInUserCollection { get; set; }

        /// <summary>
        /// Language for the product listing
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Link to the Store listing
        /// </summary>
        public Uri LinkUri { get; set; }

        /// <summary>
        /// Gets a value that indicates whether the add-on (also called an in-app product or IAP) is on sale.
        /// </summary>
        public bool IsOnSale { get; set; }

        /// <summary>
        /// Gets the end date of the sale period for the add-on (also called an in-app product or IAP).
        /// </summary>
        public DateTimeOffset SaleEndDate { get; set; }

        /// <summary>
        /// Gets the custom developer data string (also called a tag) that contains custom information about an add-on (also called an in-app product or IAP). This string corresponds to the value of the Custom developer data field in the properties page for the add-on in Partner Center.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Product type consumable
        /// </summary>
        public bool IsConsumable { get; set; }

        /// <summary>
        /// Product type unmanaged consumable
        /// </summary>
        public bool IsUnmanagedConsumable { get; set; }

        /// <summary>
        /// Product type durable
        /// </summary>
        public bool IsDurable { get; set; }

        /// <summary>
        /// Whether this product is a subscription
        /// </summary>
        public bool IsSubscription { get; set; }

        /// <summary>
        /// Detailed subscription information including SKUs and pricing options
        /// </summary>
        public WindowsSubscriptionInfo SubscriptionInfo { get; set; }

        /// <summary>
        /// Gets the list of keywords associated with the add-on (also called an in-app product or IAP). These strings correspond to the value of the Keywords field in the properties page for the add-on in Partner Center. 
        /// </summary>
        public IEnumerable<string> Keywords { get; set; }
    }

    /// <summary>
    /// Extras specific to Android
    /// </summary>
    [Preserve(AllMembers = true)]
    public class InAppBillingProductAndroidExtras
    {
        /// <summary>
        /// The period details for products that are subscriptions.
        /// </summary>
        public List<SubscriptionOfferDetail> SubscriptionOfferDetails { get; set; }

        ///// <summary>
        ///// Subscription period, specified in ISO 8601 format.
        ///// </summary>
        //public string SubscriptionPeriod { get; set; }

        ///// <summary>
        ///// Trial period, specified in ISO 8601 format.
        ///// </summary>
        //public string FreeTrialPeriod { get; set; }

        ///// <summary>
        ///// Icon of the product if present
        ///// </summary>
        //public string IconUrl { get; set; }

        ///// <summary>
        ///// Gets or sets the localized introductory price.
        ///// </summary>
        ///// <value>The localized introductory price.</value>
        //public string LocalizedIntroductoryPrice { get; set; }

        ///// <summary>
        ///// Number of subscription billing periods for which the user will be given the introductory price, such as 3
        ///// </summary>
        //public int IntroductoryPriceCycles { get; set; }

        ///// <summary>
        ///// Billing period of the introductory price, specified in ISO 8601 format
        ///// </summary>
        //public string IntroductoryPricePeriod { get; set; }

        ///// <summary>
        ///// Introductory price of the product in micro-units
        ///// </summary>
        ///// <value>The introductory price.</value>
        //public Int64 MicrosIntroductoryPrice { get; set; }

        ///// <summary>
        ///// Formatted original price of the item, including its currency sign.
        ///// </summary>
        //public string OriginalPrice { get; set; }

        ///// <summary>
        ///// Original price in micro-units, where 1,000,000, micro-units equal one unit of the currency
        ///// </summary>
        //public long MicrosOriginalPriceAmount { get; set; }
    }

    /// <summary>
    /// Product being offered
    /// </summary>
    [Preserve(AllMembers = true)]
    public class InAppBillingProduct
    {
        /// <summary>
        /// Name of the product
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the product
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Product ID or sku
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// Localized Price (not including tax)
        /// </summary>
        public string LocalizedPrice { get; set; }

        /// <summary>
        /// ISO 4217 currency code for price. For example, if price is specified in British pounds sterling is "GBP".
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Price in micro-units, where 1,000,000 micro-units equal one unit of the 
        /// currency. For example, if price is "€7.99", price_amount_micros is "7990000". 
        /// This value represents the localized, rounded price for a particular currency.
        /// </summary>
        public Int64 MicrosPrice { get; set; }

        /// <summary>
        /// Extra information for apple platforms
        /// </summary>
        public InAppBillingProductAppleExtras AppleExtras { get; set; } = null;

        /// <summary>
        /// Extra information for Android platforms
        /// </summary>
        public InAppBillingProductAndroidExtras AndroidExtras { get; set; } = null;

        /// <summary>
        /// Extra information for Windows platforms
        /// </summary>
        public InAppBillingProductWindowsExtras WindowsExtras { get; set; } = null;
    }
}