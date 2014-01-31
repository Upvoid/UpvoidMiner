#version 140

#include <Common/Lighting.fsh>
#include <Common/Camera.csh>

// material:
uniform sampler2D uColor;

uniform float uDiscardBias = 0.5;
uniform vec4 uColorModulation = vec4(1.0);

in vec2 vTexCoord;
in vec3 vNormal;
in vec3 vEyePos;

OUTPUT_CHANNEL_Color(vec3)
OUTPUT_CHANNEL_Normal(vec3)
OUTPUT_CHANNEL_Position(vec3)

void main()
{
    vec4 texColor = texture(uColor, vTexCoord);

    float disc = uDiscardBias;
    disc = -vEyePos.z;//distance(uCameraPosition, vec3(uInverseViewMatrix*vec4(vEyePos,1)));
    disc = 0.901-clamp(disc/100,0,0.9);

    if(texColor.a < disc)
        discard;

    texColor.rgb /= texColor.a + 0.001; // premultiplied alpha

    texColor.rgb *= uColorModulation.rgb;


    vec3 normalFront = mix(vNormal, -vNormal, float(!gl_FrontFacing));

    vec3 colorFront = lighting(vEyePos, normalFront, texColor.rgb, vec4(vec3(0),1));
    vec3 colorBack = lighting(vEyePos, -normalFront, texColor.rgb, vec4(vec3(0),1));

    const float translucency = 1.0;

    vec3 color = colorFront + translucency*colorBack;

    OUTPUT_Color(color);
    OUTPUT_Normal(normalFront);
    OUTPUT_Position(vEyePos);
}
