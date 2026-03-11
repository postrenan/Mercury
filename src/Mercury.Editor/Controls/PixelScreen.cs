using System;
using System.Buffers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;

namespace Mercury.Editor.Controls;

/// <summary>
/// Simple control that shows a buffer as a bitmap to the screen using
/// Skia rendering. 
/// </summary>
public class PixelScreen : Control {
    
    public static readonly StyledProperty<bool> ContinuousUpdateProperty =
        AvaloniaProperty.Register<PixelScreen, bool>(nameof(ContinuousUpdate), defaultValue: false);
    /// <summary>
    /// Whether to continue updating the UI or only redraw the surface when <see cref="Redraw"/>
    /// is called or the bitmap is changed. 
    /// </summary>
    public bool ContinuousUpdate {
        get => GetValue(ContinuousUpdateProperty);
        set {
            SetValue(ContinuousUpdateProperty, value);
            if (value) {
                InvalidateVisual();
            }
        }
    }

    private SKBitmap? bitmap;
    private MemoryHandle? bufferHandle;
    
    /// <summary>
    /// Triggers a redraw of the surface.
    /// </summary>
    public void Redraw() {
        InvalidateVisual();
    }

    /// <summary>
    /// Sets the new buffer that will be drawn to the screen.
    /// </summary>
    /// <remarks>
    /// This function does not handle disposing of the <paramref name="buffer"/>. 
    /// </remarks>
    /// <param name="width">The width in pixels of the image</param>
    /// <param name="height">The height in pixels of the image</param>
    /// <param name="buffer">The buffer that will be shown. Must have at least capacity for the
    /// given image</param>
    public void SubmitBuffer(uint width, uint height, ReadOnlyMemory<byte> buffer) {
        bitmap?.Dispose();
        bitmap = null;
        bufferHandle?.Dispose();
        bufferHandle = null;
        if (buffer.IsEmpty || width <= 0 || height <= 0) {
            InvalidateVisual();
            return;
        }
        if (buffer.Length < width * height * 4) {
            Console.WriteLine("Buffer size is smaller than width * height * 4");
            return;
        }
        bitmap = new SKBitmap();
        SKImageInfo info = new((int)width, (int)height, SKColorType.Rgba8888, SKAlphaType.Opaque);
        bufferHandle = buffer.Pin();
        unsafe {
            bitmap.InstallPixels(info, (IntPtr)bufferHandle.Value.Pointer, info.RowBytes);
        }

        InvalidateVisual();
    }
    
    public override void Render(DrawingContext context) {
        context.Custom(new DrawBitmapOp(new Rect(0, 0, Bounds.Width, Bounds.Height), bitmap));
        if (ContinuousUpdate) {
            Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
        }
    }
    
    private class DrawBitmapOp(Rect bounds, SKBitmap? bitmap) : ICustomDrawOperation {
        public bool Equals(ICustomDrawOperation? other) => false;

        public void Dispose() {
            // nop
        }

        public bool HitTest(Point p) => Bounds.Contains(p);

        public void Render(ImmediateDrawingContext context) {
            bool result = context.TryGetFeature<ISkiaSharpApiLeaseFeature>(out ISkiaSharpApiLeaseFeature? feature);
            if (!result) {
                Console.WriteLine("Error getting feature lease");
                return;
            }

            using ISkiaSharpApiLease lease = feature!.Lease();
            SKCanvas canvas = lease.SkCanvas;

            if (bitmap is null) {
                // draw special missing buffer texture
                using SKPaint paint = new();
                paint.Color = SKColors.Magenta;
                canvas.DrawRect(0,0, (float)Bounds.Width*0.5f, (float)Bounds.Height*0.5f, paint);
                canvas.DrawRect((float)Bounds.Width*0.5f, (float)Bounds.Height*0.5f, 
                    (float)Bounds.Width*0.5f, (float)Bounds.Height*0.5f, paint);
                paint.Color = SKColors.Black;
                canvas.DrawRect((float)Bounds.Width*0.5f,0, (float)Bounds.Width*0.5f, (float)Bounds.Height*0.5f, paint);
                canvas.DrawRect(0,(float)Bounds.Height*0.5f, (float)Bounds.Width*0.5f, (float)Bounds.Height*0.5f, paint);
                return;
            }
            
            int width = bitmap.Width;
            int height = bitmap.Height;
            float pixelSize = (float)Math.Min(Bounds.Width/width, Bounds.Height/height);

            SKPoint boardStart = new(pixelSize * width < (float)Bounds.Width ? (float)Bounds.Width*0.5f - pixelSize*width*0.5f: 0f, 
                pixelSize * height < (float)Bounds.Height ? (float)Bounds.Height*0.5f-pixelSize*height*0.5f : 0f);
            SKRect rect = new(boardStart.X, boardStart.Y, boardStart.X+width*pixelSize, boardStart.Y+height*pixelSize);
            canvas.DrawBitmap(bitmap, rect);
        }

        public Rect Bounds { get; } = bounds;
    }
}