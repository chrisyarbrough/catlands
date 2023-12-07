#version 330

in vec2 fragTexCoord;

uniform sampler2D texture0;
uniform float sharpness = 300.0;

out vec4 frag_color;

float sharpen(float pix_coord) {
    float norm = (fract(pix_coord) - 0.5) * 2.0;
    float norm2 = norm * norm;
    return floor(pix_coord) + norm * pow(norm2, sharpness) / 2.0 + 0.5;
}

void main() {

    vec2 vres = textureSize(texture0, 0);
    frag_color = texture(texture0, vec2(
        sharpen(fragTexCoord.x * vres.x) / vres.x,
        sharpen(fragTexCoord.y * vres.y) / vres.y
    ));

    // To visualize how this makes the grid:
    // frag_color = vec4(
    //     fract(sharpen(fragTexCoord.x * vres.x)),
    //     fract(sharpen(fragTexCoord.y * vres.y)),
    //     0.5, 1.0
    // );
}