using System;

namespace Base.DomainClasses;

public class SupportResponse
{
    protected SupportResponse()
    {
        RespondedAt = DateTime.Now;
    }

    public SupportResponse(string responseMessage, int requestId, int adminId) : this()
    {
        EditResponse(responseMessage);
        SupportRequestId = requestId;
        AdminId = adminId;
    }

    public void EditResponse(string responseMessage)
    {
        ResponseMessage = responseMessage ?? throw new ArgumentNullException(nameof(responseMessage));
    }

    public int Id { get; private set; }
    public string ResponseMessage { get; private set; }
    public DateTime RespondedAt { get; private set; }
    public int SupportRequestId { get; private set; }
    public virtual SupportRequest SupportRequest { get; private set; }
    public int AdminId { get; private set; }
    public virtual User Admin { get; private set; }
}
