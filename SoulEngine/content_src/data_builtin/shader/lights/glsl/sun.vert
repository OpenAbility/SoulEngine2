struct VertexData {
    vec3 position;
    vec4 uv01;
    vec3 normal;
    vec3 tangent;
    vec4 colour;
};

layout(std430, binding=20) restrict readonly buffer ib_vertex_buffer {
    VertexData vertices[];
} ib_vertices;

out vec3 v_position;
out vec2 v_uv;
out vec2 v_uv2;
out vec3 v_normal;
out vec4 v_colour;

void main() {
    VertexData v = ib_vertices.vertices[gl_VertexID];
    
    gl_Position = vec4(v.position, 1.0);
    gl_Position.z = 0.0f;
    
    v_position = v.position;
    v_uv = v.uv01.xy;
    v_uv2 = v.uv01.zw;
    // We need to make sure all normals are applied world-space :D
    v_normal = v.normal;
    
    v_colour = v.colour;
}