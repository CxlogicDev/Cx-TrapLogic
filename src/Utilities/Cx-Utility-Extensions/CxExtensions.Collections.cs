using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Concurrent;

namespace CxUtility.Extensions;

public static partial class CxUtilityExtensions
{
    /// <summary>
    /// Warning!!! Method runs all the calls in parallel.
    /// Database Call could overrite 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="dop"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    private static Task ForEachAsync<T>(this IEnumerable<T> source, int dop, Func<T, Task> body)
    {
        return Task.WhenAll(from partition in Partitioner.Create(source).GetPartitions(dop)
                            select Task.Run(async delegate
                            {
                                using (partition)
                                {
                                    while (partition.MoveNext())
                                    {
                                        await body(partition.Current);
                                    }
                                }
                            }));
    }
}
