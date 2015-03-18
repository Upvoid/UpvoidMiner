#version 140

// material:
uniform sampler2D uColor;

uniform float uShadowExp;
uniform float uVisibility = 1.0;

in vec2 vTexCoord;

in float vZ;

out float fShadow;

float random(vec2 p)
{
  // e^pi, 2^sqrt(2)
  return fract(cos(dot(p, vec2(23.140693,2.665144)))*123456.0);
}

void main()
{
    vec4 texColor = texture(uColor, vTexCoord);

    if(texColor.a < 0.5)
        discard;
        
   // Convention: uVisibility < 0: fading out (fading in otherwise)
    bool fadingIn = uVisibility > 0;

    if(fadingIn)
    {
      if(random(gl_FragCoord.xy) >= uVisibility)
        discard;
    }
    else
    {
      if(random(gl_FragCoord.xy) <= uVisibility + 1)
        discard;
    }

    float z = vZ;
    z = (z+1) / 2.0;

    fShadow = exp(uShadowExp * z);
}
