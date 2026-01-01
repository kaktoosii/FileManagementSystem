using System;
using System.Collections.Generic;
using System.Text;

namespace Base.DomainClasses;

    public class Document
    {
    protected Document() => RegisterDate = DateTime.UtcNow;
    public Document(
            string path,
            int userId,
            string uploaderIp,
            string mimType,
            string originalFileName = null,
            int? folderId = null,
            int? filePatternId = null,
            string patternValues = null
            ):this()
        {
            Path = path;
            UserId = userId;
            UploaderIp = uploaderIp;
            MimType = mimType;
            OriginalFileName = originalFileName;
            FolderId = folderId;
            FilePatternId = filePatternId;
            PatternValues = patternValues;
        }

        public Guid Id { get; private set; }
        public string Path { get; private set; }
        public string OriginalFileName { get; private set; }
        public int UserId { get; private set; }
        public string UploaderIp { get; private set; }
        public DateTime RegisterDate { get; private set; }
        public string MimType { get; private set; }
        public int? FolderId { get; private set; }
        public int? FilePatternId { get; private set; }
        public string PatternValues { get; private set; } // JSON string of field values used

        // Navigation properties
        public Folder Folder { get; private set; }
        public FilePattern FilePattern { get; private set; }

        public void UpdateOriginalFileName(string originalFileName)
        {
            OriginalFileName = originalFileName;
        }

        public void UpdateFolder(int? folderId)
        {
            FolderId = folderId;
        }

        public void UpdatePattern(int? filePatternId, string patternValues)
        {
            FilePatternId = filePatternId;
            PatternValues = patternValues;
        }

    }

