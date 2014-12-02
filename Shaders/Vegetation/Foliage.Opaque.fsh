#version 140
#pragma Pipeline

// material:
uniform sampler2D uColor;

in vec2 vTexCoord;
in vec3 vColor;
in float vDisc;

OUTPUT_CHANNEL_Color(vec3)
OUTPUT_CHANNEL_Normal(vec3)

void main()
{
   vec4 texColor = texture(uColor, vTexCoord);

   if(texColor.a < vDisc)
      discard;

   texColor.rgb /= texColor.a + 0.001; // premultiplied alpha
   texColor.rgb *= 0.5 + 0.5 * vColor;

   OUTPUT_Color(texColor.rgb);
   OUTPUT_Normal("Fix me!");
}
