using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LiveMirror
{
    /// <summary>
    /// Detects and resolves conflicts between two directories.
    /// </summary>
    public class ConflictResloution
    {
        /// <summary>
        /// The type of conflict
        /// </summary>
        public enum ConflictType
        {
            NotInTarget,
            ModifiedInTarget,
            NotInSource,
            DirectoryNotInTarget,
            DirectoryNotInSource
        }
        /// <summary>
        /// The type of resolution
        /// </summary>
        public enum ResolutionType
        {
            CopyToSource,
            CopyToTarget,
            Delete,
            Ignore,
            None
        }
        /// <summary>
        /// Delegate to perform a resolution
        /// </summary>
        /// <param name="conflict"></param>
        /// <param name="resolution"></param>
        public delegate void ResolutionAction(Conflict conflict, Resolution resolution);
        /// <summary>
        /// A possible resolution for a conflict
        /// </summary>
        public struct Resolution
        {
            public static readonly Resolution None = new Resolution() { Action = null, Type = ResolutionType.None };
            public ResolutionType Type;
            public ResolutionAction Action;
        }
        /// <summary>
        /// A conflict between two files or directories
        /// </summary>
        public struct Conflict
        {
            public ConflictType Type;
            public bool Directory;
            /// <summary>
            /// Path name relative to the base directory, no starting /
            /// </summary>
            public string RelativePathName;
            /// <summary>
            /// Possible resolutions
            /// </summary>
            public Resolution[] Resolutions;
            /// <summary>
            /// Performs a resolution for this conflict
            /// </summary>
            /// <param name="resolution"></param>
            public void PerformResolution(Resolution resolution)
            {
                if (resolution.Action != null && Resolutions.Contains(resolution))
                {
                    resolution.Action(this, resolution);
                }
            }
            public Conflict(bool dir, ConflictType type, string relPathName)
            {
                this.Resolutions = new Resolution[3];
                this.Resolutions[0] = Resolution.None;
                this.Resolutions[1] = Resolution.None;
                this.Resolutions[2] = Resolution.None;
                this.Directory = dir;
                this.RelativePathName = relPathName;
                this.Type = type;
            }
        }
        /// <summary>
        /// Event for when a new conflict is detected.
        /// </summary>
        public event EventHandler<EventArgs<Conflict>> NewConflictEvent;

        MirrorUtils mirrorUtils;
        string sourceDirString, targetDirString;
        string[] ignoredPaths;
        Config config;

        List<Conflict> conflicts = new List<Conflict>();
        /// <summary>
        /// Gets the list of current conflicts
        /// </summary>
        public List<Conflict> Conflicts { get { return conflicts.ToList(); } }

        public ConflictResloution(Config config)
        {
            this.config = config;
            this.ignoredPaths = config.Get<string>("Mirror.IgnoredPaths", true, "").Split('|');
        }
        /// <summary>
        /// Processes a directory recursively
        /// </summary>
        /// <param name="fromDirString">Source Directory</param>
        /// <param name="toDirString">Target Directory</param>
        /// <returns>The number of conflicts</returns>
        public int Proccess(string sourceDirString, string targetDirString)
        {
            if (!Directory.Exists(sourceDirString))
                throw new DirectoryNotFoundException("From/Source directory does not exist");

            if (!Directory.Exists(targetDirString))
                throw new DirectoryNotFoundException("To/Target directory does not exist");

            this.sourceDirString = sourceDirString;
            this.targetDirString = targetDirString;
            this.mirrorUtils = new MirrorUtils(sourceDirString, targetDirString);

            DirectoryInfo sourceDirInfo = new DirectoryInfo(sourceDirString);
            DirectoryInfo targetDirInfo = new DirectoryInfo(targetDirString);

            ProcessDirectorys(sourceDirInfo, targetDirInfo);

            return conflicts.Count;
        }
        /// <summary>
        /// Detects and finds conflicts between two directories. Recursive
        /// </summary>
        /// <param name="sourceDirInfo">The source dir</param>
        /// <param name="targetDirInfo">The target dir</param>
        void ProcessDirectorys(DirectoryInfo sourceDirInfo, DirectoryInfo targetDirInfo)
        {
            //Get all files in the target directory
            List<FileInfo> targetFiles = new List<FileInfo>(targetDirInfo.GetFiles());

            foreach (FileInfo sourceFile in sourceDirInfo.GetFiles())
            {
                //Foreach file in source directory, find the corresponding file in the target dir
                FileInfo targetFile = targetFiles.Find(f => f.Name == sourceFile.Name);
                //If file does exist in target remove it from the list
                if (targetFile != null)
                    targetFiles.Remove(targetFile);
                //if target file does not exist
                if (targetFile == null)
                    NewConflict(ConflictType.NotInTarget, sourceFile, null);
                //else check for file content equality
                else if (!mirrorUtils.FilesContentsEqual(sourceFile, targetFile))
                    NewConflict(ConflictType.ModifiedInTarget, sourceFile, targetFile);
            }
            //New conflict for each target file not in source directory
            foreach (FileInfo targetFile in targetFiles)
                NewConflict(ConflictType.NotInSource, null, targetFile);

            //Get all directories in target directory
            List<DirectoryInfo> targetDirs = new List<DirectoryInfo>(targetDirInfo.GetDirectories());

            foreach (DirectoryInfo sourceDir in sourceDirInfo.GetDirectories())
            {
                //Foreach directory in source dir, find corresponding dir in target dir
                DirectoryInfo targetDir = targetDirs.Find(d => d.Name == sourceDir.Name);
                //If not found..
                if (targetDir == null)
                    NewConflict(ConflictType.DirectoryNotInTarget, sourceDir, targetDir);
                else
                {
                    //Does exist
                    //Remove from list
                    targetDirs.Remove(targetDir);
                    //Process the directory (down the rabbit hole we go)
                    ProcessDirectorys(sourceDir, targetDir);
                }
            }
            //New conflict for each directory not in target directory
            foreach (DirectoryInfo targetDir in targetDirs)
                NewConflict(ConflictType.DirectoryNotInSource, null, targetDir);
        }
        /// <summary>
        /// New File conflict
        /// </summary>
        /// <param name="conflictType"></param>
        /// <param name="sourceFile"></param>
        /// <param name="targetFile"></param>
        void NewConflict(ConflictType conflictType, FileInfo sourceFile, FileInfo targetFile)
        {
            //Ignore this conflict if the path is in the ignored list
            if (sourceFile != null && ignoredPaths.Contains(mirrorUtils.BasePath(sourceFile.FullName)))
                return;
            if (targetFile != null && ignoredPaths.Contains(mirrorUtils.BasePath(targetFile.FullName)))
                return;

            //Create a new conflict object, set common options
            string relPath = mirrorUtils.BasePath((sourceFile != null) ? sourceFile.FullName : targetFile.FullName);
            Conflict newConflict = new Conflict(false, conflictType, relPath);
            //Action 3 is Ignore by default
            newConflict.Resolutions[2] = new Resolution() { Action = new ResolutionAction(Ignore), Type = ResolutionType.Ignore };

            if (conflictType == ConflictType.ModifiedInTarget)
            {
                //Copy to Source
                newConflict.Resolutions[0] = new Resolution() { Action = new ResolutionAction(CopyFile), Type = ResolutionType.CopyToSource };
                //Copy to Target
                newConflict.Resolutions[1] = new Resolution() { Action = new ResolutionAction(CopyFile), Type = ResolutionType.CopyToTarget };
            }
            else if (conflictType == ConflictType.NotInSource)
            {
                //Copy to source
                newConflict.Resolutions[0] = new Resolution() { Action = new ResolutionAction(CopyFile), Type = ResolutionType.CopyToSource };
                //Delete
                newConflict.Resolutions[1] = new Resolution() { Action = new ResolutionAction(Delete), Type = ResolutionType.Delete };
            }
            else if (conflictType == ConflictType.NotInTarget)
            {
                //Copy to target
                newConflict.Resolutions[0] = new Resolution() { Action = new ResolutionAction(CopyFile), Type = ResolutionType.CopyToTarget };
            }
            //Add to internal list
            conflicts.Add(newConflict);
            //Raise a new event for conflict
            NewConflictEvent.Raise(this, new EventArgs<Conflict>(newConflict));
        }
        /// <summary>
        /// New Directory conflict
        /// </summary>
        /// <param name="conflictType"></param>
        /// <param name="sourceDir"></param>
        /// <param name="targetDir"></param>
        void NewConflict(ConflictType conflictType, DirectoryInfo sourceDir, DirectoryInfo targetDir)
        {
            //Ignore this conflict if the path is in the ignored list
            if (sourceDir != null && ignoredPaths.Contains(mirrorUtils.BasePath(sourceDir.FullName)))
                return;
            if (targetDir != null && ignoredPaths.Contains(mirrorUtils.BasePath(targetDir.FullName)))
                return;

            //Create a new conflict object, set common options
            string relPath = mirrorUtils.BasePath((sourceDir != null) ? sourceDir.FullName : targetDir.FullName);
            Conflict newConflict = new Conflict(true, conflictType, relPath);
            //Action 3 is Ignore by default
            newConflict.Resolutions[2] = new Resolution() { Action = new ResolutionAction(Ignore), Type = ResolutionType.Ignore };

            if (conflictType == ConflictType.DirectoryNotInSource)
            {
                //Copy to Source
                newConflict.Resolutions[0] = new Resolution() { Action = new ResolutionAction(CopyDirectory), Type = ResolutionType.CopyToSource };
                //Delete
                newConflict.Resolutions[1] = new Resolution() { Action = new ResolutionAction(Delete), Type = ResolutionType.Delete };
            }
            if (conflictType == ConflictType.DirectoryNotInTarget)
            {
                //Copy to Target
                newConflict.Resolutions[0] = new Resolution() { Action = new ResolutionAction(CopyDirectory), Type = ResolutionType.CopyToTarget };
            }
            //Add to internal list
            conflicts.Add(newConflict);
            //Raise new event for conflict
            NewConflictEvent.Raise(this, new EventArgs<Conflict>(newConflict));
        }
        /// <summary>
        /// Ignores a conflict
        /// </summary>
        /// <param name="conflict"></param>
        /// <param name="resolution">Ignored</param>
        void Ignore(Conflict conflict, Resolution resolution)
        {
            conflicts.Remove(conflict);
        }
        /// <summary>
        /// Copys a file
        /// </summary>
        /// <param name="conflict"></param>
        /// <param name="resolution"></param>
        void CopyFile(Conflict conflict, Resolution resolution)
        {
            if (resolution.Type == ResolutionType.CopyToSource)
            {
                File.Copy(Path.Combine(targetDirString, conflict.RelativePathName)
                    , Path.Combine(sourceDirString, conflict.RelativePathName));
            }
            else if (resolution.Type == ResolutionType.CopyToTarget)
            {
                File.Copy(Path.Combine(sourceDirString, conflict.RelativePathName),
                    Path.Combine(targetDirString, conflict.RelativePathName));
            }
            conflicts.Remove(conflict);
        }
        /// <summary>
        /// Copys a directory recursively
        /// </summary>
        /// <param name="conflict"></param>
        /// <param name="resolution"></param>
        void CopyDirectory(Conflict conflict, Resolution resolution)
        {
            if (resolution.Type == ResolutionType.CopyToSource)
            {
                mirrorUtils.CopyDirectory(Path.Combine(targetDirString, conflict.RelativePathName),
                    Path.Combine(sourceDirString, conflict.RelativePathName));
            }
            else if (resolution.Type == ResolutionType.CopyToTarget)
            {
                mirrorUtils.CopyDirectory(Path.Combine(sourceDirString, conflict.RelativePathName),
                    Path.Combine(targetDirString, conflict.RelativePathName));
            }
            conflicts.Remove(conflict);
        }
        /// <summary>
        /// Deletes a file or directory recursively
        /// </summary>
        /// <param name="conflict"></param>
        /// <param name="resolution"></param>
        void Delete(Conflict conflict, Resolution resolution)
        {
            if (conflict.Directory)
            {
                if (conflict.Type == ConflictType.DirectoryNotInSource) //Delete from target
                    Directory.Delete(Path.Combine(targetDirString, conflict.RelativePathName), true);
                else if (conflict.Type == ConflictType.DirectoryNotInTarget) //Delete from source
                    Directory.Delete(Path.Combine(sourceDirString, conflict.RelativePathName), true);
            }
            else
            {
                if (conflict.Type == ConflictType.NotInSource) //Delete from target
                    File.Delete(Path.Combine(targetDirString, conflict.RelativePathName));
                else if (conflict.Type == ConflictType.NotInTarget) //Delete from source
                    File.Delete(Path.Combine(sourceDirString, conflict.RelativePathName));
            }
            conflicts.Remove(conflict);
        }
    }
}
