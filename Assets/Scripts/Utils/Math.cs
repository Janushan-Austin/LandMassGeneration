using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
	public static class Math
	{
		public static float Map(float value, float orgMin, float orgMax, float newMin, float newMax)
		{
			return (value - orgMin) * (newMax - newMin) / (orgMax - orgMin) + newMin;
		}

	}
}

