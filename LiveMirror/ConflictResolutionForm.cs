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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Linq;

namespace LiveMirror
{
    public partial class ConflictResolutionForm : Form
    {
        public enum ConflictType
        {
            NotInTarget,
            ModifiedInTarget,
            NotInSource,
            DirectoryNotInTarget,
            DirectoryNotInSource
        }

        string fromDir, toDir;
        string[] ignoredPaths;
        Config config;
        MirrorUtils mirrorUtils;

        public ConflictResolutionForm(string fromDir, string toDir, Config config)
        {
            if (!Directory.Exists(fromDir))
                throw new DirectoryNotFoundException("From directory does not exist");

            if (!Directory.Exists(toDir))
                throw new DirectoryNotFoundException("To directory does not exist");

            this.mirrorUtils = new MirrorUtils(fromDir, toDir);
            this.fromDir = fromDir;
            this.toDir = toDir;
            this.config = config;
            this.ignoredPaths = config.Get<string>("Mirror.IgnoredPaths", true, "").Split('|');

            InitializeComponent();

            new FormPositionSaver("Form.ConflictResolution", this, config);
        }

        private void ConflictResolutionForm_Load(object sender, EventArgs e)
        {
            DirectoryInfo source = new DirectoryInfo(fromDir);
            DirectoryInfo target = new DirectoryInfo(toDir);

            ProcessDirectory(source, target);
            if (pnlConflicts.Controls.Count == 0)
            {
                DialogResult = DialogResult.OK;
                this.Close();
            }
        }
        void ProcessDirectory(DirectoryInfo sourceDirInfo, DirectoryInfo targetDirInfo)
        {
            List<FileInfo> targetFiles = new List<FileInfo>(targetDirInfo.GetFiles());

            foreach (FileInfo sourceFile in sourceDirInfo.GetFiles())
            {
                FileInfo targetFile = targetFiles.Find(f => f.Name == sourceFile.Name);
                if (targetFile != null)
                    targetFiles.Remove(targetFile);
                if (targetFile == null)
                    NewConflict(ConflictType.NotInTarget, sourceFile, null);
                else if (!mirrorUtils.FilesContentsEqual(sourceFile, targetFile))
                    NewConflict(ConflictType.ModifiedInTarget, sourceFile, targetFile);
            }
            foreach (FileInfo targetFile in targetFiles)
                NewConflict(ConflictType.NotInSource, null, targetFile);

            List<DirectoryInfo> targetDirs = new List<DirectoryInfo>(targetDirInfo.GetDirectories());

            foreach (DirectoryInfo sourceDir in sourceDirInfo.GetDirectories())
            {
                DirectoryInfo targetDir = targetDirs.Find(d => d.Name == sourceDir.Name);
                if (targetDir == null)
                    NewConflict(ConflictType.DirectoryNotInTarget, sourceDir, targetDir);
                else
                {
                    targetDirs.Remove(targetDir);
                    ProcessDirectory(sourceDir, targetDir);
                }
            }
            foreach (DirectoryInfo targetDir in targetDirs)
                NewConflict(ConflictType.DirectoryNotInSource, null, targetDir);
        }
        void NewConflict(ConflictType type, DirectoryInfo sourceDir, DirectoryInfo targetDir)
        {
            if (sourceDir != null && ignoredPaths.Contains(mirrorUtils.BasePath(sourceDir.FullName)))
                return;
            if (targetDir != null && ignoredPaths.Contains(mirrorUtils.BasePath(targetDir.FullName)))
                return;

            FileActionControl action = new FileActionControl();
            if (type == ConflictType.DirectoryNotInSource)
            {
                action.SetFileInfo(mirrorUtils.BasePath(targetDir.FullName) + " (Directory)", "Not in Source");
                action.SetAction1("Copy to Source");
                action.Action1 += new EventHandler(
                    delegate(object sender, EventArgs e)
                    {
                        mirrorUtils.CopyDirectory(targetDir.FullName, Path.Combine(fromDir, mirrorUtils.BasePath(targetDir.FullName)));
                        RemoveConflict(action);
                    });
                action.SetAction2("Delete");
                action.Action2 += new EventHandler(
                    delegate(object sender, EventArgs e)
                    {
                        targetDir.Delete(true);
                        RemoveConflict(action);
                    });
            }
            else if (type == ConflictType.DirectoryNotInTarget)
            {
                action.SetFileInfo(mirrorUtils.BasePath(sourceDir.FullName) + " (Directory)", "Not in Target");
                action.SetAction1("Copy to Target");
                action.Action1 += new EventHandler(
                    delegate(object sender, EventArgs e)
                    {
                        mirrorUtils.CopyDirectory(sourceDir.FullName, Path.Combine(toDir, mirrorUtils.BasePath(sourceDir.FullName)));
                        RemoveConflict(action);
                    });
            }
            action.SetAction3("Ignore");
            action.Action3 += new EventHandler(
                delegate(object sender, EventArgs e)
                {
                    RemoveConflict(action);
                });
            pnlConflicts.Controls.Add(action);
        }
        void NewConflict(ConflictType type, FileInfo sourceFile, FileInfo targetFile)
        {
            if (sourceFile != null && ignoredPaths.Contains(mirrorUtils.BasePath(sourceFile.FullName)))
                return;
            if (targetFile != null && ignoredPaths.Contains(mirrorUtils.BasePath(targetFile.FullName)))
                return;

            FileActionControl action = new FileActionControl();
            if (type == ConflictType.ModifiedInTarget)
            {
                action.SetFileInfo(mirrorUtils.BasePath(sourceFile.FullName), "Modified in Target");
                action.SetAction1("Copy to Target");
                action.Action1 += new EventHandler(
                    delegate(object sender, EventArgs e)
                    {
                        File.Copy(sourceFile.FullName, Path.Combine(toDir, mirrorUtils.BasePath(sourceFile.FullName)), true);
                        RemoveConflict(action);
                    });
                action.SetAction2("Copy to Source");
                action.Action2 += new EventHandler(
                    delegate(object sender, EventArgs e)
                    {
                        File.Copy(Path.Combine(toDir, mirrorUtils.BasePath(sourceFile.FullName)),sourceFile.FullName, true);
                        RemoveConflict(action);
                    });

            }
            else if (type == ConflictType.NotInSource)
            {
                action.SetFileInfo(mirrorUtils.BasePath(targetFile.FullName), "Not in Source");
                action.SetAction1("Copy to Source");
                action.Action1 += new EventHandler(
                    delegate(object sender, EventArgs e)
                    {
                        File.Copy(targetFile.FullName, Path.Combine(fromDir, mirrorUtils.BasePath(targetFile.FullName)));
                        RemoveConflict(action);
                    });
                action.SetAction2("Delete");
                action.Action2 += new EventHandler(
                    delegate(object sender, EventArgs e)
                    {
                        targetFile.Delete();
                        RemoveConflict(action);
                    });
            }
            else if (type == ConflictType.NotInTarget)
            {
                action.SetFileInfo(mirrorUtils.BasePath(sourceFile.FullName), "Not in Target");
                action.SetAction1("Copy to Target");
                action.Action1 += new EventHandler(
                    delegate(object sender, EventArgs e)
                    {
                        File.Copy(sourceFile.FullName, Path.Combine(toDir, mirrorUtils.BasePath(sourceFile.FullName)));
                        RemoveConflict(action);
                    });
            }
            action.SetAction3("Ignore");
            action.Action3 += new EventHandler(
                delegate(object sender, EventArgs e)
                {
                    RemoveConflict(action);
                });
            pnlConflicts.Controls.Add(action);
        }
        void RemoveConflict(FileActionControl conflict)
        {
            pnlConflicts.Controls.Remove(conflict);
            if (pnlConflicts.Controls.Count == 0)
            {
                DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void ConflictResolutionForm_FormClosed(object sender, FormClosedEventArgs e)
        {
        }
    }
}
