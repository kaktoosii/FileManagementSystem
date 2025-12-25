using System;
using System.ComponentModel.DataAnnotations;
using Base.DomainClasses;

namespace Base.DomainClasses;

public class MessageSeen : BaseModel
{
    protected MessageSeen()
    {
        CreateDate = DateTime.Now;
    }

    public MessageSeen(int userId, int messageId) : this()
    {
        // بررسی مقدار معتبر بودن senderUserId (باید مقدار مثبت باشد)
        if (messageId <= 0)
        {
            throw new ArgumentException("شناسه پیام نامعتبر است.");
        }

        UserId = userId;
        MessageId = messageId;
    }

    public int MessageId { get; set; }

    public virtual Message Message { get; set; }
    public int UserId { get; set; }

    public virtual User User { get; set; }
}
