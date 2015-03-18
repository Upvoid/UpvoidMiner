#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>

// material:
uniform sampler2D uColor;

uniform float uRoughness = 0.5;
uniform float uFresnel = 1.3;
uniform float uGlossiness = 0.2;

uniform vec4 uColorModulation = vec4(1.0);
uniform float uVisibility = 1.0;

in vec2 vTexCoord;
in vec3 vNormal;
in vec3 vWorldPos;
in float vDisc;

OUTPUT_CHANNEL_GBuffer1(vec4)
OUTPUT_CHANNEL_GBuffer2(vec4)

float random(vec2 p)
{
  // e^pi, 2^sqrt(2)
  return fract(cos(dot(p, vec2(23.140693,2.665144)))*123456.0);
}

void main()
{
    vec4 texColor = texture(uColor, vTexCoord);

    if(texColor.a < vDisc)
        discard;

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

    texColor.rgb /= texColor.a + 0.001; // premultiplied alpha
    texColor.rgb *= uColorModulation.rgb;


    vec3 normalFront = vNormal * sign(dot(uSunDirection, vNormal));

    vec4 gb1, gb2;
    writeGBuffer(
       texColor.rgb,
       normalize(normalFront),
       uRoughness,
       uFresnel,
       uGlossiness,
       gb1, gb2
    );
    OUTPUT_GBuffer1(gb1);
    OUTPUT_GBuffer2(gb2);
}
