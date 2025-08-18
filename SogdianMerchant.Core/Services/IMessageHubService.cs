namespace SogdianMerchant.Core.Services
{
    public interface IMessageHubService
    {
        void Publish<T>(T message);
        string ToString();
    }
}