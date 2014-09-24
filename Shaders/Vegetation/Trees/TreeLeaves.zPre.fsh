#version 140
#pragma Pipeline

#pragma ACGLimport  <Common/Camera.csh>

// material:
uniform sampler2D uColor;

uniform float uDiscardBias = 0.5;

in vec2 vTexCoord;
in vec3 vWorldPos;

OUTPUT_CHANNEL_Color(vec3)

void main()
{
    vec4 texColor = texture(uColor, vTexCoord);

    float disc = uDiscardBias;
    disc = distance(vWorldPos, uCameraPosition);
    disc = 0.901-clamp(disc/100,0,0.9);

    if(texColor.a < disc)
      discard;

    OUTPUT_Color(vec3(1));
}
