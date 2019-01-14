using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Samples.AspNetCoreApp
{
    public interface IEnabledAffiliatesReader
    {
        Task<ISet<int>> Get();
    }

    public class EnabledAffiliatesReader : IEnabledAffiliatesReader
    {
        public async Task<ISet<int>> Get()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            
            return new HashSet<int> { 1, 2 };
        }
    }
}