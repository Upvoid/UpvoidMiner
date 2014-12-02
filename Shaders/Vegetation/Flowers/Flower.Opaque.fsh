#version 140
#pragma Pipeline

// material:
uniform sampler2DArray uColor;

in vec2 vTexCoord;
in float vTexUnit;
in float vDisc;

OUTPUT_CHANNEL_Color(vec3)
OUTPUT_CHANNEL_Normal(vec3)

void main()
{ 
   vec4 texColor = texture(uColor, vec3(vTexCoord, vTexUnit));

   if(texColor.a < vDisc)
     discard;

   texColor.rgb /= texColor.a + 0.001; // premultiplied alpha

   OUTPUT_Color(texColor.rgb);
   OUTPUT_Normal("Fix me!");
}
