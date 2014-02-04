#version 140

#include <Common/Lighting.fsh>
#include <Common/Normalmapping.fsh>
#include <Common/Camera.csh>

uniform sampler2D uColor;
uniform sampler2D uNormal;

uniform vec4 uSpecularColor;

uniform float uTexScale;
uniform float uBlendingCoefficient;
uniform float uColorModulation;

uniform float uBlackness = 1.0;

//uniform vec3 uRefNormal = vec3(0,1,0);

uniform mat4 uModelMatrix;

in vec3 vColor;
in vec3 vEyePos;
in vec3 vObjectPos;
in vec3 vObjectNormal;
in mat3 vNormalMatrix;
in float vOffset;

//OUTPUT_CHANNEL_Color(vec3)
//OUTPUT_CHANNEL_Normal(vec3)
//OUTPUT_CHANNEL_Position(vec3)

OUTPUT_CHANNEL_TransparentColor(vec4)

void main()
{
    INIT_CHANNELS;

    vec3 baseColor = vec3(0);
    vec3 normal = vec3(0);

    // texturing
    vec4 texColor = texture(uColor, (vObjectPos.xz) / (uTexScale * 2.0)).rgba;

    //texColor.rgb /= texColor.a + 0.005;

    const float maxOffset = 1;
    const float maxOffsetInv = 1.0/maxOffset;
    float p = clamp(vOffset * maxOffsetInv, 0, 1);

    /*if(texColor.a < mix(0.9, 0.5, p))
        discard;*/

    // normal mapping
    vec3 normalMap = unpack8bitNormalmap(texture(uNormal, vObjectPos.xz / (uTexScale * 11.0)).rgb);

    vec3 tangent = cross(vObjectNormal, vec3(1,0,0));

    normal = vObjectNormal;//applyNormalmap(vObjectNormal, tangent, normalMap);

    // color modulation
    /*if ( uColorModulation > 0.5 )
        baseColor *= vColor;*/

    baseColor = texColor.rgb;
    //baseColor = vec3(0.2, 1.0, 0.7);

    // illumination
    //normal = normalize(mat3(uModelMatrix) * normal);
    vec3 color = lighting(vEyePos, normal, baseColor, uSpecularColor);

    OUTPUT_TransparentColor(vec4(color, 0.5*p*texColor.a));

    //OUTPUT_Color(color);
    //OUTPUT_Color(vec3(1-vOffset*10, vOffset*10, 0));
    //OUTPUT_Normal(normal);
    //OUTPUT_Position(vEyePos);
}

