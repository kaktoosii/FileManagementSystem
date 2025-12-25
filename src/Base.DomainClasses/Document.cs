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
            string mimType
            ):this()
        {
            Path = path;
            UserId = userId;
            UploaderIp = uploaderIp;
            MimType = mimType;
        }

        public Guid Id { get; private set; }
        public string Path { get; private set; }
        public int UserId { get; private set; }
        public string UploaderIp { get; private set; }
        public DateTime RegisterDate { get; private set; }
        public string MimType { get; private set; }

        
      
       

    }

