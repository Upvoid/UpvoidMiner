#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>
#pragma ACGLimport <Common/Normalmapping.fsh>

uniform sampler2D uColor;
uniform sampler2D uNormal;
uniform vec4 uSpecularColor;
uniform float uVisibility = 1.0;

in vec3 vNormal;
in vec3 vTangent;
in vec3 vWorldPos;
in vec2 vTexCoord;

OUTPUT_CHANNEL_Color(vec3)
OUTPUT_CHANNEL_Normal(vec3)
OUTPUT_CHANNEL_Position(vec3)

void main()
{
    INIT_CHANNELS;
    
   float posFactor = uVisibility;
   if(int(13.479*gl_FragCoord.x + gl_FragCoord.y * 273.524 * gl_FragCoord.x) % 200 >= posFactor * 250)
      discard;

    // normalmap
    vec3 normal = applyNormalmap(vNormal, vTangent, unpack8bitNormalmap(texture(uNormal, vTexCoord).rgb));

    // illumination
    vec3 baseColor = texture(uColor, vTexCoord).rgb;
    vec3 color = lighting(vWorldPos, normal, baseColor, uSpecularColor);

    OUTPUT_Color(color);
    OUTPUT_Normal(normal);
    OUTPUT_Position(vWorldPos);
}
