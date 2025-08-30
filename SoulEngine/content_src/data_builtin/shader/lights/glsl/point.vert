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

uniform mat4 um_projection;
uniform mat4 um_view;
uniform mat4 um_model;

void main() {
    VertexData v = ib_vertices.vertices[gl_VertexID];
    
    gl_Position = um_projection * um_view * um_model * vec4(v.position, 1);
}