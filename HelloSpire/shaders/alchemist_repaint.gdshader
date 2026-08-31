// Alchemist browns: repaints the SILENT's spine atlas at render time.
// Sage/dark-green cloak and hood -> brown leathers; chartreuse slash vfx -> amber;
// teal daggers and tan bandage wraps are left alone.
shader_type canvas_item;

uniform sampler2D weapon_page : filter_linear;

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
    vec2 ts = vec2(textureSize(TEXTURE, 0));
    if (abs(ts.x - 618.0) < 0.5 && abs(ts.y - 523.0) < 0.5) {
        vec2 px = UV * ts;
        if (px.x >= 389.0 && px.x < 463.0 && px.y >= 74.0 && px.y < 112.0) {
            tex = texture(weapon_page, UV);   // back hand: dagger becomes a potion
        } else if (px.x >= 389.0 && px.x < 457.0 && px.y >= 114.0 && px.y < 173.0) {
            tex = vec4(0.0);                  // front hand: no blade, just the fist
        }
    }
    vec3 hsv = rgb2hsv(tex.rgb);
    float hd = hsv.x * 360.0;
    if (hd >= 55.0 && hd < 85.0 && hsv.y > 0.55 && hsv.z > 0.55) {
        // chartreuse slash vfx -> amber
        hsv = vec3(40.0 / 360.0, hsv.y, min(1.0, hsv.z * 1.05));
    } else if (hd >= 80.0 && hd < 175.0 && hsv.y > 0.18) {
        // sage and dark green cloth -> brown leather
        hsv = vec3(28.0 / 360.0, min(1.0, hsv.y * 0.85), hsv.z);
    }
    COLOR = vec4(hsv2rgb(hsv), tex.a);
}
