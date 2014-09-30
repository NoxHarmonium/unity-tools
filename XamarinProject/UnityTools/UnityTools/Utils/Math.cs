using System;

namespace UnityTools.Utils
{
	public class Math
	{
		// Thanks: http://stackoverflow.com/a/3875619/1153203
		static public bool NearlyEqual(double a, double b, double epsilon = 0.000001f)
		{
		    double absA = System.Math.Abs(a);
			double absB = System.Math.Abs(b);
			double diff = System.Math.Abs(a - b);

		    if (a == b)
		    { // shortcut, handles infinities
		        return true;
		    } 
		    else if (a == 0 || b == 0 || diff < Double.MinValue) 
		    {
		        // a or b is zero or both are extremely close to it
		        // relative error is less meaningful here
		        return diff < (epsilon * Double.MinValue);
		    }
		    else
		    { // use relative error
		        return diff / (absA + absB) < epsilon;
		    }
		}

		// Thanks: http://stackoverflow.com/a/3875619/1153203
		static public bool NearlyEqual(float a, float b, double epsilon = 0.000001)
		{
			double absA = System.Math.Abs(a);
			double absB = System.Math.Abs(b);
			double diff = System.Math.Abs(a - b);

		    if (a == b)
		    { // shortcut, handles infinities
		        return true;
		    } 
			else if (a == 0 || b == 0 || diff < float.MinValue) 
		    {
		        // a or b is zero or both are extremely close to it
		        // relative error is less meaningful here
				return diff < (epsilon * float.MinValue);
		    }
		    else
		    { // use relative error
		        return diff / (absA + absB) < epsilon;
		    }
		}
	}
}

