#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>
#pragma ACGLimport <Common/Camera.csh>

// material:
uniform sampler2D uColor;

uniform float uDiscardBias = 0.5;

in vec2 vTexCoord;
in vec3 vNormal;
in vec3 vWorldPos;
in vec3 vColor;
in vec3 vObjPos;
in vec3 vInstNormal;

OUTPUT_CHANNEL_Color(vec3)
OUTPUT_CHANNEL_Normal(vec3)
OUTPUT_CHANNEL_Position(vec3)

void main()
{
    vec4 texColor = texture(uColor, vTexCoord);

    float disc = uDiscardBias;
    disc = distance(vWorldPos, uCameraPosition);
    disc = 0.901-clamp(disc/100,0,0.9);

    if(texColor.a < disc)
        discard;

    texColor.rgb /= texColor.a + 0.001; // premultiplied alpha
    texColor.rgb *= vColor;

    vec3 normal = normalize(vNormal + vInstNormal * 1.9);

    vec3 color = leafLighting(vWorldPos, normal, 1.0, texColor.rgb, vec4(vec3(0),1));

    //float gmod = smoothstep(0.0, 0.6, vObjPos.y) * 0.7 + 0.3;
    //color *= gmod;

    //color = normal * 0.5 + 0.5;
    //color = vInstNormal;

    OUTPUT_Color(color);
    OUTPUT_Normal(vNormal);
    OUTPUT_Position(vWorldPos);
}
