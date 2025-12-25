using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ViewModels.Dto;
    public class DocumentDto
    {
        public DocumentDto(IFormFile image)
        {
            Image = (image == null || image.Length == 0) ?
                throw new InvalidOperationException()
                : image;
        }

        public void SetUploaderIp(string IpAddress)
        {
            this.UploaderIp = IpAddress;
        }

        public IFormFile Image { get; private set; }
        public string? UploaderIp { get; private set; }

    }

   


