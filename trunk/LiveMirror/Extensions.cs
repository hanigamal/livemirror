#region License
/* 
 * LiveMirror - Directory Sync
 * Copyright (C) 2009  Sam Stevens
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 * 
 */
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace LiveMirror
{
    static class Extensions
    {
        /// <summary>
        /// Raises an event only if it isnt null (ie has been attached to)
        /// </summary>
        /// <param name="eventHandler"></param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void Raise(this EventHandler eventHandler, object sender, EventArgs e)
        {
            if (eventHandler != null)
                eventHandler(sender, e);
        }
        /// <summary>
        /// Raises an event only if it isnt null (ie has been attached to)
        /// </summary>
        /// <param name="eventHandler"></param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void Raise<TEventArgs>(this EventHandler<TEventArgs> eventHandler, object sender, TEventArgs e) where TEventArgs : EventArgs
        {
            if (eventHandler != null)
                eventHandler(sender, e);
        }
    }
}
