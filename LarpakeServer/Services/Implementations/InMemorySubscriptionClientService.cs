


namespace LarpakeServer.Services.Implementations;

public class InMemorySubscriptionClientService : ISubscriptionClientPool
{
    public bool IsFull => throw new NotImplementedException();

    public void Add(HttpResponse client, Guid? clientId)
    {
        throw new NotImplementedException();
    }



    public bool Remove(HttpResponse client)
    {
        throw new NotImplementedException();
    }

    public HttpResponse? TryFind(Guid clientId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<HttpResponse> GetAll()
    {
        throw new NotImplementedException();
    }
}
