using System;
using System.Collections.Generic;
using System.Text;

namespace NexGoCSharp;

public class StreamIn
{
    private readonly byte[] _data;
    private int _position;

    public StreamIn(byte[] data)
    {
        _data = data ?? Array.Empty<byte>();
        _position = 0;
    }

    public int Position => _position;
    public int Remaining => _data.Length - _position;

    public byte ReadByteNext()
    {
        if (_position >= _data.Length)
            throw new InvalidOperationException("StreamIn: cannot read beyond the end of the buffer.");
        return _data[_position++];
    }

    public bool ReadBool() => ReadByteNext() == 1;
    public byte ReadUInt8() => ReadByteNext();

    public ushort ReadUInt16LE()
    {
        var b0 = ReadByteNext();
        var b1 = ReadByteNext();
        return (ushort)(b0 | (b1 << 8));
    }

    public uint ReadUInt32LE()
    {
        var b0 = ReadByteNext();
        var b1 = ReadByteNext();
        var b2 = ReadByteNext();
        var b3 = ReadByteNext();
        return (uint)(b0 | (b1 << 8) | (b2 << 16) | (b3 << 24));
    }

    public int ReadInt32LE() => unchecked((int)ReadUInt32LE());

    public ulong ReadUInt64LE()
    {
        ulong result = 0;
        for (var i = 0; i < 8; i++)
            result |= (ulong)ReadByteNext() << (i * 8);
        return result;
    }

    public string ReadString()
    {
        var length = ReadUInt16LE();
        if (_position + length > _data.Length)
            throw new InvalidOperationException("StreamIn: Nex string length is longer than remaining data size.");

        var bytes = new byte[length];
        Array.Copy(_data, _position, bytes, 0, length);
        _position += length;
        return Encoding.ASCII.GetString(bytes).TrimEnd('\0');
    }

    public byte[] ReadBuffer()
    {
        var length = (int)ReadUInt32LE();
        if (_position + length > _data.Length)
            throw new InvalidOperationException("StreamIn: Nex buffer length is longer than remaining data size.");

        var bytes = new byte[length];
        Array.Copy(_data, _position, bytes, 0, length);
        _position += length;
        return bytes;
    }

    public byte[] ReadQBuffer()
    {
        var length = ReadUInt16LE();
        if (_position + length > _data.Length)
            throw new InvalidOperationException("StreamIn: Nex qBuffer length is longer than remaining data size.");

        var bytes = new byte[length];
        Array.Copy(_data, _position, bytes, 0, length);
        _position += length;
        return bytes;
    }

    public List<byte> ReadListUInt8()
    {
        var length = (int)ReadUInt32LE();
        var list = new List<byte>(length);
        for (var i = 0; i < length; i++)
            list.Add(ReadUInt8());
        return list;
    }

    public List<ushort> ReadListUInt16LE()
    {
        var length = (int)ReadUInt32LE();
        var list = new List<ushort>(length);
        for (var i = 0; i < length; i++)
            list.Add(ReadUInt16LE());
        return list;
    }

    public List<uint> ReadListUInt32LE()
    {
        var length = (int)ReadUInt32LE();
        var list = new List<uint>(length);
        for (var i = 0; i < length; i++)
            list.Add(ReadUInt32LE());
        return list;
    }

    public List<int> ReadListInt32LE()
    {
        var length = (int)ReadUInt32LE();
        var list = new List<int>(length);
        for (var i = 0; i < length; i++)
            list.Add(ReadInt32LE());
        return list;
    }

    public List<string> ReadListString()
    {
        var length = (int)ReadUInt32LE();
        var list = new List<string>(length);
        for (var i = 0; i < length; i++)
            list.Add(ReadString());
        return list;
    }

    public byte[] ReadRemainingBytes()
    {
        var remaining = _data.Length - _position;
        var bytes = new byte[remaining];
        Array.Copy(_data, _position, bytes, 0, remaining);
        _position = _data.Length;
        return bytes;
    }
}

public class StreamOut
{
    private readonly List<byte> _buffer = new();

    public byte[] ToArray() => _buffer.ToArray();
    public int Length => _buffer.Count;

    public void WriteBool(bool value) => _buffer.Add(value ? (byte)1 : (byte)0);
    public void WriteUInt8(byte value) => _buffer.Add(value);

    public void WriteUInt16LE(ushort value)
    {
        _buffer.Add((byte)(value & 0xFF));
        _buffer.Add((byte)((value >> 8) & 0xFF));
    }

    public void WriteUInt32LE(uint value)
    {
        _buffer.Add((byte)(value & 0xFF));
        _buffer.Add((byte)((value >> 8) & 0xFF));
        _buffer.Add((byte)((value >> 16) & 0xFF));
        _buffer.Add((byte)((value >> 24) & 0xFF));
    }

    public void WriteInt32LE(int value) => WriteUInt32LE(unchecked((uint)value));

    public void WriteUInt64LE(ulong value)
    {
        for (var i = 0; i < 8; i++)
            _buffer.Add((byte)((value >> (i * 8)) & 0xFF));
    }

    public void WriteInt64LE(long value) => WriteUInt64LE(unchecked((ulong)value));

    public void WriteString(string value)
    {
        var bytes = Encoding.ASCII.GetBytes(value ?? string.Empty);
        var withNull = new List<byte>(bytes.Length + 1);
        withNull.AddRange(bytes);
        withNull.Add(0);
        WriteUInt16LE((ushort)withNull.Count);
        _buffer.AddRange(withNull);
    }

    public void WriteBuffer(byte[] data)
    {
        WriteUInt32LE((uint)(data?.Length ?? 0));
        if (data != null && data.Length > 0)
            _buffer.AddRange(data);
    }

    public void WriteQBuffer(byte[] data)
    {
        WriteUInt16LE((ushort)(data?.Length ?? 0));
        if (data != null && data.Length > 0)
            _buffer.AddRange(data);
    }

    public void WriteBytes(byte[] data)
    {
        if (data != null)
            _buffer.AddRange(data);
    }
}

public class Counter
{
    private uint _value;

    public Counter(uint start = 0)
    {
        _value = start;
    }

    public uint Increment() => _value++;
    public uint Value => _value;
    public void Set(uint value) => _value = value;
}
