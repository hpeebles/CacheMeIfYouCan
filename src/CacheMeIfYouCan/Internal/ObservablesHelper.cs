using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace CacheMeIfYouCan.Configuration
{
    internal static class ObservablesHelper
    {
        public static TConfig SetupObservable<T, TConfig>(
            Action<IObservable<T>> action,
            Func<Action<T>, ActionOrdering, TConfig> configFunc,
            ActionOrdering ordering)
        {
            var subject = new Subject<T>();

            action(subject.AsObservable());

            return configFunc(subject.OnNext, ordering);
        }
    }
}