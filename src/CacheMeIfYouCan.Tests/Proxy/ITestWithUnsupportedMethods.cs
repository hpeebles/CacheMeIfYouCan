namespace CacheMeIfYouCan.Tests.Proxy
{
    public interface ITestWithUnsupportedMethods : ITest
    {
        string UnsupportedFunc(int a, int b, int c, int d, int e);

        void UnsupportedAction(int a);
        
        int UnsupportedProperty { get; set; }
    }
}