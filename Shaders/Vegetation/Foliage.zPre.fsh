#version 140
#pragma Pipeline

// material:
uniform sampler2D uColor;

uniform float uDiscardBias = 0.5;

in vec2 vTexCoord;

OUTPUT_CHANNEL_Color(vec3)

void main()
{
    vec4 texColor = texture(uColor, vTexCoord);

    if(texColor.a < uDiscardBias)
        discard;

    vec3 color = vec3(1);

    OUTPUT_Color(color);
}
