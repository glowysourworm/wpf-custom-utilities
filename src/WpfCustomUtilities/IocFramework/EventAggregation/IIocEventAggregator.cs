
namespace WpfCustomUtilities.IocFramework.EventAggregation
{
    public interface IIocEventAggregator
    {
        TEventType GetEvent<TEventType>() where TEventType : IocEventBase;
    }
}
