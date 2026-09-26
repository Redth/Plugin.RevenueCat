#nullable enable

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using Svg.Skia;

namespace Plugin.RevenueCat.Paywalls.Rendering.Maui;

sealed class SvgPaywallImageView : SKCanvasView
{
	static readonly HttpClient HttpClient = new();

	CancellationTokenSource? loadCancellation;
	SKSvg? svg;
	Exception? loadError;
	string? source;
	int loadVersion;

	public SvgPaywallImageView()
	{
		Loaded += (_, _) => StartLoad(source);
		Unloaded += (_, _) => StartLoad(null);
	}

	public Aspect Aspect { get; set; } = Aspect.AspectFit;

	public CornerRadius ClipCornerRadius { get; set; }

	public string? Source
	{
		get => source;
		set
		{
			if (string.Equals(source, value, StringComparison.Ordinal))
			{
				return;
			}

			source = value;
			if (IsLoaded)
			{
				StartLoad(value);
			}
		}
	}

	protected override void OnHandlerChanging(HandlerChangingEventArgs args)
	{
		if (args.NewHandler is null)
		{
			StartLoad(null);
		}

		base.OnHandlerChanging(args);
	}

	protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
	{
		base.OnPaintSurface(e);

		var canvas = e.Surface.Canvas;
		canvas.Clear(SKColors.Transparent);

		var picture = svg?.Picture;
		if (picture is null)
		{
			if (loadError is not null)
			{
				DrawLoadError(canvas, e.Info);
			}

			return;
		}

		DrawPicture(canvas, picture, e.Info);
	}

	void StartLoad(string? nextSource)
	{
		var version = ++loadVersion;
		loadCancellation?.Cancel();
		loadCancellation?.Dispose();
		loadCancellation = null;
		svg?.Dispose();
		svg = null;
		loadError = null;
		InvalidateSurface();

		if (string.IsNullOrWhiteSpace(nextSource))
		{
			return;
		}

		var cancellation = new CancellationTokenSource();
		loadCancellation = cancellation;
		_ = LoadAsync(nextSource, cancellation.Token, version);
	}

	async Task LoadAsync(string nextSource, CancellationToken cancellationToken, int version)
	{
		SKSvg? loadedSvg = null;
		try
		{
			await using var stream = await OpenSvgStreamAsync(nextSource, cancellationToken);
			loadedSvg = new SKSvg();
			await Task.Run(() => loadedSvg.Load(stream), cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();

			await MainThread.InvokeOnMainThreadAsync(() =>
			{
				if (version != loadVersion || cancellationToken.IsCancellationRequested)
				{
					return;
				}

				svg?.Dispose();
				svg = loadedSvg;
				loadedSvg = null;
				loadError = null;
				InvalidateSurface();
			});
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
		}
		catch (Exception ex) when (ex is IOException or HttpRequestException or InvalidOperationException or NotSupportedException or ArgumentException or UnauthorizedAccessException or System.Xml.XmlException or FormatException)
		{
			await MainThread.InvokeOnMainThreadAsync(() =>
			{
				if (version != loadVersion)
				{
					return;
				}

				System.Diagnostics.Trace.TraceError("Paywall SVG failed to load: {0}", ex);
				loadError = ex;
				InvalidateSurface();
			});
		}
		finally
		{
			loadedSvg?.Dispose();
		}
	}

	static async Task<Stream> OpenSvgStreamAsync(string source, CancellationToken cancellationToken)
	{
		if (Uri.TryCreate(source, UriKind.Absolute, out var uri))
		{
			if (uri.IsFile)
			{
				return File.OpenRead(uri.LocalPath);
			}

			return await HttpClient.GetStreamAsync(uri, cancellationToken);
		}

		if (File.Exists(source))
		{
			return File.OpenRead(source);
		}

		var appPackagePath = source.TrimStart('/', '\\').Replace('\\', '/');
		try
		{
			return await FileSystem.OpenAppPackageFileAsync(appPackagePath);
		}
		catch (FileNotFoundException) when (!string.Equals(Path.GetFileName(appPackagePath), appPackagePath, StringComparison.Ordinal))
		{
			return await FileSystem.OpenAppPackageFileAsync(Path.GetFileName(appPackagePath));
		}
	}

	void DrawPicture(SKCanvas canvas, SKPicture picture, SKImageInfo info)
	{
		var bounds = picture.CullRect;
		if (bounds.Width <= 0 || bounds.Height <= 0 || info.Width <= 0 || info.Height <= 0)
		{
			return;
		}

		var scaleX = info.Width / bounds.Width;
		var scaleY = info.Height / bounds.Height;

		canvas.Save();
		ClipCanvas(canvas, info);
		if (Aspect == Aspect.Fill)
		{
			canvas.Scale(scaleX, scaleY);
			canvas.Translate(-bounds.Left, -bounds.Top);
		}
		else
		{
			var scale = Aspect == Aspect.AspectFill
				? Math.Max(scaleX, scaleY)
				: Math.Min(scaleX, scaleY);
			var x = (info.Width - bounds.Width * scale) / 2f;
			var y = (info.Height - bounds.Height * scale) / 2f;
			canvas.Translate(x, y);
			canvas.Scale(scale);
			canvas.Translate(-bounds.Left, -bounds.Top);
		}

		canvas.DrawPicture(picture);
		canvas.Restore();
	}

	void ClipCanvas(SKCanvas canvas, SKImageInfo info)
	{
		if (ClipCornerRadius == default || Width <= 0 || Height <= 0)
		{
			return;
		}

		var scale = Math.Min(info.Width / (float)Width, info.Height / (float)Height);
		var radii = new[]
		{
			ScaleRadius(ClipCornerRadius.TopLeft, scale),
			ScaleRadius(ClipCornerRadius.TopRight, scale),
			ScaleRadius(ClipCornerRadius.BottomRight, scale),
			ScaleRadius(ClipCornerRadius.BottomLeft, scale)
		};
		using var roundedRect = new SKRoundRect();
		roundedRect.SetRectRadii(new SKRect(0, 0, info.Width, info.Height), radii);
		canvas.ClipRoundRect(roundedRect, SKClipOperation.Intersect, true);
	}

	static SKPoint ScaleRadius(double radius, float scale)
	{
		var scaledRadius = (float)radius * scale;
		return new SKPoint(scaledRadius, scaledRadius);
	}

	static void DrawLoadError(SKCanvas canvas, SKImageInfo info)
	{
		using var paint = new SKPaint
		{
			Color = new SKColor(180, 180, 180),
			IsAntialias = true,
			StrokeWidth = 2,
			Style = SKPaintStyle.Stroke
		};

		var rect = new SKRect(1, 1, Math.Max(1, info.Width - 1), Math.Max(1, info.Height - 1));
		canvas.DrawRect(rect, paint);
		canvas.DrawLine(rect.Left, rect.Top, rect.Right, rect.Bottom, paint);
		canvas.DrawLine(rect.Right, rect.Top, rect.Left, rect.Bottom, paint);
	}
}
