in vec3 a_position;
in vec2 a_uv;
in vec2 a_uv2;
in vec3 a_normal;
in vec4 a_colour;

in uvec4 a_indices;
in vec4 a_weights;

out vec3 v_position;
out vec2 v_uv;
out vec2 v_uv2;
out vec3 v_normal;
out vec4 v_colour;

uniform mat4 um_projection;
uniform mat4 um_view;
uniform mat4 um_model;

uniform bool ub_skeleton;

layout(std430, binding=10) buffer um_joint_buffer
{
    mat4 data[];
} um_joints;

void main() {
    // My IDE yells at me if I don't do the cast.
    // Dumb but oh well.
    
    mat4 joint_matrix;
    if(ub_skeleton && a_indices.rgba != uvec4(0, 0, 0, 0) && a_weights != vec4(0, 0, 0, 0)) {
        joint_matrix = 
                um_joints.data[a_indices.x] * a_weights.x +
                um_joints.data[a_indices.y] * a_weights.y +
                um_joints.data[a_indices.z] * a_weights.z +
                um_joints.data[a_indices.w] * a_weights.w;
    } else {
        joint_matrix = mat4(
            1, 0, 0, 0, 
            0, 1, 0, 0, 
            0, 0, 1, 0, 
            0, 0, 0, 1
        );
    }
    
    vec4 p = vec4(um_projection * um_view * um_model * joint_matrix * vec4(a_position, 1));
    gl_Position = p;
    
    v_position = a_position;
    v_uv = a_uv;
    v_uv2 = a_uv2;
    // We need to make sure all normals are applied world-space :D
    v_normal = mat3(transpose(inverse(um_model))) * a_normal;
    v_colour = a_colour;
}