namespace MegaCrit.Sts2.Core.Platform;

public static class SupportedWindowModeExtensions
{
	public static bool ShouldForceFullscreen(this SupportedWindowMode mode)
	{
		if (mode != SupportedWindowMode.FullscreenOnly)
		{
			return mode == SupportedWindowMode.FullscreenOnlyDisplayToggle;
		}
		return true;
	}
}
You are not using the latest version of the tool, please update.
Latest version is '10.0.0.8330' (yours is '9.1.0.7988')
