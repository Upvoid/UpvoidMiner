#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>

// material:
uniform sampler2DArray uColor;

uniform float uRoughness = 0.5;
uniform float uFresnel = 1.3;
uniform float uGlossiness = 0.5;

in vec2 vTexCoord;
in vec3 vNormal;
in float vTexUnit;
in float vDisc;

OUTPUT_CHANNEL_GBuffer1(vec4)
OUTPUT_CHANNEL_GBuffer2(vec4)

void main()
{
   vec4 texColor = texture(uColor, vec3(vTexCoord, vTexUnit));

   if(texColor.a < vDisc)
     discard;

   texColor.rgb /= texColor.a + 0.001; // premultiplied alpha

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
