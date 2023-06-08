using Android.Graphics;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Frontend;


static class AndroidExtraMappers
{
    static private void MapTextColorToBorderColor(IViewHandler handler, Android.Graphics.Color color)
    {
        if (handler.PlatformView is AppCompatEditText editText)
        {
            var colorFilter = new PorterDuffColorFilter(color, PorterDuff.Mode.SrcAtop);
            editText.Background.SetColorFilter(colorFilter);
        }
    }

    static internal void AddMappers()
    {
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
    }
}
