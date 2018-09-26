﻿namespace CacheMeIfYouCan
{
    public interface ISerializer : IKeySerializer
    {
        T Deserialize<T>(string value);
    }

    public interface ISerializer<T>
    {
        string Serialize(T value);
        
        T Deserialize(string value);
    }
}