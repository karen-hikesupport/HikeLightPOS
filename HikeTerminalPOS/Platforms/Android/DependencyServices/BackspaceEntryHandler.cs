using Android.Views;
using AndroidX.AppCompat.Widget;
using HikePOS.UserControls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;

namespace HikePOS.Droid.DependencyServices;

public class BackspaceEntryHandler : EntryHandler
{
    protected override void ConnectHandler(AppCompatEditText nativeView)
    {
        base.ConnectHandler(nativeView);
        if (nativeView == null)
            return;
            
        nativeView.KeyPress += (sender, e) =>
        {
            if (VirtualView is not BackspaceEntry entry)
                return;

            switch (e.KeyCode)
            {
                case Keycode.Del:
                    if (string.IsNullOrEmpty(nativeView.Text))
                        entry.RaiseBackspacePressed();
                    entry.Text = ""; // You may want to conditionally do this
                    break;

                case Keycode.Num0:
                case Keycode.Num1:
                case Keycode.Num2:
                case Keycode.Num3:
                case Keycode.Num4:
                case Keycode.Num5:
                case Keycode.Num6:
                case Keycode.Num7:
                case Keycode.Num8:
                case Keycode.Num9:
                    var txt = ((int)e.KeyCode - (int)Keycode.Num0).ToString();
                    entry.Text = entry.Text + txt;
                    break;
            }
        };

        nativeView.SetBackground(null);

    }
}