using System;
using System.Collections.Generic;
using System.Text;

namespace LiveMirror
{
    static class Extensions
    {
        public static void Raise(this EventHandler eventHandler, object sender, EventArgs e)
        {
            if (eventHandler != null)
                eventHandler.Invoke(sender, e);
        }
        public static void Raise<TEventArgs>(this EventHandler<TEventArgs> eventHandler, object sender, TEventArgs e) where TEventArgs : EventArgs
        {
            if (eventHandler != null)
                eventHandler.Invoke(sender, e);
        }
    }
}
