using System;

namespace NexGoCSharp;

public class RmcRequest
{
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
