namespace UnityTest
{
	public interface IUnitTestEngine : IUnitBasicTestEngine
	{
		UnitTestRendererLine GetTests (out UnitTestResult[] results, out string[] categories);
	}
}