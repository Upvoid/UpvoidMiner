#version 140
#pragma Pipeline

// material:
uniform sampler2D uColor;

uniform float uDiscardBias = 0.5;

in vec2 vTexCoord;
in vec3 vEyePos;

OUTPUT_CHANNEL_Color(vec3)

void main()
{
    vec4 texColor = texture(uColor, vTexCoord);

    float disc = uDiscardBias;
    disc = -vEyePos.z;
    disc = 0.901-clamp(disc/100,0,0.9);

    if(texColor.a < disc)
      discard;

    OUTPUT_Color(vec3(1));
}
