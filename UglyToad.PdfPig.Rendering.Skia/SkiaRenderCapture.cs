// Copyright 2024 BobLd
//
// Licensed under the Apache License, Version 2.0 (the "License").
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Threading;
using SkiaSharp;
using UglyToad.PdfPig.Core;

#nullable enable

namespace UglyToad.PdfPig.Rendering.Skia
{
    /// <summary>
    /// Provides global capture for Skia rendering commands produced by <see cref="SkiaStreamProcessor"/>.
    /// </summary>
    public static class SkiaRenderCapture
    {
        private sealed class Scope : IDisposable
        {
            private readonly ISkiaPageRenderListener? _previous;
            private bool _disposed;

            internal Scope(ISkiaPageRenderListener? previous)
            {
                _previous = previous;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _current.Value = _previous;
                _disposed = true;
            }
        }

        private static readonly AsyncLocal<ISkiaPageRenderListener?> _current = new AsyncLocal<ISkiaPageRenderListener?>();

        /// <summary>
        /// Begins capturing Skia rendering commands using the specified listener.
        /// </summary>
        /// <param name="listener">The listener that will receive rendering callbacks.</param>
        /// <returns>A disposable scope that must be disposed to restore the previous listener.</returns>
        public static IDisposable Begin(ISkiaPageRenderListener listener)
        {
            if (listener is null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            var scope = new Scope(_current.Value);
            _current.Value = listener;
            return scope;
        }

        internal static ISkiaPageRenderListener? GetCurrent()
        {
            return _current.Value;
        }
    }

    /// <summary>
    /// Receives callbacks for Skia rendering commands during page processing.
    /// </summary>
    public interface ISkiaPageRenderListener
    {
        /// <summary>
        /// Called before any rendering commands for the given page are emitted.
        /// </summary>
        /// <param name="pageInfo">Information about the page being rendered.</param>
        void BeginPage(SkiaRenderPageInfo pageInfo);

        /// <summary>
        /// Called for every path rendered on the page.
        /// </summary>
        /// <param name="path">The rendered path.</param>
        void OnPath(SkiaRenderPath path);

        /// <summary>
        /// Called for every text glyph rendered on the page.
        /// </summary>
        /// <param name="glyph">The rendered glyph information.</param>
        void OnGlyph(SkiaRenderGlyph glyph);

        /// <summary>
        /// Called for every raster image rendered on the page.
        /// </summary>
        /// <param name="image">The rendered image.</param>
        void OnImage(SkiaRenderImage image);

        /// <summary>
        /// Called after all rendering commands for the page have been emitted.
        /// </summary>
        void EndPage();
    }

    /// <summary>
    /// Describes the page that is being rendered.
    /// </summary>
    public readonly struct SkiaRenderPageInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkiaRenderPageInfo"/> struct.
        /// </summary>
        /// <param name="pageNumber">Page index starting from 1.</param>
        /// <param name="width">Page width in user units.</param>
        /// <param name="height">Page height in user units.</param>
        public SkiaRenderPageInfo(int pageNumber, int width, int height)
        {
            PageNumber = pageNumber;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Gets the one-based page index.
        /// </summary>
        public int PageNumber { get; }

        /// <summary>
        /// Gets the page width in user units.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the page height in user units.
        /// </summary>
        public int Height { get; }
    }

    /// <summary>
    /// Represents a rendered path and its styling information.
    /// </summary>
    public sealed class SkiaRenderPath : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkiaRenderPath"/> class.
        /// </summary>
        /// <param name="path">The rendered Skia path.</param>
        /// <param name="stroke">Stroke information, if any.</param>
        /// <param name="fill">Fill information, if any.</param>
        /// <param name="isText">Indicates whether the path originated from a text glyph.</param>
        public SkiaRenderPath(SKPath path, SkiaStrokeStyle? stroke, SkiaFillStyle? fill, bool isText)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Stroke = stroke;
            Fill = fill;
            IsText = isText;
        }

        /// <summary>
        /// Gets the captured Skia path instance.
        /// </summary>
        public SKPath Path { get; }

        /// <summary>
        /// Gets the stroke styling applied to the path, if any.
        /// </summary>
        public SkiaStrokeStyle? Stroke { get; }

        /// <summary>
        /// Gets the fill styling applied to the path, if any.
        /// </summary>
        public SkiaFillStyle? Fill { get; }

        /// <summary>
        /// Gets a value indicating whether the path originated from a text glyph.
        /// </summary>
        public bool IsText { get; }

        /// <summary>
        /// Releases the underlying Skia path instance.
        /// </summary>
        public void Dispose()
        {
            Path.Dispose();
        }
    }

    /// <summary>
    /// Describes the stroke styling for a rendered path.
    /// </summary>
    public sealed class SkiaStrokeStyle
    {
        /// <summary>
        /// Initializes a new instance describing stroke styling information.
        /// </summary>
        public SkiaStrokeStyle(SKColor color, float alpha, float width, SKStrokeJoin join, SKStrokeCap cap, float[]? dashArray, float dashPhase)
        {
            Color = color;
            Alpha = alpha;
            Width = width;
            Join = join;
            Cap = cap;
            DashArray = dashArray != null && dashArray.Length > 0 ? (float[])dashArray.Clone() : null;
            DashPhase = dashPhase;
        }

        /// <summary>
        /// Gets the stroke color.
        /// </summary>
        public SKColor Color { get; }

        /// <summary>
        /// Gets the alpha component used when stroking.
        /// </summary>
        public float Alpha { get; }

        /// <summary>
        /// Gets the stroke width in user units.
        /// </summary>
        public float Width { get; }

        /// <summary>
        /// Gets the stroke join style.
        /// </summary>
        public SKStrokeJoin Join { get; }

        /// <summary>
        /// Gets the stroke cap style.
        /// </summary>
        public SKStrokeCap Cap { get; }

        /// <summary>
        /// Gets the dash pattern applied to the stroke.
        /// </summary>
        public float[]? DashArray { get; }

        /// <summary>
        /// Gets the dash phase offset applied to the stroke.
        /// </summary>
        public float DashPhase { get; }
    }

    /// <summary>
    /// Represents a rendered text glyph and its styling information.
    /// </summary>
    public sealed class SkiaRenderGlyph
    {
        /// <summary>
        /// Initializes a new instance containing glyph information.
        /// </summary>
        public SkiaRenderGlyph(string unicode, SKMatrix transform, SkiaFillStyle? fill, SkiaStrokeStyle? stroke, float fontSize, string? fontName, SKRect bounds)
        {
            Unicode = unicode;
            Transform = transform;
            Fill = fill;
            Stroke = stroke;
            FontSize = fontSize;
            FontName = fontName;
            Bounds = bounds;
        }

        /// <summary>
        /// Gets the Unicode string represented by this glyph.
        /// </summary>
        public string Unicode { get; }

        /// <summary>
        /// Gets the transformation matrix applied to the glyph.
        /// </summary>
        public SKMatrix Transform { get; }

        /// <summary>
        /// Gets the fill style used when rendering the glyph, if any.
        /// </summary>
        public SkiaFillStyle? Fill { get; }

        /// <summary>
        /// Gets the stroke style used when rendering the glyph, if any.
        /// </summary>
        public SkiaStrokeStyle? Stroke { get; }

        /// <summary>
        /// Gets the glyph font size in user units.
        /// </summary>
        public float FontSize { get; }

        /// <summary>
        /// Gets the font name if available.
        /// </summary>
        public string? FontName { get; }

        /// <summary>
        /// Gets the axis-aligned bounds of the glyph in page coordinates before scaling.
        /// </summary>
        public SKRect Bounds { get; }
    }

    /// <summary>
    /// Describes the fill styling for a rendered path.
    /// </summary>
    public sealed class SkiaFillStyle
    {
        /// <summary>
        /// Initializes a new instance describing fill styling information.
        /// </summary>
        public SkiaFillStyle(SKColor color, float alpha)
        {
            Color = color;
            Alpha = alpha;
        }

        /// <summary>
        /// Gets the fill color.
        /// </summary>
        public SKColor Color { get; }

        /// <summary>
        /// Gets the alpha component used when filling.
        /// </summary>
        public float Alpha { get; }
    }

    /// <summary>
    /// Represents a raster image rendered onto a page.
    /// </summary>
    public sealed class SkiaRenderImage
    {
        /// <summary>
        /// Initializes a new instance containing a captured raster image.
        /// </summary>
        public SkiaRenderImage(byte[] data, SKRect destination, TransformationMatrix transform, int pixelWidth, int pixelHeight)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Destination = destination;
            Transform = transform;
            PixelWidth = pixelWidth;
            PixelHeight = pixelHeight;
        }

        /// <summary>
        /// Gets the encoded image bytes (PNG format).
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// Gets the destination rectangle in page coordinates.
        /// </summary>
        public SKRect Destination { get; }

        /// <summary>
        /// Gets the transformation matrix active when rendering the image.
        /// </summary>
        public TransformationMatrix Transform { get; }

        /// <summary>
        /// Gets the original image width in pixels.
        /// </summary>
        public int PixelWidth { get; }

        /// <summary>
        /// Gets the original image height in pixels.
        /// </summary>
        public int PixelHeight { get; }
    }
}
