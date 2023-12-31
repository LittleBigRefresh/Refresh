using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Metadata;

namespace Refresh.GameServer.Importing.Mip;

public class MipDecoder : ImageDecoder
{
    protected override Image<TPixel> Decode<TPixel>(DecoderOptions options, Stream stream, CancellationToken cancellationToken)
    {
        MipHeader header = MipHeader.Read(stream);

        Image<TPixel> image = new(options.Configuration, (int)header.Width, (int)header.Height);
        Buffer2D<TPixel> pixels = image.Frames.RootFrame.PixelBuffer;
        
        this.ProcessPixels(header, stream, pixels);

        return image;
    }

    private void ProcessPixels<TPixel>(MipHeader header, Stream stream, Buffer2D<TPixel> pixels) where TPixel : unmanaged, IPixel<TPixel>
    {
        stream.Seek(header.DataOffset, SeekOrigin.Begin);

        BinaryReader reader = new(stream);

        const int blockWidth = 16;
        const int blockHeight = 8;

        int x = 0;
        int y = 0;
        int xStart = 0;
        int xTarget = blockWidth;
        int yStart = 0;
        int yTarget = blockHeight;

        int bytesToRead = (int)(header.Width * header.Height / (8 / header.Bpp));
        
        for (int i = 0; i < bytesToRead; i++)
        {
            #region hack to get swizzled coordinates

            if (x == xTarget && y == yTarget - 1)
            {
                xStart += blockWidth;
                xTarget += blockWidth;

                if (xStart == header.Width)
                {
                    xStart = 0;
                    xTarget = blockWidth;
                    yStart += blockHeight;
                    yTarget += blockHeight;
                }

                x = xStart;
                y = yStart;
            }
            else
            {
                if (x == xTarget)
                {
                    y += 1;
                    x = xStart;
                }

                if (y == yTarget)
                {
                    xStart += blockWidth;
                    xTarget += blockWidth;
                    x = xStart;
                    y = yStart;
                }
            }

            #endregion

            switch (header.Bpp)
            {
                case 8: {
                    TPixel pixel = new();
                    pixel.FromRgba32(header.Clut[reader.ReadByte()]);
                    pixels[x, (int)(header.Height - y - 1)] = pixel;

                    x++;
                    break;
                }
                case 4: {
                    TPixel pixel = new();

                    byte data = reader.ReadByte();

                    pixel.FromRgba32(header.Clut[data & 0x0f]);
                    pixels[x, (int)(header.Height - y - 1)] = pixel;

                    pixel.FromRgba32(header.Clut[data >> 4]);
                    pixels[x + 1, (int)(header.Height - y - 1)] = pixel;

                    x += 2;
                    break;
                }
                default:
                    throw new Exception("Unknown BPP");
            }

        }
    }

    protected override Image Decode(DecoderOptions options, Stream stream, CancellationToken cancellationToken)
    {
        return this.Decode<Rgba32>(options, stream, cancellationToken);
    }
    
    protected override ImageInfo Identify(DecoderOptions options, Stream stream, CancellationToken cancellationToken)
    {
        MipHeader header = MipHeader.Read(stream);

        return new ImageInfo(
            new PixelTypeInfo(32), 
            new Size((int)header.Width, (int)header.Height), 
            new ImageMetadata
            {
                HorizontalResolution = header.Width,
                VerticalResolution = header.Height,
                ResolutionUnits = PixelResolutionUnit.AspectRatio,
            }
        );
    }
}