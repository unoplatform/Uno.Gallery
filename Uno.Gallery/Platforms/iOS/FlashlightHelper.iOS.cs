using System;
using System.Threading.Tasks;
using AVFoundation;
using Foundation;

namespace Uno.Gallery.Platforms.iOS
{
    public static class FlashlightHelper
    {
        public static async Task<bool> IsFlashlightAvailableAsync()
        {
            return await Task.Run(() =>
            {
                var device = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);
                return device?.HasTorch == true;
            });
        }

        public static async Task<bool> SetFlashlightAsync(bool enabled, float brightness = 1.0f)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    var device = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);
                    if (device?.HasTorch != true)
                        return false;

                    // Request camera permission
                    var status = await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVAuthorizationMediaType.Video);
                    if (!status)
                        return false;

                    device.LockForConfiguration(out var error);
                    if (error != null)
                        return false;

                    try
                    {
                        if (enabled)
                        {
                            // Clamp brightness between 0.0 and 1.0
                            brightness = Math.Max(0.0f, Math.Min(1.0f, brightness));
                            device.SetTorchModeLevel(brightness, out error);
                            if (error != null)
                                return false;
                        }
                        else
                        {
                            device.TorchMode = AVCaptureTorchMode.Off;
                        }
                        return true;
                    }
                    finally
                    {
                        device.UnlockForConfiguration();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"FlashlightHelper error: {ex.Message}");
                    return false;
                }
            });
        }

        public static async Task<bool> IsFlashlightEnabledAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var device = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);
                    return device?.TorchMode == AVCaptureTorchMode.On;
                }
                catch
                {
                    return false;
                }
            });
        }
    }
}
