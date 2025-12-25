using Base.DomainClasses.Enums;
using System;

namespace Base.DomainClasses;

public class SupportRequest
{
    protected SupportRequest()
    {
        CreatedAt = DateTime.Now;
        Status = RequestStatus.PENDING;
    }

    public SupportRequest(string subject, string message, int customerId) : this()
    {
        EditRequest(subject, message);
        CustomerId = customerId;
    }

    public void EditRequest(string subject, string message)
    {
        Subject = subject ?? throw new ArgumentNullException(nameof(subject));
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    public void UpdateStatus(RequestStatus status)
    {
        Status = status;
    }

    public int Id { get; private set; }
    public string Subject { get; private set; }
    public string Message { get; private set; }
    public RequestStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public int CustomerId { get; private set; }
    public virtual User Customer { get; private set; }
    public virtual SupportResponse Response { get; private set; }
}
