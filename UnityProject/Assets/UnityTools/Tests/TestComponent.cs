using UnityEngine;
using System.Collections;

using NUnit.Framework;
using UnityTest;
using UnityTest.UnitTestRunner;

/// <summary>
/// Handles the test events
/// </summary>
public class TestRunnerCallback : ITestRunnerCallback {

	public void TestStarted (string fullName)
	{
		Debug.Log(string.Format("Test started: {0}", fullName));
	}

	public void TestFinished (ITestResult result)
	{
		Debug.Log(string.Format("Test finished: {0}. Elapsed: {1}s, Success: {2}, State: {3},  \n", result.FullName, result.Duration, result.IsSuccess, result.ResultState));
	}

	public void RunStarted (string suiteName, int testCount)
	{
		Debug.Log(string.Format("Run started: suiteName: {0}, testCount: {1}", suiteName, testCount));
	}

	public void RunFinished ()
	{
		Debug.Log("Run finished");
	}

	public void RunFinishedException (System.Exception exception)
	{
		Debug.Log("Run finished with exception (see below)");
		Debug.LogException(exception);
	}
}


/// <summary>
/// Runs the unit tests on device on application start.
/// </summary>
public class TestComponent : MonoBehaviour {

	// Use this for initialization
	void Start () {
		var testEngine = new NUnitBasicTestEngine ();
		testEngine.RunTests(new TestRunnerCallback());
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
