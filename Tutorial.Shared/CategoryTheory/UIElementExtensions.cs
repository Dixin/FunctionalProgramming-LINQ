#if NETFX
namespace Tutorial.CategoryTheory
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Windows;
    using System.Windows.Input;

    public static class UIElementExtensions
    {
        public static IObservable<EventPattern<MouseEventArgs>> MouseDrag
            (this UIElement element) =>
#pragma warning disable SA1312 // Variable names must begin with lower-case letter
                from _ in Observable.FromEventPattern<MouseEventArgs>(element, nameof(element.MouseDown))
#pragma warning restore SA1312 // Variable names must begin with lower-case letter
                from @event in Observable.FromEventPattern<MouseEventArgs>(element, nameof(element.MouseMove))
                    .TakeUntil(Observable.FromEventPattern<MouseEventArgs>(element, nameof(element.MouseUp)))
                select @event;
    }
}
#endif
