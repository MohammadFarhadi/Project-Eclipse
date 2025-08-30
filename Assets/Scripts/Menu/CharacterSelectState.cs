using System;
using Unity.Netcode;

public struct CharacterSelectState : INetworkSerializable, IEquatable<CharacterSelectState>
{
    public ulong clientID;
    public int characterID;
    public bool isConfirmed;

    public CharacterSelectState(ulong clientID, int characterID = -1)
    {
        this.clientID = clientID;
        this.characterID = characterID;
        isConfirmed = false;

    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientID);
        serializer.SerializeValue(ref characterID);
        serializer.SerializeValue(ref isConfirmed);

    }

    public bool Equals(CharacterSelectState other)
    {
        return clientID == other.clientID && characterID == other.characterID && isConfirmed == other.isConfirmed;
    }
}   