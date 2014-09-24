#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>
#pragma ACGLimport <Common/Camera.csh>
#pragma ACGLimport <Common/Normalmapping.fsh>

// material:
uniform sampler2D uColor;
uniform sampler2D uNormal;
uniform sampler2D uTranslucency;

uniform float uDiscardBias = 0.5;

in vec2 vTexCoord;
in vec3 vTangent;
in vec3 vNormal;
in vec3 vWorldPos;
in vec3 vColor;

OUTPUT_CHANNEL_TransparentColor(vec4)

void main()
{
    vec4 texColor = texture(uColor, vTexCoord);

    if(texColor.a > uDiscardBias)
        discard;

    texColor.rgb /= texColor.a + 0.001; // premultiplied alpha

    vec3 normalFront = vNormal;

    if(!gl_FrontFacing)
        normalFront = -normalFront;

    texColor.rgb *= vColor;

    // The vertex shader flips the normal so it always points towards the camera
    normalFront = applyNormalmap(normalFront, vTangent, unpack8bitNormalmap(texture(uNormal, vTexCoord).rgb));
    vec3 normalBack = applyNormalmap(-normalFront, vTangent, unpack8bitNormalmap(texture(uNormal, vTexCoord).rgb));

    // TODO(ks) only one shadow computation!
    vec3 colorFront = lighting(vWorldPos, normalFront, texColor.rgb, vec4(vec3(0),1));
    vec3 colorBack = lighting(vWorldPos, normalBack, texColor.rgb, vec4(vec3(0),1));

    vec3 translucency = texture(uTranslucency, vTexCoord).rgb;

    vec3 color = colorFront + translucency*colorBack;

    OUTPUT_TransparentColor(vec4(color, texColor.a));
}
