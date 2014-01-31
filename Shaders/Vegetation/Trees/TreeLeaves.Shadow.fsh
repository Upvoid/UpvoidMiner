#version 140

// material:
uniform sampler2D uColor;

uniform float uShadowExp;w

in vec2 vTexCoord;
in vec3 vEyePos;

in float vZ;

out float fShadow;

void main()
{
    vec4 texColor = texture(uColor, vTexCoord);

    if(texColor.a < 0.5)
        discard;

    float z = vZ;
    z = (z+1) / 2.0;

    fShadow = exp(uShadowExp * z);
}
