using System;

namespace Game.Core.Utility
{
    public static class RandomUtility
    {
        public static uint CreateSeed()
        {
            byte[] bytes = Guid.NewGuid().ToByteArray();
            uint seed = BitConverter.ToUInt32(bytes, 0) ^
                        BitConverter.ToUInt32(bytes, 4) ^
                        BitConverter.ToUInt32(bytes, 8) ^
                        BitConverter.ToUInt32(bytes, 12);

            return EnsureNonZero(seed);
        }

        public static uint CombineSeed(uint seed, string salt)
        {
            return CombineSeed(seed, GetStableHash(salt));
        }

        public static uint CombineSeed(uint seed, uint salt)
        {
            unchecked
            {
                uint hash = seed;

                hash ^= salt;
                hash *= 16777619u;
                hash ^= hash >> 16;

                return EnsureNonZero(hash);
            }
        }

        private static uint GetStableHash(string value)
        {
            unchecked
            {
                uint hash = 2166136261u;

                for (int i = 0; i < value.Length; i++)
                {
                    hash ^= value[i];
                    hash *= 16777619u;
                }

                return EnsureNonZero(hash);
            }
        }

        private static uint EnsureNonZero(uint value)
        {
            return value == 0 ? 1u : value;
        }
    }
}
