#version 140

uniform float uShadowExp;

in float vZ;

out float fShadow;

void main()
{
    float z = vZ;

    z = (z+1) / 2.0;

    z = clamp(z, 0, 1);

    fShadow = exp(uShadowExp * z);// / 5000;
}
