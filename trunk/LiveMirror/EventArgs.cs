using System;
using System.Collections.Generic;
using System.Text;

namespace LiveMirror
{
    class EventArgs<TData> : EventArgs
    {
        public TData Data { private set; get; }

        public EventArgs(TData data)
        {
            this.Data = data;
        }

        public static implicit operator EventArgs<TData>(TData data)
        {
            return new EventArgs<TData>(data);
        }
    }
}
