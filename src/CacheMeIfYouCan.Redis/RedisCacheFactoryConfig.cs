using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Redis
{
    public class RedisCacheFactoryConfig
    {
        public IList<string> Endpoints = new List<string>();
        public int Database;
        public Func<FunctionInfo, string> KeySpacePrefixFunc;
        public string KeySpacePrefix { set => KeySpacePrefixFunc = f => value; }
    }
}
