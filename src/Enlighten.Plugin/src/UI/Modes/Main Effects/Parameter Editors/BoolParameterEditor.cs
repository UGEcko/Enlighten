using System.Collections.Generic;
using Enlighten.Core;
using UnityEngine;

namespace Enlighten.UI
{
    internal class BoolParameterEditor : ChartParameterEditor<bool>
    {
        protected override bool ChartYPositionToValue(float y) => y > 0.5f;
        protected override float ValueToChartYPosition(bool value) => value ? 1 : 0;

        protected override IEnumerable<Vector2> GetCurveLocalPoints(GenericParameter<bool> parameter, GenericParameter<bool>.Keyframe[] keyframes)
        {
            bool lastValue = keyframes[0].m_value;

            Vector2 firstChartPosition = new Vector2(0, ValueToChartYPosition(lastValue));
            yield return ChartToLocalPosition(firstChartPosition);

            foreach (GenericParameter<bool>.Keyframe keyframe in keyframes)
            {
                if (keyframe.m_value == lastValue)
                    continue;
                
                float lastY = ValueToChartYPosition(lastValue);
                float y = ValueToChartYPosition(keyframe.m_value);
                lastValue = keyframe.m_value;

                yield return ChartToLocalPosition(new Vector2(keyframe.m_time, lastY));
                yield return ChartToLocalPosition(new Vector2(keyframe.m_time, y));
            }
            
            Vector2 lastChartPosition = new Vector2(1, ValueToChartYPosition(lastValue));
            yield return ChartToLocalPosition(lastChartPosition);
        }
    }
}
