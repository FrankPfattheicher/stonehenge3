using IctBaden.Stonehenge3.Core;
using Xunit;

namespace IctBaden.Stonehenge3.Test.Session;

public class CreateTypeTests
{
    [Fact]
    public void SetViewModelTypeShouldMatchTypeNameExactly()
    {
        var session = new AppSession();
        var instance = session.SetViewModelType("TestVm");
        
        Assert.Equal(nameof(TestVm), instance.GetType().Name);

        instance = session.SetViewModelType("ExtraTestVm");
        
        Assert.Equal(nameof(ExtraTestVm), instance.GetType().Name);
    }

}