#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>
#pragma ACGLimport <Common/Camera.csh>

// material:
uniform sampler2D uColor;

uniform float uDiscardBias = 0.5;

in vec2 vTexCoord;
in vec3 vNormal;
in vec3 vWorldPos;
in vec3 vColor;

OUTPUT_CHANNEL_Color(vec3)
OUTPUT_CHANNEL_Normal(vec3)
OUTPUT_CHANNEL_Position(vec3)

void main()
{
    vec4 texColor = texture(uColor, vTexCoord);

    float disc = uDiscardBias;
    disc = distance(vWorldPos, uCameraPosition);
    disc = 0.901-clamp(disc/100,0,0.9);

    if(texColor.a < disc)
        discard;

    texColor.rgb /= texColor.a + 0.001; // premultiplied alpha
    texColor.rgb *= vColor;

    vec3 color = leafLighting(vWorldPos, vNormal, 1.0, texColor.rgb, vec4(vec3(0),1));

    OUTPUT_Color(color);
    OUTPUT_Normal(vNormal);
    OUTPUT_Position(vWorldPos);
}
