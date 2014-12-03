#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>

// material:
uniform sampler2D uColor;

uniform vec4 uColorModulation = vec4(1.0);
uniform float uVisibility = 1.0;

in vec2 vTexCoord;
in vec3 vNormal;
in vec3 vWorldPos;
in float vDisc;

OUTPUT_CHANNEL_Color(vec3)
OUTPUT_CHANNEL_Normal(vec3)
OUTPUT_CHANNEL_Position(vec3)

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
    const float translucency = 0.5;
    vec3 color = leafLighting(vWorldPos, normalFront, translucency, texColor.rgb, vec4(vec3(0),1));

    OUTPUT_Color(color);
    OUTPUT_Normal("Fix me!");
    OUTPUT_Position("Fix me!");
}
