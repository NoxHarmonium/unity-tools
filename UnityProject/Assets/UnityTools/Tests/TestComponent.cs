using UnityEngine;
using System.Collections;
using System;

using NUnit.Framework;
using UnityTest;
using UnityTest.UnitTestRunner;

using UnityTools.Threading;

/// <summary>
/// Handles the test events
/// </summary>
public class TestRunnerCallback : ITestRunnerCallback {

	public TestRunnerCallback(Action<string> logAction, UnityTask parentTask) {
		_logAction = logAction;
		_parentTask = parentTask;
	}

	public void TestStarted (string fullName)
	{
		_logAction(string.Format("Test started: {0}", fullName));
	}

	public void TestFinished (ITestResult result)
	{
		_logAction(string.Format("Test finished: {0}. Elapsed: {1}s, Success: {2}, State: {3}", result.FullName, result.Duration, result.IsSuccess, result.ResultState));
		_testsRun++;
		if (result.IsSuccess || result.ResultState == TestResultState.Inconclusive) {
			_testsSucceeded++; 
		} else {
			_testsFailed++;
		}
	}

	public void RunStarted (string suiteName, int testCount)
	{
		_logAction(string.Format("Run started: suiteName: {0}, testCount: {1}", suiteName, testCount));
	}

	public void RunFinished ()
	{
		_logAction("Run finished");
		if (_testsFailed > 0) {
			_parentTask.Reject(new Exception("Not all tests passed."));
		} else {
			_parentTask.Resolve();
		}
	}

	public void RunFinishedException (System.Exception exception)
	{
		_logAction(string.Format("Run finished with exception ({0}) Stacktrace:\n{1}", exception.Message, exception.StackTrace));
		Debug.LogException(exception);
	}

	private Action<string> _logAction;
	private UnityTask _parentTask;
	private int _testsRun = 0;
	private int _testsSucceeded = 0;
	private int _testsFailed = 0;
}


/// <summary>
/// Runs the unit tests on device on application start.
/// </summary>
public class TestComponent : MonoBehaviour {


	void LogString (string s) {
		Debug.Log(s);
		_log = string.Format("{0:T} {1}\n", DateTime.Now, s) + _log;
	}

	// Use this for initialization
	void Start () {
		var testEngine = new NUnitBasicTestEngine ("UnityTools");
		
		var testTask = new UnityTask( (task) => {
			testEngine.RunTests(new TestRunnerCallback(LogString, task));
		});

		testTask.Then(
			onFulfilled: (o) => LogString("All tests succeeded. UnityTools works on this device!"), 
			onFailure: (ex) => LogString("Some tests failed. UnityTools does not work on this device.")
		);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI() {
		GUI.TextField(new Rect(0,0,Screen.width, Screen.height), _log); 
	}

	private string _log = "";
}
