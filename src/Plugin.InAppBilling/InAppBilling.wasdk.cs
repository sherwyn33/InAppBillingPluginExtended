using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Windows.Services.Store;
using Microsoft.Maui.Platform;

namespace Plugin.InAppBilling
{
    public class InAppBillingImplementation : BaseInAppBilling
    {
        private StoreContext _context;

        public static Func<Window> GetActiveWindow { get; set; }

        private StoreContext GetStoreContext()
        {
            if (_context == null)
            {
                var window = GetActiveWindow?.Invoke();
                if (window is null)
                    throw new NullReferenceException("GetActiveWindow returned null");

                var handle = window.GetWindowHandle();

                _context = StoreContext.GetDefault();
                WinRT.Interop.InitializeWithWindow.Initialize(_context, handle);
            }
            return _context;
        }

        public override bool InTestingMode { get; set; } = false;

        public override async Task<bool> ConsumePurchaseAsync(string productId, string transactionIdentifier, int quantity, CancellationToken cancellationToken = default)
        {
            var context = GetStoreContext();

            var trackingId = Guid.NewGuid();
            var result = await context.ReportConsumableFulfillmentAsync(productId, (uint)quantity, trackingId);

            switch (result.Status)
            {
                case StoreConsumableStatus.InsufficentQuantity:
                    throw new InAppBillingPurchaseException(PurchaseError.InsufficentQuantity, result.ExtendedError?.Message);
                case StoreConsumableStatus.Succeeded:
                    return true;
                case StoreConsumableStatus.NetworkError:
                    throw new InAppBillingPurchaseException(PurchaseError.ProductRequestFailed, result.ExtendedError?.Message);
                case StoreConsumableStatus.ServerError:
                    throw new InAppBillingPurchaseException(PurchaseError.ServiceUnavailable, result.ExtendedError?.Message);
                default:
                    throw new InAppBillingPurchaseException(PurchaseError.GeneralError, result.ExtendedError?.Message);
            }
        }

        public override async Task<IEnumerable<InAppBillingProduct>> GetProductInfoAsync(ItemType itemType, string[] productIds, CancellationToken cancellationToken = default)
        {
            var context = GetStoreContext();

            var filter = itemType.ToProductFilter();
            var results = await context.GetStoreProductsAsync(filter, productIds);

            if (results.ExtendedError != null)
            {
                System.Diagnostics.Debug.WriteLine($"GetProductInfoAsync ExtendedError: {results.ExtendedError}");
            }

            return results.Products.Values.Select(item => item.ToInAppBillingProduct()).ToList();
        }

        public override async Task<IEnumerable<InAppBillingPurchase>> GetPurchasesAsync(ItemType itemType, CancellationToken cancellationToken = default)
        {
            var context = GetStoreContext();
            var results = await context.GetAppLicenseAsync();

            return results.AddOnLicenses
                .Where(l => l.Value.IsActive)
                .Select(item => item.Value.ToInAppBillingPurchase()).ToList();
        }

        public override async Task<InAppBillingPurchase> PurchaseAsync(string productId, ItemType itemType, string obfuscatedAccountId = null, string obfuscatedProfileId = null, string subOfferToken = null, CancellationToken cancellationToken = default)
        {
            var context = GetStoreContext();

            if (itemType == ItemType.InAppPurchase || itemType == ItemType.InAppPurchaseConsumable)
            {
                var result = await context.RequestPurchaseAsync(productId);
                return HandlePurchaseResult(result, productId);
            }
            else if (itemType == ItemType.Subscription)
            {
                var userOwnsSubscription = await CheckIfUserHasSubscriptionAsync(context, productId);
                if (userOwnsSubscription)
                {
                    var existingPurchase = await GetExistingSubscriptionPurchase(context, productId);
                    if (existingPurchase != null)
                        return existingPurchase;

                    throw new InAppBillingPurchaseException(PurchaseError.AlreadyOwned, "User already has an active subscription");
                }

                var productResult = await context.GetStoreProductsAsync(new string[] { "Durable" }, new string[] { productId });

                if (productResult.ExtendedError != null || !productResult.Products.ContainsKey(productId))
                {
                    System.Diagnostics.Debug.WriteLine($"PurchaseAsync Subscription: {productResult.ExtendedError}");
                    throw new InAppBillingPurchaseException(PurchaseError.ProductRequestFailed, $"Subscription product {productId} not found");
                }

                var subscriptionStoreProduct = productResult.Products[productId];

                var result = await subscriptionStoreProduct.RequestPurchaseAsync();
                return HandlePurchaseResult(result, productId);
            }
            else
            {
                throw new InAppBillingPurchaseException(PurchaseError.GeneralError, $"Unsupported item type: {itemType}");
            }
        }

        public override Task<InAppBillingPurchase> UpgradePurchasedSubscriptionAsync(string newProductId, string purchaseTokenOfOriginalSubscription, SubscriptionProrationMode prorationMode = SubscriptionProrationMode.ImmediateWithTimeProration, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        private InAppBillingPurchase HandlePurchaseResult(StorePurchaseResult result, string productId)
        {
            switch (result.Status)
            {
                case StorePurchaseStatus.AlreadyPurchased:
                    throw new InAppBillingPurchaseException(PurchaseError.AlreadyOwned, result.ExtendedError?.Message);

                case StorePurchaseStatus.Succeeded:
                    return new InAppBillingPurchase
                    {
                        ProductId = productId,
                        State = PurchaseState.Purchased,
                        TransactionIdentifier = Guid.NewGuid().ToString(), // Windows doesn't provide transaction ID
                        PurchaseToken = string.Empty,
                        TransactionDateUtc = DateTime.UtcNow,
                        OriginalJson = string.Empty
                    };

                case StorePurchaseStatus.NotPurchased:
                    throw new InAppBillingPurchaseException(PurchaseError.UserCancelled, result.ExtendedError?.Message ?? "User cancelled the purchase");

                case StorePurchaseStatus.NetworkError:
                    throw new InAppBillingPurchaseException(PurchaseError.ProductRequestFailed, result.ExtendedError?.Message ?? "Network error occurred");

                case StorePurchaseStatus.ServerError:
                    throw new InAppBillingPurchaseException(PurchaseError.ServiceUnavailable, result.ExtendedError?.Message ?? "Server error occurred");

                default:
                    throw new InAppBillingPurchaseException(PurchaseError.GeneralError, result.ExtendedError?.Message ?? "Unknown purchase error");
            }
        }

        private async Task<InAppBillingPurchase> GetExistingSubscriptionPurchase(StoreContext context, string productId)
        {
            try
            {
                var appLicense = await context.GetAppLicenseAsync();

                foreach (var addOnLicense in appLicense.AddOnLicenses)
                {
                    var license = addOnLicense.Value;
                    if (license.SkuStoreId.StartsWith(productId, StringComparison.OrdinalIgnoreCase) && license.IsActive)
                    {
                        return new InAppBillingPurchase
                        {
                            ProductId = productId,
                            State = PurchaseState.Purchased,
                            TransactionIdentifier = license.InAppOfferToken,
                            ExpirationDate = license.ExpirationDate,
                            TransactionDateUtc = DateTime.UtcNow, // Windows doesn't provide original purchase date
                            OriginalJson = license.ExtendedJsonData
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting existing subscription: {ex.Message}");
            }
            return null;
        }

        private async Task<bool> CheckIfUserHasSubscriptionAsync(StoreContext context, string subscriptionStoreId)
        {
            try
            {
                var appLicense = await context.GetAppLicenseAsync();

                foreach (var addOnLicense in appLicense.AddOnLicenses)
                {
                    var license = addOnLicense.Value;
                    if (license.SkuStoreId.StartsWith(subscriptionStoreId, StringComparison.OrdinalIgnoreCase) && license.IsActive)
                    {
                        if (license.ExpirationDate == null || license.ExpirationDate > DateTimeOffset.UtcNow)
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking subscription status: {ex.Message}");
            }
            return false;
        }
    }

    static class WindowsUtils
    {
        public static List<string> ToProductFilter(this ItemType itemType)
        {
            var filterList = new List<string>();
            if (itemType == ItemType.InAppPurchase)
                filterList.Add("Durable");
            else if (itemType == ItemType.InAppPurchaseConsumable)
            {
                filterList.Add("Consumable");
                filterList.Add("UnmanagedConsumable");
            }
            else if (itemType == ItemType.Subscription)
            {
                filterList.Add("Durable"); // Subscriptions are Durable add-ons
            }

            return filterList;
        }

        public static InAppBillingPurchase ToInAppBillingPurchase(this StoreLicense license)
        {
            return new InAppBillingPurchase
            {
                ProductId = license.SkuStoreId,
                State = license.IsActive ? PurchaseState.Purchased : PurchaseState.Unknown,
                ExpirationDate = license.ExpirationDate,
                OriginalJson = license.ExtendedJsonData
            };
        }

        public static InAppBillingProduct ToInAppBillingProduct(this StoreProduct product)
        {
            var localizedPrice = !string.IsNullOrEmpty(product.Price.FormattedRecurrencePrice)
                ? product.Price.FormattedRecurrencePrice
                : product.Price.FormattedPrice;

            return new InAppBillingProduct
            {
                Name = product.Title,
                Description = product.Description,
                ProductId = product.StoreId,
                LocalizedPrice = localizedPrice,
                MicrosPrice = 0,
                CurrencyCode = product.Price.CurrencyCode ?? "USD",
                WindowsExtras = new InAppBillingProductWindowsExtras
                {
                    ExtendedJsonData = product.ExtendedJsonData,
                    HasDigitalDownload = product.HasDigitalDownload,
                    InAppOfferToken = product.InAppOfferToken,
                    IsInUserCollection = product.IsInUserCollection,
                    Language = product.Language,
                    LinkUri = product.LinkUri,
                    FormattedBasePrice = product.Price.FormattedBasePrice,
                    FormattedRecurrencePrice = product.Price.FormattedRecurrencePrice,
                    IsOnSale = product.Price.IsOnSale,
                    SaleEndDate = product.Price.SaleEndDate,
                    IsConsumable = product.ProductKind == "Consumable",
                    IsDurable = product.ProductKind == "Durable",
                    IsUnmanagedConsumable = product.ProductKind == "UnmanagedConsumable",
                    Keywords = product.Keywords,
                    Tag = string.Empty,
                    IsSubscription = product.Skus?.Any(sku => sku.IsSubscription) ?? false,
                    SubscriptionInfo = ExtractSubscriptionInfo(product)
                }
            };
        }

        // Extracts full subscription details for each subscription SKU
        private static WindowsSubscriptionInfo ExtractSubscriptionInfo(StoreProduct product)
        {
            if (product.Skus == null || !product.Skus.Any())
                return null;

            var subscriptionSkus = product.Skus.Where(sku => sku.IsSubscription).ToList();
            if (!subscriptionSkus.Any())
                return null;

            var skuInfos = subscriptionSkus.Select(sku => new WindowsSkuInfo
            {
                StoreId = sku.StoreId,
                Title = sku.Title,
                Description = sku.Description,
                Price = new WindowsPriceInfo
                {
                    FormattedPrice = sku.Price.FormattedPrice,
                    FormattedBasePrice = sku.Price.FormattedBasePrice,
                    FormattedRecurrencePrice = sku.Price.FormattedRecurrencePrice,
                    UnformattedPrice = TryParsePrice(sku.Price.FormattedPrice),
                    UnformattedBasePrice = TryParsePrice(sku.Price.FormattedBasePrice),
                    UnformattedRecurrencePrice = TryParsePrice(sku.Price.FormattedRecurrencePrice),
                    CurrencyCode = sku.Price.CurrencyCode ?? "USD",
                    IsOnSale = sku.Price.IsOnSale,
                    SaleEndDate = sku.Price.SaleEndDate
                },
                IsSubscription = sku.IsSubscription,
                IsTrial = sku.IsTrial,
                Language = sku.Language,
                CustomDeveloperData = sku.CustomDeveloperData,
                ExtendedJsonData = sku.ExtendedJsonData,
                SubscriptionInfo = sku.SubscriptionInfo != null ? new WindowsStoreSubscriptionInfo
                {
                    BillingPeriod = sku.SubscriptionInfo.BillingPeriod,
                    BillingPeriodUnit = sku.SubscriptionInfo.BillingPeriodUnit.ToString(),
                    HasTrialPeriod = sku.SubscriptionInfo.HasTrialPeriod,
                    TrialPeriod = sku.SubscriptionInfo.TrialPeriod,
                    TrialPeriodUnit = sku.SubscriptionInfo.TrialPeriodUnit.ToString()
                } : null,
                Availabilities = sku.Availabilities?.Select(avail => new WindowsAvailabilityInfo
                {
                    StoreId = avail.StoreId,
                    EndDate = avail.EndDate,
                    Price = new WindowsPriceInfo
                    {
                        FormattedPrice = avail.Price.FormattedPrice,
                        FormattedBasePrice = avail.Price.FormattedBasePrice,
                        FormattedRecurrencePrice = avail.Price.FormattedRecurrencePrice,
                        UnformattedPrice = TryParsePrice(avail.Price.FormattedPrice), // Unformated Price not implemented in Windows SDK, but interface exists
                        UnformattedBasePrice = TryParsePrice(avail.Price.FormattedBasePrice),
                        UnformattedRecurrencePrice = TryParsePrice(avail.Price.FormattedRecurrencePrice),
                        CurrencyCode = avail.Price.CurrencyCode ?? "USD",
                        IsOnSale = avail.Price.IsOnSale,
                        SaleEndDate = avail.Price.SaleEndDate
                    }
                }).ToList()
            }).ToList();

            return new WindowsSubscriptionInfo
            {
                Skus = skuInfos,
                HasTrialOptions = skuInfos.Any(sku => sku.IsTrial || (sku.SubscriptionInfo?.HasTrialPeriod ?? false)),
                DefaultSku = skuInfos.FirstOrDefault(sku => !sku.IsTrial) ?? skuInfos.FirstOrDefault()
            };
        }

        private static decimal TryParsePrice(string price)
        {
            if (string.IsNullOrWhiteSpace(price)) return 0;
            // Remove all but digits, dot, and comma.
            var clean = new string(price.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
            decimal.TryParse(clean, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var result);
            return result;
        }
    }
}
