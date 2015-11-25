﻿using System.Collections.Generic;
using System.Reflection;
using Xunit.Abstractions;

#if PLATFORM_DOTNET
using System.IO;
#endif

namespace Xunit
{
    /// <summary>
    /// This class be used to do discovery and execution of xUnit.net v2 tests
    /// using a reflection-based implementation of <see cref="IAssemblyInfo"/>.
    /// </summary>
    public class Xunit2 : Xunit2Discoverer, IFrontController
    {
        readonly ITestFrameworkExecutor executor;

        /// <summary>
        /// Initializes a new instance of the <see cref="Xunit2"/> class.
        /// </summary>
        /// <param name="appDomainSupport">Determines whether tests should be run in a separate app domain.</param>
        /// <param name="sourceInformationProvider">The source code information provider.</param>
        /// <param name="assemblyInfo">Provides information about the test assembly</param>
        /// <param name="assemblyFileName">The test assembly.</param>
        /// <param name="configFileName">The test assembly configuration file.</param>
        /// <param name="shadowCopy">If set to <c>true</c>, runs tests in a shadow copied app domain, which allows
        /// tests to be discovered and run without locking assembly files on disk.</param>
        /// <param name="shadowCopyFolder">The path on disk to use for shadow copying; if <c>null</c>, a folder
        /// will be automatically (randomly) generated</param>
        /// <param name="diagnosticMessageSink">The message sink which received <see cref="IDiagnosticMessage"/> messages.</param>
        /// <param name="verifyTestAssemblyExists">Determines whether or not the existence of the test assembly is verified.</param>
        public Xunit2(AppDomainSupport appDomainSupport,
                      ISourceInformationProvider sourceInformationProvider,
                      IAssemblyInfo assemblyInfo,
                      string assemblyFileName,
                      string configFileName = null,
                      bool shadowCopy = true,
                      string shadowCopyFolder = null,
                      IMessageSink diagnosticMessageSink = null,
                      bool verifyTestAssemblyExists = true)
            : base(appDomainSupport, sourceInformationProvider, assemblyInfo, assemblyFileName, configFileName, shadowCopy, shadowCopyFolder, diagnosticMessageSink, verifyTestAssemblyExists)
        {
#if PLATFORM_DOTNET
            AssemblyName assemblyName = null;
            if (assemblyInfo != null)
            {
                assemblyName = new AssemblyName(Path.GetFileNameWithoutExtension(assemblyFileName));
                executor = Framework.GetExecutor(assemblyInfo);
            }
            else
            {
                var an = Assembly.Load(new AssemblyName { Name = Path.GetFileNameWithoutExtension(assemblyFileName) }).GetName();
                assemblyName = new AssemblyName { Name = an.Name, Version = an.Version };
                executor = Framework.GetExecutor(assemblyName);
            }
#else
            var assemblyName = AssemblyName.GetAssemblyName(assemblyFileName);
            executor = Framework.GetExecutor(assemblyName);
#endif
        }

        /// <inheritdoc/>
        public ITestCase Deserialize(string value)
        {
            return executor.Deserialize(value);
        }

        /// <inheritdoc/>
        public override sealed void Dispose()
        {
            executor.SafeDispose();

            base.Dispose();
        }

        /// <summary>
        /// Starts the process of running all the xUnit.net v2 tests in the assembly.
        /// </summary>
        /// <param name="messageSink">The message sink to report results back to.</param>
        /// <param name="discoveryOptions">The options to be used during test discovery.</param>
        /// <param name="executionOptions">The options to be used during test execution.</param>
        public void RunAll(IMessageSink messageSink, ITestFrameworkDiscoveryOptions discoveryOptions, ITestFrameworkExecutionOptions executionOptions)
        {
            executor.RunAll(messageSink, discoveryOptions, executionOptions);
        }

        /// <summary>
        /// Starts the process of running the selected xUnit.net v2 tests.
        /// </summary>
        /// <param name="testCases">The test cases to run; if null, all tests in the assembly are run.</param>
        /// <param name="messageSink">The message sink to report results back to.</param>
        /// <param name="executionOptions">The options to be used during test execution.</param>
        public void RunTests(IEnumerable<ITestCase> testCases, IMessageSink messageSink, ITestFrameworkExecutionOptions executionOptions)
        {
            executor.RunTests(testCases, messageSink, executionOptions);
        }
    }
}
