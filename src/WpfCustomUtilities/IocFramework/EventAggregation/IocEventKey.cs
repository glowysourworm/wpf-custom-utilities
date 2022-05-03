using System;

namespace WpfCustomUtilities.IocFramework.EventAggregation
{
    /// <summary>
    /// Event priority for executing rogue event subscriptions
    /// </summary>
    public enum IocEventPriority : uint
    {
        /// <summary>
        /// Executed last (and) in no guaranteed order
        /// </summary>
        None = uint.MaxValue,

        Critical = 0,
        High = 1,
        Medium = 2,
        Low = 3
    }

    /// <summary>
    /// Event key used for sorting rogue events by subscription priority
    /// </summary>
    public class IocEventKey
    {
        /// <summary>
        /// Unique GUID for the event instance (created in constructor)
        /// </summary>
        public string Token { get; private set; }

        /// <summary>
        /// Order (low number = highest priority) of the event handling during publishing
        /// </summary>
        public IocEventPriority Priority { get; private set; }

        // Max order is used to set the default priority for event subscriptions
        public IocEventKey(IocEventPriority priority = IocEventPriority.None)
        {
            this.Token = Guid.NewGuid().ToString();
            this.Priority = priority;
        }
    }
}
