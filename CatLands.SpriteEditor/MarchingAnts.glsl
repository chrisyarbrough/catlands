#version 330

uniform float TimeInSeconds;
out vec4 FragColor;

#define COUNT 16.0
#define SPEED 4.0

float sdBox(vec2 position, vec2 halfSize) {
    position = abs(position) - halfSize;
    return max(position.x, position.y);
}

vec3 diagonals(vec2 pos) {
    int w = 0;
    float time = SPEED * 10.0 * -TimeInSeconds;
    w = int(pos.x - pos.y + time) & int(COUNT);
    return vec3(w, w, w);
}


void main() {
    vec2 fragCoord = gl_FragCoord.xy;
    vec3 col = diagonals(fragCoord);
    FragColor = vec4(col.r, col.g, col.b, 1.0);
}
