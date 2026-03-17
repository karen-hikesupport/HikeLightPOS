using System;
using Android.Content;
using Android.OS;
using HikePOS.Droid.DependencyServices;
using HikePOS.Interfaces;
using Stream = Android.Media.Stream;
using Application = Android.App.Application;
using Android.Media;

[assembly: Dependency(typeof(PlayAndVibrate))]
namespace HikePOS.Droid.DependencyServices
{
	public class PlayAndVibrate : IPlayAndVibrate
    {
        public void PlayBeepAndVibrate()
        {
            try
            {
                Vibrator vibrator = (Vibrator)Application.Context.GetSystemService(Context.VibratorService);
                vibrator.Vibrate(VibrationEffect.CreateOneShot(150, VibrationEffect.DefaultAmplitude));

                ToneGenerator toneGenerator = new ToneGenerator(Stream.Music, 200);
                toneGenerator.StartTone(Tone.PropBeep);
            }
            catch (Exception ex)
            {

            }
        }
    }
}

