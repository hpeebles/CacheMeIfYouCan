using System;
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
        private Action<CachedObjectSuccessfulUpdateResult<T, TUpdates>> _onValueUpdated;
        private Action<CachedObjectUpdateException> _onException;

        internal CachedObjectConfigurationManager(
            Func<Task<T>> initialiseValueFunc,
            Func<T, TUpdates, Task<T>> updateValueFunc = null,
            ICachedObjectUpdateScheduler<T, TUpdates> updateScheduler = null)
        {
            InitialiseValueFunc = initialiseValueFunc;
            UpdateValueFunc = updateValueFunc;
            _updateScheduler = updateScheduler;

            if (DefaultSettings.CachedObject.OnValueUpdatedAction != null)
                OnValueUpdated(DefaultSettings.CachedObject.OnValueUpdatedAction);
            
            if (DefaultSettings.CachedObject.OnExceptionAction != null)
                OnException(DefaultSettings.CachedObject.OnExceptionAction);
        }
        
        public CachedObjectConfigurationManager<T, TUpdates> Named(string name)
        {
            _name = name;
            return this;
        }
        
        public CachedObjectConfigurationManager<T, TUpdates> OnValueUpdated(
            Action<CachedObjectSuccessfulUpdateResult<T, TUpdates>> onValueUpdated,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onValueUpdated = ActionsHelper.Combine(_onValueUpdated, onValueUpdated, behaviour);
            return this;
        }

        public CachedObjectConfigurationManager<T, TUpdates> OnException(
            Action<CachedObjectUpdateException> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onException = ActionsHelper.Combine(_onException, onException, behaviour);
            return this;
        }

        public ICachedObject<T, TUpdates> Build()
        {
            var name = _name ?? $"{nameof(CachedObject<T, TUpdates>)}_{TypeNameHelper.GetNameIncludingInnerGenericTypeNames(typeof(T))}";
            
            var cachedObject = new CachedObject<T, TUpdates>(
                InitialiseValueFunc,
                UpdateValueFunc ?? ((_, __) => InitialiseValueFunc()),
                _updateScheduler,
                name,
                _onValueUpdated,
                _onException);

            CachedObjectInitializer.Add(cachedObject);

            return cachedObject;
        }
    }
}