// Volatile Potions: derives a purple outline from the potion's own icon alpha edge, instead of
// requiring a dedicated white-silhouette asset per potion (see PotionModel.Outline). Applied to
// the belt's Outline layer only when VolatilePotionOutlinePatch decides a Potion is Volatile.
shader_type canvas_item;

uniform vec4 outline_color : source_color = vec4(0.62, 0.35, 0.98, 1.0);
uniform float outline_width : hint_range(0.0, 4.0) = 1.5;

void fragment() {
    vec4 tex = texture(TEXTURE, UV);
    if (tex.a > 0.5) {
        COLOR = vec4(0.0);
        return;
    }

    vec2 px = outline_width * TEXTURE_PIXEL_SIZE;
    float alpha = 0.0;
    alpha = max(alpha, texture(TEXTURE, UV + vec2(px.x, 0.0)).a);
    alpha = max(alpha, texture(TEXTURE, UV + vec2(-px.x, 0.0)).a);
    alpha = max(alpha, texture(TEXTURE, UV + vec2(0.0, px.y)).a);
    alpha = max(alpha, texture(TEXTURE, UV + vec2(0.0, -px.y)).a);
    alpha = max(alpha, texture(TEXTURE, UV + vec2(px.x, px.y)).a);
    alpha = max(alpha, texture(TEXTURE, UV + vec2(-px.x, px.y)).a);
    alpha = max(alpha, texture(TEXTURE, UV + vec2(px.x, -px.y)).a);
    alpha = max(alpha, texture(TEXTURE, UV + vec2(-px.x, -px.y)).a);

    COLOR = vec4(outline_color.rgb, alpha * outline_color.a);
}
