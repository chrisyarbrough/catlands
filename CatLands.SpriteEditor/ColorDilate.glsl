#version 330

in vec2 fragTexCoord;
out vec4 fragColor;

uniform sampler2D textureSampler;

void main()
{
    vec2 size = vec2(textureSize(textureSampler, 0));
    vec2 delta = vec2(1.0) / size;
    vec4 col = texture(textureSampler, fragTexCoord);
    if (col.a > 0)
    {
        fragColor = col;
        return;
    }

    vec4 c0 = texture(textureSampler, fragTexCoord + vec2(0, delta.y));
    vec4 c1 = texture(textureSampler, fragTexCoord + vec2(delta.x, 0));
    vec4 c2 = texture(textureSampler, fragTexCoord + vec2(0, -delta.y));
    vec4 c3 = texture(textureSampler, fragTexCoord + vec2(-delta.x, 0));

    vec4 a = vec4(ceil(c0.a),
                  ceil(c1.a),
                  ceil(c2.a),
                  ceil(c3.a));

    float sum = a.x +
                a.y +
                a.z +
                a.w;

    if (sum <= 0)
    {
        fragColor = col;
        return;
    }

    col.rgb = ( a.x * c0.rgb +
                a.y * c1.rgb +
                a.z * c2.rgb +
                a.w * c3.rgb )
                / sum;

    col.a = 1.0;
    fragColor = col;
}