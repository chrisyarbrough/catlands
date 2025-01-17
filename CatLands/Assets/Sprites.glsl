#version 330

in vec2 fragTexCoord;

uniform sampler2D texture0;



out vec4 finalColor;

// Requires filtering enabled on the texture.
vec4 texturePointSmooth(sampler2D tex, vec2 uv) {
    vec2 size = vec2(textureSize(tex, 0));
    vec2 pixel = vec2(1.0) / size;
    uv -= pixel * vec2(0.5);
    vec2 uv_pixels = uv * size;
    vec2 delta_pixel = fract(uv_pixels) - vec2(0.5);
    vec2 ddxy = fwidth(uv_pixels);
    vec2 mip = log2(ddxy) - 0.5;
    return textureLod(tex, uv + (clamp(delta_pixel / ddxy, 0.0, 1.0) - delta_pixel) * pixel, min(mip.x, mip.y));
}

void main()
{
    vec4 sampled = texturePointSmooth(texture0, fragTexCoord);
    sampled.rgb *= sampled.a;
    finalColor = sampled.rgba;
}