// Author: 
// Title: 

#ifdef GL_ES
precision mediump float;
#endif

uniform vec2 u_resolution;
uniform vec3 u_mouse;
uniform vec3 primary;
uniform vec3 secondary;
uniform vec3 primaryScaling;
uniform float u_time;
uniform sampler2D u_tex0;
uniform sampler2D u_tex1;

void main() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;  

    vec3 color = vec3(0.0, 0.0, 0.0);
    
    vec3 sample = texture2D(u_tex1, st * vec2(1, 0.5) + vec2(0, mod(u_time * 0.2, 0.5))).xyz;
    
    color.r += max(0.0, (sample.r + (st.y - 1.0) * 1.0) * 0.6);
    color.g += max(0.0, (sample.r + (st.y - 1.0) * 1.0) * 0.8);
    color.b += max(0.0, (sample.r + (st.y - 1.0) * 1.0) * 1.0);
    
    vec3 sample2 = texture2D(u_tex1, st * vec2(1.0, 0.5) + vec2(0.0, mod(u_time * 0.1, 0.5))).xyz;   
    
    color.r += max(0.0, (sample2.r + (st.y - 0.7)) * 0.3);
    color.g += max(0.0, (sample2.r + (st.y - 0.7)) * 0.3);
    color.b += max(0.0, (sample2.r + (st.y - 0.7)) * 0.9);
    
    color *= texture2D(u_tex0, st).xyz;

    gl_FragColor = vec4(color,1.0);
}
