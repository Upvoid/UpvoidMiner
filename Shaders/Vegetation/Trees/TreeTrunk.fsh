#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>
#pragma ACGLimport <Common/Normalmapping.fsh>

uniform sampler2D uColor;
uniform sampler2D uNormal;

uniform float uRoughness = 0.5;
uniform float uFresnel = 1.3;
uniform float uGlossiness = 0.2;

uniform float uVisibility = 1.0;

in vec3 vNormal;
in vec3 vTangent;
in vec3 vWorldPos;
in vec2 vTexCoord;

OUTPUT_CHANNEL_GBuffer1(vec4)
OUTPUT_CHANNEL_GBuffer2(vec4)

float random(vec2 p)
{
  // e^pi, 2^sqrt(2)
  return fract(cos(dot(p, vec2(23.140693,2.665144)))*123456.0);
}

void main()
{
    INIT_CHANNELS;

   // Convention: uVisibility < 0: fading out (fading in otherwise)
    bool fadingIn = uVisibility > 0;

    if(fadingIn)
    {
      if(random(gl_FragCoord.xy) >= uVisibility)
        discard;
    }
    else
    {
      if(random(gl_FragCoord.xy) <= uVisibility + 1)
        discard;
    }

    // normalmap
    vec3 normal = applyNormalmap(vNormal, vTangent, unpack8bitNormalmap(texture(uNormal, vTexCoord).rgb));

    // illumination
    vec3 baseColor = texture(uColor, vTexCoord).rgb;

    vec4 gb1, gb2;
    writeGBuffer(
       baseColor.rgb,
       normal,
       uRoughness,
       uFresnel,
       uGlossiness,
       gb1, gb2
    );
    OUTPUT_GBuffer1(gb1);
    OUTPUT_GBuffer2(gb2);
}
