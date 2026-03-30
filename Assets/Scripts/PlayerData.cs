using System;
using Unity.Netcode;

namespace DefaultNamespace
{
    [Serializable]
    public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
    {
        public ulong playerID;
        public int score;
        public void AddScore(int s) => score += s;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref playerID);
            serializer.SerializeValue(ref score);
        }

        public bool Equals(PlayerData other)
        {
            return playerID == other.playerID && score == other.score;
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(playerID, score);
        }
    }
}