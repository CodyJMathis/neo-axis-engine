#if !NO_LITE_DB
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static Internal.LiteDB.Constants;

namespace Internal.LiteDB.Engine
{
    /// <summary>
    /// Implement similar as ArrayPool for byte array
    /// </summary>
    internal class BufferPool
    {
        private static object _lock;
        private static ArrayPool<byte> _bytePool;

        static BufferPool()
        {
            _lock = new object();
            _bytePool = new ArrayPool<byte>();
        }
        
        public static byte[] Rent(int count)
        {
            lock (_lock)
            {
                return _bytePool.Rent(count);
            }
        }

        public static void Return(byte[] buffer)
        {
            lock (_lock)
            {
                _bytePool.Return(buffer);
            }
        }
    }
}
#endif