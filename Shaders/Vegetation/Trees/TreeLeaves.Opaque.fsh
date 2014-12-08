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

void main()
{
    vec4 texColor = texture(uColor, vTexCoord);

    if(texColor.a < vDisc)
        discard;

   float posFactor = uVisibility;
   if(int(13.479*gl_FragCoord.x + gl_FragCoord.y * 273.524 * gl_FragCoord.x) % 200 >= posFactor * 250)
      discard;

    texColor.rgb /= texColor.a + 0.001; // premultiplied alpha
    texColor.rgb *= uColorModulation.rgb;


    vec3 normalFront = mix(vNormal, -vNormal, float(!gl_FrontFacing));

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
