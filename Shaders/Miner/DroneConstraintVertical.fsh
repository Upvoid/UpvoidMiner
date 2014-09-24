#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>
#pragma ACGLimport <Common/Particles.fsh>

uniform sampler2D uPattern;
uniform sampler2D uNoise;
uniform vec4 uColor1 = vec4(1);
uniform vec4 uColor2 = vec4(0);
uniform float uScaleY;
uniform float uNoiseScale;
uniform float uSpeed;
uniform float uRepX;
uniform float uOffsetY;
uniform float uHeight;

in vec3 vObjPos;
in vec3 vWorldPos;
in vec4 vScreenPos;

in vec3 vRef1;
in vec3 vRef2;
in float vScaleX;

uniform sampler2DRect uOpaqueDepth;
OUTPUT_CHANNEL_TransparentColor(vec4)

void main()
{
    INIT_CHANNELS;

    vec4 transColor = vec4(0);

    // hexagrid
    float disY = vObjPos.y * uScaleY;
    float disX = distance(vWorldPos.xz, vRef1.xz) * vScaleX / uRepX;

    vec2 texCoord = vec2(disX, disY + uOffsetY);
    vec4 grid = texture(uPattern, vec2(disX, disY + uOffsetY));

    vec3 color1 = uColor1.rgb;
    vec3 color2 = uColor2.rgb;

    transColor = vec4(color1,1 * smoothstep(.3, .6, grid.a));

    transColor = mix(transColor, vec4(color2, 1), smoothstep(.5, .8, grid.a));

    // vertical fade-out

    vec4 roughNoise = texture(uNoise, texCoord * uNoiseScale + vec2(0, uSpeed * uRuntime * sign(vObjPos.y)));
    float heightModify = 1 - smoothstep(.3, 1.0, abs(disY / uHeight) + roughNoise.x * .7 - .2);
    heightModify += 1 - softParticleFactor(vScreenPos, uOpaqueDepth, 1.0);
    transColor.a *= clamp(heightModify, 0, 1);

    transColor.a *= .5;

    OUTPUT_TransparentColor(transColor);
}
