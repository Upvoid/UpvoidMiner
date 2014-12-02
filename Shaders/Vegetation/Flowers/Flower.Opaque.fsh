#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>
#pragma ACGLimport <Common/Camera.csh>
#pragma ACGLimport <Common/Normalmapping.fsh>

// material:
uniform sampler2DArray uColor;
uniform sampler2D uNormal;

in vec2 vTexCoord;
in vec3 vWorldPos;
in vec3 vNormal;
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

    vec3 color = texColor.rgb;

    OUTPUT_Color(color);
    OUTPUT_Normal(vNormal);
}
