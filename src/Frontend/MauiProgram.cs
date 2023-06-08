namespace Frontend;

using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using Microsoft.Extensions.Configuration;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseBarcodeReader()
#if !DEBUG
            .UseSentry(options => {
                // The DSN is the only required setting.
                options.Dsn = "https://5248e12f92b54298a28ce1aa02a1ff62@o86280.ingest.sentry.io/4505081436766208";
            })
#endif
            .ConfigureFonts(fonts => {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

#if ANDROID || IOS
        builder.UseShiny();
        builder.Configuration.AddJsonPlatformBundle(optional: false);

        var cfg = builder.Configuration.GetSection("Firebase");
        builder.Services.AddPushFirebaseMessaging(new(
            false,
            cfg["AppId"],
            cfg["SenderId"],
            cfg["ProjectId"],
            cfg["ApiKey"]
        ));
#endif

        // We have to add these mappings to fix the border color of text input widgets (Picker, Entry, Editor)
        // when the android theme changes when the app is open (issue: https://github.com/dotnet/maui/issues/15359).
        // We have to add this because in the current version of Maui, there are problems with dynamic app theme
        // change. In the future once the following PR is merged (PR: https://github.com/dotnet/maui/pull/15449)
        // and released, we'll be able to remove this workaround.
        // Once PR is merged, Platforms/Android/AndroidExtraMappers.cs file can be removed.
#if ANDROID
        AndroidExtraMappers.AddMappers();
#endif

        return builder.Build();
    }
}
