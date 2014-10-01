using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Core;
using System.Linq;
using NUnit.Core.Filters;
using UnityEngine;

namespace UnityTest
{
	public class NUnitBasicTestEngine : IUnitBasicTestEngine
	{
		static string[] whitelistedAssemblies = 
		{
			"Assembly-CSharp-Editor", 
			"Assembly-Boo-Editor", 
			"Assembly-UnityScript-Editor"
		};
		private TestSuite testSuite;
		private string productName;


		public NUnitBasicTestEngine(string productName) {
			List<String> assemblies = GetAssembliesWithTests ().Select (a=>a.Location).ToList ();
			TestSuite suite = PrepareTestSuite (assemblies);
			testSuite = suite;
			this.productName = productName;
		}

		public void RunTests (UnitTestRunner.ITestRunnerCallback testRunnerEventListener)
		{
			RunTests (TestFilter.Empty, testRunnerEventListener);
		}
		
		public void RunTests ( TestFilter filter, UnitTestRunner.ITestRunnerCallback testRunnerEventListener )
		{
			try
			{
				if (testRunnerEventListener != null)
					testRunnerEventListener.RunStarted (testSuite.TestName.FullName, testSuite.TestCount);
				
				ExecuteTestSuite (testSuite, testRunnerEventListener, filter);
				
				if (testRunnerEventListener != null)
					testRunnerEventListener.RunFinished ();
			}
			catch (Exception e)
			{
				Debug.LogException (e);
				if (testRunnerEventListener != null)
					testRunnerEventListener.RunFinishedException (e);
			}
		}

		public static Assembly[] GetAssembliesWithTests ()
		{
			var libs = new List<Assembly> ();
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies ())
			{
				if(assembly.GetReferencedAssemblies ().All (a => a.Name != "nunit.framework")) continue;
				if (assembly.Location.Replace ('\\', '/').StartsWith (Application.dataPath)
				    || whitelistedAssemblies.Contains (assembly.GetName ().Name)) libs.Add (assembly);
			}
			return libs.ToArray ();
		}
		
		private TestSuite PrepareTestSuite(List<String> assemblyList)
		{
			CoreExtensions.Host.InitializeService();
			var testPackage = new TestPackage (productName, assemblyList);
			var builder = new TestSuiteBuilder();
			TestExecutionContext.CurrentContext.TestPackage = testPackage;
			TestSuite suite = builder.Build(testPackage);
			return suite;
		}
		
		private void ExecuteTestSuite ( TestSuite suite, UnitTestRunner.ITestRunnerCallback testRunnerEventListener, TestFilter filter )
		{
			EventListener eventListener;
			if (testRunnerEventListener == null)
				eventListener = new NullListener ();
			else
				eventListener = new TestRunnerEventListener (testRunnerEventListener);
			suite.Run(eventListener, GetFilter(filter));
		}
		
		private ITestFilter GetFilter (TestFilter filter)
		{
			var nUnitFilter = new AndFilter();
			
			if(filter.names != null && filter.names.Length>0)
				nUnitFilter.Add ( new SimpleNameFilter(filter.names));
			if (filter.categories != null && filter.categories.Length > 0)
				nUnitFilter.Add (new CategoryFilter(filter.categories));
			if (filter.objects != null && filter.objects.Length > 0)
				nUnitFilter.Add (new OrFilter (filter.objects.Where (o => o is TestName).Select(o => new NameFilter (o as TestName)).ToArray ()));
			return nUnitFilter;
		}
		
		public class TestRunnerEventListener : EventListener
		{
			private UnitTestRunner.ITestRunnerCallback testRunnerEventListener;
			
			public TestRunnerEventListener(UnitTestRunner.ITestRunnerCallback testRunnerEventListener)
			{
				this.testRunnerEventListener = testRunnerEventListener;
			}
			
			public void RunStarted(string name, int testCount)
			{
				testRunnerEventListener.RunStarted(name, testCount);
			}
			
			public void RunFinished(NUnit.Core.TestResult result)
			{
				testRunnerEventListener.RunFinished();
			}
			
			public void RunFinished(Exception exception)
			{
				testRunnerEventListener.RunFinishedException(exception);
			}
			
			public void TestStarted(NUnit.Core.TestName testName)
			{
				testRunnerEventListener.TestStarted(testName.FullName);
			}
			
			public void TestFinished(NUnit.Core.TestResult result)
			{
				testRunnerEventListener.TestFinished(result.UnitTestResult());
			}
			
			public void SuiteStarted(NUnit.Core.TestName testName)
			{
			}
			
			public void SuiteFinished(NUnit.Core.TestResult result)
			{
			}
			
			public void UnhandledException(Exception exception)
			{
			}
			
			public void TestOutput(NUnit.Core.TestOutput testOutput)
			{
			}
		}
	}
}
