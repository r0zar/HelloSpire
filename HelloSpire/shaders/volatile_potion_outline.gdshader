// Volatile Potions: derives a purple outline from the potion's own icon alpha edge, instead of
// requiring a dedicated white-silhouette asset per potion (see PotionModel.Outline). Applied to
// the belt's Outline layer only when VolatilePotionOutlinePatch decides a Potion is Volatile.
shader_type canvas_item;

uniform vec4 outline_color : source_color = vec4(0.62, 0.35, 0.98, 1.0);
uniform float outline_width : hint_range(0.0, 4.0) = 1.5;

void fragment() {
    // Godot's shading language doesn't allow an early `return` inside fragment() -- confirmed
    // live via a real SHADER ERROR ("Using 'return' in the 'fragment' processor function is
    // incorrect"), which silently failed the whole shader and left every Volatile Potion with no
    // outline at all. Restructured as a single expression instead of an early exit.
    vec4 tex = texture(TEXTURE, UV);
    vec2 px = outline_width * TEXTURE_PIXEL_SIZE;
    float edgeAlpha = 0.0;
    edgeAlpha = max(edgeAlpha, texture(TEXTURE, UV + vec2(px.x, 0.0)).a);
    edgeAlpha = max(edgeAlpha, texture(TEXTURE, UV + vec2(-px.x, 0.0)).a);
    edgeAlpha = max(edgeAlpha, texture(TEXTURE, UV + vec2(0.0, px.y)).a);
    edgeAlpha = max(edgeAlpha, texture(TEXTURE, UV + vec2(0.0, -px.y)).a);
    edgeAlpha = max(edgeAlpha, texture(TEXTURE, UV + vec2(px.x, px.y)).a);
    edgeAlpha = max(edgeAlpha, texture(TEXTURE, UV + vec2(-px.x, px.y)).a);
    edgeAlpha = max(edgeAlpha, texture(TEXTURE, UV + vec2(px.x, -px.y)).a);
    edgeAlpha = max(edgeAlpha, texture(TEXTURE, UV + vec2(-px.x, -px.y)).a);

    // Inside the sprite (tex.a > 0.5): fully transparent, so only the traced edge shows.
    // Outside it: the traced edge alpha, tinted purple.
    float outlineAlpha = mix(edgeAlpha, 0.0, step(0.5, tex.a));
    COLOR = vec4(outline_color.rgb, outlineAlpha * outline_color.a);
}
