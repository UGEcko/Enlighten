namespace Enlighten.UI
{
    internal class BoolParameterEditor : ChartParameterEditor<bool>
    {
        protected override bool ChartYPositionToValue(float y) => y > 0.5f;
        protected override float ValueToChartYPosition(bool value) => value ? 1 : 0;
    }
}
