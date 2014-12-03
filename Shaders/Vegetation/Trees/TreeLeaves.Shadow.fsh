#version 140

// material:
uniform sampler2D uColor;

uniform float uShadowExp;
uniform float uVisibility = 1.0;

in vec2 vTexCoord;

in float vZ;

out float fShadow;

void main()
{
    vec4 texColor = texture(uColor, vTexCoord);

    if(texColor.a < 0.5)
        discard;
        
   float posFactor = uVisibility;
   if(int(13.479*gl_FragCoord.x + gl_FragCoord.y * 273.524 * gl_FragCoord.x) % 200 >= posFactor * 250)
      discard;

    float z = vZ;
    z = (z+1) / 2.0;

    fShadow = exp(uShadowExp * z);
}
