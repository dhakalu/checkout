using WebApi.Identity.Utilities;

namespace WebApi.Identity.Tests.Utilities;

public class FakerUtilTests
{
    [Fact]
    public async Task GetRandomStringWithLength_ReturnsStringWithLength()
    {
        var actual = FakerUtil.GetRandomStringWithLength(100);
        Assert.Equal(100, actual.Length);
    }
}
