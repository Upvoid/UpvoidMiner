#version 140

// material:
uniform sampler2D uColor;

uniform float uShadowExp;

uniform float uDiscardBias = 0.5;

in vec2 vTexCoord;
in vec3 vEyePos;

in float vZ;

out float fShadow;

void main()
{
    vec4 texColor = texture(uColor, vTexCoord);

    float disc = uDiscardBias;
    disc = -vEyePos.z;
    disc = 0.901-clamp(disc/100,0,0.9);

    if(texColor.a < disc)
        discard;

    float z = vZ;
    z = (z+1) / 2.0;

    fShadow = exp(uShadowExp * z);
}
