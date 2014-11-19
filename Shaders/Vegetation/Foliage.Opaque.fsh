#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>
#pragma ACGLimport <Common/Camera.csh>
#pragma ACGLimport <Common/Normalmapping.fsh>

// material:
uniform sampler2D uColor;
uniform sampler2D uNormal;

uniform float uDiscardBias = 0.5;

in vec2 vTexCoord;
in vec3 vTangent;
in vec3 vNormal;
in vec3 vWorldPos;
in vec3 vColor;

OUTPUT_CHANNEL_Color(vec3)
OUTPUT_CHANNEL_Normal(vec3)

void main()
{
    vec4 texColor = texture(uColor, vTexCoord);
    
    float disc = uDiscardBias;
    disc = distance(vWorldPos, uCameraPosition);
    disc = (1-uDiscardBias) + 0.05-clamp(disc/50,0,1-uDiscardBias);

    if(texColor.a < disc)
        discard;

    texColor.rgb /= texColor.a + 0.001; // premultiplied alpha
    texColor.rgb *= vColor;

    vec3 normal = applyNormalmap(vNormal, vTangent, unpack8bitNormalmap(texture(uNormal, vTexCoord).rgb));

    vec3 color = leafLighting(vWorldPos, normal, 1.0, texColor.rgb, vec4(vec3(0),1));

    OUTPUT_Color(color);
    OUTPUT_Normal(normal);
}
