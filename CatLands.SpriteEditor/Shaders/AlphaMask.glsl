#version 330

in vec2 fragTexCoord;
out vec4 fragColor;

uniform sampler2D textureSampler;

void main()
{
    vec4 col = texture(textureSampler, fragTexCoord);
    fragColor = col.a > 0 ? vec4(1.0) : vec4(0.0);
}