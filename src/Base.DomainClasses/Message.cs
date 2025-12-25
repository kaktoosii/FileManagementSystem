using System;
using System.ComponentModel.DataAnnotations;
using Base.DomainClasses;

namespace Base.DomainClasses;

public class Message : BaseModel
{
    protected Message()
    {
        CreateDate = DateTime.Now;
    }

    public Message(string subject, string description, int senderUserId, string pictureId) : this()
    {
        // بررسی مقدار نال یا خالی برای Subject
        if(string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("مقدار موضوع نمی‌تواند خالی یا نال باشد.");

        // بررسی مقدار نال یا خالی برای Description
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("مقدار توضیحات نمی‌تواند خالی یا نال باشد.");

        // بررسی مقدار معتبر بودن senderUserId (باید مقدار مثبت باشد)
        if (senderUserId <= 0)
        {
            throw new ArgumentException("شناسه فرستنده نامعتبر است.", nameof(senderUserId));
        }

        Subject = subject;
        Description = description;
        SenderUserId = senderUserId;
        PictureId = pictureId;
    }

    public void SoftDelete()
    {
        this.IsDeleted = true;
    }

    [Required(ErrorMessage = "عنوان پیام الزامی است.")]
    [MaxLength(200, ErrorMessage = "عنوان پیام نمی‌تواند بیشتر از ۲۰۰ کاراکتر باشد.")]
    public string Subject { get; set; }

    [Required(ErrorMessage = "متن پیام الزامی است.")]
    [MaxLength(2000, ErrorMessage = "متن پیام نمی‌تواند بیشتر از ۲۰۰۰ کاراکتر باشد.")]
    public string Description { get; set; }

    public string PictureId { get; set; }

    [Required(ErrorMessage = "شناسه فرستنده پیام الزامی است.")]
    public int SenderUserId { get; set; }

    public virtual User User { get; set; }
    public ICollection<MessageSeen> MessageSeens { get; set; }
}
