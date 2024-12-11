namespace LarpakeServer.Services;

public interface ISubscriptionClientPool
{
    void Add(HttpResponse client, Guid? clientId);
    bool Remove(HttpResponse client);
    HttpResponse? TryFind(Guid clientId);
    IEnumerable<HttpResponse> GetAll();
    bool IsFull { get; }
}
