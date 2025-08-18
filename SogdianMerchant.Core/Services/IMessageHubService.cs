namespace SogdianMerchant.Core.Services
{
    public interface IMessageHubService
    {
        void Clear();
        void Publish<T>(T message);
        string ToString();
    }
}