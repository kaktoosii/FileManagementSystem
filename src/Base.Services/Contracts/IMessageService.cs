using Base.ViewModels;


namespace Services.Contracts;
public interface IMessageService
{
    Task<int> AddNewMessageAsync(MessageDto messageDto);
    Task EditMessageAsync(int id, MessageViewModel messageDto);
    Task<PagedResponse<List<MessageViewModel>>> GetMessages(MessageFilterDto filter);
    Task<MessageViewModel> GetMessage(int id);
    Task DeleteMessageAsync(int id);
    Task SeenMessageAsync(int id);
    Task<PagedResponse<List<MessageViewModel>>> GetUserMessages(MessageFilterDto filter);
}