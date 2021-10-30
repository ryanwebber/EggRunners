using System;

namespace Extensions
{
    public static class NullableExtensions
    {
        public static bool TryGetValue<T>(this Nullable<T> self, out T value) where T: struct
        {
            if (self.HasValue)
            {
                value = self.Value;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }
    }
}
