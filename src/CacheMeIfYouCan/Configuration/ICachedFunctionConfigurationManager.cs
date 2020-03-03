using System;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration
{
    #region Async
    public interface ICachedFunctionConfigurationManagerAsync_1Param<in TParam, TResponse>
    {
        Func<TParam, Task<TResponse>> Build();
    }

    public interface ICachedFunctionConfigurationManagerAsync_2Params<in TParam1, in TParam2, TResponse>
    {
        Func<TParam1, TParam2, Task<TResponse>> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerAsync_3Params<in TParam1, in TParam2, in TParam3, TResponse>
    {
        Func<TParam1, TParam2, TParam3, Task<TResponse>> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerAsync_4Params<in TParam1, in TParam2, in TParam3, in TParam4, TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, Task<TResponse>> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerAsync_5Params<in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, TParam5, Task<TResponse>> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerAsync_6Params<in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, in TParam6, TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, Task<TResponse>> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerAsync_7Params<in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, in TParam6, in TParam7, TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, Task<TResponse>> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerAsync_8Params<in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, in TParam6, in TParam7, in TParam8, TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, Task<TResponse>> Build();
    }
    #endregion
    
    #region AsyncCanx
    public interface ICachedFunctionConfigurationManagerAsyncCanx_1Param<in TParam, TResponse>
    {
        Func<TParam, CancellationToken, Task<TResponse>> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerAsyncCanx_2Params<in TParam1, in TParam2, TResponse>
    {
        Func<TParam1, TParam2, CancellationToken, Task<TResponse>> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerAsyncCanx_3Params<in TParam1, in TParam2, in TParam3, TResponse>
    {
        Func<TParam1, TParam2, TParam3, CancellationToken, Task<TResponse>> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerAsyncCanx_4Params<in TParam1, in TParam2, in TParam3, in TParam4, TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, Task<TResponse>> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerAsyncCanx_5Params<in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, Task<TResponse>> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerAsyncCanx_6Params<in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, in TParam6, TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, Task<TResponse>> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerAsyncCanx_7Params<in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, in TParam6, in TParam7, TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, Task<TResponse>> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerAsyncCanx_8Params<in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, in TParam6, in TParam7, in TParam8, TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, Task<TResponse>> Build();
    }
    #endregion
    
    #region Sync
    public interface ICachedFunctionConfigurationManagerSync_1Param<in TParam, out TResponse>
    {
        Func<TParam, TResponse> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerSync_2Params<in TParam1, in TParam2, out TResponse>
    {
        Func<TParam1, TParam2, TResponse> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerSync_3Params<in TParam1, in TParam2, in TParam3, out TResponse>
    {
        Func<TParam1, TParam2, TParam3, TResponse> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerSync_4Params<in TParam1, in TParam2, in TParam3, in TParam4, out TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, TResponse> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerSync_5Params<in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, out TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, TParam5, TResponse> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerSync_6Params<in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, in TParam6, out TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TResponse> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerSync_7Params<in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, in TParam6, in TParam7, out TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TResponse> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerSync_8Params<in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, in TParam6, in TParam7, in TParam8, out TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TResponse> Build();
    }
    #endregion
    
    #region SyncCanx
    public interface ICachedFunctionConfigurationManagerSyncCanx_1Param<in TParam, out TResponse>
    {
        Func<TParam, CancellationToken, TResponse> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerSyncCanx_2Params<in TParam1, in TParam2, out TResponse>
    {
        Func<TParam1, TParam2, CancellationToken, TResponse> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerSyncCanx_3Params<in TParam1, in TParam2, in TParam3, out TResponse>
    {
        Func<TParam1, TParam2, TParam3, CancellationToken, TResponse> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerSyncCanx_4Params<in TParam1, in TParam2, in TParam3, in TParam4, out TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, TResponse> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerSyncCanx_5Params<in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, out TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, TResponse> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerSyncCanx_6Params<in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, in TParam6, out TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, TResponse> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerSyncCanx_7Params<in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, in TParam6, in TParam7, out TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, TResponse> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerSyncCanx_8Params<in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, in TParam6, in TParam7, in TParam8, out TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, TResponse> Build();
    }
    #endregion

    #region ValueTask
    public interface ICachedFunctionConfigurationManagerValueTask_1Param<in TParam, TResponse>
    {
        Func<TParam, ValueTask<TResponse>> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerValueTask_2Params<in TParam1, in TParam2, TResponse>
    {
        Func<TParam1, TParam2, ValueTask<TResponse>> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerValueTask_3Params<in TParam1, in TParam2, in TParam3, TResponse>
    {
        Func<TParam1, TParam2, TParam3, ValueTask<TResponse>> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerValueTask_4Params<in TParam1, in TParam2, in TParam3, in TParam4, TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, ValueTask<TResponse>> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerValueTask_5Params<in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, TParam5, ValueTask<TResponse>> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerValueTask_6Params<in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, in TParam6, TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, ValueTask<TResponse>> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerValueTask_7Params<in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, in TParam6, in TParam7, TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, ValueTask<TResponse>> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerValueTask_8Params<in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, in TParam6, in TParam7, in TParam8, TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, ValueTask<TResponse>> Build();
    }
    #endregion
    
    #region ValueTaskCanx
    public interface ICachedFunctionConfigurationManagerValueTaskCanx_1Param<in TParam, TResponse>
    {
        Func<TParam, CancellationToken, ValueTask<TResponse>> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerValueTaskCanx_2Params<in TParam1, in TParam2, TResponse>
    {
        Func<TParam1, TParam2, CancellationToken, ValueTask<TResponse>> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerValueTaskCanx_3Params<in TParam1, in TParam2, in TParam3, TResponse>
    {
        Func<TParam1, TParam2, TParam3, CancellationToken, ValueTask<TResponse>> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerValueTaskCanx_4Params<in TParam1, in TParam2, in TParam3, in TParam4, TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, ValueTask<TResponse>> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerValueTaskCanx_5Params<in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, ValueTask<TResponse>> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerValueTaskCanx_6Params<in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, in TParam6, TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, ValueTask<TResponse>> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerValueTaskCanx_7Params<in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, in TParam6, in TParam7, TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, ValueTask<TResponse>> Build();
    }
    
    public interface ICachedFunctionConfigurationManagerValueTaskCanx_8Params<in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, in TParam6, in TParam7, in TParam8, TResponse>
    {
        Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, ValueTask<TResponse>> Build();
    }
    #endregion
}