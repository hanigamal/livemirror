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
using System.IO;

namespace LiveMirror
{
    class Mirror
    {
        FileSystemWatcher fileWatcher;
        MirrorUtils mirrorUtils;

        public event EventHandler<EventArgs<string>> LogMessage;

        public Mirror(string fromDir, string toDir)
        {
            if (!Directory.Exists(fromDir))
                throw new DirectoryNotFoundException("From directory does not exist");

            if (!Directory.Exists(toDir))
                throw new DirectoryNotFoundException("To directory does not exist");

            mirrorUtils = new MirrorUtils(fromDir, toDir);
            fileWatcher = new FileSystemWatcher(fromDir);
            fileWatcher.Created += new FileSystemEventHandler(fileWatcher_Created);
            fileWatcher.Deleted += new FileSystemEventHandler(fileWatcher_Deleted);
            fileWatcher.Renamed += new RenamedEventHandler(fileWatcher_Renamed);
            fileWatcher.Changed += new FileSystemEventHandler(fileWatcher_Changed);
            fileWatcher.EnableRaisingEvents = true;
            fileWatcher.IncludeSubdirectories = true;
        }
        public void Stop()
        {
            fileWatcher.EnableRaisingEvents = false;
        }
        
        void fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            string newPath = mirrorUtils.NewPath(e.FullPath);
            if (Directory.Exists(e.FullPath))
                return;
            File.Copy(e.FullPath, newPath, true);
            LogMessage.Raise(this, "File " + mirrorUtils.BasePath(e.FullPath) + " Changed");
        }

        void fileWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            string oldPath = mirrorUtils.NewPath(e.OldFullPath);
            string newPath = mirrorUtils.NewPath(e.FullPath);
            bool directory = Directory.Exists(e.FullPath);
            if (directory)
                Directory.Move(oldPath, newPath);
            else
                File.Move(oldPath, newPath);
            LogMessage.Raise(this, ((directory) ? "Directory '" : "File '") + mirrorUtils.BasePath(e.OldFullPath) + "' Renamed to '" + mirrorUtils.BasePath(e.FullPath) + "'");
        }

        void fileWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            string newPath = mirrorUtils.NewPath(e.FullPath);
            bool directory = Directory.Exists(newPath);
            if (directory)
                Directory.Delete(newPath,true);
            else
                File.Delete(newPath);
            LogMessage.Raise(this, ((directory) ? "Directory '" : "File '") + mirrorUtils.BasePath(e.FullPath) + "' Deleted");
        }

        void fileWatcher_Created(object sender, FileSystemEventArgs e)
        {
            string newPath = mirrorUtils.NewPath(e.FullPath);
            bool directory = Directory.Exists(e.FullPath);
            if (directory)
            {
                Directory.CreateDirectory(newPath);
                mirrorUtils.CopyDirectory(e.FullPath, newPath);
            }
            else
                File.Copy(e.FullPath, newPath);
            LogMessage.Raise(this, ((directory) ? "Directory '" : "File '") + mirrorUtils.BasePath(e.FullPath) + "' Created");
        }
    }
}
