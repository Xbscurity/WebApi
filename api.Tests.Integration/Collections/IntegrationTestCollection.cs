using api.Tests.Integration.Collections.Fixtures;

namespace api.Tests.Integration.Collections
{
    [CollectionDefinition("IntegrationTestCollection")]
    public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
    {
    }
}
