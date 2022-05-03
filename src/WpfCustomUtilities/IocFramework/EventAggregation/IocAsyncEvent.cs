using System;
using System.Linq;
using System.Threading.Tasks;

using WpfCustomUtilities.Extensions.Collection;
using WpfCustomUtilities.SimpleCollections.Collection;

namespace WpfCustomUtilities.IocFramework.EventAggregation
{
    public class IocAsyncEvent<T> : IocEventBase
    {
        readonly SimpleDictionary<IocEventKey, Func<T, Task>> _functions;

        public IocAsyncEvent()
        {
            _functions = new SimpleDictionary<IocEventKey, Func<T, Task>>();
        }

        public string Subscribe(Func<T, Task> func, IocEventPriority priority = IocEventPriority.None)
        {
            var eventKey = new IocEventKey(priority);

            _functions.Add(eventKey, func);

            return eventKey.Token;
        }

        public void UnSubscribe(string token)
        {
            var eventKey = _functions.Keys.FirstOrDefault(key => key.Token == token);

            if (eventKey == null)
                throw new Exception("Trying to unsubscribe from missing event token IocAsyncEvent");

            _functions.Remove(eventKey);
        }

        public async Task Publish(T payload)
        {
            // Copying the collection because the functions may have subscriptions in them that modify the _functions
            // collection
            var functions = _functions.OrderBy(x => x.Key.Priority)
                                      .Select(x => x.Value)
                                      .Actualize();

            foreach (var function in functions)
                await function(payload);
        }
    }
}
