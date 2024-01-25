namespace CatLands;

using JetBrains.Annotations;

[PublicAPI]
public struct ClampedFloat
{
	public float Value
	{
		get => value;
		set => this.value = Math.Clamp(value, Min, Max);
	}

	private float value;

	public readonly float Min;
	public readonly float Max;

	public ClampedFloat(float value, float min, float max)
	{
		this.value = Math.Clamp(value, min, max);
		Min = min;
		Max = max;
	}

	public static implicit operator float(ClampedFloat clampedFloat) => clampedFloat.Value;
}