using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace CacheMeIfYouCan.Internal
{
    internal static class ObservablesHelper
    {
        public static TConfig SetupObservable<T, TConfig>(
            Action<IObservable<T>> action,
            Func<Action<T>, AdditionBehaviour, TConfig> configFunc,
            AdditionBehaviour behaviour)
        {
            var subject = new Subject<T>();

            action(subject.AsObservable());

            return configFunc(subject.OnNext, behaviour);
        }
    }
}