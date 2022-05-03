using System;

using WpfCustomUtilities.Extensions.Collection;
using WpfCustomUtilities.IocFramework.RegionManagement;
using WpfCustomUtilities.SimpleCollections.Collection;

namespace WpfCustomUtilities.IocFramework.EventAggregation
{
    /// <summary>
    /// Special event class used for Region loading to pass IRogueRegion instances to the
    /// IRogueRegionManager - which lies in the IModule implementations or I[something]Controller
    /// instances. (I like to centralize control of the region manager)
    /// </summary>
    public class IocRegionEvent : IocEventBase
    {
        readonly SimpleDictionary<string, Action<IocRegion>> _actions;

        public IocRegionEvent()
        {
            _actions = new SimpleDictionary<string, Action<IocRegion>>();
        }

        public string Subscribe(Action<IocRegion> action)
        {
            var token = Guid.NewGuid().ToString();

            _actions.Add(token, action);

            return token;
        }

        public void UnSubscribe(string token)
        {
            _actions.Remove(token);
        }

        public void Publish(IocRegion region)
        {
            var actions = _actions.Values.Copy();

            foreach (var action in actions)
                action.Invoke(region);
        }
    }

    /// <summary>
    /// Special event class used for Region loading to pass IRogueRegion instances to the
    /// IRogueRegionManager - which lies in the IModule implementations or I[something]Controller
    /// instances. (I like to centralize control of the region manager)
    /// </summary>
    public class IocRegionEvent<T> : IocEventBase
    {
        readonly SimpleDictionary<string, Action<IocRegion, T>> _actions;

        public IocRegionEvent()
        {
            _actions = new SimpleDictionary<string, Action<IocRegion, T>>();
        }

        public string Subscribe(Action<IocRegion, T> action)
        {
            var token = Guid.NewGuid().ToString();

            _actions.Add(token, action);

            return token;
        }

        public void Publish(IocRegion region, T payload)
        {
            var actions = _actions.Values.Copy();

            foreach (var action in actions)
                action.Invoke(region, payload);
        }

        public void UnSubscribe(string token)
        {
            _actions.Remove(token);
        }
    }
}
