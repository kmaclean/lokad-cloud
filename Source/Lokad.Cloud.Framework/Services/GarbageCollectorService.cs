﻿#region Copyright (c) Lokad 2009
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using Lokad.Cloud.ServiceFabric;
using Lokad.Cloud.Storage;

namespace Lokad.Cloud.Services
{
    /// <summary>
    /// Garbage collects temporary items stored in the <see cref="CloudService.TemporaryContainer"/>.
    /// </summary>
    /// <remarks>
    /// The container <see cref="CloudService.TemporaryContainer"/> is handy to
    /// store non-persistent data, typically state information concerning ongoing
    /// processing.
    /// </remarks>
    [ScheduledServiceSettings(
        AutoStart = true, 
        Description = "Garbage collects temporary items.",
        TriggerInterval = 60)] // 1 execution every 1min
    public class GarbageCollectorService : ScheduledService
    {
        static TimeSpan MaxExecutionTime { get { return TimeSpan.FromMinutes(10); } }

        protected override void StartOnSchedule()
        {
            const string containerName = TemporaryBlobName<object>.DefaultContainerName;
            var executionExpiration = DateTimeOffset.UtcNow.Add(MaxExecutionTime);

            // lazy enumeration over the overflowing messages
            foreach (var blobName in Blobs.ListBlobNames(containerName))
            {
                // HACK: targeted object is irrelevant
                var parsedName = UntypedBlobName.Parse<TemporaryBlobName<object>>(blobName);

                if (DateTimeOffset.UtcNow <= parsedName.Expiration)
                {
                    // overflowing messages are iterated in date-increasing order
                    // as soon a non-expired overflowing message is encountered
                    // just stop the process.
                    break;
                }

                // if the overflowing message is expired, delete it
                Blobs.DeleteBlobIfExist(containerName, blobName);

                // don't freeze the worker with this service
                if (DateTimeOffset.UtcNow > executionExpiration)
                {
                    break;
                }
            }
        }
    }
}
