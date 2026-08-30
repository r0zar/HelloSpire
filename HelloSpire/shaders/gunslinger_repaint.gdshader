// Gunslinger black-and-tan: repaints the Ironclad atlas at render time.
shader_type canvas_item;

vec3 rgb2hsv(vec3 c) {
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

void fragment() {
    vec4 tex = texture(TEXTURE, UV);
    vec3 hsv = rgb2hsv(tex.rgb);
    float hd = hsv.x * 360.0;
    bool red = hd < 25.0 || hd > 335.0;
    if (red && hsv.y > 0.45 && hsv.z > 0.55) {
        // rage glow -> ember orange
        hsv = vec3(30.0 / 360.0, hsv.y, min(1.0, hsv.z * 1.05));
    } else if ((hd < 22.0 || hd > 330.0) && hsv.y > 0.25) {
        // maroon cloth -> near-black duster
        hsv = vec3(25.0 / 360.0, hsv.y * 0.35, hsv.z * 0.38);
    } else if (hd >= 22.0 && hd < 52.0 && hsv.y > 0.28) {
        // leather -> pale tan
        hsv = vec3(38.0 / 360.0, hsv.y * 0.55, min(1.0, hsv.z * 1.3));
    }
    COLOR = vec4(hsv2rgb(hsv), tex.a);
}
