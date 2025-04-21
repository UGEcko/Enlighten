using System.Collections.Generic;
using Enlighten.Core;
using UnityEngine;
namespace Enlighten.UI
{
	internal class FloatParameterEditor : ChartParameterEditor<float>
	{
		protected override float ChartYPositionToValue(float y) => y;
		protected override float ValueToChartYPosition(float value) => value;

		protected override IEnumerable<Vector2> GetCurveLocalPoints(GenericParameter<float> parameter, GenericParameter<float>.Keyframe[] keyframes)
		{
			if (keyframes[0].m_time > 0)
				yield return SampleTime(0);

			for (int i = 0; i < keyframes.Length; i++)
			{
				GenericParameter<float>.Keyframe a = keyframes[i];
				yield return SamplePoint(a);

				if (i == keyframes.Length - 1)
					continue;

				GenericParameter<float>.Keyframe b = keyframes[i + 1];

				const int RESOLUTION = 10;
				for (int j = 1; j < RESOLUTION; j++)
				{
					float f = j / (float)RESOLUTION;
					float t = Mathf.Lerp(a.m_time, b.m_time, f);
					yield return SampleTime(t);
				}
			}

			if (keyframes[keyframes.Length - 1].m_time < 1)
				yield return SampleTime(1);

			yield break;

			Vector2 SampleTime(float t)
			{
				float value = parameter.Interpolate(t);
				float y = ValueToChartYPosition(value);
				Vector2 chartPosition = new Vector2(t, y);
				return ChartToLocalPosition(chartPosition);
			}

			Vector2 SamplePoint(GenericParameter<float>.Keyframe key)
			{
				float y = ValueToChartYPosition(key.m_value);
				Vector2 chartPosition = new Vector2(key.m_time, y);
				return ChartToLocalPosition(chartPosition);
			}
		}
	}
}
