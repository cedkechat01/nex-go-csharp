using System;

namespace NexGoCSharp;

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

    public Packet(NexClient? sender = null, byte[]? data = null)
    {
        _sender = sender;
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

    public static Packet NewPacket(NexClient? sender, byte[]? data) => new(sender, data);
}
