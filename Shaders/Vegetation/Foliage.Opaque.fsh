#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>

// material:
uniform sampler2D uColor;

uniform float uRoughness = 0.5;
uniform float uFresnel = 1.3;
uniform float uGlossiness = 0.2;

in vec2 vTexCoord;
in vec3 vColor;
in float vDisc;
in vec3 vNormal;

OUTPUT_CHANNEL_GBuffer1(vec4)
OUTPUT_CHANNEL_GBuffer2(vec4)

void main()
{
   vec4 texColor = texture(uColor, vTexCoord);

   if(texColor.a < vDisc)
      discard;

   texColor.rgb /= texColor.a + 0.001; // premultiplied alpha
   texColor.rgb *= 0.5 + 0.5 * vColor;


   vec4 gb1, gb2;
   writeGBuffer(
      texColor.rgb,
      normalize(vNormal),
      uRoughness,
      uFresnel,
      uGlossiness,
      gb1, gb2
   );
   OUTPUT_GBuffer1(gb1);
   OUTPUT_GBuffer2(gb2);
}
