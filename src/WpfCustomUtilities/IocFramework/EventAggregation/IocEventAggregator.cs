using System;
using System.Linq;

using WpfCustomUtilities.IocFramework.Application.Attribute;
using WpfCustomUtilities.SimpleCollections.Collection;

namespace WpfCustomUtilities.IocFramework.EventAggregation
{
    [IocExport(typeof(IIocEventAggregator), InstancePolicy.ShareGlobal)]
    public class IocEventAggregator : IIocEventAggregator
    {
        readonly SimpleDictionary<Type, IocEventBase> _eventDict;

        public IocEventAggregator()
        {
            _eventDict = new SimpleDictionary<Type, IocEventBase>();
        }

        public TEventType GetEvent<TEventType>() where TEventType : IocEventBase
        {
            var type = typeof(TEventType);

            if (_eventDict.Keys.Any(x => x == typeof(TEventType)))
                return (TEventType)_eventDict[type];

            var newEvent = Construct<TEventType>();

            _eventDict[type] = newEvent;

            return newEvent;
        }

        private T Construct<T>()
        {
            var constructor = typeof(T).GetConstructor(new Type[] { });
            return (T)constructor.Invoke(new object[] { });
        }
    }
}
