#version 330

uniform vec2 resolution;
uniform vec2 dir;
uniform vec2 fadeStart;
uniform vec2 fadeEnd;

in vec4 fragColor;
out vec4 finalColor;

void main()
{
    vec2 fragCoord = gl_FragCoord.xy;
    vec2 uv = fragCoord / resolution;
    uv *= 0.5;
    uv.y = 1.0 - uv.y;

    float distToStart = length(uv - fadeStart / resolution);
    float distToEnd = length(uv - fadeEnd / resolution);
    float fadeFactor = min(distToStart, distToEnd) / 0.1;
    fadeFactor = clamp(fadeFactor, 0.0, 1.0);
    
    vec2 dir = fadeEnd - fadeStart;

    // Rotate the UVs to align with the direction vector.
    float theta = atan(dir.y, dir.x);
    float cosTheta = cos(theta);
    float sinTheta = sin(theta);
    uv = vec2(
        cosTheta * uv.x + sinTheta * uv.y,
        sinTheta * uv.x - cosTheta * uv.y
    );

    float gradientWidth = 0.5;
    float stripePattern = sin(uv.x * resolution.x * 0.5);
    float alpha = smoothstep(-gradientWidth, gradientWidth, stripePattern);

    finalColor = vec4(fragColor.rgb, alpha * fadeFactor);
}