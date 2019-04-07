﻿using System;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public class CachedObjectConfigurationManager<T, TUpdates>
    {
        protected readonly Func<Task<T>> InitialiseValueFunc;
        protected readonly Func<T, TUpdates, Task<T>> UpdateValueFunc;
        private readonly ICachedObjectUpdateScheduler<T, TUpdates> _updateScheduler;
        private string _name;
        private Action<CachedObjectUpdateResult<T, TUpdates>> _onUpdate;
        private Action<CachedObjectUpdateException> _onException;

        internal CachedObjectConfigurationManager(
            Func<Task<T>> initialiseValueFunc,
            Func<T, TUpdates, Task<T>> updateValueFunc = null,
            ICachedObjectUpdateScheduler<T, TUpdates> updateScheduler = null)
        {
            InitialiseValueFunc = initialiseValueFunc;
            UpdateValueFunc = updateValueFunc;
            _updateScheduler = updateScheduler;

            if (DefaultSettings.CachedObject.OnUpdateAction != null)
                OnUpdate(DefaultSettings.CachedObject.OnUpdateAction);
            
            if (DefaultSettings.CachedObject.OnExceptionAction != null)
                OnException(DefaultSettings.CachedObject.OnExceptionAction);
        }
        
        public CachedObjectConfigurationManager<T, TUpdates> Named(string name)
        {
            _name = name;
            return this;
        }
        
        public CachedObjectConfigurationManager<T, TUpdates> OnUpdate(Action<CachedObjectUpdateResult<T, TUpdates>> onUpdate)
        {
            _onUpdate = onUpdate;
            return this;
        }

        public CachedObjectConfigurationManager<T, TUpdates> OnException(Action<CachedObjectUpdateException> onException)
        {
            _onException = onException;
            return this;
        }

        public ICachedObject<T> Build()
        {
            var name = _name ?? $"{nameof(CachedObject<T, TUpdates>)}_{typeof(T).Name}";
            
            var cachedObject = new CachedObject<T, TUpdates>(
                InitialiseValueFunc,
                UpdateValueFunc ?? ((_, __) => InitialiseValueFunc()),
                _updateScheduler,
                name,
                _onUpdate,
                _onException);

            CachedObjectInitializer.Add(cachedObject);

            return cachedObject;
        }
    }
}