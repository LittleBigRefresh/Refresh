using System.Buffers.Binary;
using System.Diagnostics;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace Refresh.GameServer.Importing;

/// <summary>
/// Streams the data contained within a TEX file
/// </summary>
public class TexStream : Stream
{
    private readonly Stream _stream;

    private static readonly int TexMagic = BinaryPrimitives.ReadInt32BigEndian("TEX "u8);
    
    private readonly ushort[] _compressedChunkSizes;
    private readonly ushort[] _decompressedChunkSizes;

    private ushort _currentChunk;
    private readonly byte[] _scratchBufferCompressed;
    private readonly byte[] _scratchBufferDecompressed;
    /// <summary>
    /// The amount of data left in the current decompressed chunk
    /// </summary>
    private int _chunkLeft;
    /// <summary>
    /// The amount of data read from the current decompressed chunk
    /// </summary>
    private int _chunkRead;
    private bool _readNextChunk;

    private readonly Inflater _inflater;
    
    public TexStream(Stream stream) {
        this._stream = stream;
        
        BEBinaryReader reader = new(stream);
        
        //Read the file magic
        int magic = reader.ReadInt32();
        if (magic != TexMagic)
        {
            throw new FormatException("Input stream is not in TEX format!");
        }

        //Unknown flag, seems to always be 0x0001
        short unknownFlag = reader.ReadInt16();

        ushort chunkCount = reader.ReadUInt16();

        this._compressedChunkSizes = new ushort[chunkCount];
        this._decompressedChunkSizes = new ushort[chunkCount];

        ushort largestCompressedChunk = 0;
        ushort largestDecompressedChunk = 0;
        for (int i = 0; i < chunkCount; i++)
        {
            this._compressedChunkSizes[i] = reader.ReadUInt16();
            this._decompressedChunkSizes[i] = reader.ReadUInt16();
            
            if (this._decompressedChunkSizes[i] > largestDecompressedChunk)
                largestDecompressedChunk = this._decompressedChunkSizes[i];
            if (this._compressedChunkSizes[i] > largestCompressedChunk)
                largestCompressedChunk = this._compressedChunkSizes[i];
            
            if(this._compressedChunkSizes[i] == 0) throw new FormatException("Zero sized compressed chunk found!");
            if(this._decompressedChunkSizes[i] == 0) throw new FormatException("Zero sized decompressed chunk found!");
        }

        this._scratchBufferDecompressed = new byte[largestDecompressedChunk];
        this._scratchBufferCompressed = new byte[largestCompressedChunk];
        this._currentChunk = 0;
        this._readNextChunk = true;

        this._inflater = new Inflater();

        this.Length = this._decompressedChunkSizes.Sum(chunk => chunk);
    }

    protected override void Dispose(bool disposing)
    {
        this._stream.Dispose();
        base.Dispose(disposing);
    }

    public override void Flush() => throw new InvalidOperationException();
    public override int Read(byte[] buffer, int offset, int count)
    {
        //Return 0 (EOF) if we are out of chunks
        if (this._currentChunk == this._compressedChunkSizes.Length) return 0;
        
        //If we need to read the next chunk
        if (this._readNextChunk)
        {
            //Read the compressed data into its scratch buffer
            this._stream.ReadAtLeast(this._scratchBufferCompressed.AsSpan()[..this._compressedChunkSizes[this._currentChunk]], this._compressedChunkSizes[this._currentChunk]);

            this._inflater.Reset();
        
            //Set the input stream to the slice of compressed scratch buffer
            this._inflater.SetInput(this._scratchBufferCompressed, 0, this._compressedChunkSizes[this._currentChunk]);
            //Inflate the first chunk into the decompressed scratch buffer
            this._inflater.Inflate(this._scratchBufferDecompressed, 0, this._decompressedChunkSizes[this._currentChunk]);

            this._readNextChunk = false;
            this._chunkLeft = this._decompressedChunkSizes[this._currentChunk];
            this._chunkRead = 0;
        }
        
        Debug.Assert(this._chunkLeft > 0);

        //Special case perfect or over-reads
        if (count >= this._chunkLeft)
        {
            this._scratchBufferDecompressed.AsSpan().Slice(this._chunkRead, this._chunkLeft).CopyTo(buffer.AsSpan().Slice(offset, count));

            this._readNextChunk = true;
            this._currentChunk += 1;
            return this._chunkLeft;
        }
        
        //Copy the data it wants into the output buffer
        this._scratchBufferDecompressed.AsSpan().Slice(this._chunkRead, count).CopyTo(buffer.AsSpan().Slice(offset, count));
        
        this._chunkRead += count;
        this._chunkLeft -= count;
        
        return count;
    }
    
    public override long Seek(long offset, SeekOrigin origin) => throw new InvalidOperationException();
    public override void SetLength(long value) => throw new InvalidOperationException();
    public override void Write(byte[] buffer, int offset, int count) => throw new InvalidOperationException();
    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length { get; }
    public override long Position
    {
        get => throw new InvalidOperationException();
        set => throw new InvalidOperationException();
    }
}