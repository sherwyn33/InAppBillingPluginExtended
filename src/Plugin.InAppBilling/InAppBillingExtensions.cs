using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Plugin.InAppBilling;


public static class InAppBillingExtensions
{
    public static MauiAppBuilder UseInAppBilling(this MauiAppBuilder builder)
    {
        builder.ConfigureLifecycleEvents(events =>
        {
#if WINDOWS
            events.AddWindows(win => win
                .OnWindowCreated(window =>
                {
                    InAppBilling.InAppBillingImplementation.GetActiveWindow = () => window;
                })
            );
#endif
        });

        return builder;
    }
}