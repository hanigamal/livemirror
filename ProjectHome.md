This is a C# project to mirror the changes in one directory to another (either may be on a local drive, mapped or network location).

I created this for the sole purpose of keeping a PHP web project that I was editing on my dev pc in sync with the dev server without having to copy over files manually.

It uses [FileSystemWatcher](http://msdn.microsoft.com/en-us/library/system.io.filesystemwatcher.aspx) to provide the notifications of file system changes.

When the mirroring starts conflicts are detected and resolved by the user, see ConflictResolution.

As for the state of the project.. Its basically working, lacking in error catching and in comments, but at a state where it works :)