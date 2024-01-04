using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Dithering;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace Refresh.GameServer.Importing.Mip;

public class MipEncoder : ImageEncoder
{
    protected override void Encode<TPixel>(Image<TPixel> origImage, Stream stream, CancellationToken cancellationToken)
    {
        Image<Rgba32> image = origImage.CloneAs<Rgba32>();
        //Quantize the image into 256 colors
        image.Mutate(ctx => ctx.Quantize(new WuQuantizer(new QuantizerOptions
        {
            Dither = ErrorDither.Atkinson,
            MaxColors = 256,
        })));
        
        Dictionary<Rgba32, byte> outputPalette = new(256);
        {
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Rgba32 col = image[x, y];
                    //If the color is not in the palette
                    if (!outputPalette.TryGetValue(col, out _))
                    {
                        //Add the color to the palette
                        outputPalette[col] = (byte)outputPalette.Count;
                    }
                }
            }
        }

        if (outputPalette.Count > 256) throw new InvalidOperationException("Too many colors in image!");

        bool alpha = outputPalette.Any(col => col.Key.A != 255);
        
        MipHeader header = new()
        {
            Width = (uint)image.Width,
            Height = (uint)image.Height,
            Bpp = 8,
            NumBlocks = 2,
            TexMode = 1,
            Alpha = alpha,
            ColorLookupTable = new Rgba32[256],
        };
        
        //Fill in the header's CLUT
        foreach ((Rgba32 key, byte value) in outputPalette) header.ColorLookupTable[value] = key;

        //Write the header
        header.Write(stream);
        
        BinaryWriter writer = new(stream);
        WriteImageData(image, writer, outputPalette);
    }

    private static void WriteImageData(Image<Rgba32> image, BinaryWriter writer, IReadOnlyDictionary<Rgba32, byte> palette)
    {
        //NOTE: this code assumes the image is 4bpp!!!
        int blockWidth = 16;
        int blockHeight = 8;

        int x = 0;
        int y = 0;
        int xStart = 0;
        int xTarget = blockWidth;
        int yStart = 0;
        int yTarget = blockHeight;
            
        int bytesToWrite = image.Width * image.Height;
        for (int i = 0; i < bytesToWrite; i++)
        {
            #region hack to get swizzled coordinates

            if (x == xTarget && y == yTarget - 1)
            {
                xStart += blockWidth;
                xTarget += blockWidth;

                if (xStart == image.Width)
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
                
            //Write the byte at the location in the image
            writer.Write(palette[image[x, image.Height - y - 1]]);
                
            x += 1;
        } 
    }
}