#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>
#pragma ACGLimport <Common/Particles.fsh>

uniform sampler3D uColor;

uniform sampler2DRect uOpaqueDepth;

in vec2 vTexCoord;
in vec3 vWorldPos;
in vec3 vEyePos;
in vec4 vScreenPos;
in vec3 iWorldPos;
in float quadID;


INPUT_CHANNEL_TransparentColor(vec4)
INPUT_CHANNEL_Distortion(vec2)
INPUT_CHANNEL_Color(vec3)
INPUT_CHANNEL_Depth(float)
OUTPUT_CHANNEL_Color(vec3)

void main()
{
    INIT_CHANNELS;

    float texUnit = fract(quadID + uRuntime);

    // illumination
    vec4 baseColor = texture(uColor, vec3(vTexCoord, texUnit));
    //baseColor.r = quadID;
    //baseColor.a *= 1.0 - 2*distance(vTexCoord.xy, vec2(0.5));
    baseColor.rgb /= baseColor.a + 0.01; // premultiplied alpha
    vec4 transColor = baseColor;

    // get current depth
    float depth = texture(uOpaqueDepth, gl_FragCoord.xy).r;
    // get (underlying) opaque fragment position in eye-space
    vec4 opaqueEyePos = uInverseProjectionMatrix * vec4(vScreenPos.xy, depth * 2 - 1, 1.0);
    opaqueEyePos /= opaqueEyePos.w;

    float dist = vEyePos.z - opaqueEyePos.z;

    transColor.a *= smoothstep(0, .1, dist);

    vec3 color = transColor.rgb * transColor.a;
    //transColor.rgba = vec4(vLife, 0.0, 0.0, 1.0);
    OUTPUT_Color(color);
}
