﻿using System;
using System.IO;
using YARG.Core.Extensions;

namespace YARG.Core.IO
{
    public interface IAbridgedInfo
    {
        public string FullName { get; }
        public DateTime LastUpdatedTime { get; }
    }

    /// <summary>
    /// A FileInfo structure that only contains the filename and time last added
    /// </summary>
    public readonly struct AbridgedFileInfo : IAbridgedInfo
    {
        public const FileAttributes RECALL_ON_DATA_ACCESS = (FileAttributes)0x00400000;

        /// <summary>
        /// The file path
        /// </summary>
        private readonly string _fullname;

        /// <summary>
        /// The time the file was last written or created on OS - whichever came later
        /// </summary>
        private readonly DateTime _lastUpdatedTime;

        public string FullName => _fullname;
        public DateTime LastUpdatedTime => _lastUpdatedTime;

        public AbridgedFileInfo(string file, bool checkCreationTime = true)
            : this(new FileInfo(file), checkCreationTime) { }

        public AbridgedFileInfo(FileInfo info, bool checkCreationTime = true)
        {
            _fullname = info.FullName;
            _lastUpdatedTime = info.LastWriteTime;
            if (checkCreationTime && info.CreationTime > _lastUpdatedTime)
            {
                _lastUpdatedTime = info.CreationTime;
            }
        }

        /// <summary>
        /// Only used when validation of the underlying file is not required
        /// </summary>
        public AbridgedFileInfo(UnmanagedMemoryStream stream)
            : this(stream.ReadString(), stream) { }

        /// <summary>
        /// Only used when validation of the underlying file is not required
        /// </summary>
        public AbridgedFileInfo(string filename, UnmanagedMemoryStream stream)
        {
            _fullname = filename;
            _lastUpdatedTime = DateTime.FromBinary(stream.Read<long>(Endianness.Little));
        }

        public AbridgedFileInfo(string filename, in DateTime lastUpdatedTime)
        {
            _fullname = filename;
            _lastUpdatedTime = lastUpdatedTime;
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(_fullname);
            writer.Write(_lastUpdatedTime.ToBinary());
        }

        public bool Exists()
        {
            return File.Exists(_fullname);
        }

        public bool IsStillValid(bool checkCreationTime = true)
        {
            var info = new FileInfo(_fullname);
            if (!info.Exists)
            {
                return false;
            }

            var timeToCompare = info.LastWriteTime;
            if (checkCreationTime && info.CreationTime > timeToCompare)
            {
                timeToCompare = info.CreationTime;
            }
            return timeToCompare == _lastUpdatedTime;
        }

        /// <summary>
        /// Used for cache validation
        /// </summary>
        public static AbridgedFileInfo? TryParseInfo(UnmanagedMemoryStream stream, bool checkCreationTime = true)
        {
            return TryParseInfo(stream.ReadString(), stream, checkCreationTime);
        }

        /// <summary>
        /// Used for cache validation
        /// </summary>
        public static AbridgedFileInfo? TryParseInfo(string file, UnmanagedMemoryStream stream, bool checkCreationTime = true)
        {
            var info = new FileInfo(file);
            if (!info.Exists)
            {
                stream.Position += sizeof(long);
                return null;
            }

            var abridged = new AbridgedFileInfo(info, checkCreationTime);
            if (abridged._lastUpdatedTime != DateTime.FromBinary(stream.Read<long>(Endianness.Little)))
            {
                return null;
            }
            return abridged;
        }
    }
}
