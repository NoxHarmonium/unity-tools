namespace UnityTest
{
	public interface IUnitBasicTestEngine 
	{
		void RunTests ( TestFilter filter, UnitTestRunner.ITestRunnerCallback testRunnerEventListener );
	}
}
