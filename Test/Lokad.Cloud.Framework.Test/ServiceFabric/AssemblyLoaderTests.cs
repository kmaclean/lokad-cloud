﻿#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using System.IO;
using System.Linq;
using Autofac;
using Lokad.Cloud.Management;
using Lokad.Cloud.Runtime;
using Lokad.Cloud.ServiceFabric.Runtime;
using NUnit.Framework;

namespace Lokad.Cloud.Test.ServiceFabric
{
    [TestFixture]
    public class AssemblyLoaderTests
    {
        [Test]
        public void LoadCheck()
        {
            var path = @"..\..\Sample\sample.dll.zip";
            if(!File.Exists(path))
            {
                // special casing the integration server
                path = @"..\..\Test\Lokad.Cloud.Framework.Test\Sample\sample.dll.zip";
            }

            byte[] buffer;
            using (var dllFile = new FileStream(path, FileMode.Open))
            {
                buffer = new byte[dllFile.Length];
                dllFile.Read(buffer, 0, buffer.Length);
            }

            var runtimeProviders = GlobalSetup.Container.Resolve<RuntimeProviders>();
            runtimeProviders.BlobStorage.CreateContainerIfNotExist(AssemblyLoader.ContainerName);

            // put the sample assembly
            runtimeProviders.BlobStorage.PutBlob(AssemblyLoader.ContainerName, AssemblyLoader.PackageBlobName, buffer);

            var loader = new AssemblyLoader(runtimeProviders);
            loader.LoadPackage();
            loader.LoadConfiguration();

            // validate that 'sample.dll' has been loaded
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Assert.That(assemblies.Any(a => a.FullName.StartsWith("sample")));

            // validate using management class
            var cloudAssemblies = new CloudAssemblies(runtimeProviders);
            Assert.That(cloudAssemblies.GetApplicationDefinition().Value.Assemblies.Any(a => a.AssemblyName.StartsWith("sample")));

            // no update, checking
            try
            {
                loader.CheckUpdate(false);
            }
            catch (TriggerRestartException)
            {
                Assert.Fail("Package has not been updated yet.");
            }

            // forcing update, this time using the management class
            cloudAssemblies.UploadApplicationZipContainer(buffer);

            // update, re-checking
            try
            {
                loader.CheckUpdate(false);
                Assert.Fail("Update should have been detected.");
            }
            catch (TriggerRestartException)
            {
                // do nothing
            }

        }
    }
}
