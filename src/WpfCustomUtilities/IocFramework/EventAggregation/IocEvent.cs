using System;
using System.Linq;

using WpfCustomUtilities.Extensions.Collection;
using WpfCustomUtilities.SimpleCollections.Collection;

namespace WpfCustomUtilities.IocFramework.EventAggregation
{
    public class IocEvent<T> : IocEventBase
    {
        readonly SimpleDictionary<IocEventKey, Action<T>> _actions;

        public IocEvent()
        {
            _actions = new SimpleDictionary<IocEventKey, Action<T>>();
        }

        public string Subscribe(Action<T> action, IocEventPriority priority = IocEventPriority.None)
        {
            var eventKey = new IocEventKey(priority);

            _actions.Add(eventKey, action);

            return eventKey.Token;
        }

        public void UnSubscribe(string token)
        {
            var eventKey = _actions.Keys.FirstOrDefault(key => key.Token == token);

            if (eventKey == null)
                throw new Exception("Trying to unsubscribe from missing event token RogueEvent<T>");

            _actions.Remove(eventKey);
        }

        public void Publish(T payload)
        {
            // Copying the collection because the actions may have subscriptions in them that modify the _actions
            // collection
            var actions = _actions.OrderBy(x => x.Key.Priority)
                                  .Select(x => x.Value)
                                  .Actualize();

            foreach (var action in actions)
                action.Invoke(payload);
        }
    }
    public class IocEvent : IocEventBase
    {
        readonly SimpleDictionary<IocEventKey, Action> _actions;

        public IocEvent()
        {
            _actions = new SimpleDictionary<IocEventKey, Action>();
        }

        public string Subscribe(Action action, IocEventPriority priority = IocEventPriority.None)
        {
            var eventKey = new IocEventKey(priority);

            _actions.Add(eventKey, action);

            return eventKey.Token;
        }

        public void UnSubscribe(string token)
        {
            var eventKey = _actions.Keys.FirstOrDefault(key => key.Token == token);

            if (eventKey == null)
                throw new Exception("Trying to unsubscribe from missing event token RogueEvent");

            _actions.Remove(eventKey);
        }

        public void Publish()
        {
            // Copying the collection because the actions may have subscriptions in them that modify the _actions
            // collection
            var actions = _actions.OrderBy(x => x.Key.Priority)
                                  .Select(x => x.Value)
                                  .Actualize();

            foreach (var action in actions)
                action.Invoke();
        }
    }
}
