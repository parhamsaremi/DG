namespace Frontend;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using Microsoft.Extensions.Configuration;

#if ANDROID
using Microsoft.Maui.Handlers;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Platform;
using Android.Graphics;
#endif

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseBarcodeReader()
            .ConfigureFonts(fonts => {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            })
            .UseSentry(options => {
                // The DSN is the only required setting.
                options.Dsn = "https://5248e12f92b54298a28ce1aa02a1ff62@o86280.ingest.sentry.io/4505081436766208";
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
#if ANDROID
        void MapTextColorToBorderColor(IViewHandler handler, Color color)
        {
            if (handler.PlatformView is AppCompatEditText editText)
            {
                var colorFilter = new PorterDuffColorFilter(color, PorterDuff.Mode.SrcAtop);
                editText.Background.SetColorFilter(colorFilter);
            }
        }
        EntryHandler.Mapper.AppendToMapping(
                        "TextColor",
                        (IEntryHandler handler, IEntry view) => {
                            MapTextColorToBorderColor(handler, view.TextColor.ToPlatform());
                        }
                    );

        EditorHandler.Mapper.AppendToMapping(
                        "TextColor",
                        (IEditorHandler handler, IEditor view) => {
                            MapTextColorToBorderColor(handler, view.TextColor.ToPlatform());
                        }
                    );

        PickerHandler.Mapper.AppendToMapping(
                        "TextColor",
                        (IPickerHandler handler, IPicker view) => {
                            MapTextColorToBorderColor(handler, view.TextColor.ToPlatform());
                        }
                    );
#endif
        return builder.Build();
    }
}
