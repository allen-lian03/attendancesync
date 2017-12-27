using System;

namespace ZKTeco.SyncBackendService.Utils
{
    internal class IdGenerator
    {
        public static long GenerateId(string userId, DateTime logDate, string projectId)
        {
            var sum = 0L;
            if (string.IsNullOrWhiteSpace(userId))
            {
                sum += 1L;
            }
            else
            {
                sum += userId.GetHashCode();
            }

            sum += logDate.Ticks;

            if (string.IsNullOrWhiteSpace(projectId))
            {
                sum += 2L;
            }
            else
            {
                sum += projectId.GetHashCode();
            }
            return sum;
        }
    }
}
