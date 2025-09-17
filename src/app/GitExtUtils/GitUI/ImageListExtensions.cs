namespace GitUI;

public static class ImageListExtensions
{
    /// <summary>
    /// A regression was introduced in .NET 8 which causes an incorrect background color to be set
    /// which manifests when using transparent images. See https://github.com/dotnet/winforms/issues/10462
    /// This method should be removed once the underlying issue is fixed.
    /// On non-Windows platforms, this fix is not needed.
    /// </summary>
    public static ImageList FixImageTransparencyRegression(this ImageList imageList)
    {
        // The image transparency regression is Windows-specific
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            NativeMethods.ImageListSetBkColor(imageList.Handle, NativeMethods.ComCtl32CLRNone);
        }
        return imageList;
    }
}
