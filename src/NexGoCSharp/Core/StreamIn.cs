using System;
using System.Collections.Generic;
using System.Linq;

namespace NexGoCSharp;

public class StreamIn
{
    private readonly byte[] _buffer;
    private int _position;

    public StreamIn(byte[] data)
    {
        _buffer = data ?? Array.Empty<byte>();
        _position = 0;
    }

    public int Length => _buffer.Length;
    public int Position => _position;
    public bool HasRemaining => _position < _buffer.Length;

    public byte ReadByteNext()
    {
        if (_position >= _buffer.Length)
            throw new InvalidOperationException("StreamIn: attempt to read past the end of the buffer.");

        return _buffer[_position++];
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
        {
            result |= (ulong)ReadByteNext() << (i * 8);
        }
        return result;
    }

    public string ReadString()
    {
        var length = ReadUInt16LE();
        if (_position + length > _buffer.Length)
            throw new InvalidOperationException("StreamIn: Nex string length is longer than the remaining data size.");

        var data = new byte[length];
        Array.Copy(_buffer, _position, data, 0, length);
        _position += length;

        var str = System.Text.Encoding.ASCII.GetString(data);
        return str.TrimEnd('\0');
    }

    public byte[] ReadBuffer()
    {
        var length = (int)ReadUInt32LE();
        if (_position + length > _buffer.Length)
            throw new InvalidOperationException("StreamIn: Nex buffer length is longer than the remaining data size.");

        var data = new byte[length];
        Array.Copy(_buffer, _position, data, 0, length);
        _position += length;
        return data;
    }

    public byte[] ReadQBuffer()
    {
        var length = ReadUInt16LE();
        if (_position + length > _buffer.Length)
            throw new InvalidOperationException("StreamIn: Nex qBuffer length is longer than the remaining data size.");

        var data = new byte[length];
        Array.Copy(_buffer, _position, data, 0, length);
        _position += length;
        return data;
    }

    public DateTime ReadDateTime() => DateTime.UnixEpoch.AddSeconds(ReadUInt64LE());

    public List<byte> ReadListUInt8()
    {
        var length = (int)ReadUInt32LE();
        var list = new List<byte>(length);
        for (var i = 0; i < length; i++)
        {
            list.Add(ReadUInt8());
        }
        return list;
    }

    public List<ushort> ReadListUInt16LE()
    {
        var length = (int)ReadUInt32LE();
        var list = new List<ushort>(length);
        for (var i = 0; i < length; i++)
        {
            list.Add(ReadUInt16LE());
        }
        return list;
    }

    public List<uint> ReadListUInt32LE()
    {
        var length = (int)ReadUInt32LE();
        var list = new List<uint>(length);
        for (var i = 0; i < length; i++)
        {
            list.Add(ReadUInt32LE());
        }
        return list;
    }

    public List<int> ReadListInt32LE()
    {
        var length = (int)ReadUInt32LE();
        var list = new List<int>(length);
        for (var i = 0; i < length; i++)
        {
            list.Add(ReadInt32LE());
        }
        return list;
    }

    public List<string> ReadListString()
    {
        var length = (int)ReadUInt32LE();
        var list = new List<string>(length);
        for (var i = 0; i < length; i++)
        {
            list.Add(ReadString());
        }
        return list;
    }

    public byte[] ReadRemainingBytes()
    {
        var remaining = _buffer.Length - _position;
        var bytes = new byte[remaining];
        Array.Copy(_buffer, _position, bytes, 0, remaining);
        _position = _buffer.Length;
        return bytes;
    }
}

public class StreamOut
{
    private readonly List<byte> _buffer;

    public StreamOut()
    {
        _buffer = new List<byte>();
    }

    public byte[] ToArray() => _buffer.ToArray();
    public int Length => _buffer.Count;

    public void WriteBool(bool value)
    {
        _buffer.Add(value ? (byte)1 : (byte)0);
    }

    public void WriteUInt8(byte value)
    {
        _buffer.Add(value);
    }

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
        {
            _buffer.Add((byte)((value >> (i * 8)) & 0xFF));
        }
    }

    public void WriteInt64LE(long value) => WriteUInt64LE(unchecked((ulong)value));

    public void WriteString(string value)
    {
        var bytes = System.Text.Encoding.ASCII.GetBytes(value ?? string.Empty);
        var withNull = bytes.Concat(new byte[] { 0 }).ToArray();
        WriteUInt16LE((ushort)withNull.Length);
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

    public uint Increment()
    {
        return _value++;
    }

    public uint Value => _value;
    public void Set(uint value) => _value = value;
}

public class Packet
{
    private NexClient? _sender;
    private byte[] _data = Array.Empty<byte>();
    private byte _version;
    private byte _source;
    private byte _destination;
    private ushort _packetType;
    private ushort _flags;
    private byte _sessionId;
    private byte[] _signature = Array.Empty<byte>();
    private ushort _sequenceId;
    private byte[] _connectionSignature = Array.Empty<byte>();
    private byte _fragmentId;
    private byte[] _payload = Array.Empty<byte>();
    private RmcRequest _rmcRequest = new();

    public Packet(NexClient? client = null, byte[]? data = null)
    {
        _sender = client;
        _data = data ?? Array.Empty<byte>();
        _payload = Array.Empty<byte>();
    }

    public byte[] Data => _data;
    public NexClient? Sender => _sender;
    public byte Version { get => _version; set => _version = value; }
    public byte Source { get => _source; set => _source = value; }
    public byte Destination { get => _destination; set => _destination = value; }
    public ushort Type { get => _packetType; set => _packetType = value; }
    public ushort Flags { get => _flags; set => _flags = value; }
    public bool HasFlag(ushort flag) => (_flags & flag) != 0;
    public void AddFlag(ushort flag) => _flags |= flag;
    public void ClearFlag(ushort flag) => _flags &= (ushort)~flag;
    public byte SessionId { get => _sessionId; set => _sessionId = value; }
    public byte[] Signature { get => _signature; set => _signature = value ?? Array.Empty<byte>(); }
    public ushort SequenceId { get => _sequenceId; set => _sequenceId = value; }
    public byte[] ConnectionSignature { get => _connectionSignature; set => _connectionSignature = value ?? Array.Empty<byte>(); }
    public byte FragmentId { get => _fragmentId; set => _fragmentId = value; }
    public byte[] Payload { get => _payload; set => _payload = value ?? Array.Empty<byte>(); }
    public RmcRequest RmcRequest => _rmcRequest;

    public static Packet NewPacket(NexClient? client, byte[]? data) => new(client, data);
}

public class NexClient
{
    private readonly object _sync = new();
    private byte[] _signatureKey = Array.Empty<byte>();
    private byte[] _sessionKey = Array.Empty<byte>();
    private uint _pid;
    private string _localStationUrl = string.Empty;
    private uint _connectionId;
    private bool _connected;
    private Counter _sequenceIdIn = new();
    private Counter _sequenceIdOut = new();

    public byte[] SignatureKey { get => _signatureKey; set => _signatureKey = value ?? Array.Empty<byte>(); }
    public byte[] SessionKey { get => _sessionKey; set => _sessionKey = value ?? Array.Empty<byte>(); }
    public uint Pid { get => _pid; set => _pid = value; }
    public string LocalStationUrl { get => _localStationUrl; set => _localStationUrl = value ?? string.Empty; }
    public uint ConnectionId { get => _connectionId; set => _connectionId = value; }
    public bool Connected { get => _connected; set => _connected = value; }
    public Counter SequenceIdIn => _sequenceIdIn;
    public Counter SequenceIdOut => _sequenceIdOut;

    public void Reset()
    {
        lock (_sync)
        {
            _sequenceIdIn = new Counter(0);
            _sequenceIdOut = new Counter(0);
            _connected = false;
        }
    }
}

public class NexServer
{
    private readonly List<NexClient> _clients = new();

    public IReadOnlyList<NexClient> Clients => _clients;

    public void AddClient(NexClient client)
    {
        if (client != null)
            _clients.Add(client);
    }

    public void RemoveClient(NexClient client)
    {
        _clients.Remove(client);
    }
}

public class RmcRequest
{
    private const int ErrorMask = 0x80000000;

    private byte _protocolId;
    private ushort _customId;
    private uint _callId;
    private uint _methodId;
    private byte[] _parameters = Array.Empty<byte>();

    public byte ProtocolId { get => _protocolId; set => _protocolId = value; }
    public ushort CustomId { get => _customId; set => _customId = value; }
    public uint CallId { get => _callId; set => _callId = value; }
    public uint MethodId { get => _methodId; set => _methodId = value; }
    public byte[] Parameters { get => _parameters; set => _parameters = value ?? Array.Empty<byte>(); }

    public static RmcRequest NewRmcRequest() => new();

    public void FromBytes(byte[] data)
    {
        if (data == null || data.Length < 13)
            throw new InvalidOperationException("[RMC] Data size less than minimum");

        var stream = new StreamIn(data);
        var size = stream.ReadUInt32LE();
        if (size != data.Length - 4)
            throw new InvalidOperationException("[RMC] Data size does not match");

        var protocolId = (byte)(stream.ReadUInt8() ^ 0x80);
        if (protocolId == 0x7F)
            _customId = stream.ReadUInt16LE();

        _callId = stream.ReadUInt32LE();
        _methodId = stream.ReadUInt32LE();
        _parameters = data[13..];
        _protocolId = protocolId;
    }

    public byte[] ToBytes()
    {
        var body = new StreamOut();
        body.WriteUInt8((byte)(_protocolId | 0x80));
        if (_protocolId == 0x7F)
            body.WriteUInt16LE(_customId);

        body.WriteUInt32LE(_callId);
        body.WriteUInt32LE(_methodId);

        if (_parameters != null && _parameters.Length > 0)
            body.WriteBytes(_parameters);

        var data = new StreamOut();
        data.WriteUInt32LE((uint)body.Length);
        data.WriteBytes(body.ToArray());
        return data.ToArray();
    }
}

public class RmcResponse
{
    private byte _protocolId;
    private ushort _customId;
    private byte _success;
    private uint _callId;
    private uint _methodId;
    private byte[] _data = Array.Empty<byte>();
    private uint _errorCode;

    public byte ProtocolId { get => _protocolId; set => _protocolId = value; }
    public ushort CustomId { get => _customId; set => _customId = value; }
    public byte Success { get => _success; set => _success = value; }
    public uint CallId { get => _callId; set => _callId = value; }
    public uint MethodId { get => _methodId; set => _methodId = value; }
    public byte[] Data { get => _data; set => _data = value ?? Array.Empty<byte>(); }
    public uint ErrorCode { get => _errorCode; set => _errorCode = value; }

    public void SetSuccess(uint methodId, byte[] data)
    {
        _success = 1;
        _methodId = methodId;
        _data = data ?? Array.Empty<byte>();
    }

    public void SetError(uint errorCode)
    {
        var code = errorCode;
        if ((code & 0x80000000) == 0)
            code |= 0x80000000;

        _success = 0;
        _errorCode = code;
    }

    public byte[] ToBytes()
    {
        var body = new StreamOut();
        body.WriteUInt8(_protocolId);
        if (_protocolId == 0x7F)
            body.WriteUInt16LE(_customId);

        body.WriteUInt8(_success);

        if (_success == 1)
        {
            body.WriteUInt32LE(_callId);
            body.WriteUInt32LE(_methodId | 0x8000);
            if (_data != null && _data.Length > 0)
                body.WriteBytes(_data);
        }
        else
        {
            body.WriteUInt32LE(_errorCode);
            body.WriteUInt32LE(_callId);
        }

        var data = new StreamOut();
        data.WriteUInt32LE((uint)body.Length);
        data.WriteBytes(body.ToArray());
        return data.ToArray();
    }

    public static RmcResponse NewRmcResponse(byte protocolId, uint callId)
    {
        return new RmcResponse
        {
            ProtocolId = protocolId,
            CallId = callId
        };
    }
}

public static class NexHelpers
{
    public static uint Sum(byte[] data)
    {
        uint sum = 0;
        foreach (var b in data)
            sum += b;
        return sum;
    }
}
